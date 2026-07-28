import { httpClient } from 'src/api//base/httpClient'

/** 邮箱分组类型 */
export const EmailGroupType = {
  Outbox: 1,
  Inbox: 2
} as const

export type EmailGroupType = typeof EmailGroupType[keyof typeof EmailGroupType]

export interface IEmailGroup {
  id?: number,
  objectId?: string,
  name: string,
  icon?: string,
  description?: string,
  order: number,
  type?: EmailGroupType,
  selectable?: boolean,
  selected?: boolean
}

/**
 * 获取用户的邮箱组
 * @param userId
 * @param type
 * @returns
 */
export function getEmailGroups (type: EmailGroupType) {
  return httpClient.get<IEmailGroup[]>('/email-group/all', {
    params: {
      type
    }
  })
}

/**
 * 创建邮箱组
 * @param userId
 * @param type
 * @returns
 */
export function createEmailCroup (groupData: IEmailGroup) {
  return httpClient.post<IEmailGroup>('/email-group', {
    data: groupData
  })
}

/**
 * 修改邮箱组
 * @param groupData
 * @returns
 */
export function updateEmailCroup (groupData: IEmailGroup) {
  return httpClient.put<boolean[]>(`/email-group/${groupData.id}`, {
    data: groupData
  })
}

/**
 * 删除邮箱组
 * @param groupId
 * @returns
 */
export function deleteEmailGroupById (groupId: number) {
  return httpClient.delete<boolean>(`/email-group/${groupId}`)
}

/**
 * 删除所有无效的发件箱
 * @returns
 */
export function deleteAllInvalidOutboxesInGroup (groupId: number) {
  return httpClient.delete<boolean>(`/email-group/${groupId}/invalid-outboxes`)
}

/**
 * 验证组中未验证通过的邮箱
 * 发件验证结果 websocket 进行回调
 * @param groupId
 * @returns
 */
export function validateAllInvalidOutboxes (groupId: number) {
  return httpClient.put<boolean>(`/email-group/${groupId}/invalid-outbox/validate`)
}
