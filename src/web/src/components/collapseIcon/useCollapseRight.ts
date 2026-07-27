import CollapseRight from './CollapseRight.vue'
import type { QTable } from 'quasar'
import { CollapsePositionSide, useTableCollapsePosition } from './useCollapsePosition'

/**
 * 使用折叠右侧组件
 * @param containerRef
 * @param offsetRight
 * @returns
 */
export function useTableCollapseRight (containerRef: Ref<InstanceType<typeof QTable> | undefined>, offsetRight: number = 10) {
  const { collapseStyleRef, isCollapseGroupList } = useTableCollapsePosition(
    containerRef,
    CollapsePositionSide.right,
    offsetRight
  )

  return {
    isCollapseGroupList,
    collapseStyleRef,
    CollapseRight
  }
}
