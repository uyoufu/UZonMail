<template>
  <div class="full-height full-width row items-start no-wrap">
    <EmailGroupList
      v-show="!isCollapseGroupList"
      v-model="selectedGroup"
      :group-category="EmailGroupCategory.Sender"
      class="q-card q-mr-sm full-height"
      style="min-width: 176px"
    />

    <q-table
      ref="tableRef"
      v-model:selected="selectedRows"
      class="col full-height"
      :rows="rows"
      :columns="columns"
      :filter="filter"
      row-key="id"
      selection="multiple"
      virtual-scroll
      dense
      :loading="isLoading"
    >
      <template #top-left>
        <CreateBtn
          :tooltip="t('accountManagement.sender.create')"
          :disable="!selectedGroup.id"
          :tooltip-when-disabled="t('accountManagement.groupRequired')"
          @click="onCreateClick"
        />
      </template>
      <template #top-right>
        <SearchInput v-model="filter" />
      </template>
      <template #body-cell-index="props">
        <QTableIndex :props="props" />
      </template>
      <template #body-cell-email="props">
        <q-td :props="props">{{ props.value }}</q-td>
        <ContextMenu
          v-model:selected-values="selectedRows"
          :items="contextMenuItems"
          :value="props.row"
        />
      </template>
      <template #body-cell-status="props">
        <q-td :props="props">
          <StatusChip :status="senderStatusLabels[props.value as SenderAccountStatus]">
            <AsyncTooltip :cache="false" :tooltip="props.row.validationFailureReason" />
          </StatusChip>
        </q-td>
      </template>
    </q-table>

    <CollapseLeft v-model="isCollapseGroupList" :style="collapseStyleRef" />
  </div>
</template>

<script lang="ts" setup>
import type { QTable, QTableColumn } from 'quasar'
import { t } from 'src/i18n/helpers'
import { EmailGroupCategory } from 'src/api/emailGroup'
import {
  ConnectionSecurity,
  OAuthApplicationSource,
  SenderAccountStatus,
  SendingProtocol,
  type ConnectionSecurity as ConnectionSecurityValue,
  type OAuthApplicationSource as OAuthApplicationSourceValue,
  type SenderAccountStatus as SenderAccountStatusValue
} from 'src/api/accountEnums'
import {
  createMicrosoftGraphSenderAccount,
  createSmtpSenderAccount,
  deleteSenderAccount,
  getSenderAccounts,
  startSenderMicrosoftAuthorization,
  updateMicrosoftGraphApplication,
  updateSenderAccount,
  updateSmtpCredential,
  validateSenderAccount,
  type IMicrosoftGraphApplicationWrite,
  type ISenderAccount,
  type ISmtpCredentialWrite
} from 'src/api/senderAccounts'
import type { IEmailGroupListItem } from '../components/types'
import EmailGroupList from '../components/EmailGroupList.vue'
import SearchInput from 'src/components/searchInput/SearchInput.vue'
import ContextMenu from 'src/components/contextMenu/ContextMenu.vue'
import StatusChip from 'src/components/statusChip/StatusChip.vue'
import AsyncTooltip from 'src/components/asyncTooltip/AsyncTooltip.vue'
import { ContextMenuIcon, type IContextMenuItem } from 'src/components/contextMenu/types'
import { useQTableIndex } from 'src/compositions/qTableUtils'
import { useTableCollapseLeft } from 'src/components/collapseIcon/useCollapseLeft'
import { LowCodeFieldType } from 'src/components/lowCode/types'
import { confirmOperation, notifySuccess, notifyUntil, showDialog } from 'src/utils/dialog'

const tableRef = ref<InstanceType<typeof QTable>>()
const { CollapseLeft, collapseStyleRef, isCollapseGroupList } = useTableCollapseLeft(tableRef)
const { indexColumn, QTableIndex } = useQTableIndex()
const selectedGroup = ref<IEmailGroupListItem>({ id: 0, name: '', label: '', order: 0 })
const rows = ref<ISenderAccount[]>([])
const selectedRows = ref<ISenderAccount[]>([])
const filter = ref('')
const isLoading = ref(false)

const senderStatusLabels: Record<SenderAccountStatusValue, string> = {
  [SenderAccountStatus.Unverified]: 'unverified',
  [SenderAccountStatus.Valid]: 'valid',
  [SenderAccountStatus.Invalid]: 'invalid'
}

