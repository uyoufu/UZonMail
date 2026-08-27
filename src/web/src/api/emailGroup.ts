import { httpClient } from 'src/api/base/httpClient'

/** 邮箱分组类型。账户身份和收件邮箱使用不同分组。 */
export const EmailGroupCategory = {
  EmailAccount: 1,
  RecipientEmail: 2
} as const

export type EmailGroupCategory = typeof EmailGroupCategory[keyof typeof EmailGroupCategory]

export interface IEmailGroup {
  id?: number
  name: string
  icon?: string
  description?: string
  order: number
  category?: EmailGroupCategory
  isDefault?: boolean
  accountCount?: number
  selectable?: boolean
  selected?: boolean
}

export interface ICreateEmailGroup {
  name: string
  icon?: string
  description?: string
  category: EmailGroupCategory
}

export interface IUpdateEmailGroup {
  name: string
  description?: string
}

export function getEmailGroups (category: EmailGroupCategory) {
  return httpClient.get<IEmailGroup[]>('/email-group/all', { params: { category } })
}

export function createEmailGroup (groupData: ICreateEmailGroup) {
  return httpClient.post<IEmailGroup>('/email-group', { data: groupData })
}

export function updateEmailGroup (groupId: number, groupData: IUpdateEmailGroup) {
  return httpClient.put<IEmailGroup>(`/email-group/${groupId}`, { data: groupData })
}

export function reorderEmailGroups (category: EmailGroupCategory, emailGroupIds: number[]) {
  return httpClient.put<boolean>('/email-group/reorder', {
    data: { category, emailGroupIds }
  })
}

export function deleteEmailGroupById (groupId: number) {
  return httpClient.delete<boolean>(`/email-group/${groupId}`)
}
