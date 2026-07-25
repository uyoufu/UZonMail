<template>
  <div class="draggable-tree">
    <ElTree ref="treeRef" class="draggable-tree__tree" :data="treeData" :node-key="props.nodeKey"
      :props="props.treeProps" :default-expand-all="props.defaultExpandAll" :highlight-current="props.highlightCurrent"
      :expand-on-click-node="props.expandOnClickNode" :draggable="props.draggable" :allow-drag="props.allowDrag"
      :allow-drop="onAllowDrop" :show-checkbox="props.showCheckbox" :default-checked-keys="checkedKeys"
      :default-expanded-keys="expandedKeys" :current-node-key="currentNodeKey ?? undefined" @node-click="onNodeClick"
      @node-expand="onNodeExpand" @node-collapse="onNodeCollapse" @node-drop="onNodeDrop" @check="onTreeCheck">
      <template #default="{ node, data: nodeData }">
        <div class="draggable-tree__node">
          <slot :node="node" :data="nodeData">
            <span>{{ nodeData?.label }}</span>
          </slot>
          <ContextMenu v-if="props.contextMenuItems.length > 0" :items="props.contextMenuItems"
            :value="{ data: nodeData, node }" />
        </div>
      </template>
    </ElTree>
    <div v-if="props.data.length < 1 && props.emptyContextMenuItems.length > 0"
      class="draggable-tree__empty-menu-target">
      <ContextMenu :items="props.emptyContextMenuItems" :value="{ data: props.data }" />
    </div>
  </div>
</template>

<script setup lang="ts">
import ContextMenu from 'src/components/contextMenu/ContextMenu.vue'
import type { IContextMenuItem } from 'src/components/contextMenu/types'
import type { AllowDropType, CheckedInfo, NodeDropType, TreeKey, TreeNodeData } from 'element-plus/es/components/tree/src/tree.type'
import type { TreeInstance } from 'element-plus'
import type {
  DraggableTreeAllowDrag,
  DraggableTreeAllowDrop,
  DraggableTreeContextValue,
  DraggableTreeEmptyContextValue,
  TreeDropType
} from './types'

const props = withDefaults(defineProps<{
  data: TreeNodeData[]
  nodeKey?: string
  treeProps?: { label?: string, children?: string, disabled?: string }
  defaultExpandAll?: boolean
  highlightCurrent?: boolean
  expandOnClickNode?: boolean
  draggable?: boolean
  showCheckbox?: boolean
  allowDrag?: DraggableTreeAllowDrag
  allowDrop?: DraggableTreeAllowDrop
  contextMenuItems?: IContextMenuItem<DraggableTreeContextValue>[]
  emptyContextMenuItems?: IContextMenuItem<DraggableTreeEmptyContextValue>[]
}>(), {
  nodeKey: 'id',
  treeProps: () => ({ label: 'label', children: 'children' }),
  defaultExpandAll: false,
  highlightCurrent: true,
  expandOnClickNode: false,
  draggable: false,
  showCheckbox: false,
  contextMenuItems: () => [],
  emptyContextMenuItems: () => []
})

const emit = defineEmits<{
  nodeClick: [data: TreeNodeData, node: unknown, event?: Event]
  nodeDrop: [draggingNode: { data: TreeNodeData }, dropNode: { data: TreeNodeData }, dropType: TreeDropType, event: Event]
  check: [data: TreeNodeData, checkedInfo: CheckedInfo]
}>()

const checkedKeys = defineModel<TreeKey[]>('checkedKeys', { default: () => [] })
const halfCheckedKeys = defineModel<TreeKey[]>('halfCheckedKeys', { default: () => [] })
const expandedKeys = defineModel<TreeKey[]>('expandedKeys', { default: () => [] })
const currentNodeKey = defineModel<TreeKey | null>('currentNodeKey', { default: null })
const treeRef = ref<TreeInstance>()
const treeData = computed(() => buildTreeData(props.data, props.nodeKey, props.treeProps.children ?? 'children'))

/** Forward a node click and restore the externally controlled current key. */
function onNodeClick(data: TreeNodeData, node: unknown, _nodeInstance?: unknown, event?: Event) {
  emit('nodeClick', data, node, event)
  void nextTick(() => treeRef.value?.setCurrentKey(currentNodeKey.value))
}

