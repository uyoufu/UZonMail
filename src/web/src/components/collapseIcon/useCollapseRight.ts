import { setTimeoutAsync } from 'src/utils/tsUtils'
import CollapseRight from './CollapseRight.vue'
import type { QTable } from 'quasar'
import logger from 'loglevel'
import { Platform } from 'quasar'

/**
 * 使用折叠右侧组件
 * @param containerRef
 * @param offsetRight
 * @returns
 */
export function useTableCollapseRight (containerRef: Ref<InstanceType<typeof QTable> | undefined>, offsetRight: number = 10) {
  const collapseStyleRef = ref({
    position: 'absolute',
    top: '40%',
    right: `${offsetRight}px`
  })

  // 初始化折叠状态
  const isCollapseGroupList = ref(!Platform.is.desktop)

  function updateCollapseLocation () {
    if (!containerRef.value) return
    const containerElement = containerRef.value.$el as HTMLElement
    const parentElement = containerElement.parentElement
    if (!parentElement) return
    const rightDistance = parentElement.offsetWidth - containerElement.offsetLeft - containerElement.offsetWidth
    logger.log('[components] containerElement right distance:', rightDistance)
    // 更新
    collapseStyleRef.value.right = `${rightDistance + offsetRight}px`
  }

  watch(isCollapseGroupList, async () => {
    await nextTick()
    updateCollapseLocation()
  })

  onMounted(async () => {
    await nextTick()
    await setTimeoutAsync(10)
    updateCollapseLocation()
  })

  return {
    isCollapseGroupList,
    collapseStyleRef,
    CollapseRight
  }
}
