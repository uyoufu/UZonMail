<template>
  <aside class="file-category-tree column no-wrap">
    <div class="row items-center justify-between q-pa-sm">
      <div class="text-subtitle2">{{ t('fileManager.categories') }}</div>
      <q-btn flat round dense icon="create_new_folder" :aria-label="t('fileManager.createCategory')" @click="onCreateCategory()">
        <q-tooltip>{{ t('fileManager.createCategory') }}</q-tooltip>
      </q-btn>
    </div>
    <q-separator />
    <DraggableTree
      class="col q-pa-xs"
      :data="treeNodes"
      node-key="id"
      default-expand-all
      draggable
      :allow-drag="onAllowDrag"
      :allow-drop="onAllowDrop"
      :context-menu-items="contextMenuItems"
      v-model:current-node-key="selectedCategoryId"
      @node-click="onNodeClick"
      @node-drop="onNodeDrop"
    >
      <template #default="{ data }">
        <q-icon :name="data.id === allFilesCategoryId ? 'folder_open' : 'folder'" size="18px" class="q-mr-xs" />
        <span class="ellipsis">{{ getCategoryLabel(data as FileCategoryTreeNode) }}</span>
      </template>
    </DraggableTree>
  </aside>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n'
import DraggableTree from 'src/components/draggableTree/DraggableTree.vue'
import type { DraggableTreeContextValue, TreeDropType } from 'src/components/draggableTree/types'
import type { IContextMenuItem } from 'src/components/contextMenu/types'
import type { TreeNodeData } from 'element-plus/es/components/tree/src/tree.type'
import { LowCodeFieldType } from 'src/components/lowCode/types'
import { confirmOperation, showDialog } from 'src/utils/dialog'
import {
  createFileCategory,
  deleteFileCategory,
  getFileCategories,
  moveFileCategory,
  renameFileCategory,
  type FileCategoryPlacement,
  type IFileCategory
} from 'src/api/fileCategory'

interface FileCategoryTreeNode extends IFileCategory {
  label: string
}

const allFilesCategoryId = 0
const emit = defineEmits<{ change: [categoryId?: number] }>()
const { t } = useI18n()
const categories = ref<IFileCategory[]>([])
const selectedCategoryId = ref<number>(allFilesCategoryId)
const treeNodes = computed<FileCategoryTreeNode[]>(() => [
  {
    id: allFilesCategoryId,
    name: '',
    label: t('fileManager.allFiles'),
    sort: Number.MIN_SAFE_INTEGER,
    isDefault: false
  },
  ...categories.value.map(category => ({ ...category, label: getCategoryLabel(category) }))
])

const contextMenuItems: IContextMenuItem<DraggableTreeContextValue>[] = [
  {
    name: 'create',
    label: t('fileManager.createSubcategory'),
    vif: value => value.data.id !== allFilesCategoryId && !value.data.isDefault,
    onClick: value => onCreateCategory(value.data as FileCategoryTreeNode)
  },
  {
    name: 'rename',
    label: t('fileManager.rename'),
    vif: value => value.data.id !== allFilesCategoryId && !value.data.isDefault,
    onClick: value => onRenameCategory(value.data as FileCategoryTreeNode)
  },
  {
    name: 'delete',
    label: t('fileManager.delete'),
    color: 'negative',
    vif: value => value.data.id !== allFilesCategoryId && !value.data.isDefault,
    onClick: value => onDeleteCategory(value.data as FileCategoryTreeNode)
  }
]

/** 重新加载分类并保证当前选择仍然有效。 */
async function refreshCategories () {
  const { data } = await getFileCategories()
  categories.value = data
  if (selectedCategoryId.value !== allFilesCategoryId && !data.some(x => x.id === selectedCategoryId.value)) {
    selectedCategoryId.value = allFilesCategoryId
    emit('change', undefined)
  }
}

/** 默认分类名称由前端本地化，数据库名称保持稳定。 */
function getCategoryLabel (category: Pick<IFileCategory, 'name' | 'isDefault'>) {
  return category.isDefault ? t('fileManager.defaultCategory') : category.name
}

function onNodeClick (nodeData: TreeNodeData) {
  const category = nodeData as FileCategoryTreeNode
  selectedCategoryId.value = category.id
  emit('change', category.id === allFilesCategoryId ? undefined : category.id)
}

function onAllowDrag (node: { data: TreeNodeData }) {
  const category = node.data as FileCategoryTreeNode
  return category.id !== allFilesCategoryId && !category.isDefault
}

function onAllowDrop (
  _draggingNode: { data: TreeNodeData },
  dropNode: { data: TreeNodeData }
) {
  const dropCategory = dropNode.data as FileCategoryTreeNode
  return dropCategory.id !== allFilesCategoryId && !dropCategory.isDefault
}

async function onNodeDrop (
  draggingNode: { data: TreeNodeData },
  dropNode: { data: TreeNodeData },
  dropType: TreeDropType
) {
  const placement: Record<TreeDropType, FileCategoryPlacement> = {
    before: 'Before',
    after: 'After',
    inner: 'Inside'
  }
  const draggingCategory = draggingNode.data as FileCategoryTreeNode
  const dropCategory = dropNode.data as FileCategoryTreeNode
  await moveFileCategory(draggingCategory.id, dropCategory.id, placement[dropType])
  await refreshCategories()
}

async function onCreateCategory (parent?: FileCategoryTreeNode) {
  if (parent?.isDefault) return
  const result = await showCategoryNameDialog(t('fileManager.createCategory'), '')
  if (!result) return
  await createFileCategory(result, parent?.id || undefined)
  await refreshCategories()
}

async function onRenameCategory (category: FileCategoryTreeNode) {
  if (category.id === allFilesCategoryId || category.isDefault) return
  const result = await showCategoryNameDialog(t('fileManager.renameCategory'), category.name)
  if (!result) return
  await renameFileCategory(category.id, result)
  await refreshCategories()
}

async function onDeleteCategory (category: FileCategoryTreeNode) {
  if (category.id === allFilesCategoryId || category.isDefault) return
  const confirmed = await confirmOperation(
    t('fileManager.deleteCategory'),
    t('fileManager.deleteCategoryConfirm', { name: getCategoryLabel(category) })
  )
  if (!confirmed) return
  await deleteFileCategory(category.id)
  await refreshCategories()
}

async function showCategoryNameDialog (title: string, value: string) {
  const result = await showDialog({
    title,
    fields: [{ name: 'name', label: t('fileManager.categoryName'), type: LowCodeFieldType.text, required: true, value }],
    oneColumn: true
  })
  return result.ok ? String(result.data.name).trim() : undefined
}

onMounted(refreshCategories)
defineExpose({ refreshCategories })
</script>

<style scoped>
.file-category-tree {
  width: 220px;
  min-width: 180px;
  height: 100%;
  border: 1px solid var(--q-separator-color);
  overflow: hidden;
}
</style>
