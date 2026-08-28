import {
  ConnectionSecurity,
  OAuthApplicationSource,
  type ConnectionSecurity as ConnectionSecurityValue,
  type OAuthApplicationSource as OAuthApplicationSourceValue
} from 'src/api/accountEnums'

export const EmailAccountTab = {
  Sender: 'sender',
  Receiving: 'receiving'
} as const

export type EmailAccountTab = (typeof EmailAccountTab)[keyof typeof EmailAccountTab]

export const MailProtocol = {
  Smtp: 'smtp',
  Imap: 'imap'
} as const

export type MailProtocol = (typeof MailProtocol)[keyof typeof MailProtocol]

export const CredentialField = {
  Host: 'host',
  Port: 'port',
  LoginName: 'loginName',
  Password: 'password',
  ConnectionSecurity: 'connectionSecurity'
} as const

export type CredentialField = (typeof CredentialField)[keyof typeof CredentialField]

export type EmailAccountFormRule = (value: unknown) => boolean | string

export const connectionSecurityOptions = [
  { label: 'None', value: ConnectionSecurity.None },
  { label: 'SSL', value: ConnectionSecurity.SSL },
  { label: 'TLS', value: ConnectionSecurity.TLS },
  { label: 'StartTLS', value: ConnectionSecurity.StartTLS }
] as const

export interface IProtocolCredentialForm {
  host: string
  port: number
  loginName: string
  password: string
  connectionSecurity: ConnectionSecurityValue
}

export interface IEmailAccountDialogForm {
  email: string
  name: string
  description: string
  remark: string
  sender: IProtocolCredentialForm & {
    proxyId: number | null
    maxSendCountPerDay: number
    replyToEmails: string
  }
  receiving: IProtocolCredentialForm & {
    contentRetentionDays: number
  }
  replaceMicrosoftGraphApplication: boolean
  microsoftGraphApplication: {
    applicationSource: OAuthApplicationSourceValue
    tenantId: string
    clientId: string
    clientSecret: string
  }
}

export function createEmailAccountDialogForm(): IEmailAccountDialogForm {
  return {
    email: '',
    name: '',
    description: '',
    remark: '',
    sender: {
      proxyId: null,
      maxSendCountPerDay: 0,
      replyToEmails: '',
      host: '',
      port: 465,
      loginName: '',
      password: '',
      connectionSecurity: ConnectionSecurity.SSL
    },
    receiving: {
      contentRetentionDays: 30,
      host: '',
      port: 993,
      loginName: '',
      password: '',
      connectionSecurity: ConnectionSecurity.SSL
    },
    replaceMicrosoftGraphApplication: false,
    microsoftGraphApplication: {
      applicationSource: OAuthApplicationSource.System,
      tenantId: '',
      clientId: '',
      clientSecret: ''
    }
  }
}
