<template>
  <div class="full-height full-width row items-start no-wrap">
    <EmailGroupList ref="groupListRef" v-show="!isCollapseGroupList" v-model="selectedGroup"
      :group-category="EmailGroupCategory.EmailAccount" :context-menu-items="groupContextMenuItems"
      class="q-card q-mr-sm full-height" style="min-width: 188px" />

    <q-table ref="tableRef" v-model:selected="selectedRows" class="col full-height" :rows="rows" :columns="columns"
      :filter="filter" row-key="id" selection="multiple" virtual-scroll dense :loading="isLoading">
      <template #top-left>
        <div class="row items-center q-gutter-sm">
          <q-fab v-model="isCreateMenuOpen" color="primary" icon="add" direction="down" vertical-actions-align="left"
            padding="xs" :disable="!selectedGroup.id">
            <q-fab-action square padding="xs" color="primary" icon="dns"
              :label="t('accountManagement.emailAccount.basic')"
              @click="onCreate(EmailAccountConfigurationKind.Basic)" />
            <q-fab-action square padding="xs" color="secondary" icon="cloud" label="Microsoft Graph"
              @click="onCreate(EmailAccountConfigurationKind.MicrosoftGraph)" />
            <q-tooltip v-if="!selectedGroup.id">{{ t('accountManagement.groupRequired') }}</q-tooltip>
          </q-fab>
          <ExportBtn label="" :tooltip="t('accountManagement.emailAccount.exportTemplate')" @click="onExportTemplate" />
          <ImportBtn label="" :tooltip="t('accountManagement.emailAccount.importExcel')" :disable="!selectedGroup.id"
            @click="onImportExcel" />
          <ImportBtn label="" icon="description" :tooltip="t('accountManagement.emailAccount.importText')"
            :disable="!selectedGroup.id" @click="onImportText" />
        </div>
      </template>
      <template #top-right>
        <SearchInput v-model="filter" />
      </template>
      <template #body-cell-index="tableCell">
        <QTableIndex :props="tableCell" />
      </template>
      <template #body-cell-email="tableCell">
        <q-td :props="tableCell">{{ tableCell.value }}</q-td>
        <ContextMenu v-model:selected-values="selectedRows" :items="accountContextMenuItems" :value="tableCell.row" />
      </template>
      <template #body-cell-type="tableCell">
        <q-td :props="tableCell" class="q-gutter-xs">
          <q-chip v-for="protocol in accountProtocols(tableCell.row)" :key="protocol" dense square color="blue-grey-1"
            text-color="blue-grey-9">{{ protocol }}</q-chip>
        </q-td>
      </template>
      <template #body-cell-validation="tableCell">
        <q-td :props="tableCell" class="q-gutter-xs">
          <StatusChip v-if="tableCell.row.sender" :status="senderStatusLabel(tableCell.row.sender.status)">
            <AsyncTooltip :cache="false" :tooltip="tableCell.row.sender.validationFailureReason" />
          </StatusChip>
          <StatusChip v-if="tableCell.row.receiving" :status="receivingStatusLabel(tableCell.row.receiving.status)">
            <AsyncTooltip :cache="false" :tooltip="tableCell.row.receiving.lastError" />
          </StatusChip>
        </q-td>
      </template>
      <template #body-cell-credential="tableCell">
        <q-td :props="tableCell">
          {{ hasConfiguredCredential(tableCell.row) ? t('accountManagement.configured') :
            t('accountManagement.notConfigured') }}
        </q-td>
      </template>
    </q-table>

    <CollapseLeft v-model="isCollapseGroupList" :style="collapseStyleRef" />
    <EmailAccountDialog v-model="isAccountDialogOpen" :email-group-id="selectedGroup.id ?? 0"
      :configuration-kind="editingConfigurationKind" :account="editingAccount" @saved="onAccountSaved" />
  </div>
</template>

