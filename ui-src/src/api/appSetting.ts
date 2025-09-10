/* eslint-disable @typescript-eslint/no-explicit-any */


import { httpClient } from 'src/api//base/httpClient'
import { useConfig } from 'src/config'

export enum AppSettingType {
  System = 1,
  Organization = 2,
  User = 4
}

export interface IAppSetting {
  key: string,
  type: AppSettingType,
  userId: number,
  organizationId: number,
  stringValue?: string | null,
  boolValue?: boolean,
  intValue?: number,
  longValue?: number,
  dateTime?: string,
  json?: Record<string, any>
}

// #region 通用设置
/**
 * 获取用户设置
 * @returns
 */
export function updateServerBaseApiUrl () {
  const config = useConfig()
  return httpClient.put<boolean>('/app-setting/base-api-url', {
    data: {
      baseApiUrl: config.baseUrl
    }
  })
}

/**
 * 更新单个字符串型系统设置
 * @param key
 * @param value
 * @returns
 */
export function updateSystemSettingString (key: string, value: string, type: AppSettingType = AppSettingType.System) {
  return httpClient.put<boolean>('/app-setting/string', {
    params: {
      key,
      value,
      type
    }
  })
}

/**
 * 更新单个数字型系统设置
 * @param key
 * @param value
 * @returns
 */
export function updateSystemSettingBoolean (key: string, value: boolean, type: AppSettingType = AppSettingType.System) {
  return httpClient.put<boolean>('/app-setting/boolean', {
    params: {
      key,
      value,
      type
    }
  })
}

/**
 * 批量更新系统设置
 * @param key
 * @param value
 * @returns
 */
export function updateSystemSettingJson (key: string, value: Record<string, any>, type: AppSettingType = AppSettingType.System) {
  return httpClient.put<boolean>('/app-setting/json', {
    params: {
      key,
      type
    },
    data: value
  })
}

/**
 * 获取系统设置
 * @param key
 * @returns
 */
export function getSystemSetting<T = IAppSetting> (key: string, type: AppSettingType = AppSettingType.System) {
  return httpClient.get<T>('/app-setting/kv', {
    params: {
      key,
      type
    }
  })
}
// #endregion

// #region 发送设置
export interface ISendingSetting {
  userId: string,
  maxSendCountPerEmailDay: number,
  minOutboxCooldownSecond: number,
  maxOutboxCooldownSecond: number,
  maxSendingBatchSize: number,
  minInboxCooldownHours: number,
  replyToEmails?: string,
  changeIpAfterEmailCount: number,
  maxCountPerIPDomainHour: number,
}

/**
 * 获取发送设置
 * @returns
 */
export function getSendingSetting (type: AppSettingType = AppSettingType.System) {
  return httpClient.get<ISendingSetting>('/app-setting/sending-setting', {
    params: {
      type
    }
  })
}

/**
 * 更新发送设置
 * @param sendingSetting
 * @returns
 */
export function updateSendingSetting (sendingSetting: ISendingSetting, type: AppSettingType = AppSettingType.System) {
  return httpClient.put<Record<string, any>>('/app-setting/sending-setting', {
    params: {
      type
    },
    data: sendingSetting
  })
}
// #endregion