const columns = computed<QTableColumn[]>(() => [
  indexColumn,
  { name: 'email', label: t('accountManagement.email'), align: 'left', field: 'email', sortable: true },
  { name: 'name', label: t('accountManagement.name'), align: 'left', field: 'name', sortable: true },
  {
    name: 'protocol',
    label: t('accountManagement.protocol'),
    align: 'left',
    field: 'protocol',
    format: (value: number) => value === SendingProtocol.Smtp ? 'SMTP' : 'Microsoft Graph'
  },
  {
    name: 'credential',
    label: t('accountManagement.credential'),
    align: 'left',
    field: (row: ISenderAccount) => row.hasSmtpCredential || row.hasOAuthAuthorization,
    format: (value: boolean) => value ? t('accountManagement.configured') : t('accountManagement.notConfigured')
  },
  { name: 'sentTotalToday', label: t('accountManagement.sentToday'), align: 'right', field: 'sentTotalToday' },
  { name: 'maxSendCountPerDay', label: t('accountManagement.dailyLimit'), align: 'right', field: 'maxSendCountPerDay' },
  { name: 'status', label: t('accountManagement.status'), align: 'left', field: 'status' }
])

watch(() => selectedGroup.value.id, onReload, { immediate: true })

async function onReload() {
  if (!selectedGroup.value.id) {
    rows.value = []
    return
  }
  isLoading.value = true
  try {
    const response = await getSenderAccounts(selectedGroup.value.id)
    rows.value = response.data
  } finally {
    isLoading.value = false
  }
}

async function onCreateClick() {
  const protocolResult = await showDialog<{ protocol: number }>({
    title: t('accountManagement.sender.create'),
    oneColumn: true,
    fields: [{
      name: 'protocol',
      label: t('accountManagement.protocol'),
      type: LowCodeFieldType.selectOne,
      value: SendingProtocol.Smtp,
      options: [
        { label: 'SMTP', value: SendingProtocol.Smtp },
        { label: 'Microsoft Graph', value: SendingProtocol.MicrosoftGraph }
      ],
      mapOptions: true,
      emitValue: true,
      required: true
    }]
  })
  if (!protocolResult.ok || !selectedGroup.value.id) return

  if (Number(protocolResult.data.protocol) === SendingProtocol.MicrosoftGraph) {
    await onCreateMicrosoftGraph(selectedGroup.value.id)
    return
  }
  await onCreateSmtp(selectedGroup.value.id)
}

async function onCreateSmtp(emailGroupId: number) {
  const result = await showDialog<{
    email: string
    name?: string
    host: string
    port: number
    loginName: string
    password: string
    connectionSecurity: number
    maxSendCountPerDay: number
  }>({
    title: t('accountManagement.sender.createSmtp'),
    oneColumn: true,
    fields: [
      { name: 'email', label: t('accountManagement.email'), type: LowCodeFieldType.email, required: true },
      { name: 'name', label: t('accountManagement.name'), type: LowCodeFieldType.text },
      { name: 'host', label: t('accountManagement.host'), type: LowCodeFieldType.text, required: true },
      { name: 'port', label: t('accountManagement.port'), type: LowCodeFieldType.number, value: 465, required: true },
      { name: 'loginName', label: t('accountManagement.loginName'), type: LowCodeFieldType.text, required: true },
      { name: 'password', label: t('accountManagement.password'), type: LowCodeFieldType.password, required: true },
      {
        name: 'connectionSecurity',
        label: t('accountManagement.connectionSecurity'),
        type: LowCodeFieldType.selectOne,
        value: ConnectionSecurity.SSL,
        options: connectionSecurityOptions,
        mapOptions: true,
        emitValue: true,
        required: true
      },
      { name: 'maxSendCountPerDay', label: t('accountManagement.dailyLimit'), type: LowCodeFieldType.number, value: 0 }
    ]
  })
  if (!result.ok) return
  await notifyUntil(
    () => createSmtpSenderAccount({
      email: result.data.email,
      name: result.data.name,
      emailGroupId,
      maxSendCountPerDay: Number(result.data.maxSendCountPerDay),
      weight: 1,
      credential: {
        host: result.data.host,
        port: Number(result.data.port),
        loginName: result.data.loginName,
        password: result.data.password,
        connectionSecurity: toConnectionSecurity(result.data.connectionSecurity)
      }
    }),
    t('accountManagement.saving'),
    t('accountManagement.sender.createSmtp')
  )
  await onReload()
  notifySuccess(t('accountManagement.created'))
}

