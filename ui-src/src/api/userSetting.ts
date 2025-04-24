/* eslint-disable @typescript-eslint/no-explicit-any */

import { httpClient } from 'src/api//base/httpClient'
import type { ISystemSetting } from './systemSetting'

/**
 * 更新单个字符串型系统设置
 * @param key
 * @param value
 * @returns
 */
export function updateUserSettingString (key: string, value: string) {
  return httpClient.put<boolean>('/user-setting/string', {
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
export function updateUserSettingBoolean (key: string, value: boolean) {
  return httpClient.put<boolean>('/user-setting/boolean', {
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
export function updateUserSettingJson (key: string, value: Record<string, any>) {
  return httpClient.put<boolean>('/user-setting/json', {
    params: {
      key
    },
    data: value
  })
}

/**
 * 获取系统设置
 * @param key
 * @returns
 */
export function getUserSetting<T = ISystemSetting> (key: string) {
  return httpClient.get<T>('/user-setting/kv', {
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
export function getUserSettings (keys: string[]) {
  return httpClient.get<Record<string, string | number | boolean>>('/user-setting/kvs', {
    params: {
      keys: keys.join(',')
    }
  })
}
