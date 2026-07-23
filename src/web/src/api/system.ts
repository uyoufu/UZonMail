import { httpClient } from 'src/api/base/httpClient'

/**
 * 获取服务器版本
 * @returns
 */
export function getServerVersion () {
  return httpClient.get<string>('/system-info/version')
}
