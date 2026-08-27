import { httpClient } from 'src/api/base/httpClient'
import type {
  AuthenticationMethod,
  ConnectionSecurity,
  OAuthApplicationSource,
  ReceivingAccountStatus,
  ReceivingProtocol
} from './accountEnums'
import type { IMicrosoftGraphApplicationWrite } from './senderAccounts'

export interface IImapCredentialWrite {
  host: string
  port: number
  connectionSecurity: ConnectionSecurity
  loginName: string
  password?: string
}

export interface IReceivingAccount {
  id: number
  emailAccountId: number
  email: string
  name?: string
  protocol: ReceivingProtocol
  authenticationMethod: AuthenticationMethod
  status: ReceivingAccountStatus
  contentRetentionDays: number
  lastConnectedAtUtc?: string
  lastError?: string
  hasImapCredential: boolean
  oAuthApplicationSource?: OAuthApplicationSource
  hasOAuthAuthorization: boolean
  senderAccountIds: number[]
  primarySenderAccountId?: number
}

export interface IReceivingAccountBaseWrite {
  email: string
  name?: string
  description?: string
  remark?: string
  contentRetentionDays: number
  senderAccountIds: number[]
  primarySenderAccountId?: number
}

export interface ICreateBasicReceivingAccount extends IReceivingAccountBaseWrite {
  credential: IImapCredentialWrite
}

export interface ICreateMicrosoftGraphReceivingAccount extends IReceivingAccountBaseWrite {
  application: IMicrosoftGraphApplicationWrite
}

export type IUpdateReceivingAccount = Omit<IReceivingAccountBaseWrite, 'email'>

export function getReceivingAccounts() {
  return httpClient.get<IReceivingAccount[]>('/receiving-accounts')
}

export function createBasicReceivingAccount(request: ICreateBasicReceivingAccount) {
  return httpClient.post<IReceivingAccount>('/receiving-accounts/basic', { data: request })
}

export function createMicrosoftGraphReceivingAccount(request: ICreateMicrosoftGraphReceivingAccount) {
  return httpClient.post<IReceivingAccount>('/receiving-accounts/microsoft-graph', { data: request })
}

export function updateReceivingAccount(receivingAccountId: number, request: IUpdateReceivingAccount) {
  return httpClient.put<IReceivingAccount>(`/receiving-accounts/${receivingAccountId}`, { data: request })
}

export function updateImapCredential(receivingAccountId: number, credential: IImapCredentialWrite) {
  return httpClient.put<IReceivingAccount>(`/receiving-accounts/${receivingAccountId}/imap-credential`, {
    data: credential
  })
}

export function updateReceivingMicrosoftGraphApplication(
  receivingAccountId: number,
  application: IMicrosoftGraphApplicationWrite
) {
  return httpClient.put<IReceivingAccount>(
    `/receiving-accounts/${receivingAccountId}/microsoft-graph-application`,
    { data: application }
  )
}

export function validateReceivingAccount(receivingAccountId: number) {
  return httpClient.post<boolean>(`/receiving-accounts/${receivingAccountId}/validate`)
}

export function deleteReceivingAccount(receivingAccountId: number) {
  return httpClient.delete<boolean>(`/receiving-accounts/${receivingAccountId}`)
}

export function startReceivingMicrosoftAuthorization(emailAccountId: number) {
  return httpClient.post<string>(`/outlook-authorization/email-accounts/${emailAccountId}`)
}
