import { httpClient } from 'src/api//base/httpClient'
import type { AppSettingType } from './appSetting'

export interface INotificationSettings {
  email: string
  smtpHost: string
  smtpPort: number
  password: string
  isValid: boolean
}

/**
 * 获取 smtp 邮件通知设置
 * @param type
 * @returns
 */
export function getSmtpNotificationSetting (type: AppSettingType) {
  return httpClient.get<INotificationSettings>('/notification-setting', {
    params: {
      type
    },
    passError: true
  })
}


/**
 * 验证通知邮箱设置
 * @returns
 */
export function updateSmtpNotificationSetting (settings: INotificationSettings, type: AppSettingType) {
  return httpClient.put<boolean>('/notification-setting', {
    data: settings,
    params: {
      type
    },
    passError: true
  })
}
