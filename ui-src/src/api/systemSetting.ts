
import { httpClient } from 'src/api//base/httpClient'
import { useConfig } from 'src/config'

/**
 * 获取用户设置
 * @returns
 */
export function updateServerBaseApiUrl () {
  const config = useConfig()
  return httpClient.put<boolean>('/system-setting/base-api-url', {
    data: {
      baseApiUrl: config.baseUrl
    }
  })
}
