import type { TreeNodeData } from 'element-plus/es/components/tree/src/tree.type'

export type TreeDropType = 'before' | 'after' | 'inner'

export interface DraggableTreeContextValue<TNode extends TreeNodeData = TreeNodeData> {
  data: TNode
  node: unknown
}

export interface DraggableTreeEmptyContextValue<TNode extends TreeNodeData = TreeNodeData> {
  data: TNode[]
}

export type DraggableTreeAllowDrag<TNode extends TreeNodeData = TreeNodeData> = (
  node: { data: TNode }
) => boolean

export type DraggableTreeAllowDrop<TNode extends TreeNodeData = TreeNodeData> = (
  draggingNode: { data: TNode },
  dropNode: { data: TNode },
  dropType: TreeDropType
) => boolean
