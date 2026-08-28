import { afterEach, describe, expect, it, vi } from 'vitest'
import { i18n } from 'src/boot/i18n'
import { ConnectionSecurity } from 'src/api/accountEnums'
import {
  EmailAccountImportField,
  EmailAccountImportIssueReason,
  createBasicEmailAccountImportRequests,
  createEmailAccountGroupExportMappers,
  createEmailAccountImportMappers,
  createEmailAccountTemplateExample,
  hasConfiguredProxy,
  mapLocalizedEmailAccountRows,
  parseTextEmailAccountRows
} from 'src/pages/emailManager/emailAccounts/emailAccountImportExport'

vi.mock('#q-app/wrappers', () => ({
  defineBoot: <T>(bootCallback: T): T => bootCallback
}))

const initialLocale = i18n.global.locale.value

describe('emailAccountImportExport', () => {
  afterEach(() => {
    i18n.global.locale.value = initialLocale
  })

  it('rebuilds localized template and group export columns after locale changes', () => {
    i18n.global.locale.value = 'zh-CN'
    const chineseImportHeaders = createEmailAccountImportMappers().map((mapper) => mapper.headerName)

    i18n.global.locale.value = 'en-US'
    const englishImportHeaders = createEmailAccountImportMappers().map((mapper) => mapper.headerName)
    const englishGroupHeaders = createEmailAccountGroupExportMappers().map((mapper) => mapper.headerName)

    expect(chineseImportHeaders).not.toEqual(englishImportHeaders)
    expect(englishImportHeaders).toEqual(
      expect.arrayContaining([
        'SMTP host',
        'Daily limit',
        'Proxy',
        'Reply-to addresses',
        'Retention days'
      ])
    )
    expect(englishGroupHeaders).toEqual(expect.arrayContaining(['Sender status', 'Receiving status']))
  })

  it('provides a localized template file name', () => {
    i18n.global.locale.value = 'zh-CN'
    expect(i18n.global.t('accountManagement.emailAccount.importExport.templateFileName')).toBe(
      '邮箱账户导入模板.xlsx'
    )

    i18n.global.locale.value = 'en-US'
    expect(i18n.global.t('accountManagement.emailAccount.importExport.templateFileName')).toBe(
      'email-account-import-template.xlsx'
    )
  })

  it('exports a template example with credentials and account settings', () => {
    const example = createEmailAccountTemplateExample()

    expect(example).toMatchObject({
      [EmailAccountImportField.DailyLimit]: 0,
      [EmailAccountImportField.Proxy]: '',
      [EmailAccountImportField.ReplyToEmails]: 'reply@example.com',
      [EmailAccountImportField.RetentionDays]: 30,
      [EmailAccountImportField.SmtpPassword]: 'app-password',
      [EmailAccountImportField.ImapPassword]: 'app-password'
    })
  })

  it('maps only exact current-language headers', () => {
    i18n.global.locale.value = 'en-US'
    const mappers = createEmailAccountImportMappers()
    const emailHeader = mappers.find(
      (mapper) => mapper.fieldName === EmailAccountImportField.EmailAddress
    )!.headerName

    const [localizedRow, legacyRow] = mapLocalizedEmailAccountRows(
      [{ [emailHeader]: 'current@example.com' }, { email: 'legacy@example.com', smtpPassword: 'secret' }],
      mappers
    )

    expect(localizedRow?.values[EmailAccountImportField.EmailAddress]).toBe('current@example.com')
    expect(legacyRow?.values).toEqual({})
  })

  it('builds the complete write request and resolves a proxy by exact name', () => {
    const result = createBasicEmailAccountImportRequests(
      [
        {
          rowNumber: 2,
          values: {
            [EmailAccountImportField.EmailAddress]: 'sender@example.com',
            [EmailAccountImportField.SenderName]: 'Sender',
            [EmailAccountImportField.Description]: 'Description',
            [EmailAccountImportField.Remark]: 'Remark',
            [EmailAccountImportField.SmtpHost]: 'smtp.example.com',
            [EmailAccountImportField.SmtpPort]: 587,
            [EmailAccountImportField.SmtpPassword]: 'smtp-secret',
            [EmailAccountImportField.SmtpSecurity]: 'StartTLS',
            [EmailAccountImportField.DailyLimit]: 250,
            [EmailAccountImportField.Proxy]: 'Primary proxy',
            [EmailAccountImportField.ReplyToEmails]: 'reply@example.com;backup@example.com',
            [EmailAccountImportField.ImapHost]: 'imap.example.com',
            [EmailAccountImportField.ImapPort]: 993,
            [EmailAccountImportField.ImapPassword]: 'imap-secret',
            [EmailAccountImportField.RetentionDays]: 90
          }
        }
      ],
      17,
      [{ id: 8, name: 'Primary proxy', isActive: true, url: 'socks5://proxy:1080', userId: 1, organizationId: 1 }]
    )

    expect(result.issues).toEqual([])
    expect(result.requests[0]).toMatchObject({
      email: 'sender@example.com',
      emailGroupId: 17,
      name: 'Sender',
      description: 'Description',
      remark: 'Remark',
      sender: {
        isEnabled: true,
        proxyId: 8,
        maxSendCountPerDay: 250,
        replyToEmails: 'reply@example.com;backup@example.com',
        smtpCredential: {
          port: 587,
          connectionSecurity: ConnectionSecurity.StartTLS,
          loginName: 'sender@example.com'
        }
      },
      receiving: {
        isEnabled: true,
        contentRetentionDays: 90,
        imapCredential: { port: 993, loginName: 'sender@example.com' }
      }
    })
  })

  it('accepts a usable numeric proxy id and rejects invalid account settings by row', () => {
    const rows = [
      createSenderRow(2, { [EmailAccountImportField.Proxy]: 12 }),
      createSenderRow(3, { [EmailAccountImportField.Proxy]: 'missing' }),
      createSenderRow(4, { [EmailAccountImportField.DailyLimit]: -1 }),
      createReceivingRow(5, { [EmailAccountImportField.RetentionDays]: 3651 })
    ]

    expect(hasConfiguredProxy(rows)).toBe(true)
    const result = createBasicEmailAccountImportRequests(rows, 3, [
      { id: 12, name: 'Proxy 12', isActive: true, url: 'http://proxy:8080', userId: 1, organizationId: 1 }
    ])

    expect(result.requests).toHaveLength(1)
    expect(result.requests[0]?.sender.proxyId).toBe(12)
    expect(result.issues).toEqual([
      { rowNumber: 3, reason: EmailAccountImportIssueReason.ProxyNotFound },
      { rowNumber: 4, reason: EmailAccountImportIssueReason.InvalidDailyLimit },
      { rowNumber: 5, reason: EmailAccountImportIssueReason.InvalidRetentionDays }
    ])
  })

  it('parses supported separators and preserves numeric or dotted passwords', () => {
    const result = parseTextEmailAccountRows(
      ['first@example.com 123456', 'second@example.com,password.with.dots', 'invalid-only'].join('\n')
    )

    expect(result.rows).toEqual([
      { rowNumber: 1, email: 'first@example.com', password: '123456' },
      { rowNumber: 2, email: 'second@example.com', password: 'password.with.dots' }
    ])
    expect(result.issues).toEqual([
      { rowNumber: 3, reason: EmailAccountImportIssueReason.InvalidTextRow }
    ])
  })
})

function createSenderRow(
  rowNumber: number,
  settings: Partial<Record<(typeof EmailAccountImportField)[keyof typeof EmailAccountImportField], unknown>>
) {
  return {
    rowNumber,
    values: {
      [EmailAccountImportField.EmailAddress]: `sender-${rowNumber}@example.com`,
      [EmailAccountImportField.SmtpPassword]: 'secret',
      ...settings
    }
  }
}

function createReceivingRow(
  rowNumber: number,
  settings: Partial<Record<(typeof EmailAccountImportField)[keyof typeof EmailAccountImportField], unknown>>
) {
  return {
    rowNumber,
    values: {
      [EmailAccountImportField.EmailAddress]: `receiver-${rowNumber}@example.com`,
      [EmailAccountImportField.ImapPassword]: 'secret',
      ...settings
    }
  }
}