<script lang="ts" setup>
import type { QTable, QTableColumn } from 'quasar'
import { t, translateEmailGroup, translateGlobal } from 'src/i18n/helpers'
import { EmailGroupCategory, getEmailGroups } from 'src/api/emailGroup'
import {
  ConnectionSecurity,
  ReceivingAccountStatus,
  ReceivingProtocol,
  SenderAccountStatus,
  SendingProtocol,
  type ConnectionSecurity as ConnectionSecurityValue
} from 'src/api/accountEnums'
import {
  EmailAccountConfigurationKind,
  createBasicEmailAccount,
  deleteEmailAccount,
  deleteInvalidSenderCapabilities,
  getEmailAccounts,
  moveEmailAccounts,
  startMicrosoftAuthorization,
  validateEmailAccount,
  validateEmailAccounts,
  type IEmailAccount,
  type IEmailAccountWrite
} from 'src/api/emailAccounts'
import { guessSmtpInfoPost } from 'src/api/smtpInfo'
import type { IEmailGroupListItem } from '../components/types'
import EmailGroupList from '../components/EmailGroupList.vue'
import EmailAccountDialog from './EmailAccountDialog.vue'
import SearchInput from 'src/components/searchInput/SearchInput.vue'
import ContextMenu from 'src/components/contextMenu/ContextMenu.vue'
import StatusChip from 'src/components/statusChip/StatusChip.vue'
import AsyncTooltip from 'src/components/asyncTooltip/AsyncTooltip.vue'
import ExportBtn from 'src/components/quasarWrapper/buttons/ExportBtn.vue'
import ImportBtn from 'src/components/quasarWrapper/buttons/ImportBtn.vue'
import { ContextMenuIcon, type IContextMenuItem } from 'src/components/contextMenu/types'
import { useQTableIndex } from 'src/compositions/qTableUtils'
import { useTableCollapseLeft } from 'src/components/collapseIcon/useCollapseLeft'
import { LowCodeFieldType, type IPopupDialogParams } from 'src/components/lowCode/types'
import { confirmOperation, notifyError, notifySuccess, notifyUntil, showDialog } from 'src/utils/dialog'
import { readExcel, writeExcel, type IExcelColumnMapper } from 'src/utils/file'
import { splitString } from 'src/utils/stringHelper'

const tableRef = ref<InstanceType<typeof QTable>>()
const { CollapseLeft, collapseStyleRef, isCollapseGroupList } = useTableCollapseLeft(tableRef)
const { indexColumn, QTableIndex } = useQTableIndex()
const groupListRef = ref<{ reloadGroups: () => Promise<void> }>()
const selectedGroup = ref<IEmailGroupListItem>({ name: '', label: '', order: 0 })
const rows = ref<IEmailAccount[]>([])
const selectedRows = ref<IEmailAccount[]>([])
const filter = ref('')
const isLoading = ref(false)
const isCreateMenuOpen = ref(false)
const isAccountDialogOpen = ref(false)
const editingAccount = ref<IEmailAccount>()
const editingConfigurationKind = ref<EmailAccountConfigurationKind>(EmailAccountConfigurationKind.Basic)

const columns = computed<QTableColumn[]>(() => [
  indexColumn,
  { name: 'email', label: t('accountManagement.email'), align: 'left', field: 'email', sortable: true },
  { name: 'name', label: t('accountManagement.name'), align: 'left', field: 'name', sortable: true },
  { name: 'type', label: t('accountManagement.emailAccount.type'), align: 'left', field: accountProtocols },
  { name: 'credential', label: t('accountManagement.credential'), align: 'left', field: hasConfiguredCredential },
  { name: 'validation', label: t('accountManagement.emailAccount.validationResult'), align: 'left', field: accountValidationText },
  { name: 'sentTotalToday', label: t('accountManagement.sentToday'), align: 'right', field: account => account.sender?.sentTotalToday ?? '-' },
  { name: 'maxSendCountPerDay', label: t('accountManagement.dailyLimit'), align: 'right', field: account => account.sender?.maxSendCountPerDay ?? '-' }
])

const excelMappers: IExcelColumnMapper[] = [
  { headerName: 'email', fieldName: 'email', required: true },
  { headerName: 'name', fieldName: 'name' },
  { headerName: 'smtpHost', fieldName: 'smtpHost' },
  { headerName: 'smtpPort', fieldName: 'smtpPort' },
  { headerName: 'smtpLoginName', fieldName: 'smtpLoginName' },
  { headerName: 'smtpPassword', fieldName: 'smtpPassword' },
  { headerName: 'smtpSecurity', fieldName: 'smtpSecurity' },
  { headerName: 'imapHost', fieldName: 'imapHost' },
  { headerName: 'imapPort', fieldName: 'imapPort' },
  { headerName: 'imapLoginName', fieldName: 'imapLoginName' },
  { headerName: 'imapPassword', fieldName: 'imapPassword' },
  { headerName: 'imapSecurity', fieldName: 'imapSecurity' }
]

watch(() => selectedGroup.value.id, loadRows, { immediate: true })

async function loadRows() {
  const groupId = selectedGroup.value.id
  if (!groupId) {
    rows.value = []
    return
  }
  isLoading.value = true
  try {
    const { data } = await getEmailAccounts(groupId)
    rows.value = data
    selectedRows.value = selectedRows.value.filter(selected => data.some(account => account.id === selected.id))
  } finally {
    isLoading.value = false
  }
}

