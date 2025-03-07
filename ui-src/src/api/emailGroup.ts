import { httpClient } from 'src/api//base/httpClient'

export interface IEmailGroup {
  id?: number,
  name: string,
  icon?: string,
  description?: string,
  order: number,
  type?: 1 | 2,
  selectable?: boolean,
  selected?: boolean
}

/**
 * 获取用户的邮箱组
 * @param userId
 * @param type
 * @returns
 */
export function getEmailGroups (type: 1 | 2) {
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
export function deleteAllInvalidBoxesInGroup (groupId: number) {
  return httpClient.delete<boolean>(`/email-group//${groupId}/invalid-outboxes`)
}
