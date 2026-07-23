import { httpClientPro } from 'src/api//base/httpClient'
import type { IRequestPagination } from 'src/compositions/types'

// #region 变量数据源

export interface IApiApiAccess {
  id: number,
  objectId: string

  /// <summary>
  /// 名称
  /// </summary>
  name: string

  /// <summary>
  /// 描述
  /// </summary>
  description: string

  /// <summary>
  /// JWT ID
  /// </summary>
  jwtId: string

  /// <summary>
  /// 过期时间
  /// </summary>
  expireDate: string

  /// <summary>
  /// 是否启用
  /// 可以通过设置这个值来控制令牌的有效性
  /// </summary>
  enable: boolean,

  // 最终结果值
  token?: string
}

export function upsertApiAccess (data: IApiApiAccess) {
  return httpClientPro.post<IApiApiAccess>('/api-access', {
    data
  })
}

/**
 * 获取变量数据源数量
 * @param groupId
 * @param filter
 */
export function getApiAccessCount (filter: string | undefined) {
  return httpClientPro.get<number>('/api-access/filtered-count', {
    params: {
      filter
    }
  })
}

/**
 * 获取变量数据源数据
 * @param groupId
 * @param filter
 * @param pagination
 * @returns
 */
export function getApiAccessData (filter: string | undefined, pagination: IRequestPagination) {
  return httpClientPro.post<IApiApiAccess[]>('/api-access/filtered-data', {
    params: {
      filter
    },
    data: pagination
  })
}

/**
 * 删除变量的数据源
 * @param filter
 * @param pagination
 * @returns
 */
export function deleteApiAccessData (sourceId: string) {
  return httpClientPro.delete<boolean[]>(`/api-access/${sourceId}`)
}
// #endregion