function onCreate(configurationKind: EmailAccountConfigurationKind) {
  if (!selectedGroup.value.id) return
  isCreateMenuOpen.value = false
  editingAccount.value = undefined
  editingConfigurationKind.value = configurationKind
  isAccountDialogOpen.value = true
}

function onEdit(account: IEmailAccount) {
  editingAccount.value = account
  editingConfigurationKind.value = accountConfigurationKind(account)
  isAccountDialogOpen.value = true
}

async function onAccountSaved() {
  await Promise.all([loadRows(), groupListRef.value?.reloadGroups()])
}

function accountConfigurationKind(account: IEmailAccount): EmailAccountConfigurationKind {
  return account.sender?.protocol === SendingProtocol.MicrosoftGraph || account.receiving?.protocol === ReceivingProtocol.MicrosoftGraph
    ? EmailAccountConfigurationKind.MicrosoftGraph
    : EmailAccountConfigurationKind.Basic
}

function accountProtocols(account: IEmailAccount) {
  const protocols: string[] = []
  if (account.sender) protocols.push(account.sender.protocol === SendingProtocol.Smtp ? 'SMTP' : 'Microsoft Graph')
  if (account.receiving) protocols.push(account.receiving.protocol === ReceivingProtocol.Imap ? 'IMAP' : 'Microsoft Graph')
  return [...new Set(protocols)]
}

function senderStatusLabel(status: SenderAccountStatus) {
  return status === SenderAccountStatus.Valid ? 'valid' : status === SenderAccountStatus.Invalid ? 'invalid' : 'unverified'
}

function receivingStatusLabel(status: ReceivingAccountStatus) {
  if (status === ReceivingAccountStatus.Active) return 'valid'
  if (status === ReceivingAccountStatus.Paused) return 'paused'
  return status === ReceivingAccountStatus.Unverified ? 'unverified' : 'invalid'
}

function accountValidationText(account: IEmailAccount) {
  return [account.sender ? senderStatusLabel(account.sender.status) : '', account.receiving ? receivingStatusLabel(account.receiving.status) : ''].filter(Boolean).join(', ')
}

function hasConfiguredCredential(account: IEmailAccount) {
  return account.sender?.hasCredential || account.receiving?.hasCredential || account.hasOAuthAuthorization
}

const accountContextMenuItems = computed<IContextMenuItem<IEmailAccount>[]>(() => [
  { name: 'edit', label: t('accountManagement.edit'), icon: ContextMenuIcon.edit, when: 'onlySingle', onClick: onEdit },
  {
    name: 'authorize',
    label: t('accountManagement.authorize'),
    icon: ContextMenuIcon.adminPanelSettings,
    when: 'onlySingle',
    vif: account => accountConfigurationKind(account) === EmailAccountConfigurationKind.MicrosoftGraph,
    onClick: onAuthorize
  },
  { name: 'validate', label: t('accountManagement.validate'), icon: ContextMenuIcon.verified, when: 'onlySingle', onClick: onValidate },
  { name: 'move', label: translateGlobal('move'), icon: ContextMenuIcon.driveFileMove, onClick: (_account, context) => onMove(context.targetValues) },
  { name: 'validateSelected', label: t('accountManagement.emailAccount.validateSelected'), icon: ContextMenuIcon.verified, onClick: (_account, context) => onValidateMany(context.targetValues) },
  {
    name: 'delete',
    label: t('accountManagement.delete'),
    icon: ContextMenuIcon.delete,
    color: 'negative',
    onClick: (_account, context) => onDelete(context.targetValues, context.clearSelection)
  }
])

const groupContextMenuItems = computed<IContextMenuItem<IEmailGroupListItem>[]>(() => [
  { name: 'importExcel', label: t('accountManagement.emailAccount.importExcel'), icon: ContextMenuIcon.uploadFile, onClick: group => onImportExcel(group.id) },
  { name: 'importText', label: t('accountManagement.emailAccount.importText'), icon: ContextMenuIcon.uploadFile, onClick: group => onImportText(group.id) },
  { name: 'export', label: translateGlobal('export'), icon: ContextMenuIcon.download, onClick: group => onExportGroup(group.id, group.name) },
  {
    name: 'deleteInvalid',
    label: t('accountManagement.emailAccount.deleteInvalidSenders'),
    icon: ContextMenuIcon.deleteSweep,
    color: 'negative',
    onClick: group => onDeleteInvalidSenders(group.id)
  }
])

async function onAuthorize(account: IEmailAccount) {
  const { data: authorizationUrl } = await startMicrosoftAuthorization(account.id)
  window.open(authorizationUrl, '_blank', 'popup,width=720,height=760')
}

