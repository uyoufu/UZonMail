import { ConnectionSecurity } from 'src/api/accountEnums'
import { createBasicEmailAccount, getEmailAccounts, type IEmailAccountWrite } from 'src/api/emailAccounts'
import { getUsableProxies } from 'src/api/proxy'
import { guessSmtpInfoPost } from 'src/api/smtpInfo'
import { LowCodeFieldType } from 'src/components/lowCode/types'
import { t } from 'src/i18n/helpers'
import { notifyError, notifySuccess, notifyUntil, notifyWarning, showDialog } from 'src/utils/dialog'
import { readExcelCore, writeExcel } from 'src/utils/file'
import {
  EmailAccountImportField,
  EmailAccountImportIssueReason,
  createBasicEmailAccountImportRequests,
  createEmailAccountGroupExportMappers,
  createEmailAccountImportMappers,
  createEmailAccountTemplateExample,
  hasConfiguredProxy,
  mapLocalizedEmailAccountRows,
  parseTextEmailAccountRows,
  toEmailAccountGroupExportRows,
  type IEmailAccountImportIssue,
  type ILocalizedEmailAccountImportRow
} from './emailAccountImportExport'

export function useEmailAccountImportExport(
  currentGroupId: Readonly<Ref<number | undefined>>,
  onAccountsChanged: () => Promise<void>
) {
  const importMappers = computed(createEmailAccountImportMappers)
  const groupExportMappers = computed(createEmailAccountGroupExportMappers)
  const exportTemplateTooltip = computed(
    () =>
      `${t('accountManagement.emailAccount.importExport.exportTemplateTooltip')}\n${t('accountManagement.emailAccount.importExport.currentLanguageColumnsHint')}`
  )
  const importExcelTooltip = computed(() => [
    t('accountManagement.emailAccount.importExport.importExcelTooltip'),
    t('accountManagement.emailAccount.importExport.excelFormatHint')
  ])
  const importTextTooltip = computed(() => [
    t('accountManagement.emailAccount.importExport.importTextTooltip'),
    t('accountManagement.emailAccount.importExport.textFormatHint')
  ])
  const exportGroupTooltip = computed(() =>
    t('accountManagement.emailAccount.importExport.exportGroupTooltip')
  )

  async function onExportTemplate() {
    await writeExcel([createEmailAccountTemplateExample()], {
      fileName: t('accountManagement.emailAccount.importExport.templateFileName'),
      sheetName: t('accountManagement.emailAccount.importExport.sheetName'),
      mappers: importMappers.value,
      strict: true
    })
    notifySuccess(t('accountManagement.emailAccount.importExport.templateExported'))
  }

  async function onExportGroup(groupId?: number, groupName = 'email-accounts') {
    if (!groupId) return
    const { data: accounts } = await getEmailAccounts(groupId)
    if (accounts.length === 0) {
      notifyError(t('accountManagement.emailAccount.noAccountsToExport'))
      return
    }

    await writeExcel(toEmailAccountGroupExportRows(accounts), {
      fileName: `${groupName}-email-accounts.xlsx`,
      sheetName: groupName,
      mappers: groupExportMappers.value,
      strict: true
    })
    notifySuccess(t('accountManagement.emailAccount.importExport.groupExported'))
  }

  async function onImportExcel(groupId = currentGroupId.value) {
    if (!groupId) return
    const { data: sourceRows } = await readExcelCore({ sheetIndex: 0, selectSheet: true })
    const localizedRows = mapLocalizedEmailAccountRows(sourceRows, importMappers.value)
    const usableProxies = hasConfiguredProxy(localizedRows) ? (await getUsableProxies()).data : []
    const importResult = createBasicEmailAccountImportRequests(localizedRows, groupId, usableProxies)
    await importBasicAccounts(importResult.requests, importResult.issues)
  }

  async function onImportText(groupId = currentGroupId.value) {
    if (!groupId) return
    const result = await showDialog<{ text: string }>({
      title: t('accountManagement.emailAccount.importText'),
      oneColumn: true,
      fields: [
        {
          name: 'text',
          label: t('accountManagement.emailAccount.textAccounts'),
          type: LowCodeFieldType.textarea,
          disableAutoGrow: true,
          placeholder: t('accountManagement.emailAccount.importExport.textExample'),
          tooltip: [
            t('accountManagement.emailAccount.importExport.textFormatHint'),
            t('accountManagement.emailAccount.importExport.textExample')
          ],
          validate: (value) => ({
            ok: typeof value === 'string' && value.trim().length > 0,
            message: t('accountManagement.emailAccount.importExport.textRequired')
          })
        }
      ]
    })
    if (!result.ok) return

    const parsedText = parseTextEmailAccountRows(result.data.text)
    if (parsedText.rows.length === 0) {
      notifyImportIssues(parsedText.issues, true)
      return
    }

    const { data: smtpInfos } = await guessSmtpInfoPost(parsedText.rows.map((row) => row.email))
    const localizedRows: ILocalizedEmailAccountImportRow[] = parsedText.rows.map((row) => {
      const smtpInfo = smtpInfos.find((info) => row.email.endsWith(`@${info.domain}`))
      return {
        rowNumber: row.rowNumber,
        values: {
          [EmailAccountImportField.EmailAddress]: row.email,
          [EmailAccountImportField.SmtpPassword]: row.password,
          [EmailAccountImportField.SmtpHost]: smtpInfo?.host ?? `smtp.${row.email.split('@')[1]}`,
          [EmailAccountImportField.SmtpPort]: smtpInfo?.port ?? 465,
          [EmailAccountImportField.SmtpLoginName]: row.email,
          [EmailAccountImportField.SmtpSecurity]:
            smtpInfo?.connectionSecurity ?? ConnectionSecurity.SSL
        }
      }
    })
    const importResult = createBasicEmailAccountImportRequests(localizedRows, groupId, [])
    await importBasicAccounts(importResult.requests, [...parsedText.issues, ...importResult.issues])
  }

  async function importBasicAccounts(requests: IEmailAccountWrite[], issues: IEmailAccountImportIssue[]) {
    if (requests.length === 0) {
      if (issues.length > 0) notifyImportIssues(issues, true)
      else notifyError(t('accountManagement.emailAccount.noImportableAccounts'))
      return
    }

    let importedCount = 0
    try {
      await notifyUntil(
        async (update) => {
          for (const request of requests) {
            await createBasicEmailAccount(request)
            importedCount += 1
            update(t('accountManagement.emailAccount.importExport.importProgress', {
              current: importedCount,
              total: requests.length
            }))
          }
        },
        t('accountManagement.emailAccount.importExport.importing'),
        t('accountManagement.emailAccount.importExport.importProgress', {
          current: 0,
          total: requests.length
        })
      )
    } finally {
      if (importedCount > 0) await onAccountsChanged()
    }

    notifySuccess(t('accountManagement.emailAccount.importExport.imported', { count: importedCount }))
    if (issues.length > 0) notifyImportIssues(issues, false)
  }

  function notifyImportIssues(issues: readonly IEmailAccountImportIssue[], isAllInvalid: boolean) {
    const caption = groupIssueRowsByReason(issues)
    const message = isAllInvalid
      ? t('accountManagement.emailAccount.importExport.allRowsInvalid')
      : t('accountManagement.emailAccount.importExport.skippedRows', { count: issues.length })
    const options = { message, caption }
    if (isAllInvalid) notifyError(options)
    else notifyWarning(options)
  }

  function groupIssueRowsByReason(issues: readonly IEmailAccountImportIssue[]): string {
    const rowsByReason = new Map<IEmailAccountImportIssue['reason'], number[]>()
    for (const issue of issues) {
      const rowNumbers = rowsByReason.get(issue.reason) ?? []
      rowNumbers.push(issue.rowNumber)
      rowsByReason.set(issue.reason, rowNumbers)
    }

    return [...rowsByReason.entries()]
      .map(([reason, rowNumbers]) =>
        t('accountManagement.emailAccount.importExport.issueRows', {
          reason: issueReasonLabel(reason),
          rows: rowNumbers.join(', ')
        })
      )
      .join('; ')
  }

  function issueReasonLabel(reason: IEmailAccountImportIssue['reason']): string {
    switch (reason) {
      case EmailAccountImportIssueReason.InvalidEmail:
        return t('accountManagement.emailAccount.importExport.issues.invalidEmail')
      case EmailAccountImportIssueReason.CredentialRequired:
        return t('accountManagement.emailAccount.importExport.issues.credentialRequired')
      case EmailAccountImportIssueReason.InvalidDailyLimit:
        return t('accountManagement.emailAccount.importExport.issues.invalidDailyLimit')
      case EmailAccountImportIssueReason.InvalidRetentionDays:
        return t('accountManagement.emailAccount.importExport.issues.invalidRetentionDays')
      case EmailAccountImportIssueReason.ProxyNotFound:
        return t('accountManagement.emailAccount.importExport.issues.proxyNotFound')
      case EmailAccountImportIssueReason.InvalidTextRow:
        return t('accountManagement.emailAccount.importExport.issues.invalidTextRow')
    }
  }

  return {
    exportTemplateTooltip,
    importExcelTooltip,
    importTextTooltip,
    exportGroupTooltip,
    onExportTemplate,
    onExportGroup,
    onImportExcel,
    onImportText
  }
}
