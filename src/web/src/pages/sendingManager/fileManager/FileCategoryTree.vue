<template>
  <aside class="column no-wrap">
    <q-list dense class="column no-wrap justify-start">
      <q-item class="plain-list__item text-primary bg-grey-11" v-ripple>
        <q-item-section avatar class="q-pr-none">
          <q-icon name="attachment" />
        </q-item-section>

        <q-item-section class="q-px-lg q-py-sm text-bold">
          {{ t('fileManager.categories') }}
          <AsyncTooltip :tooltip="t('fileManager.categories')" />
        </q-item-section>

        <q-item-section side>
          <q-btn flat round dense icon="create_new_folder" :aria-label="t('fileManager.createCategory')"
            @click="onCreateCategory()">
            <q-tooltip>{{ t('fileManager.createCategory') }}</q-tooltip>
          </q-btn>
        </q-item-section>
      </q-item>
    </q-list>

    <DraggableTree :data="treeNodes" node-key="id" default-expand-all draggable :allow-drag="onAllowDrag"
      :allow-drop="onAllowDrop" :context-menu-items="contextMenuItems" v-model:current-node-key="selectedCategoryId"
      @node-click="onNodeClick" @node-drop="onNodeDrop">
      <template #default="{ data }">
        <q-icon :name="data.id === allFilesCategoryId ? 'folder_open' : 'folder'" size="18px" class="q-mr-xs" />
        <span class="ellipsis q-pr-sm">{{ getCategoryDisplayName(data as IFileCategory) }}</span>
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

const allFilesCategoryId = 0
const emit = defineEmits<{ change: [categoryId?: number] }>()
const { t } = useI18n()
const categories = ref<IFileCategory[]>([])
const selectedCategoryId = ref<number>(allFilesCategoryId)
const treeNodes = computed<IFileCategory[]>(() => [
  {
    id: allFilesCategoryId,
    name: t('fileManager.allFiles'),
    sort: Number.MIN_SAFE_INTEGER,
    isDefault: false
  },
  ...categories.value
])

const contextMenuItems: IContextMenuItem<DraggableTreeContextValue>[] = [
  {
    name: 'create',
    label: t('fileManager.createSubcategory'),
    vif: value => value.data.id !== allFilesCategoryId && !value.data.isDefault,
    onClick: value => onCreateCategory(value.data as IFileCategory)
  },
  {
    name: 'rename',
    label: t('fileManager.rename'),
    vif: value => value.data.id !== allFilesCategoryId && !value.data.isDefault,
    onClick: value => onRenameCategory(value.data as IFileCategory)
  },
  {
    name: 'delete',
    label: t('fileManager.delete'),
    color: 'negative',
    vif: value => value.data.id !== allFilesCategoryId && !value.data.isDefault,
    onClick: value => onDeleteCategory(value.data as IFileCategory)
  }
]

/** 重新加载分类并保证当前选择仍然有效。 */
async function refreshCategories() {
  const { data } = await getFileCategories()
  categories.value = data
  if (selectedCategoryId.value !== allFilesCategoryId && !data.some(x => x.id === selectedCategoryId.value)) {
    selectedCategoryId.value = allFilesCategoryId
    emit('change', undefined)
  }
}

/** 默认分类名称由前端本地化，数据库名称保持稳定。 */
function getCategoryDisplayName(category: Pick<IFileCategory, 'name' | 'isDefault'>) {
  return category.isDefault ? t('fileManager.defaultCategory') : category.name
}

function onNodeClick(nodeData: TreeNodeData) {
  const category = nodeData as IFileCategory
  selectedCategoryId.value = category.id
  emit('change', category.id === allFilesCategoryId ? undefined : category.id)
}

function onAllowDrag(node: { data: TreeNodeData }) {
  const category = node.data as IFileCategory
  return category.id !== allFilesCategoryId && !category.isDefault
}

function onAllowDrop(
  _draggingNode: { data: TreeNodeData },
  dropNode: { data: TreeNodeData }
) {
  const dropCategory = dropNode.data as IFileCategory
  return dropCategory.id !== allFilesCategoryId && !dropCategory.isDefault
}

async function onNodeDrop(
  draggingNode: { data: TreeNodeData },
  dropNode: { data: TreeNodeData },
  dropType: TreeDropType
) {
  const placement: Record<TreeDropType, FileCategoryPlacement> = {
    before: 'Before',
    after: 'After',
    inner: 'Inside'
  }
  const draggingCategory = draggingNode.data as IFileCategory
  const dropCategory = dropNode.data as IFileCategory
  await moveFileCategory(draggingCategory.id, dropCategory.id, placement[dropType])
  await refreshCategories()
}

async function onCreateCategory(parent?: IFileCategory) {
  if (parent?.isDefault) return
  const result = await showCategoryNameDialog(t('fileManager.createCategory'), '')
  if (!result) return
  await createFileCategory(result, parent?.id || undefined)
  await refreshCategories()
}

async function onRenameCategory(category: IFileCategory) {
  if (category.id === allFilesCategoryId || category.isDefault) return
  const result = await showCategoryNameDialog(t('fileManager.renameCategory'), category.name)
  if (!result) return
  await renameFileCategory(category.id, result)
  await refreshCategories()
}

async function onDeleteCategory(category: IFileCategory) {
  if (category.id === allFilesCategoryId || category.isDefault) return
  const confirmed = await confirmOperation(
    t('fileManager.deleteCategory'),
    t('fileManager.deleteCategoryConfirm', { name: getCategoryDisplayName(category) })
  )
  if (!confirmed) return
  await deleteFileCategory(category.id)
  await refreshCategories()
}

async function showCategoryNameDialog(title: string, value: string) {
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

<style lang="scss" scoped>
.plain-list__item {
  :deep(.q-item__section--avatar) {
    min-width: auto !important;
  }
}
</style>
