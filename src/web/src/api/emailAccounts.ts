import { httpClient } from 'src/api/base/httpClient'
import type {
  ConnectionSecurity,
  OAuthApplicationSource,
  ReceivingAccountStatus,
  ReceivingProtocol,
  SenderAccountStatus,
  SendingProtocol
} from './accountEnums'

export const EmailAccountConfigurationKind = {
  Basic: 1,
  MicrosoftGraph: 2
} as const

export type EmailAccountConfigurationKind =
  (typeof EmailAccountConfigurationKind)[keyof typeof EmailAccountConfigurationKind]

/** 可安全回显到账户编辑表单的协议连接参数。 */
export interface IEmailAccountProtocolCredential {
  host: string
  port: number
  connectionSecurity: ConnectionSecurity
  loginName: string
}

export interface IEmailAccountSenderCapability {
  id: number
  protocol: SendingProtocol
  status: SenderAccountStatus
  validationFailureReason?: string
  proxyId?: number
  maxSendCountPerDay: number
  sentTotalToday: number
  replyToEmails?: string
  weight: number
  hasCredential: boolean
  smtpCredential?: IEmailAccountProtocolCredential
}

export interface IEmailAccountReceivingCapability {
  id: number
  protocol: ReceivingProtocol
  status: ReceivingAccountStatus
  contentRetentionDays: number
  lastConnectedAtUtc?: string
  lastError?: string
  hasCredential: boolean
  imapCredential?: IEmailAccountProtocolCredential
}

export interface IEmailAccount {
  id: number
  emailGroupId: number
  email: string
  name?: string
  description?: string
  remark?: string
  sender?: IEmailAccountSenderCapability
  receiving?: IEmailAccountReceivingCapability
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

export interface IImapCredentialWrite {
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

export interface IEmailAccountSenderCapabilityWrite {
  isEnabled: boolean
  proxyId?: number
  maxSendCountPerDay: number
  replyToEmails?: string
  weight: number
  smtpCredential?: ISmtpCredentialWrite
}

export interface IEmailAccountReceivingCapabilityWrite {
  isEnabled: boolean
  contentRetentionDays: number
  imapCredential?: IImapCredentialWrite
}

export interface IEmailAccountWrite {
  email?: string
  emailGroupId: number
  name?: string
  description?: string
  remark?: string
  configurationKind: EmailAccountConfigurationKind
  sender: IEmailAccountSenderCapabilityWrite
  receiving: IEmailAccountReceivingCapabilityWrite
  microsoftGraphApplication?: IMicrosoftGraphApplicationWrite
}

export interface ISenderEmailAccountOption {
  id: number
  email: string
  name?: string
  description?: string
}

export function getEmailAccounts(emailGroupId?: number, filter?: string) {
  return httpClient.get<IEmailAccount[]>('/email-accounts', { params: { emailGroupId, filter } })
}

export function getSenderEmailAccountOptions() {
  return httpClient.get<ISenderEmailAccountOption[]>('/email-accounts/senders')
}

export function createBasicEmailAccount(request: IEmailAccountWrite) {
  return httpClient.post<IEmailAccount>('/email-accounts/basic', { data: request })
}

export function createMicrosoftGraphEmailAccount(request: IEmailAccountWrite) {
  return httpClient.post<IEmailAccount>('/email-accounts/microsoft-graph', { data: request })
}

export function updateEmailAccount(emailAccountId: number, request: IEmailAccountWrite) {
  return httpClient.put<IEmailAccount>(`/email-accounts/${emailAccountId}`, { data: request })
}

export function moveEmailAccounts(emailAccountIds: number[], targetEmailGroupId: number) {
  return httpClient.put<boolean>('/email-accounts/move', {
    data: { emailAccountIds, targetEmailGroupId }
  })
}

export function validateEmailAccount(emailAccountId: number) {
  return httpClient.post<IEmailAccount>(`/email-accounts/${emailAccountId}/validate`)
}

export function validateEmailAccounts(emailAccountIds: number[]) {
  return httpClient.post<IEmailAccount[]>('/email-accounts/validate', { data: { emailAccountIds } })
}

export function deleteEmailAccount(emailAccountId: number) {
  return httpClient.delete<boolean>(`/email-accounts/${emailAccountId}`)
}

export function deleteInvalidSenderCapabilities(emailGroupId: number) {
  return httpClient.delete<boolean>(`/email-accounts/groups/${emailGroupId}/invalid-sender-capabilities`)
}

export function startMicrosoftAuthorization(emailAccountId: number) {
  return httpClient.post<string>(`/outlook-authorization/email-accounts/${emailAccountId}`)
}
