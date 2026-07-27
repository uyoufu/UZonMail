import CollapseLeft from './CollapseLeft.vue'
import type { QTable } from 'quasar'
import { CollapsePositionSide, useTableCollapsePosition } from './useCollapsePosition'

/**
 * 使用折叠左侧组件
 * @param containerRef
 * @param offsetLeft
 * @returns
 */
export function useTableCollapseLeft (containerRef: Ref<InstanceType<typeof QTable> | undefined>, offsetLeft: number = 10) {
  const { collapseStyleRef, isCollapseGroupList } = useTableCollapsePosition(
    containerRef,
    CollapsePositionSide.left,
    offsetLeft
  )

  return {
    isCollapseGroupList,
    collapseStyleRef,
    CollapseLeft
  }
}