/** Add one expanded node to the controlled key set. */
function onNodeExpand(data: TreeNodeData): void {
  const key = data[props.nodeKey]
  if (!expandedKeys.value.includes(key)) expandedKeys.value = [...expandedKeys.value, key]
}

/** Remove one collapsed node and its descendants from the controlled key set. */
function onNodeCollapse(data: TreeNodeData): void {
  const path = String(data[props.nodeKey])
  expandedKeys.value = expandedKeys.value.filter((key) => String(key) !== path && !String(key).startsWith(`${path}/`))
}

/** Delegate drag/drop acceptance to the optional caller policy. */
function onAllowDrop(
  draggingNode: { data: TreeNodeData },
  dropNode: { data: TreeNodeData },
  dropType: AllowDropType
) {
  if (!props.allowDrop) return true
  return props.allowDrop(draggingNode, dropNode, normalizeAllowDropType(dropType))
}

/** Normalize and forward an accepted tree drop. */
function onNodeDrop(
  draggingNode: { data: TreeNodeData },
  dropNode: { data: TreeNodeData },
  dropType: Exclude<NodeDropType, 'none'>,
  event: Event
) {
  emit('nodeDrop', draggingNode, dropNode, dropType, event)
}

/** Synchronize checked and half-checked model keys. */
function onTreeCheck(data: TreeNodeData, checkedInfo: CheckedInfo) {
  checkedKeys.value = checkedInfo.checkedKeys
  halfCheckedKeys.value = checkedInfo.halfCheckedKeys
  emit('check', data, checkedInfo)
}

/** Normalize Element Plus drop names into the shared tree contract. */
function normalizeAllowDropType(dropType: AllowDropType): TreeDropType {
  if (dropType === 'prev') return 'before'
  if (dropType === 'next') return 'after'
  return dropType
}

/** Convert flat parent-linked nodes into a sorted nested tree. */
function buildTreeData(nodes: TreeNodeData[], nodeKey: string, childrenKey: string) {
  const nodeMap = new Map<unknown, TreeNodeData>()
  const roots: TreeNodeData[] = []

  nodes.forEach((node) => {
    const children = Array.isArray(node[childrenKey])
      ? [...node[childrenKey]]
      : []
    nodeMap.set(node[nodeKey], { ...node, [childrenKey]: children })
  })

  nodes.forEach((node) => {
    const treeNode = nodeMap.get(node[nodeKey])
    if (!treeNode) return

    const parentId = node.parentId
    const parentNode = parentId === undefined || parentId === null
      ? undefined
      : nodeMap.get(parentId)
    if (parentNode) {
      const children = parentNode[childrenKey] as TreeNodeData[]
      children.push(treeNode)
      return
    }

    roots.push(treeNode)
  })

  sortTreeNodes(roots, childrenKey)
  return roots
}

/** Sort every tree level using the node ordering contract. */
function sortTreeNodes(nodes: TreeNodeData[], childrenKey: string) {
  nodes.sort(compareTreeNodeOrder)
  nodes.forEach((node) => sortTreeNodes(node[childrenKey] as TreeNodeData[], childrenKey))
}

/** Compare two tree nodes by explicit sort order and stable identifier. */
function compareTreeNodeOrder(leftNode: TreeNodeData, rightNode: TreeNodeData) {
  const leftSort = typeof leftNode.sort === 'number' ? leftNode.sort : 0
  const rightSort = typeof rightNode.sort === 'number' ? rightNode.sort : 0
  if (leftSort !== rightSort) return leftSort - rightSort

  const leftId = Number(leftNode.id)
  const rightId = Number(rightNode.id)
  if (Number.isFinite(leftId) && Number.isFinite(rightId)) return leftId - rightId

  return String(leftNode.id ?? '').localeCompare(String(rightNode.id ?? ''))
}
</script>

<style scoped>
.draggable-tree {
  min-height: 100%;
  position: relative;
}

.draggable-tree__tree {
  min-height: 100%;
}

.draggable-tree__node {
  display: flex;
  align-items: center;
  width: 100%;
}

.draggable-tree__empty-menu-target {
  min-height: 120px;
}
</style>
