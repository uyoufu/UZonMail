import { ConnectionSecurity, type ConnectionSecurity as ConnectionSecurityValue } from 'src/api/accountEnums'
import {
  EmailAccountConfigurationKind,
  type IEmailAccount,
  type IEmailAccountWrite
} from 'src/api/emailAccounts'
import type { IProxy } from 'src/api/proxy'
import type { IExcelColumnMapper } from 'src/utils/file'
import { t } from 'src/i18n/helpers'
import {
  accountProtocols,
  receivingStatusLabel,
  senderStatusLabel
} from './emailAccountPresentation'

export const EmailAccountImportField = {
  EmailAddress: 'emailAddress',
  SenderName: 'senderName',
  Description: 'accountDescription',
  Remark: 'accountRemark',
  SmtpHost: 'senderHost',
  SmtpPort: 'senderPort',
  SmtpLoginName: 'senderLoginName',
  SmtpPassword: 'senderPassword',
  SmtpSecurity: 'senderSecurity',
  DailyLimit: 'dailyLimit',
  Proxy: 'senderProxy',
  ReplyToEmails: 'replyToEmails',
  ImapHost: 'receivingHost',
  ImapPort: 'receivingPort',
  ImapLoginName: 'receivingLoginName',
  ImapPassword: 'receivingPassword',
  ImapSecurity: 'receivingSecurity',
  RetentionDays: 'retentionDays'
} as const

export type EmailAccountImportField =
  (typeof EmailAccountImportField)[keyof typeof EmailAccountImportField]

export const EmailAccountImportIssueReason = {
  InvalidEmail: 'invalidEmail',
  CredentialRequired: 'credentialRequired',
  InvalidDailyLimit: 'invalidDailyLimit',
  InvalidRetentionDays: 'invalidRetentionDays',
  ProxyNotFound: 'proxyNotFound',
  InvalidTextRow: 'invalidTextRow'
} as const

export type EmailAccountImportIssueReason =
  (typeof EmailAccountImportIssueReason)[keyof typeof EmailAccountImportIssueReason]

export interface ILocalizedEmailAccountImportRow {
  rowNumber: number
  values: Partial<Record<EmailAccountImportField, unknown>>
}

export interface IEmailAccountImportIssue {
  rowNumber: number
  reason: EmailAccountImportIssueReason
}

export interface IEmailAccountImportResult {
  requests: IEmailAccountWrite[]
  issues: IEmailAccountImportIssue[]
}

export interface ITextEmailAccountRow {
  rowNumber: number
  email: string
  password: string
}

const GroupExportField = {
  EmailAddress: 'emailAddress',
  SenderName: 'senderName',
  Description: 'description',
  AccountType: 'accountType',
  SenderStatus: 'senderStatus',
  ReceivingStatus: 'receivingStatus',
  SentToday: 'sentToday',
  DailyLimit: 'dailyLimit'
} as const

