
import { httpClientPro } from 'src/api/base/httpClient'
import type { IRequestPagination } from 'src/compositions/types'

export enum UnsubscibeType {
  /// <summary>
  /// 系统
  /// </summary>
  System,

  /// <summary>
  /// 外部链接
  /// </summary>
  External
}

export interface IUnsubscribeSettings {
  id: number,

  /// <summary>
  /// 是否启用退订
  /// </summary>
  enable: boolean,

  /// <summary>
  /// 类型
  /// </summary>
  type: UnsubscibeType,

  /// <summary>
  /// 外部 URL
  /// </summary>
  externalUrl?: string,

  /// <summary>
  /// 退订按钮的 Id
  /// </summary>
  unsubscribeButtonId?: string,
}

export interface IUnsubscribeEmail {
  organizationId: number,
  email: string,
  host?: string,
}

/**
 * 退订
 * @param token
 * @returns
 */
export function unsubscribe (token: string) {
  return httpClientPro.post<boolean>('/unsubscribe', {
    params: {
      token
    }
  })
}

/**
 * 是否取消退订
 * @param token
 * @returns
 */
export function isUnsubscribed (token: string) {
  return httpClientPro.get<boolean>('/unsubscribe/unsubscribed', {
    params: {
      token
    }
  })
}

/**
 * 获取退定设置
 * @returns
 */
export function getUnsubscribeSettings () {
  return httpClientPro.get<IUnsubscribeSettings>('/unsubscribe')
}

/**
 * 更新退订设置
 * @param settingId
 * @param data
 * @returns
 */
export function updateUnsubscribeSettings (settingId: number, data: IUnsubscribeSettings) {
  return httpClientPro.put<boolean>(`/unsubscribe/${settingId}`, { data })
}

/**
 * 获取发件邮箱数量
 * @param groupId
 * @param filter
 */
export function getUnsubscribesCount (filter: string | undefined) {
  return httpClientPro.get<number>('/unsubscribe/filtered-count', {
    params: {
      filter
    }
  })
}

/**
 * 获取发件邮箱数据
 * @param groupId
 * @param filter
 * @param pagination
 * @returns
 */
export function getUnsubscribesData (filter: string | undefined, pagination: IRequestPagination) {
  return httpClientPro.post<IUnsubscribeEmail[]>('/unsubscribe/filtered-data', {
    params: {
      filter
    },
    data: pagination
  })
}
