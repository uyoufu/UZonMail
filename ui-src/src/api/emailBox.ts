import { httpClient } from 'src/api//base/httpClient'
import { IRequestPagination } from 'src/compositions/types'

/**
 * 收件箱
 */
export interface IInbox {
  id?: number,
  emailGroupId?: number,
  userId?: number,
  email: string,
  name?: string,
  minInboxCooldownHours: number,
  description?: string,
}

/**
 * 发件箱
 */
export interface IOutbox extends IInbox {
  smtpHost: string,
  smtpPort?: number,
  userName?: string,
  password: string,
  proxyId?: number,
  // 是否显示密码
  showPassword?: boolean,
  // 密码已解密
  decryptedPassword?: boolean
}

/**
 * 创建发件箱
 * @param outbox
 * @param secretKey 用于加密 smtp 的密码
 * @returns
 */
export function createOutbox (outbox: IOutbox) {
  return httpClient.post<IOutbox[]>('/email-box/outbox', {
    data: outbox
  })
}

/**
 * 批量创建发件箱
 * @param outboxes
 * @returns
 */
export function createOutboxes (outboxes: IOutbox[]) {
  return httpClient.post<IOutbox[]>('/email-box/outboxes', {
    data: outboxes
  })
}

/**
 * 更新发件箱
 * @param outbox
 * @returns
 */
export function updateOutbox (outboxId: number, outbox: IOutbox) {
  return httpClient.put<IOutbox[]>(`/email-box/outbox/${outboxId}`, {
    data: outbox
  })
}

/**
 * 获取发件邮箱数量
 * @param groupId
 * @param filter
 */
export function getOutboxesCount (groupId: number | undefined, filter?: string) {
  return httpClient.get<number>('/email-box/outbox/filtered-count', {
    params: {
      groupId,
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
export function getOutboxesData (groupId: number | undefined, filter: string | undefined, pagination: IRequestPagination) {
  return httpClient.post<IOutbox[]>('/email-box/outbox/filtered-data', {
    params: {
      groupId,
      filter
    },
    data: pagination
  })
}

/**
 * 通过 id 删除邮箱
 * @param emailBoxId
 * @returns
 */
export function deleteOutboxById (emailBoxId: number) {
  return httpClient.delete<boolean>(`/email-box/outboxes/${emailBoxId}`)
}

/**
 * 获取收件邮箱数量
 * @param groupId
 * @param filter
 */
export function getInboxesCount (groupId: number | undefined, filter?: string) {
  return httpClient.get<number>('/email-box/inbox/filtered-count', {
    params: {
      groupId,
      filter
    }
  })
}

/**
 * 获取收件邮箱数据
 * @param groupId
 * @param filter
 * @param pagination
 * @returns
 */
export function getInboxesData (groupId: number | undefined, filter: string | undefined, pagination: IRequestPagination) {
  return httpClient.post<IInbox[]>('/email-box/inbox/filtered-data', {
    params: {
      groupId,
      filter
    },
    data: pagination
  })
}

/**
 * 通过 id 删除邮箱
 * @param emailBoxId
 * @returns
 */
export function deleteInboxById (emailBoxId: number) {
  return httpClient.delete<boolean>(`/email-box/inboxes/${emailBoxId}`)
}

/**
 * 批量创建收件箱
 * @param outbox
 * @returns
 */
export function createInbox (outbox: IInbox) {
  return httpClient.post<IInbox>('/email-box/inbox', {
    data: outbox
  })
}

/**
 * 添加未分组的发件箱
 * @param outbox
 * @returns
 */
export function createUngroupedInbox (outbox: IInbox) {
  return httpClient.post<IInbox>('/email-box/inbox/ungrouped', {
    data: outbox
  })
}

/**
 * 批量创建收件箱
 * @param outboxes
 * @returns
 */
export function createInboxes (outboxes: IInbox[]) {
  return httpClient.post<IInbox[]>('/email-box/inboxes', {
    data: outboxes
  })
}

/**
 * 更新收件箱
 * @param inboxId
 * @param inbox
 * @returns
 */
export function updateInbox (inboxId: number, inbox: IInbox) {
  return httpClient.put<IInbox[]>(`/email-box/inbox/${inboxId}`, {
    data: inbox
  })
}