async function onValidate(account: IEmailAccount) {
  await notifyUntil(() => validateEmailAccount(account.id), t('accountManagement.validating'), t('accountManagement.validate'))
  await loadRows()
}

async function onValidateMany(accounts: readonly IEmailAccount[]) {
  if (accounts.length === 0) return
  await notifyUntil(() => validateEmailAccounts(accounts.map(account => account.id)), t('accountManagement.validating'), t('accountManagement.emailAccount.validateSelected'))
  await loadRows()
}

async function onMove(accounts: readonly IEmailAccount[]) {
  if (accounts.length === 0) return
  const { data: groups } = await getEmailGroups(EmailGroupCategory.EmailAccount)
  const targetGroups = groups.filter(group => group.id !== selectedGroup.value.id)
  if (targetGroups.length === 0) {
    notifyError(translateEmailGroup('noAvailableTargetGroup'))
    return
  }
  const result = await showDialog<{ targetEmailGroupId: number }>({
    title: translateGlobal('move'),
    oneColumn: true,
    fields: [{
      name: 'targetEmailGroupId',
      label: translateEmailGroup('selectTargetGroup'),
      type: LowCodeFieldType.selectOne,
      options: targetGroups.map(group => ({ label: group.name, value: group.id })),
      mapOptions: true,
      emitValue: true,
      required: true
    }]
  })
  if (!result.ok) return
  await moveEmailAccounts(accounts.map(account => account.id), Number(result.data.targetEmailGroupId))
  selectedRows.value = []
  await Promise.all([loadRows(), groupListRef.value?.reloadGroups()])
}

async function onDelete(accounts: readonly IEmailAccount[], clearSelection: () => void) {
  if (accounts.length === 0) return
  const confirmed = await confirmOperation(
    t('accountManagement.delete'),
    t('accountManagement.deleteConfirm', { count: accounts.length })
  )
  if (!confirmed) return
  for (const account of accounts) await deleteEmailAccount(account.id)
  clearSelection()
  await Promise.all([loadRows(), groupListRef.value?.reloadGroups()])
}

async function onDeleteInvalidSenders(groupId?: number) {
  if (!groupId) return
  const confirmed = await confirmOperation(
    t('accountManagement.delete'),
    t('accountManagement.emailAccount.deleteInvalidSendersConfirm')
  )
  if (!confirmed) return
  await deleteInvalidSenderCapabilities(groupId)
  await Promise.all([loadRows(), groupListRef.value?.reloadGroups()])
}

async function onExportTemplate() {
  await writeExcel([
    {
      email: 'sender@example.com',
      name: 'Example sender',
      smtpHost: 'smtp.example.com',
      smtpPort: 465,
      smtpLoginName: 'sender@example.com',
      smtpPassword: 'app-password',
      smtpSecurity: 'SSL',
      imapHost: '',
      imapPort: '',
      imapLoginName: '',
      imapPassword: '',
      imapSecurity: 'SSL'
    }
  ], {
    fileName: 'email-accounts-template.xlsx',
    sheetName: 'Email accounts',
    mappers: excelMappers,
    strict: true
  })
}

async function onExportGroup(groupId?: number, groupName = 'email-accounts') {
  if (!groupId) return
  const { data: accounts } = await getEmailAccounts(groupId)
  if (accounts.length === 0) {
    notifyError(t('accountManagement.emailAccount.noAccountsToExport'))
    return
  }
  await writeExcel(accounts.map(account => ({
    email: account.email,
    name: account.name ?? '',
    description: account.description ?? '',
    type: accountProtocols(account).join(', '),
    senderStatus: account.sender ? senderStatusLabel(account.sender.status) : '',
    receivingStatus: account.receiving ? receivingStatusLabel(account.receiving.status) : '',
    sentTotalToday: account.sender?.sentTotalToday ?? '',
    maxSendCountPerDay: account.sender?.maxSendCountPerDay ?? ''
  })), {
    fileName: `${groupName}-email-accounts.xlsx`,
    sheetName: groupName,
    strict: false
  })
}

async function onImportExcel(groupId = selectedGroup.value.id) {
  if (!groupId) return
  const importedRows = await readExcel({ sheetIndex: 0, selectSheet: true, mappers: excelMappers, strict: true })
  const requests = importedRows.map(row => toBasicImportRequest(row, groupId)).filter((request): request is IEmailAccountWrite => request !== null)
  await importBasicAccounts(requests)
}

