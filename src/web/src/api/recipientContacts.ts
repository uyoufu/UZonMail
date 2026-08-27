import { httpClient } from 'src/api/base/httpClient'
import type { RecipientValidationStatus } from './accountEnums'

export interface IRecipientContact {
  id: number
  emailGroupId: number
  email: string
  name?: string
  description?: string
  remark?: string
  minimumCooldownHours: number
  validationStatus: RecipientValidationStatus
  validationFailureReason?: string
  lastDeliveredAtUtc?: string
  lastSuccessDeliveryDate?: string
}

export interface IRecipientContactWrite {
  emailGroupId: number
  email: string
  name?: string
  description?: string
  remark?: string
  minimumCooldownHours: number
}

export function getRecipientContacts(emailGroupId?: number, filter?: string) {
  return httpClient.get<IRecipientContact[]>('/recipient-contacts', { params: { emailGroupId, filter } })
}

export function createRecipientContact(request: IRecipientContactWrite) {
  return httpClient.post<IRecipientContact>('/recipient-contacts', { data: request })
}

export function createRecipientContacts(requests: IRecipientContactWrite[]) {
  return httpClient.post<IRecipientContact[]>('/recipient-contacts/batch', { data: requests })
}

export function updateRecipientContact(recipientContactId: number, request: IRecipientContactWrite) {
  return httpClient.put<IRecipientContact>(`/recipient-contacts/${recipientContactId}`, { data: request })
}

export function updateRecipientValidationStatus(
  recipientContactIds: number[],
  validationStatus: RecipientValidationStatus
) {
  return httpClient.put<boolean>('/recipient-contacts/validation-status', {
    data: { recipientContactIds, validationStatus }
  })
}

export function deleteRecipientContact(recipientContactId: number) {
  return httpClient.delete<boolean>(`/recipient-contacts/${recipientContactId}`)
}
