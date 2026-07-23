/* eslint-disable @typescript-eslint/no-explicit-any */

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

/**
 * 更新单个字符串型系统设置
 * @param key
 * @param value
 * @returns
 */
export function updateSystemSettingString (key: string, value: string) {
  return httpClient.put<boolean>('/system-setting/string', {
    params: {
      key,
      value
    }
  })
}

/**
 * 更新单个数字型系统设置
 * @param key
 * @param value
 * @returns
 */
export function updateSystemSettingBoolean (key: string, value: boolean) {
  return httpClient.put<boolean>('/system-setting/boolean', {
    params: {
      key,
      value
    }
  })
}

/**
 * 批量更新系统设置
 * @param key
 * @param value
 * @returns
 */
export function updateSystemSettingJson (key: string, value: Record<string, any>) {
  return httpClient.put<boolean>('/system-setting/json', {
    params: {
      key
    },
    data: value
  })
}

export interface ISystemSetting {
  key: string,
  stringValue?: string | null,
  boolValue?: boolean,
  intValue?: number,
  longValue?: number,
  dateTime?: string,
  json?: Record<string, any>
}

/**
 * 获取系统设置
 * @param key
 * @returns
 */
export function getSystemSetting<T = ISystemSetting> (key: string) {
  return httpClient.get<T>('/system-setting/kv', {
    params: {
      key
    }
  })
}

/**
 * 获取系统设置
 * @param keys
 * @returns
 */
export function getSystemSettings (keys: string[]) {
  return httpClient.get<Record<string, string | number | boolean>>('/system-setting/kvs', {
    params: {
      keys: keys.join(',')
    }
  })
}
