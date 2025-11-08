/* eslint-disable @typescript-eslint/no-explicit-any */
import { httpClient } from 'src/api//base/httpClient'
import type { IUserInfo, UserStatus, UserType } from 'src/stores/types'
import type { IRequestPagination } from 'src/compositions/types'
import { sha256, getSmtpPasswordSecretKeys } from 'src/utils/encrypt'

export interface IUserLoginInfo {
  token: string,
  access: string[],
  userInfo: IUserInfo,
  installedPlugins: string[]
}

/**
 * 用户登录
 * @returns
 */
export function userLogin (userId: string, password: string, lang: string) {
  // 对密码加密
  password = sha256(password)
  return httpClient.post<IUserLoginInfo>('/user/sign-in', {
    data: {
      userId,
      password,
      lang
    }
  })
}

/**
 * 用户重新登录
 * 获取新的权限等数据
 * @returns
 */
export function userRelogin () {
  return httpClient.put<IUserLoginInfo>('/user/sign-in')
}

/**
 * 更新用户的 AES 密钥
 * 用于加密发件箱的密码
 * @param key
 * @param iv
 * @returns
 */
export function updateUserEncryptKeys (key: string, iv: string) {
  return httpClient.put<boolean>('/user/encrypt-keys', {
    data: {
      key,
      iv
    }
  })
}

// 检查用户ID是否存在
export function checkUserId (userId: string) {
  return httpClient.get<boolean>('/user/check-user-id', {
    params: {
      userId
    }
  })
}

/**
 * 新建用户
 * @param userId
 * @param password
 * @returns
 */
export function createUser (userId: string, password: string) {
  return httpClient.post<Record<string, any>>('/user/sign-up', {
    data: {
      userId,
      password
    }
  })
}

/**
 * 获取过滤后的用户数量
 * @param filter
 * @returns
 */
export function getFilteredUsersCount (filter?: string) {
  return httpClient.get<number>('/user/filtered-count', {
    data: {
      filter
    }
  })
}

/**
 * 获取过滤后的用户数据
 * @param filter
 * @param sortBy
 * @param descending
 * @param skip
 * @param limit
 * @returns
 */
export function getFilteredUsersData (filter: string, pagination: IRequestPagination) {
  return httpClient.post<Array<IUserInfo>>('/user/filtered-data', {
    params: {
      filter
    },
    data: pagination
  })
}

/**
 * 获取所有的用户
 * @returns
 */
export function getAllUsers () {
  return httpClient.get<Array<IUserInfo>>('/user/all')
}

/**
 * 获取默认的用户密码
 * @returns
 */
export function getDefaultPassword () {
  return httpClient.get<string>('/user/default-password')
}

/**
 * 重置用户密码
 * @param userId
 * @returns
 */
export function resetUserPassword (userId: string) {
  return httpClient.put<boolean>('/user/reset-password', {
    data: {
      userId
    }
  })
}

/**
 * 获取用户信息
 * @param userId
 * @returns
 */
export function getUserInfo (userId: string) {
  return httpClient.get<Record<string, any>>('/user/user-info', {
    params: {
      userId
    }
  })
}

/**
 * 修改用户密码
 * @param oldPassword
 * @param newPassword
 * @returns
 */
export function changeUserPassword (oldPassword: string, newPassword: string) {
  const oldSmtpPasswordSecretKeys = getSmtpPasswordSecretKeys(oldPassword)
  const newSmtpPasswordSecretKeys = getSmtpPasswordSecretKeys(newPassword)

  return httpClient.put<boolean>('/user/password', {
    data: {
      oldPassword: sha256(oldPassword),
      newPassword: sha256(newPassword),
      oldSmtpPasswordSecretKeys,
      newSmtpPasswordSecretKeys
    }
  })
}

/**
 * 修改用户头像，返回头像的路径
 * @param blob
 * @returns
 */
export function updateUserAvatar (blob: Blob) {
  const formData = new FormData()
  formData.append('file', blob, 'avatar.png')
  return httpClient.put<string>('/user/avatar', {
    data: formData
  })
}

/**
 * 修改用户类型
 * @param userId
 * @param userType
 */
export function setUserType (userId: number, userType: UserType) {
  return httpClient.put<boolean>(`/user/${userId}/type`, {
    params: {
      userType
    }
  })
}

/**
 * 修改状态
 * @param userId
 * @param status
 * @returns
 */
export function setUserStatus (userId: number, status: UserStatus) {
  return httpClient.put<boolean>(`/user/${userId}/status`, {
    params: {
      status
    }
  })
}
