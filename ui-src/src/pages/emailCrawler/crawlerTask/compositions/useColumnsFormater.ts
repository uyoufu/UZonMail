import { CrawlerStatus, CrawlerType } from 'src/api/pro/crawlerTask'
import { getUsableProxies, IProxy } from 'src/api/proxy'

export function useColumnsFormater () {
  const usableProxies: Ref<IProxy[]> = ref([])
  // 获取用户的代理
  onMounted(async () => {
    const { data: proxies } = await getUsableProxies()
    usableProxies.value = proxies
  })

  /**
   * 格式化代理id
   * @param proxyId
   * @returns
   */
  function formatProxyId (proxyId: number) {
    return usableProxies.value.find(x => x.id === proxyId)?.name || ''
  }

  function formatCrawlerType (value: number) {
    return CrawlerType[value]
  }

  function formatCrawlerStatus (value: number) {
    return CrawlerStatus[value]
  }

  return {
    formatProxyId,
    formatCrawlerType,
    formatCrawlerStatus
  }
}
