/* eslint-disable @typescript-eslint/no-explicit-any */
import { httpClientPro } from 'src/api/base/httpClient'
import type { IOrganizationSetting } from '../organizationSetting'

/**
 * 获取用户设置
 * @returns
 */
export function getCurrentDepartmentSetting (departmentId: number) {
  return httpClientPro.get<IOrganizationSetting>(`/department-setting/${departmentId}`)
}

/**
 * 更新用户设置
 * @param userSetting
 * @returns
 */
export function updateDepartmentSetting (departmentId: number, userSetting: IOrganizationSetting) {
  return httpClientPro.put<Record<string, any>>(`/department-setting/${departmentId}`, {
    data: userSetting
  })
}
