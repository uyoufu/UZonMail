/* eslint-disable @typescript-eslint/no-explicit-any */
import { httpClient } from 'src/api//base/httpClient'
import type { IEmailTemplate } from './emailTemplate'
import type { IInbox, IOutbox } from './emailBox'
import type { IEmailGroupListItem } from 'src/pages/emailManager/components/types'

export interface IEmailCreateInfo {
  subjects: string, // 主题
  templates: IEmailTemplate[], // 模板 id
  data: Record<string, any>[], // 用户发件数据
  outboxGroups: IEmailGroupListItem[], // 发件人邮箱组
  outboxes: IOutbox[], // 发件人邮箱
  inboxGroups: IEmailGroupListItem[], // 收件人邮箱组
  inboxes: IInbox[], // 收件人邮箱
  ccBoxes: IInbox[], // 抄送人邮箱
  bccBoxes: IInbox[], // 密送人邮箱
  body: string, // 邮件正文
  // 附件必须先上传，此处保存的是附件的Id
  attachments: Record<string, any>[], // 附件
  sendBatch: boolean, // 多个收件箱时，是否批量发送
  proxyIds: number[], // 代理人id

  // ip 预热作为参数传递给后端
  sendStartDate?: string, // 计划发送时间，UTC时间字符串
  sendEndDate?: string, // 计划发送结束时间，UTC时间字符串
}

interface IEmailAddressRequest {
  email: string,
  name?: string
}

interface ISendEmailRequest {
  subjects: string,
  templateIds: number[],
  data: Record<string, any>[],
  outboxGroupIds: number[],
  outboxIds: number[],
  inboxGroupIds: number[],
  inboxes: IEmailAddressRequest[],
  ccBoxes: IEmailAddressRequest[],
  bccBoxes: IEmailAddressRequest[],
  body: string,
  attachmentIds: number[],
  sendBatch: boolean,
  proxyIds: number[]
}

interface IScheduleEmailRequest extends ISendEmailRequest {
  scheduleDate: string
}

export interface ISendingItemPreview {
  subject: string, // 主题
  body: string, // 邮件正文
  data: Record<string, any>, // 用户发件数据
  outbox: string, // 发件人邮箱
  inbox: string, // 收件人邮箱
  ccBoxes: string[], // 抄送人邮箱
  bccBoxes: string[], // 密送人邮箱
}

/**
 * 预览发件项
 * @param data
 * @returns
 */
export function previewSendingItem (data: ISendingItemPreview) {
  return httpClient.post<ISendingItemPreview>('/email-sending/preview', {
    data
  })
}

/**
 * 立即发件
 * @param userId
 * @param type
 * @returns
 */
export function sendEmailNow (sendingGroup: IEmailCreateInfo) {
  return httpClient.post<boolean>('/email-sending/now', {
    data: toSendEmailRequest(sendingGroup)
  })
}

/**
 * 定时发件
 * @param sendingGroup
 * @param scheduleDate UTC 时间字符串
 * @returns
 */
export function sendSchedule (sendingGroup: IEmailCreateInfo, scheduleDate: string) {
  const request: IScheduleEmailRequest = {
    ...toSendEmailRequest(sendingGroup),
    scheduleDate
  }
  return httpClient.post<boolean>('/email-sending/schedule', {
    data: request
  })
}

/** 将页面发件模型收敛为后端允许的请求字段。 */
function toSendEmailRequest (sendingGroup: IEmailCreateInfo): ISendEmailRequest {
  const toIds = (records: { id?: number }[]) => records
    .map(record => record.id)
    .filter((id): id is number => typeof id === 'number' && id > 0)
  const toEmailAddresses = (inboxes: IInbox[]): IEmailAddressRequest[] => inboxes.map(inbox => ({
    email: inbox.email,
    name: inbox.name
  }))

  return {
    subjects: sendingGroup.subjects,
    templateIds: toIds(sendingGroup.templates),
    data: sendingGroup.data,
    outboxGroupIds: toIds(sendingGroup.outboxGroups),
    outboxIds: toIds(sendingGroup.outboxes),
    inboxGroupIds: toIds(sendingGroup.inboxGroups),
    inboxes: toEmailAddresses(sendingGroup.inboxes),
    ccBoxes: toEmailAddresses(sendingGroup.ccBoxes),
    bccBoxes: toEmailAddresses(sendingGroup.bccBoxes),
    body: sendingGroup.body,
    attachmentIds: sendingGroup.attachments
      .map(attachment => Number(attachment.__fileUsageId))
      .filter(attachmentId => Number.isSafeInteger(attachmentId) && attachmentId > 0),
    sendBatch: sendingGroup.sendBatch,
    proxyIds: sendingGroup.proxyIds
  }
}

/**
 * 重新发送邮件
 * @param sendingItemId
 * @returns
 */
export function resendSendingItem (sendingItemId: number) {
  return httpClient.post<boolean>(`/email-sending/sending-items/${sendingItemId}/resend`)
}

/**
 * 重新发送邮件
 * @param sendingItemId
 * @returns
 */
export function resendSendingGroup (sendingGroupId: number) {
  return httpClient.post<boolean>(`/email-sending/sending-groups/${sendingGroupId}/resend`)
}

/**
 * 暂停发件
 * @param sendingGroupId
 * @returns
 */
export function pauseSending (sendingGroupId: number) {
  return httpClient.post<boolean>(`/email-sending/sending-groups/${sendingGroupId}/pause`)
}

/**
 * 重新开始发件
 * @param sendingGroupId
 * @returns
 */
export function restartSending (sendingGroupId: number) {
  return httpClient.post<boolean>(`/email-sending/sending-groups/${sendingGroupId}/restart`)
}

/**
 * 取消发件
 * @param sendingGroupId
 * @returns
 */
export function cancelSending (sendingGroupId: number) {
  return httpClient.post<boolean>(`/email-sending/sending-groups/${sendingGroupId}/cancel`)
}