async function onCreateMicrosoftGraph(emailGroupId: number) {
  const result = await showMicrosoftApplicationDialog(t('accountManagement.sender.createGraph'), true)
  if (!result?.email) return
  const response = await createMicrosoftGraphSenderAccount({
    email: result.email,
    name: result.name,
    emailGroupId,
    maxSendCountPerDay: Number(result.maxSendCountPerDay ?? 0),
    weight: 1,
    application: toMicrosoftApplication(result)
  })
  await onReload()
  notifySuccess(t('accountManagement.created'))
  await onAuthorize(response.data)
}

const contextMenuItems = computed<IContextMenuItem<ISenderAccount>[]>(() => [
  { name: 'edit', label: t('accountManagement.edit'), icon: ContextMenuIcon.edit, when: 'onlySingle', onClick: onEdit },
  {
    name: 'credential',
    label: t('accountManagement.updateCredential'),
    icon: ContextMenuIcon.lockReset,
    when: 'onlySingle',
    onClick: onUpdateCredential
  },
  {
    name: 'authorize',
    label: t('accountManagement.authorize'),
    icon: ContextMenuIcon.adminPanelSettings,
    when: 'onlySingle',
    vif: (row) => row.protocol === SendingProtocol.MicrosoftGraph,
    onClick: onAuthorize
  },
  {
    name: 'validate',
    label: t('accountManagement.validate'),
    icon: ContextMenuIcon.verified,
    when: 'onlySingle',
    onClick: onValidate
  },
  {
    name: 'delete',
    label: t('accountManagement.delete'),
    icon: ContextMenuIcon.delete,
    color: 'negative',
    onClick: async (_row, context) => onDelete(context.targetValues, context.clearSelection)
  }
])

async function onEdit(senderAccount: ISenderAccount) {
  const result = await showDialog<{
    name?: string
    description?: string
    remark?: string
    maxSendCountPerDay: number
    replyToEmails?: string
    weight: number
  }>({
    title: t('accountManagement.sender.edit'),
    oneColumn: true,
    fields: [
      { name: 'name', label: t('accountManagement.name'), value: senderAccount.name },
      { name: 'description', label: t('accountManagement.description'), type: LowCodeFieldType.textarea, value: senderAccount.description },
      { name: 'remark', label: t('accountManagement.remark'), type: LowCodeFieldType.textarea, value: senderAccount.remark },
      { name: 'maxSendCountPerDay', label: t('accountManagement.dailyLimit'), type: LowCodeFieldType.number, value: senderAccount.maxSendCountPerDay },
      { name: 'replyToEmails', label: t('accountManagement.replyTo'), value: senderAccount.replyToEmails },
      { name: 'weight', label: t('accountManagement.weight'), type: LowCodeFieldType.number, value: senderAccount.weight }
    ]
  })
  if (!result.ok) return
  await updateSenderAccount(senderAccount.id, {
    ...result.data,
    emailGroupId: senderAccount.emailGroupId,
    maxSendCountPerDay: Number(result.data.maxSendCountPerDay),
    weight: Number(result.data.weight)
  })
  await onReload()
  notifySuccess(t('accountManagement.updated'))
}

async function onUpdateCredential(senderAccount: ISenderAccount) {
  if (senderAccount.protocol === SendingProtocol.MicrosoftGraph) {
    const result = await showMicrosoftApplicationDialog(t('accountManagement.updateApplication'), false)
    if (!result) return
    await updateMicrosoftGraphApplication(senderAccount.id, toMicrosoftApplication(result))
    await onReload()
    return
  }

  const result = await showDialog<ISmtpCredentialWrite>({
    title: t('accountManagement.updateCredential'),
    oneColumn: true,
    fields: [
      { name: 'host', label: t('accountManagement.host'), required: true },
      { name: 'port', label: t('accountManagement.port'), type: LowCodeFieldType.number, value: 465, required: true },
      { name: 'loginName', label: t('accountManagement.loginName'), required: true },
      { name: 'password', label: t('accountManagement.newPassword'), type: LowCodeFieldType.password },
      {
        name: 'connectionSecurity',
        label: t('accountManagement.connectionSecurity'),
        type: LowCodeFieldType.selectOne,
        value: ConnectionSecurity.SSL,
        options: connectionSecurityOptions,
        mapOptions: true,
        emitValue: true,
        required: true
      }
    ]
  })
  if (!result.ok) return
  await updateSmtpCredential(senderAccount.id, {
    ...result.data,
    port: Number(result.data.port),
    connectionSecurity: toConnectionSecurity(result.data.connectionSecurity)
  })
  await onReload()
}