/** Creates localized columns for the account import template. */
export function createEmailAccountImportMappers(): IExcelColumnMapper[] {
  return [
    { headerName: t('accountManagement.email'), fieldName: EmailAccountImportField.EmailAddress, required: true },
    {
      headerName: t('accountManagement.emailAccount.senderName'),
      fieldName: EmailAccountImportField.SenderName
    },
    { headerName: t('accountManagement.description'), fieldName: EmailAccountImportField.Description },
    { headerName: t('accountManagement.remark'), fieldName: EmailAccountImportField.Remark },
    {
      headerName: t('accountManagement.emailAccount.importExport.columns.smtpHost'),
      fieldName: EmailAccountImportField.SmtpHost
    },
    {
      headerName: t('accountManagement.emailAccount.importExport.columns.smtpPort'),
      fieldName: EmailAccountImportField.SmtpPort
    },
    {
      headerName: t('accountManagement.emailAccount.importExport.columns.smtpLoginName'),
      fieldName: EmailAccountImportField.SmtpLoginName
    },
    {
      headerName: t('accountManagement.emailAccount.importExport.columns.smtpPassword'),
      fieldName: EmailAccountImportField.SmtpPassword
    },
    {
      headerName: t('accountManagement.emailAccount.importExport.columns.smtpSecurity'),
      fieldName: EmailAccountImportField.SmtpSecurity
    },
    { headerName: t('accountManagement.dailyLimit'), fieldName: EmailAccountImportField.DailyLimit },
    { headerName: t('accountManagement.emailAccount.proxy'), fieldName: EmailAccountImportField.Proxy },
    { headerName: t('accountManagement.replyTo'), fieldName: EmailAccountImportField.ReplyToEmails },
    {
      headerName: t('accountManagement.emailAccount.importExport.columns.imapHost'),
      fieldName: EmailAccountImportField.ImapHost
    },
    {
      headerName: t('accountManagement.emailAccount.importExport.columns.imapPort'),
      fieldName: EmailAccountImportField.ImapPort
    },
    {
      headerName: t('accountManagement.emailAccount.importExport.columns.imapLoginName'),
      fieldName: EmailAccountImportField.ImapLoginName
    },
    {
      headerName: t('accountManagement.emailAccount.importExport.columns.imapPassword'),
      fieldName: EmailAccountImportField.ImapPassword
    },
    {
      headerName: t('accountManagement.emailAccount.importExport.columns.imapSecurity'),
      fieldName: EmailAccountImportField.ImapSecurity
    },
    { headerName: t('accountManagement.retentionDays'), fieldName: EmailAccountImportField.RetentionDays }
  ]
}

/** Creates localized columns for the human-readable group export. */
export function createEmailAccountGroupExportMappers(): IExcelColumnMapper[] {
  return [
    { headerName: t('accountManagement.email'), fieldName: GroupExportField.EmailAddress },
    { headerName: t('accountManagement.emailAccount.senderName'), fieldName: GroupExportField.SenderName },
    { headerName: t('accountManagement.description'), fieldName: GroupExportField.Description },
    { headerName: t('accountManagement.emailAccount.type'), fieldName: GroupExportField.AccountType },
    {
      headerName: t('accountManagement.emailAccount.importExport.columns.senderStatus'),
      fieldName: GroupExportField.SenderStatus
    },
    {
      headerName: t('accountManagement.emailAccount.importExport.columns.receivingStatus'),
      fieldName: GroupExportField.ReceivingStatus
    },
    { headerName: t('accountManagement.sentToday'), fieldName: GroupExportField.SentToday },
    { headerName: t('accountManagement.dailyLimit'), fieldName: GroupExportField.DailyLimit }
  ]
}

export function createEmailAccountTemplateExample(): Record<EmailAccountImportField, string | number> {
  return {
    [EmailAccountImportField.EmailAddress]: 'sender@example.com',
    [EmailAccountImportField.SenderName]: t('accountManagement.emailAccount.importExport.exampleSenderName'),
    [EmailAccountImportField.Description]: '',
    [EmailAccountImportField.Remark]: '',
    [EmailAccountImportField.SmtpHost]: 'smtp.example.com',
    [EmailAccountImportField.SmtpPort]: 465,
    [EmailAccountImportField.SmtpLoginName]: 'sender@example.com',
    [EmailAccountImportField.SmtpPassword]: 'app-password',
    [EmailAccountImportField.SmtpSecurity]: 'SSL',
    [EmailAccountImportField.DailyLimit]: 0,
    [EmailAccountImportField.Proxy]: '',
    [EmailAccountImportField.ReplyToEmails]: 'reply@example.com',
    [EmailAccountImportField.ImapHost]: 'imap.example.com',
    [EmailAccountImportField.ImapPort]: 993,
    [EmailAccountImportField.ImapLoginName]: 'sender@example.com',
    [EmailAccountImportField.ImapPassword]: 'app-password',
    [EmailAccountImportField.ImapSecurity]: 'SSL',
    [EmailAccountImportField.RetentionDays]: 30
  }
}

