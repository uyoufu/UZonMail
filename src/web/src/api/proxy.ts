import { httpClient } from 'src/api//base/httpClient'
import type { IRequestPagination } from 'src/compositions/types'

export interface IProxy {
  id?: number
  name: string
  priority?: number
  emailMatch?: string
  description?: string
  isActive: boolean
  url: string,
  isShared?: boolean,
  userId: number,
  organizationId: number
}

/**
 * 验证代理名称是否满足要求
 * @param name
 * @returns
 */
export function validateProxyName (name: string) {
  return httpClient.get<IProxy>('/proxy/valid-name', {
    params: {
      name
    }
  })
}

/**
 * 创建代理
 * @param proxyInfo
 * @returns
 */
export function createProxy (proxyInfo: IProxy) {
  return httpClient.post<IProxy>('/proxy', {
    data: proxyInfo
  })
}

/**
 * 更新代理
 * @param proxyInfo
 * @returns
 */
export function updateProxy (proxyInfo: IProxy) {
  return httpClient.put<boolean>('/proxy', {
    data: proxyInfo
  })
}

/**
 * 更新 isShared 状态
 * @param proxyId
 * @param isShared
 * @returns
 */
export function updateProxySharedStatus (proxyId: number, isShared: boolean) {
  return httpClient.put<boolean>(`/proxy/${proxyId}/shared`, {
    params: {
      isShared
    }
  })
}

/**
 * 删除代理
 * @param proxyId
 * @returns
 */
export function deleteProxy (proxyId: number) {
  return httpClient.delete<boolean>(`/proxy/${proxyId}`)
}

/**
 * 获取代理数量
 * @param filter
 * @returns
 */
export function getProxiesCount (filter?: string) {
  return httpClient.get<number>('/proxy/filtered-count', {
    params: {
      filter
    }
  })
}

/**
 * 获取代理数据
 * @param filter
 * @param pagination
 * @returns
 */
export function getProxiesData (filter: string | undefined, pagination: IRequestPagination) {
  return httpClient.post<IProxy[]>('/proxy/filtered-data', {
    params: {
      filter
    },
    data: {
      pagination
    }
  })
}

/**
 * 获取代理数量
 * @param filter
 * @returns
 */
export function getEnabledProxiesCount (filter?: string) {
  return httpClient.get<number>('/proxy/enabled/filtered-count', {
    params: {
      filter
    }
  })
}

/**
 * 获取代理数据
 * @param filter
 * @param pagination
 * @returns
 */
export function getEnabledProxiesData (filter: string | undefined, pagination: IRequestPagination) {
  return httpClient.post<IProxy[]>('/proxy/enabled/filtered-data', {
    params: {
      filter
    },
    data: {
      pagination
    }
  })
}

/**
 * 获取当前用户可用的代理
 * @returns
 */
export function getUsableProxies () {
  return httpClient.get<IProxy[]>('/proxy/usable')
}
