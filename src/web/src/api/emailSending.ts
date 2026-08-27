/* eslint-disable @typescript-eslint/no-explicit-any */
import { httpClient } from 'src/api//base/httpClient'
import type { IEmailTemplate } from './emailTemplate'
import type { IEmailGroupListItem } from 'src/pages/emailManager/components/types'

export interface ISenderAccountSelection {
  id?: number
  email: string
  name?: string
  description?: string
}

export interface IRecipientContactSelection {
  id?: number
  email: string
  name?: string
  description?: string
}

export interface IEmailCreateInfo {
  subjects: string, // 主题
  templates: IEmailTemplate[], // 模板 id
  data: Record<string, any>[], // 用户发件数据
  senderAccountGroups: IEmailGroupListItem[], // 发件人邮箱组
  senderAccounts: ISenderAccountSelection[], // 发件账户
  recipientContactGroups: IEmailGroupListItem[], // 收件人邮箱组
  recipients: IRecipientContactSelection[], // 收件联系人
  ccBoxes: IRecipientContactSelection[], // 抄送联系人
  bccBoxes: IRecipientContactSelection[], // 密送联系人
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
  senderAccountGroupIds: number[],
  senderAccountIds: number[],
  recipientContactGroupIds: number[],
  recipients: IEmailAddressRequest[],
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
  senderAccount: string, // 发件账户地址
  recipientContact: string, // 收件联系人地址
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
  const toEmailAddresses = (recipients: IRecipientContactSelection[]): IEmailAddressRequest[] => recipients.map(recipient => ({
    email: recipient.email,
    name: recipient.name
  }))

  return {
    subjects: sendingGroup.subjects,
    templateIds: toIds(sendingGroup.templates),
    data: sendingGroup.data,
    senderAccountGroupIds: toIds(sendingGroup.senderAccountGroups),
    senderAccountIds: toIds(sendingGroup.senderAccounts),
    recipientContactGroupIds: toIds(sendingGroup.recipientContactGroups),
    recipients: toEmailAddresses(sendingGroup.recipients),
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
