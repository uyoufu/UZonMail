import { httpClientPro } from 'src/api//base/httpClient'

export interface IRecipientContactVerificationBatchSummary {
  totalCount: number
  validCount: number
  invalidCount: number
  unknownCount: number
}

/** 验证当前分类中的全部收件联系人。 */
export function validateRecipientContactGroup (groupId: number) {
  return httpClientPro.put<IRecipientContactVerificationBatchSummary>(`/email-verify/groups/${groupId}/verify`)
}

/** 验证指定收件联系人。 */
export function validateRecipientContacts (recipientContactIds: number[]) {
  return httpClientPro.put<IRecipientContactVerificationBatchSummary>('/email-verify/recipient-contacts/verify', {
    data: { recipientContactIds }
  })
}
