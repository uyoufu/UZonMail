import { httpClient } from 'src/api//base/httpClient'
import type { IRequestPagination } from 'src/compositions/types'
import type { IUserInfo } from 'src/stores/types'

// #region 权限码
export interface IPermissionCode {
  code: string,
  description?: string
}

/**
 * 获取权限码数量
 * @param filter
 * @returns
 */
export function getPermissionCodesCount (filter: string | undefined) {
  return httpClient.get<number>('/permission/permission-code/filtered-count', {
    params: {
      filter
    }
  })
}

/**
 * 获取权限码数据
 * @param filter
 * @param pagination
 * @returns
 */
export function getPermissionCodesData (filter: string | undefined, pagination: IRequestPagination) {
  return httpClient.post<IPermissionCode[]>('/permission/permission-code/filtered-data', {
    params: {
      filter
    },
    data: {
      pagination
    }
  })
}

/**
 * 获取所有的权限码
 * @returns
 */
export function getAllPermissionCodes () {
  return httpClient.get<IPermissionCode[]>('/permission/permission-code/all')
}

/**
 * 更新权限码
 * @param routePermissionCodes
 */
export function updateRoutePermissionCodes (routePermissionCodes: IPermissionCode[]) {
  return httpClient.put<IPermissionCode[]>('/permission/permission-code', {
    data: routePermissionCodes
  })
}
// #endregion

// #region 角色
export interface IRole {
  name: string, // 名称
  icon: string, // 图标
  permissionCodesCount: number, // 权限码数量
  description?: string,
  permissionCodeIds: number[]
}

/**
 * 获取权限码数量
 * @param filter
 * @returns
 */
export function getRolesCount (filter: string | undefined) {
  return httpClient.get<number>('/permission/role/filtered-count', {
    params: {
      filter
    }
  })
}

/**
 * 获取权限码数据
 * @param filter
 * @param pagination
 * @returns
 */
export function getRolesData (filter: string | undefined, pagination: IRequestPagination) {
  return httpClient.post<IRole[]>('/permission/role/filtered-data', {
    params: {
      filter
    },
    data: {
      pagination
    }
  })
}

/**
 * 更新角色
 */
export function upsertRole (role: IRole) {
  return httpClient.post<IRole>('/permission/role/upsert', {
    data: role
  })
}

/**
 * 删除角色
 * @param roleId
 * @returns
 */
export function deleteRole (roleId: number) {
  return httpClient.delete(`/permission/role/${roleId}`)
}

/**
 * 获取所有的角色
 * @returns
 */
export function getAllRoles () {
  return httpClient.get<IRole[]>('/permission/role/all')
}
// #endregion

// #region userRole
export interface IUserRole {
  id: number,
  userId: number,
  user: IUserInfo,
  roles: IRole[]
}

/**
 * 新增或者修改用户角色
 * @returns
 */
export function upsertUserRole (userRole: IUserRole) {
  return httpClient.post<IUserRole[]>('/permission/user-role', {
    data: userRole
  })
}

/**
 * 获取用户角色数量
 * @param filter
 * @returns
 */
export function getUserRolesCount (filter: string | undefined) {
  return httpClient.get<number>('/permission/user-role/filtered-count', {
    params: {
      filter
    }
  })
}

/**
 * 获取用户角色数据
 * @param filter
 * @param pagination
 * @returns
 */
export function getUserRolesData (filter: string | undefined, pagination: IRequestPagination) {
  return httpClient.post<IUserRole[]>('/permission/user-role/filtered-data', {
    params: {
      filter
    },
    data: {
      pagination
    }
  })
}

/**
 * 删除用户角色
 * @param userRoleId
 * @returns
 */
export function deleteUserRoles (userRoleId: number) {
  return httpClient.delete<boolean[]>(`/permission/user-role/${userRoleId}`)
}
// #endregion
