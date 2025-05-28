
import { httpClientPro } from 'src/api/base/httpClient'

export interface ISystemConfig {
  name: string,
  loginWelcome: string,
  icon: string,
  copyright: string,
  icpInfo: string
}

/**
 * 获取系统设置
 * @returns
 */
export function getSystemConfig () {
  return httpClientPro.get<ISystemConfig>('/system-info/config', {
    stopNotifyError: true
  })
}