export function toEmailAccountGroupExportRows(accounts: readonly IEmailAccount[]): Record<string, unknown>[] {
  return accounts.map((account) => ({
    [GroupExportField.EmailAddress]: account.email,
    [GroupExportField.SenderName]: account.name ?? '',
    [GroupExportField.Description]: account.description ?? '',
    [GroupExportField.AccountType]: accountProtocols(account).join(', '),
    [GroupExportField.SenderStatus]: account.sender ? senderStatusLabel(account.sender.status) : '',
    [GroupExportField.ReceivingStatus]: account.receiving ? receivingStatusLabel(account.receiving.status) : '',
    [GroupExportField.SentToday]: account.sender?.sentTotalToday ?? '',
    [GroupExportField.DailyLimit]: account.sender?.maxSendCountPerDay ?? ''
  }))
}

/** Maps only exact localized headers, preventing legacy internal column names from leaking through. */
export function mapLocalizedEmailAccountRows(
  sourceRows: readonly Record<string, unknown>[],
  mappers: readonly IExcelColumnMapper[]
): ILocalizedEmailAccountImportRow[] {
  const mapperByHeader = new Map(mappers.map((mapper) => [mapper.headerName, mapper.fieldName]))
  return sourceRows.map((sourceRow, index) => {
    const values: Partial<Record<EmailAccountImportField, unknown>> = {}
    for (const [headerName, value] of Object.entries(sourceRow)) {
      const fieldName = mapperByHeader.get(headerName) as EmailAccountImportField | undefined
      if (fieldName) values[fieldName] = value
    }
    return { rowNumber: index + 2, values }
  })
}

export function hasConfiguredProxy(rows: readonly ILocalizedEmailAccountImportRow[]): boolean {
  return rows.some((row) => toTextValue(row.values[EmailAccountImportField.Proxy]).length > 0)
}

export function createBasicEmailAccountImportRequests(
  rows: readonly ILocalizedEmailAccountImportRow[],
  emailGroupId: number,
  usableProxies: readonly IProxy[]
): IEmailAccountImportResult {
  const requests: IEmailAccountWrite[] = []
  const issues: IEmailAccountImportIssue[] = []

  for (const row of rows) {
    const result = toBasicImportRequest(row.values, emailGroupId, usableProxies)
    if (result.request) requests.push(result.request)
    else issues.push({ rowNumber: row.rowNumber, reason: result.reason })
  }

  return { requests, issues }
}

export function parseTextEmailAccountRows(text: string): {
  rows: ITextEmailAccountRow[]
  issues: IEmailAccountImportIssue[]
} {
  const rows: ITextEmailAccountRow[] = []
  const issues: IEmailAccountImportIssue[] = []
  const lines = text.split(/\r?\n/)

  lines.forEach((line, index) => {
    if (!line.trim()) return
    const tokens = line.split(/[,，;；\s]+/).filter(Boolean)
    const email = tokens.find((token) => token.includes('@'))
    const password = tokens.find((token) => token !== email)
    if (!email || !password) {
      issues.push({ rowNumber: index + 1, reason: EmailAccountImportIssueReason.InvalidTextRow })
      return
    }
    rows.push({ rowNumber: index + 1, email: email.trim(), password })
  })

  return { rows, issues }
}

