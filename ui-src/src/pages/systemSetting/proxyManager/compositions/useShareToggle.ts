/* eslint-disable @typescript-eslint/no-explicit-any */
import { IProxy } from 'src/api/proxy'
import { useUserInfoStore } from 'src/stores/user'

export function useShareToggle () {
  const userInfo = useUserInfoStore()
  function isOwner (proxyInfo: Record<string, any>) {
    return userInfo.userSqlId === proxyInfo.userId
  }

  /**
   * 是否禁用共享开关
   * @param proxyInfo
   * @returns
   */
  function disableShareToggle (proxyInfo: Record<string, any>) {
    return !isOwner(proxyInfo)
  }

  function getProxyShareTooltip (proxyInfo: Record<string, any>) {
    const proxy = proxyInfo as IProxy
    if (!proxy.isShared) {
      return '未共享'
    }

    if (isOwner(proxyInfo)) {
      return '共享中'
    }

    return '由其他用户共享'
  }

  return {
    isOwner,
    disableShareToggle,
    getProxyShareTooltip
  }
}
