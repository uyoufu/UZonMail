import { useResizeObserver } from '@vueuse/core'
import type { QTable } from 'quasar'
import { Platform } from 'quasar'

export const CollapsePositionSide = {
  left: 'left',
  right: 'right'
} as const

type CollapsePositionSide = typeof CollapsePositionSide[keyof typeof CollapsePositionSide]

/** 创建随表格和定位容器尺寸变化而更新的折叠图标位置。 */
export function useTableCollapsePosition(
  containerRef: Ref<InstanceType<typeof QTable> | undefined>,
  positionSide: CollapsePositionSide,
  offset: number
) {
  const collapseStyleRef = ref<Record<string, string>>({
    position: 'absolute',
    top: '40%',
    [positionSide]: `${offset}px`
  })
  const isCollapseGroupList = ref(!Platform.is.desktop)
  const observedElements = shallowRef<HTMLElement[]>([])

  function updateCollapseLocation() {
    const containerElement = containerRef.value?.$el as HTMLElement | undefined
    if (!containerElement) return

    // 折叠图标与表格共享绝对定位上下文，使用边界差可兼容嵌套布局和动态侧栏宽度。
    const positioningElement = containerElement.offsetParent as HTMLElement | null
      ?? containerElement.parentElement
    if (!positioningElement) return

    const containerRect = containerElement.getBoundingClientRect()
    const positioningRect = positioningElement.getBoundingClientRect()
    const distance = positionSide === CollapsePositionSide.left
      ? containerRect.left - positioningRect.left
      : positioningRect.right - containerRect.right
    collapseStyleRef.value[positionSide] = `${Math.max(0, distance + offset)}px`
  }

  async function initializeResizeObservation() {
    await nextTick()
    const containerElement = containerRef.value?.$el as HTMLElement | undefined
    if (!containerElement) return

    const positioningElement = containerElement.offsetParent as HTMLElement | null
      ?? containerElement.parentElement
    observedElements.value = positioningElement && positioningElement !== containerElement
      ? [containerElement, positioningElement]
      : [containerElement]
    updateCollapseLocation()
  }

  useResizeObserver(observedElements, updateCollapseLocation)
  watch(isCollapseGroupList, initializeResizeObservation, { flush: 'post' })
  watch(containerRef, initializeResizeObservation, { flush: 'post' })
  onMounted(initializeResizeObservation)

  return {
    isCollapseGroupList,
    collapseStyleRef
  }
}
