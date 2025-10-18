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
  body: string, // 邮件正文
  // 附件必须先上传，此处保存的是附件的Id
  attachments: Record<string, any>[], // 附件
  smtpPasswordSecretKeys?: string[], // 发件人邮箱密码密钥, 发件时，需要由用户上传到服务器
  sendBatch: boolean, // 多个收件箱时，是否批量发送
  proxyIds: number[], // 代理人id

  // ip 预热作为参数传递给后端
  sendStartDate?: string, // 计划发送时间，UTC时间字符串
  sendEndDate?: string, // 计划发送结束时间，UTC时间字符串
}

export interface ISendingItemPreview {
  subject: string, // 主题
  body: string, // 邮件正文
  data: Record<string, any>, // 用户发件数据
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
    data: sendingGroup
  })
}

/**
 * 立即发件
 * @param sendingGroup
 * @returns
 */
export function sendSchedule (sendingGroup: IEmailCreateInfo) {
  return httpClient.post<boolean>('/email-sending/schedule', {
    data: sendingGroup
  })
}

/**
 * 重新发送邮件
 * @param sendingItemId
 * @returns
 */
export function resendSendingItem (sendingItemId: number, smtpPasswordSecretKeys: string[]) {
  return httpClient.post<boolean>(`/email-sending/sending-items/${sendingItemId}/resend`, {
    data: {
      smtpPasswordSecretKeys
    }
  })
}

/**
 * 重新发送邮件
 * @param sendingItemId
 * @returns
 */
export function resendSendingGroup (sendingGroupId: number, smtpPasswordSecretKeys: string[]) {
  return httpClient.post<boolean>(`/email-sending/sending-groups/${sendingGroupId}/resend`, {
    data: {
      smtpPasswordSecretKeys
    }
  })
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
export function restartSending (sendingGroupId: number, smtpPasswordSecretKeys: string[]) {
  return httpClient.post<boolean>(`/email-sending/sending-groups/${sendingGroupId}/restart`, {
    data: {
      smtpPasswordSecretKeys
    }
  })
}

/**
 * 取消发件
 * @param sendingGroupId
 * @returns
 */
export function cancelSending (sendingGroupId: number) {
  return httpClient.post<boolean>(`/email-sending/sending-groups/${sendingGroupId}/cancel`)
}