async function onAuthorize(senderAccount: ISenderAccount) {
  const response = await startSenderMicrosoftAuthorization(senderAccount.id)
  window.open(response.data, '_blank', 'popup,width=720,height=760')
}

async function onValidate(senderAccount: ISenderAccount) {
  await notifyUntil(
    () => validateSenderAccount(senderAccount.id),
    t('accountManagement.validating'),
    t('accountManagement.validate')
  )
  await onReload()
}

async function onDelete(targetRows: readonly ISenderAccount[], clearSelection: () => void) {
  const confirmed = await confirmOperation(
    t('accountManagement.delete'),
    t('accountManagement.deleteConfirm', { count: targetRows.length })
  )
  if (!confirmed) return
  for (const row of targetRows) await deleteSenderAccount(row.id)
  clearSelection()
  await onReload()
}

const connectionSecurityOptions = [
  { label: 'None', value: ConnectionSecurity.None },
  { label: 'SSL', value: ConnectionSecurity.SSL },
  { label: 'TLS', value: ConnectionSecurity.TLS },
  { label: 'StartTLS', value: ConnectionSecurity.StartTLS }
]

interface IMicrosoftApplicationDialogResult {
  email?: string
  name?: string
  maxSendCountPerDay?: number
  applicationSource: number
  tenantId?: string
  clientId?: string
  clientSecret?: string
}

async function showMicrosoftApplicationDialog(title: string, includeIdentity: boolean) {
  const result = await showDialog<IMicrosoftApplicationDialogResult>({
    title,
    oneColumn: true,
    fields: [
      { name: 'email', label: t('accountManagement.email'), type: LowCodeFieldType.email, required: true, visible: includeIdentity },
      { name: 'name', label: t('accountManagement.name'), visible: includeIdentity },
      { name: 'maxSendCountPerDay', label: t('accountManagement.dailyLimit'), type: LowCodeFieldType.number, value: 0, visible: includeIdentity },
      {
        name: 'applicationSource',
        label: t('accountManagement.applicationSource'),
        type: LowCodeFieldType.selectOne,
        value: OAuthApplicationSource.System,
        options: [
          { label: t('accountManagement.systemApplication'), value: OAuthApplicationSource.System },
          { label: t('accountManagement.customApplication'), value: OAuthApplicationSource.Custom }
        ],
        mapOptions: true,
        emitValue: true,
        required: true
      },
      { name: 'tenantId', label: 'Tenant ID', visible: (values) => Number(values.applicationSource) === OAuthApplicationSource.Custom, required: true },
      { name: 'clientId', label: 'Client ID', visible: (values) => Number(values.applicationSource) === OAuthApplicationSource.Custom, required: true },
      { name: 'clientSecret', label: 'Client Secret', type: LowCodeFieldType.password, visible: (values) => Number(values.applicationSource) === OAuthApplicationSource.Custom }
    ]
  })
  return result.ok ? result.data : undefined
}

function toMicrosoftApplication(values: IMicrosoftApplicationDialogResult): IMicrosoftGraphApplicationWrite {
  return {
    applicationSource: toOAuthApplicationSource(values.applicationSource),
    tenantId: values.tenantId,
    clientId: values.clientId,
    clientSecret: values.clientSecret
  }
}

function toConnectionSecurity(value: number): ConnectionSecurityValue {
  return Number(value) as ConnectionSecurityValue
}

function toOAuthApplicationSource(value: number): OAuthApplicationSourceValue {
  return Number(value) as OAuthApplicationSourceValue
}
</script>

<style lang="scss" scoped></style>
