import { httpClient } from 'src/api//base/httpClient'

/**
 * 子用户策略
 */
export enum SubUserStrategy {
  followMaster, // 跟随主账户
  independent // 独立
}

export interface IDepartmentSetting {
  departmentId: number,
  subUserStrategy: SubUserStrategy
}

/**
 * 获取部门设置
 * @param departmentId
 * @returns
 */
export function getDepartmentSetting (departmentId: number) {
  return httpClient.get<IDepartmentSetting>(`/department/${departmentId}`)
}

/**
 * 更新部门设置
 * @param departmentSetting
 * @returns
 */
export function updateDepartmentSetting (departmentSetting: IDepartmentSetting) {
  return httpClient.put<boolean>(`/department/${departmentSetting.departmentId}`, {
    data: departmentSetting
  })
}
