import { setTimeoutAsync } from 'src/utils/tsUtils'
import CollapseLeft from './CollapseLeft.vue'
import type { QTable } from 'quasar'
import logger from 'loglevel'

/**
 * 使用折叠左侧组件
 * @param containerRef
 * @param offsetLeft
 * @returns
 */
export function useTableCollapseLeft (containerRef: Ref<InstanceType<typeof QTable> | undefined>, updateSignal: Ref<boolean>, offsetLeft: number = 10) {
  const collapseStyleRef = ref({
    position: 'absolute',
    top: '40%',
    left: `${offsetLeft}px`
  })

  function updateCollapseLocation () {
    if (!containerRef.value) return
    const containerElement = containerRef.value.$el as HTMLElement
    logger.log('[components] containerElement offsetLeft:', containerElement.offsetLeft)
    // 更新
    collapseStyleRef.value.left = `${containerElement.offsetLeft + offsetLeft}px`
  }

  watch(updateSignal, async () => {
    await nextTick()
    updateCollapseLocation()
  })

  onMounted(async () => {
    await nextTick()
    await setTimeoutAsync(10)
    updateCollapseLocation()
  })

  return {
    collapseStyleRef,
    CollapseLeft
  }
}
