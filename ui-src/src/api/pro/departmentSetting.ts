/* eslint-disable @typescript-eslint/no-explicit-any */
import { httpClientPro } from 'src/api/base/httpClient'
import type { ISendingSetting } from '../appSetting'

/**
 * 获取用户设置
 * @returns
 */
export function getCurrentDepartmentSetting (departmentId: number) {
  return httpClientPro.get<ISendingSetting>(`/department-setting/${departmentId}`)
}

/**
 * 更新用户设置
 * @param userSetting
 * @returns
 */
export function updateDepartmentSetting (departmentId: number, userSetting: ISendingSetting) {
  return httpClientPro.put<Record<string, any>>(`/department-setting/${departmentId}`, {
    data: userSetting
  })
}