function toBasicImportRequest(
  row: Partial<Record<EmailAccountImportField, unknown>>,
  emailGroupId: number,
  usableProxies: readonly IProxy[]
): { request: IEmailAccountWrite; reason?: never } | { request?: never; reason: EmailAccountImportIssueReason } {
  const email = toTextValue(row[EmailAccountImportField.EmailAddress])
  if (!email || !email.includes('@')) return { reason: EmailAccountImportIssueReason.InvalidEmail }

  const smtpPassword = toTextValue(row[EmailAccountImportField.SmtpPassword])
  const imapPassword = toTextValue(row[EmailAccountImportField.ImapPassword])
  const isSenderEnabled = smtpPassword.length > 0
  const isReceivingEnabled = imapPassword.length > 0
  if (!isSenderEnabled && !isReceivingEnabled) return { reason: EmailAccountImportIssueReason.CredentialRequired }

  const dailyLimit = toOptionalInteger(row[EmailAccountImportField.DailyLimit], 0)
  if (isSenderEnabled && (dailyLimit === null || dailyLimit < 0)) {
    return { reason: EmailAccountImportIssueReason.InvalidDailyLimit }
  }

  const retentionDays = toOptionalInteger(row[EmailAccountImportField.RetentionDays], 30)
  if (isReceivingEnabled && (retentionDays === null || retentionDays < 1 || retentionDays > 3650)) {
    return { reason: EmailAccountImportIssueReason.InvalidRetentionDays }
  }

  const proxyReference = toTextValue(row[EmailAccountImportField.Proxy])
  const proxyId = isSenderEnabled ? resolveProxyId(proxyReference, usableProxies) : undefined
  if (isSenderEnabled && proxyReference && proxyId === undefined) {
    return { reason: EmailAccountImportIssueReason.ProxyNotFound }
  }

  return {
    request: {
      email,
      emailGroupId,
      name: optionalString(row[EmailAccountImportField.SenderName]),
      description: optionalString(row[EmailAccountImportField.Description]),
      remark: optionalString(row[EmailAccountImportField.Remark]),
      configurationKind: EmailAccountConfigurationKind.Basic,
      sender: {
        isEnabled: isSenderEnabled,
        proxyId,
        maxSendCountPerDay: isSenderEnabled ? (dailyLimit ?? 0) : 0,
        replyToEmails: isSenderEnabled ? optionalString(row[EmailAccountImportField.ReplyToEmails]) : undefined,
        smtpCredential: isSenderEnabled
          ? {
              host: toTextValue(row[EmailAccountImportField.SmtpHost]),
              port: toPort(row[EmailAccountImportField.SmtpPort], 465),
              loginName: toTextValue(row[EmailAccountImportField.SmtpLoginName]) || email,
              password: smtpPassword,
              connectionSecurity: toConnectionSecurity(row[EmailAccountImportField.SmtpSecurity])
            }
          : undefined
      },
      receiving: {
        isEnabled: isReceivingEnabled,
        contentRetentionDays: isReceivingEnabled ? (retentionDays ?? 30) : 30,
        imapCredential: isReceivingEnabled
          ? {
              host: toTextValue(row[EmailAccountImportField.ImapHost]),
              port: toPort(row[EmailAccountImportField.ImapPort], 993),
              loginName: toTextValue(row[EmailAccountImportField.ImapLoginName]) || email,
              password: imapPassword,
              connectionSecurity: toConnectionSecurity(row[EmailAccountImportField.ImapSecurity])
            }
          : undefined
      }
    }
  }
}

function resolveProxyId(reference: string, usableProxies: readonly IProxy[]): number | undefined {
  if (!reference) return undefined
  const proxyByName = usableProxies.find((proxy) => proxy.name === reference)
  if (proxyByName?.id) return proxyByName.id

  const proxyId = Number(reference)
  if (!Number.isSafeInteger(proxyId) || proxyId <= 0) return undefined
  return usableProxies.find((proxy) => proxy.id === proxyId)?.id
}

function optionalString(value: unknown): string | undefined {
  return toTextValue(value) || undefined
}

function toTextValue(value: unknown): string {
  if (typeof value === 'string') return value.trim()
  if (typeof value === 'number') return value.toString()
  return ''
}

function toOptionalInteger(value: unknown, fallback: number): number | null {
  if (value === undefined || value === null || toTextValue(value) === '') return fallback
  const numericValue = Number(value)
  return Number.isSafeInteger(numericValue) ? numericValue : null
}

function toPort(value: unknown, fallback: number): number {
  const port = Number(value)
  return Number.isSafeInteger(port) && port > 0 && port <= 65535 ? port : fallback
}

function toConnectionSecurity(value: unknown): ConnectionSecurityValue {
  if (typeof value === 'number' && value >= ConnectionSecurity.None && value <= ConnectionSecurity.StartTLS) {
    return value as ConnectionSecurityValue
  }
  const key = toTextValue(value).toLowerCase()
  if (key === 'starttls') return ConnectionSecurity.StartTLS
  if (key === 'tls') return ConnectionSecurity.TLS
  if (key === 'none') return ConnectionSecurity.None
  return ConnectionSecurity.SSL
}
