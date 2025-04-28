import { httpClientPro } from 'src/api//base/httpClient'

/**
* 批量验证当前组中的所有未验证过的收件箱
* @param groupId
* @returns
*/
export function validateAllInvalidInboxes (groupId: number) {
  return httpClientPro.put<boolean>(`/email-verify/groups/${groupId}/verify-invalid-inboxes`)
}
