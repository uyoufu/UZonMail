import { debounce } from 'quasar'
import { notifySuccess } from 'src/utils/dialog'
import { watch } from 'vue'

/* eslint-disable @typescript-eslint/no-explicit-any */
export function useSettingsAutoSaver<T = Record<string, any>> (settings: Ref<T>, getSettingsAsync: () => Promise<T>, updateSettingsAsync: () => Promise<void>) {
  const isStopWatching = ref(false)
  async function onBeforeShow () {
    // 获取设置
    const data = await getSettingsAsync()

    // 获取数据时，不要触发 watcher
    isStopWatching.value = true
    settings.value = data

    await nextTick()
    isStopWatching.value = false
  }

  const updateSettingsDebounce = debounce(async () => {
    // 更新设置
    await updateSettingsAsync()
  }, 1000)

  watch(
    settings,
    () => {
      if (isStopWatching.value) return

      updateSettingsDebounce()
      notifySuccess('设置更改已生效')
    },
    { deep: true }
  )

  async function updateValueSilently (task: () => void) {
    isStopWatching.value = true
    task()
    await nextTick()
    isStopWatching.value = false
  }

  return {
    updateValueSilently,
    onBeforeShow
  }
}

