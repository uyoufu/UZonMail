/* eslint-disable @typescript-eslint/no-explicit-any */
import type { IProxy } from 'src/api/proxy'
import { useUserInfoStore } from 'src/stores/user'
import { t } from 'src/i18n/helpers'

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
      return t('pages.proxy.unshared')
    }

    if (isOwner(proxyInfo)) {
      return t('pages.proxy.sharing')
    }

    return t('pages.proxy.sharedByOther')
  }

  return {
    isOwner,
    disableShareToggle,
    getProxyShareTooltip
  }
}
