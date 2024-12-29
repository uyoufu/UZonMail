/* eslint-disable @typescript-eslint/no-explicit-any */
import { IRequestPagination } from 'src/compositions/types'
import { httpClient } from './base/httpClient'

export interface ISendingItem {
  id: number
  subject: string
  fromEmail: string,
  inboxes: Record<string, any>[],
  sendDate: string,
  status: number,
  SendResult?: string
}

/// <summary>
/// 邮件状态
/// </summary>
export enum SendingItemStatus {
  /// <summary>
  /// 发送失败
  /// </summary>
  Failed,

  /// <summary>
  /// 无效
  /// </summary>
  Invalid,

  /// <summary>
  /// 已取消
  /// </summary>
  Cancel,

  /// <summary>
  /// 初始状态
  /// </summary>
  Created,

  /// <summary>
  /// 等待发件中
  /// </summary>
  Pending,

  /// <summary>
  /// 发送状态
  /// </summary>
  Sending,

  /// <summary>
  /// 发送成功
  /// </summary>
  Success,

  /// <summary>
  /// 已读
  /// </summary>
  Read,

  /// <summary>
  /// 已取消订阅
  /// </summary>
  Unsubscribed,
}

/**
 * 获取发件项数量
 * @param filter
 * @returns
 */
export function getSendingItemsCount (sendingGroupId: number, filter?: string, itemStatus: number = -1) {
  return httpClient.get<number>('/sending-item/filtered-count', {
    params: {
      sendingGroupId,
      filter,
      itemStatus
    }
  })
}

/**
 * 获取发件项数据
 * @param filter
 * @param pagination
 * @returns
 */
export function getSendingItemsData (sendingGroupId: number, filter: string | undefined, itemStatus: number = -1, pagination: IRequestPagination) {
  return httpClient.post<ISendingItem[]>('/sending-item/filtered-data', {
    params: {
      sendingGroupId,
      filter,
      itemStatus
    },
    data: pagination
  })
}

/**
 * 获取发件项的正文
 * @param sendingItemId
 * @returns
 */
export function getSendingItemBody (sendingItemId: number) {
  return httpClient.get<string>(`/sending-item/${sendingItemId}/body`)
}
