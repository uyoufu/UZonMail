/* eslint-disable @typescript-eslint/no-explicit-any */

import { httpClient } from 'src/api//base/httpClient'
import type { IRequestPagination } from 'src/compositions/types'

/**
 * 发送组状态
 */
export const SendingGroupStatus = {
  Created: 0,
  Scheduled: 1,
  Sending: 2,
  Pause: 3,
  Cancel: 4,
  Finish: 5,
  WaitingForQuotaReset: 6
} as const

export type SendingGroupStatus = typeof SendingGroupStatus[keyof typeof SendingGroupStatus]

/** 发送组状态对应的稳定显示键。 */
export const sendingGroupStatusNames: Readonly<Record<SendingGroupStatus, string>> = {
  [SendingGroupStatus.Created]: 'Created',
  [SendingGroupStatus.Scheduled]: 'Scheduled',
  [SendingGroupStatus.Sending]: 'Sending',
  [SendingGroupStatus.Pause]: 'Pause',
  [SendingGroupStatus.Cancel]: 'Cancel',
  [SendingGroupStatus.Finish]: 'Finish',
  [SendingGroupStatus.WaitingForQuotaReset]: 'WaitingForQuotaReset'
}

/**
 * 发送组类型
 */
export enum SendingGroupType {
  /// <summary>
  /// 即时发送
  /// </summary>
  Instant,

  /// <summary>
  /// 计划发送
  /// </summary>
  Scheduled,
}

export interface ISendingGroupInfo {
  id: number, // id
  userId: number, // 用户 id
  subjects: string, // 主题
  attachments: object[], // 附件
  totalCount: number, // 总数
  sentCount: number, // 已发送数
  successCount: number, // 成功数
  progress?: number, // 由实时进度事件更新
  status: SendingGroupStatus, // 状态
  statusReason?: string,
  resumeAtUtc?: string,
  sendStartDate: string, // 发送开始时间
  sendingType?: SendingGroupType, // 发送类型
  scheduleDate?: string, // 计划发送时间
  createDate?: string,
}

/**
 * 发送组历史
 */
export interface ISendingGroupHistory extends ISendingGroupInfo {
  objectId: string,
  templatesCount: number, // 模板数量
  senderAccountCount: number, // 发件账户数量
  recipientCount: number, // 收件联系人数量
  ccBoxesCount: number, // 抄送人邮箱数量
  bccBoxesCount: number, // 密送人邮箱数量
}

/**
 * 获取模板数量
 * @param filter
 * @returns
 */
export function getSendingGroupsCount (filter?: string) {
  return httpClient.get<number>('/sending-group/filtered-count', {
    params: {
      filter
    }
  })
}

/**
 * 获取模板数据
 * @param filter
 * @param pagination
 * @returns
 */
export function getEmailTemplatesData (filter: string | undefined, pagination: IRequestPagination) {
  return httpClient.post<ISendingGroupHistory[]>('/sending-group/filtered-data', {
    params: {
      filter
    },
    data: pagination
  })
}

export interface IRunningSendingGroup {
  id: number
  subjects: string
  progress: number,
  totalCount: number,
  sentCount: number,
  successCount?: number,
  status?: number
}

/**
 * 获取正在执行的发送组
 * @returns
 */
export function getRunningSendingGroups () {
  return httpClient.get<IRunningSendingGroup[]>('/sending-group/running')
}

/**
 * 获取发送组中的 subjects 值
 * @param sendingGroupId
 * @returns
 */
export function getSendingGroupSubjects (sendingGroupId: number) {
  return httpClient.get<string>(`/sending-group/${sendingGroupId}/subjects`)
}

export interface ISendingGroupStatusInfo {
  id: number
  progress: number,
  totalCount: number,
  sentCount: SendingGroupStatus,
  successCount?: number,
  status?: number
}

/**
 * 获取发送组中的 subjects 值
 * @param sendingGroupId
 * @returns
 */
export function getSendingGroupRunningInfo (sendingGroupId: number) {
  return httpClient.get<ISendingGroupStatusInfo>(`/sending-group/${sendingGroupId}/status-info`)
}

export interface ISendingGroupFull extends ISendingGroupInfo {
  objectId: string,
  templates: Array<{ id: number, name: string }>,
  senderAccounts: Array<{ id: number, email: string, name?: string }>,
  senderAccountGroups: Array<{ id: number, name: string }>,
  recipientContactGroups: Array<{ id: number, name: string }>,
  recipients: Array<{ email: string, name?: string }>,
  ccBoxes: Array<{ email: string, name?: string }>,
  bccBoxes: Array<{ email: string, name?: string }>,
  attachments: Array<{ id: number, fileName: string, sha256: string, size: number }>,
  data: Record<string, any>[],
  body: string,
  sendBatch: boolean,
  proxyIds: number[]
}

/**
 * 获取发件组信息
 * @param sendingGroupObjId
 * @returns
 */
export function getSendingGroup (sendingGroupObjId: string) {
  return httpClient.get<ISendingGroupFull>(`/sending-group/${sendingGroupObjId}`)
}

/**
 * 通过 id 批量删除发件组
 * @param sendingGroupIds
 * @returns
 */
export function deleteSendingGroups (sendingGroupIds: number[]) {
  return httpClient.delete('/sending-group/ids/many', {
    data: sendingGroupIds
  })
}
