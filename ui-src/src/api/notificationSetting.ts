import { httpClient } from 'src/api//base/httpClient'

/**
 * 验证通知邮箱设置
 * @returns
 */
export function validateNotificationEmailSettings () {
  return httpClient.put<boolean>('/notification-setting/validate', {
    passError: true
  })
}
