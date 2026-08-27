import { httpClient } from 'src/api/base/httpClient'
import type {
  AuthenticationMethod,
  ConnectionSecurity,
  OAuthApplicationSource,
  SenderAccountStatus,
  SendingProtocol
} from './accountEnums'

export interface ISenderAccount {
  id: number
  emailAccountId: number
  emailGroupId: number
  email: string
  name?: string
  description?: string
  remark?: string
  protocol: SendingProtocol
  authenticationMethod: AuthenticationMethod
  status: SenderAccountStatus
  validationFailureReason?: string
  proxyId?: number
  maxSendCountPerDay: number
  sentTotalToday: number
  replyToEmails?: string
  weight: number
  hasSmtpCredential: boolean
  oAuthApplicationSource?: OAuthApplicationSource
  hasOAuthAuthorization: boolean
}

export interface ISmtpCredentialWrite {
  host: string
  port: number
  connectionSecurity: ConnectionSecurity
  loginName: string
  password?: string
}

export interface IMicrosoftGraphApplicationWrite {
  applicationSource: OAuthApplicationSource
  tenantId?: string
  clientId?: string
  clientSecret?: string
}

export interface ICreateSmtpSenderAccount {
  email: string
  name?: string
  description?: string
  remark?: string
  emailGroupId: number
  proxyId?: number
  maxSendCountPerDay: number
  replyToEmails?: string
  weight: number
  credential: ISmtpCredentialWrite
}

export interface ICreateMicrosoftGraphSenderAccount {
  email: string
  name?: string
  description?: string
  remark?: string
  emailGroupId: number
  maxSendCountPerDay: number
  replyToEmails?: string
  weight: number
  application: IMicrosoftGraphApplicationWrite
}

export type IUpdateSenderAccount = Omit<ICreateSmtpSenderAccount, 'email' | 'credential'>

export function getSenderAccounts(emailGroupId?: number, filter?: string) {
  return httpClient.get<ISenderAccount[]>('/sender-accounts', { params: { emailGroupId, filter } })
}

export function getSenderAccount(senderAccountId: number) {
  return httpClient.get<ISenderAccount>(`/sender-accounts/${senderAccountId}`)
}

export function createSmtpSenderAccount(request: ICreateSmtpSenderAccount) {
  return httpClient.post<ISenderAccount>('/sender-accounts/smtp', { data: request })
}

export function createMicrosoftGraphSenderAccount(request: ICreateMicrosoftGraphSenderAccount) {
  return httpClient.post<ISenderAccount>('/sender-accounts/microsoft-graph', { data: request })
}

export function updateSenderAccount(senderAccountId: number, request: IUpdateSenderAccount) {
  return httpClient.put<ISenderAccount>(`/sender-accounts/${senderAccountId}`, { data: request })
}

export function updateSmtpCredential(senderAccountId: number, credential: ISmtpCredentialWrite) {
  return httpClient.put<ISenderAccount>(`/sender-accounts/${senderAccountId}/smtp-credential`, { data: credential })
}

export function updateMicrosoftGraphApplication(
  senderAccountId: number,
  application: IMicrosoftGraphApplicationWrite
) {
  return httpClient.put<ISenderAccount>(`/sender-accounts/${senderAccountId}/microsoft-graph-application`, {
    data: application
  })
}

export function validateSenderAccount(senderAccountId: number) {
  return httpClient.post<boolean>(`/sender-accounts/${senderAccountId}/validate`)
}

export function deleteSenderAccount(senderAccountId: number) {
  return httpClient.delete<boolean>(`/sender-accounts/${senderAccountId}`)
}

export function startSenderMicrosoftAuthorization(senderAccountId: number) {
  return httpClient.post<string>(`/outlook-authorization/${senderAccountId}`)
}
