import logger from 'loglevel'
import { getUsableProxies, type IProxy } from 'src/api/proxy'

/** 按需加载邮箱账户可绑定的代理，避免在弹窗打开时提前请求无关数据。 */
export function useEmailAccountProxyOptions() {
  const proxyOptions = ref<IProxy[]>([])
  const isLoadingProxyOptions = ref(false)
  let hasLoadedProxyOptions = false

  async function onProxyPopupShow() {
    if (hasLoadedProxyOptions || isLoadingProxyOptions.value) return

    isLoadingProxyOptions.value = true
    try {
      const { data: proxies } = await getUsableProxies()
      proxyOptions.value = proxies ?? []
      hasLoadedProxyOptions = true
    } catch (error) {
      logger.warn('[EmailAccountDialog] 可用代理加载失败', error)
    } finally {
      isLoadingProxyOptions.value = false
    }
  }

  return {
    proxyOptions,
    isLoadingProxyOptions,
    onProxyPopupShow
  }
}