async function onImportText(groupId = selectedGroup.value.id) {
  if (!groupId) return
  const dialogParams: IPopupDialogParams = {
    title: t('accountManagement.emailAccount.importText'),
    oneColumn: true,
    fields: [{
      name: 'text',
      label: t('accountManagement.emailAccount.textAccounts'),
      type: LowCodeFieldType.textarea,
      required: true,
      disableAutoGrow: true
    }]
  }
  const result = await showDialog<{ text: string }>(dialogParams)
  if (!result.ok) return
  const parsedRows = result.data.text
    .split(/\r?\n/)
    .map(line => splitString(line))
    .map(tokens => ({ email: tokens.find(token => token.includes('@')), password: tokens.find(token => !token.includes('@') && !/^\d+$/.test(token) && !token.includes('.')) }))
    .filter((row): row is { email: string, password: string } => Boolean(row.email && row.password))
  if (parsedRows.length === 0) {
    notifyError(t('accountManagement.emailAccount.noImportableAccounts'))
    return
  }
  const { data: smtpInfos } = await guessSmtpInfoPost(parsedRows.map(row => row.email))
  const requests = parsedRows.map(row => {
    const smtpInfo = smtpInfos.find(info => row.email.endsWith(`@${info.domain}`))
    return toBasicImportRequest({
      email: row.email,
      smtpPassword: row.password,
      smtpHost: smtpInfo?.host ?? `smtp.${row.email.split('@')[1]}`,
      smtpPort: smtpInfo?.port ?? 465,
      smtpLoginName: row.email,
      smtpSecurity: smtpInfo?.connectionSecurity ?? ConnectionSecurity.SSL
    }, groupId)
  }).filter((request): request is IEmailAccountWrite => request !== null)
  await importBasicAccounts(requests)
}

function toBasicImportRequest(row: Record<string, unknown>, emailGroupId: number): IEmailAccountWrite | null {
  const email = toTextValue(row.email)
  const smtpPassword = toTextValue(row.smtpPassword)
  const imapPassword = toTextValue(row.imapPassword)
  const isSenderEnabled = smtpPassword.length > 0
  const isReceivingEnabled = imapPassword.length > 0
  if (!email || (!isSenderEnabled && !isReceivingEnabled)) return null
  return {
    email,
    emailGroupId,
    name: optionalString(row.name),
    configurationKind: EmailAccountConfigurationKind.Basic,
    sender: {
      isEnabled: isSenderEnabled,
      maxSendCountPerDay: 0,
      weight: 1,
      smtpCredential: isSenderEnabled ? {
        host: toTextValue(row.smtpHost),
        port: toPort(row.smtpPort, 465),
        loginName: toTextValue(row.smtpLoginName) || email,
        password: smtpPassword,
        connectionSecurity: toConnectionSecurity(row.smtpSecurity)
      } : undefined
    },
    receiving: {
      isEnabled: isReceivingEnabled,
      contentRetentionDays: 30,
      imapCredential: isReceivingEnabled ? {
        host: toTextValue(row.imapHost),
        port: toPort(row.imapPort, 993),
        loginName: toTextValue(row.imapLoginName) || email,
        password: imapPassword,
        connectionSecurity: toConnectionSecurity(row.imapSecurity)
      } : undefined
    }
  }
}

async function importBasicAccounts(requests: IEmailAccountWrite[]) {
  if (requests.length === 0) {
    notifyError(t('accountManagement.emailAccount.noImportableAccounts'))
    return
  }
  for (const request of requests) await createBasicEmailAccount(request)
  await Promise.all([loadRows(), groupListRef.value?.reloadGroups()])
  notifySuccess(t('accountManagement.emailAccount.imported', { count: requests.length }))
}

function optionalString(value: unknown) {
  const result = toTextValue(value)
  return result || undefined
}

/** Restricts imported spreadsheet values to scalar cells before building credential requests. */
function toTextValue(value: unknown) {
  if (typeof value === 'string') return value.trim()
  if (typeof value === 'number') return value.toString()
  return ''
}

function toPort(value: unknown, fallback: number) {
  const port = Number(value)
  return port > 0 && port <= 65535 ? port : fallback
}

function toConnectionSecurity(value: unknown): ConnectionSecurityValue {
  if (typeof value === 'number' && value >= ConnectionSecurity.None && value <= ConnectionSecurity.StartTLS) return value as ConnectionSecurityValue
  const key = toTextValue(value).toLowerCase()
  if (key === 'starttls') return ConnectionSecurity.StartTLS
  if (key === 'tls') return ConnectionSecurity.TLS
  if (key === 'none') return ConnectionSecurity.None
  return ConnectionSecurity.SSL
}
</script>

<style lang="scss" scoped></style>
