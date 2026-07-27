import { httpClientPro } from 'src/api//base/httpClient'

export interface IInboxVerificationBatchSummary {
  totalCount: number
  validCount: number
  invalidCount: number
  unknownCount: number
}

/** 验证当前分类中的全部收件箱。 */
export function validateInboxGroup (groupId: number) {
  return httpClientPro.put<IInboxVerificationBatchSummary>(`/email-verify/groups/${groupId}/verify`)
}

/** 验证指定收件箱。 */
export function validateInboxes (inboxIds: number[]) {
  return httpClientPro.put<IInboxVerificationBatchSummary>('/email-verify/inboxes/verify', { data: { inboxIds } })
}
