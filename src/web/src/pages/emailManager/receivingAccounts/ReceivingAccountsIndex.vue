<template>
  <q-table
    class="full-height full-width"
    :rows="rows"
    :columns="columns"
    :filter="filter"
    row-key="id"
    virtual-scroll
    dense
    :loading="isLoading"
  >
    <template #top-left>
      <q-fab
        v-model="isCreateMenuOpen"
        color="primary"
        icon="add"
        direction="right"
        :label="t('accountManagement.receiving.create')"
        padding="xs md"
      >
        <q-fab-action
          color="primary"
          icon="dns"
          :label="t('accountManagement.receiving.basic')"
          @click="onCreateBasicClick"
        />
        <q-fab-action
          color="secondary"
          icon="cloud"
          :label="t('accountManagement.receiving.microsoftGraph')"
          @click="onCreateMicrosoftGraphClick"
        />
        <q-fab-action
          color="grey-6"
          icon="key"
          :label="t('accountManagement.receiving.oauth')"
          disable
        >
          <q-tooltip>{{ t('accountManagement.receiving.oauthReserved') }}</q-tooltip>
        </q-fab-action>
      </q-fab>
    </template>
    <template #top-right>
      <SearchInput v-model="filter" />
    </template>
    <template #body-cell-index="props">
      <QTableIndex :props="props" />
    </template>
    <template #body-cell-email="props">
      <q-td :props="props">{{ props.value }}</q-td>
      <ContextMenu :items="contextMenuItems" :value="props.row" />
    </template>
    <template #body-cell-status="props">
      <q-td :props="props">
        <StatusChip :status="receivingStatusLabels[props.value as ReceivingAccountStatusValue]">
          <AsyncTooltip :cache="false" :tooltip="props.row.lastError" />
        </StatusChip>
      </q-td>
    </template>
  </q-table>
</template>

<script lang="ts" setup>
import type { QTableColumn } from 'quasar'
import { t } from 'src/i18n/helpers'
import {
  ConnectionSecurity,
  OAuthApplicationSource,
  ReceivingAccountStatus,
  ReceivingProtocol,
  type ConnectionSecurity as ConnectionSecurityValue,
  type OAuthApplicationSource as OAuthApplicationSourceValue,
  type ReceivingAccountStatus as ReceivingAccountStatusValue
} from 'src/api/accountEnums'
import {
  createBasicReceivingAccount,
  createMicrosoftGraphReceivingAccount,
  deleteReceivingAccount,
  getReceivingAccounts,
  startReceivingMicrosoftAuthorization,
  updateImapCredential,
  updateReceivingAccount,
  updateReceivingMicrosoftGraphApplication,
  validateReceivingAccount,
  type IImapCredentialWrite,
  type IReceivingAccount
} from 'src/api/receivingAccounts'
import {
  getSenderAccounts,
  type IMicrosoftGraphApplicationWrite,
  type ISenderAccount
} from 'src/api/senderAccounts'
import SearchInput from 'src/components/searchInput/SearchInput.vue'
import ContextMenu from 'src/components/contextMenu/ContextMenu.vue'
import StatusChip from 'src/components/statusChip/StatusChip.vue'
import AsyncTooltip from 'src/components/asyncTooltip/AsyncTooltip.vue'
import { ContextMenuIcon, type IContextMenuItem } from 'src/components/contextMenu/types'
import { useQTableIndex } from 'src/compositions/qTableUtils'
import { LowCodeFieldType, type ILowCodeField } from 'src/components/lowCode/types'
import { confirmOperation, notifySuccess, notifyUntil, showDialog } from 'src/utils/dialog'
import { formatDate } from 'src/utils/format'

const { indexColumn, QTableIndex } = useQTableIndex()
const rows = ref<IReceivingAccount[]>([])
const senderAccounts = ref<ISenderAccount[]>([])
const filter = ref('')
const isLoading = ref(false)
const isCreateMenuOpen = ref(false)

const receivingStatusLabels: Record<ReceivingAccountStatusValue, string> = {
  [ReceivingAccountStatus.Unverified]: 'unverified',
  [ReceivingAccountStatus.Active]: 'valid',
  [ReceivingAccountStatus.Paused]: 'unknown',
  [ReceivingAccountStatus.AuthenticationFailed]: 'invalid',
  [ReceivingAccountStatus.ConnectionFailed]: 'invalid'
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
    format: (value: number) => value === ReceivingProtocol.Imap ? 'IMAP' : 'Microsoft Graph'
  },
  {
    name: 'credential',
    label: t('accountManagement.credential'),
    align: 'left',
    field: (row: IReceivingAccount) => row.hasImapCredential || row.hasOAuthAuthorization,
    format: (value: boolean) => value ? t('accountManagement.configured') : t('accountManagement.notConfigured')
  },
  {
    name: 'contentRetentionDays',
    label: t('accountManagement.retentionDays'),
    align: 'right',
    field: 'contentRetentionDays'
  },
  {
    name: 'lastConnectedAtUtc',
    label: t('accountManagement.lastConnected'),
    align: 'left',
    field: 'lastConnectedAtUtc',
    format: (value?: string) => formatDate(value)
  },
  { name: 'status', label: t('accountManagement.status'), align: 'left', field: 'status' }
])

onMounted(async () => {
  await Promise.all([onReload(), loadSenderAccounts()])
})

async function onReload() {
  isLoading.value = true
  try {
    const response = await getReceivingAccounts()
    rows.value = response.data
  } finally {
    isLoading.value = false
  }
}

async function loadSenderAccounts() {
  const response = await getSenderAccounts()
  senderAccounts.value = response.data
}

async function onCreateBasicClick() {
  const result = await showDialog<IBasicReceivingForm>({
    title: t('accountManagement.receiving.createBasic'),
    oneColumn: true,
    fields: [...identityFields(), ...linkFields(), ...imapCredentialFields(true)]
  })
  if (!result.ok) return
  const response = await createBasicReceivingAccount({
    ...toReceivingBase(result.data),
    credential: toCredential(result.data)
  })
  rows.value.push(response.data)
  notifySuccess(t('accountManagement.created'))
}

async function onCreateMicrosoftGraphClick() {
  const result = await showDialog<IMicrosoftGraphReceivingForm>({
    title: t('accountManagement.receiving.createGraph'),
    oneColumn: true,
    fields: [...identityFields(), ...linkFields(), ...microsoftApplicationFields()]
  })
  if (!result.ok) return
  const response = await createMicrosoftGraphReceivingAccount({
    ...toReceivingBase(result.data),
    application: toMicrosoftApplication(result.data)
  })
  rows.value.push(response.data)
  notifySuccess(t('accountManagement.created'))
  await onAuthorize(response.data)
}

const contextMenuItems = computed<IContextMenuItem<IReceivingAccount>[]>(() => [
  { name: 'edit', label: t('accountManagement.edit'), icon: ContextMenuIcon.edit, onClick: onEdit },
  {
    name: 'credential',
    label: t('accountManagement.updateCredential'),
    icon: ContextMenuIcon.lockReset,
    onClick: onUpdateCredential
  },
  {
    name: 'authorize',
    label: t('accountManagement.authorize'),
    icon: ContextMenuIcon.adminPanelSettings,
    vif: (row) => row.protocol === ReceivingProtocol.MicrosoftGraph,
    onClick: onAuthorize
  },
  {
    name: 'validate',
    label: t('accountManagement.validate'),
    icon: ContextMenuIcon.verified,
    onClick: onValidate
  },
  {
    name: 'delete',
    label: t('accountManagement.delete'),
    icon: ContextMenuIcon.delete,
    color: 'negative',
    onClick: onDelete
  }
])

async function onEdit(account: IReceivingAccount) {
  const result = await showDialog<IReceivingMetadataForm>({
    title: t('accountManagement.receiving.edit'),
    oneColumn: true,
    fields: [
      { name: 'name', label: t('accountManagement.name'), value: account.name },
      {
        name: 'contentRetentionDays',
        label: t('accountManagement.retentionDays'),
        type: LowCodeFieldType.number,
        value: account.contentRetentionDays,
        required: true
      },
      ...linkFields(account)
    ]
  })
  if (!result.ok) return
  const response = await updateReceivingAccount(account.id, {
    name: result.data.name,
    contentRetentionDays: Number(result.data.contentRetentionDays),
    senderAccountIds: toNumberList(result.data.senderAccountIds),
    primarySenderAccountId: toOptionalNumber(result.data.primarySenderAccountId)
  })
  replaceRow(response.data)
  notifySuccess(t('accountManagement.updated'))
}

async function onUpdateCredential(account: IReceivingAccount) {
  if (account.protocol === ReceivingProtocol.MicrosoftGraph) {
    const result = await showDialog<IMicrosoftApplicationForm>({
      title: t('accountManagement.updateApplication'),
      oneColumn: true,
      fields: microsoftApplicationFields()
    })
    if (!result.ok) return
    const response = await updateReceivingMicrosoftGraphApplication(
      account.id,
      toMicrosoftApplication(result.data)
    )
    replaceRow(response.data)
    return
  }

  const result = await showDialog<IImapCredentialForm>({
    title: t('accountManagement.updateCredential'),
    oneColumn: true,
    fields: imapCredentialFields(false)
  })
  if (!result.ok) return
  const response = await updateImapCredential(account.id, toCredential(result.data))
  replaceRow(response.data)
}

async function onAuthorize(account: IReceivingAccount) {
  const response = await startReceivingMicrosoftAuthorization(account.emailAccountId)
  window.open(response.data, '_blank', 'popup,width=720,height=760')
}

async function onValidate(account: IReceivingAccount) {
  await notifyUntil(
    () => validateReceivingAccount(account.id),
    t('accountManagement.validating'),
    t('accountManagement.validate')
  )
  await onReload()
}

async function onDelete(account: IReceivingAccount) {
  const confirmed = await confirmOperation(
    t('accountManagement.delete'),
    t('accountManagement.deleteConfirm', { count: 1 })
  )
  if (!confirmed) return
  await deleteReceivingAccount(account.id)
  rows.value = rows.value.filter((row) => row.id !== account.id)
}

function identityFields(): ILowCodeField[] {
  return [
    { name: 'email', label: t('accountManagement.email'), type: LowCodeFieldType.email, required: true },
    { name: 'name', label: t('accountManagement.name') },
    { name: 'description', label: t('accountManagement.description'), type: LowCodeFieldType.textarea },
    { name: 'remark', label: t('accountManagement.remark') },
    {
      name: 'contentRetentionDays',
      label: t('accountManagement.retentionDays'),
      type: LowCodeFieldType.number,
      value: 30,
      required: true
    }
  ]
}

function linkFields(account?: IReceivingAccount): ILowCodeField[] {
  const options = senderAccounts.value.map((senderAccount) => ({
    label: senderAccount.name ? `${senderAccount.name} <${senderAccount.email}>` : senderAccount.email,
    value: senderAccount.id
  }))
  return [
    {
      name: 'senderAccountIds',
      label: t('accountManagement.receiving.linkedSenders'),
      type: LowCodeFieldType.selectMany,
      value: account?.senderAccountIds ?? [],
      options,
      mapOptions: true,
      emitValue: true
    },
    {
      name: 'primarySenderAccountId',
      label: t('accountManagement.receiving.primarySender'),
      type: LowCodeFieldType.selectOne,
      value: account?.primarySenderAccountId,
      options,
      mapOptions: true,
      emitValue: true
    }
  ]
}

function imapCredentialFields(isPasswordRequired: boolean): ILowCodeField[] {
  return [
    { name: 'host', label: t('accountManagement.host'), required: true },
    { name: 'port', label: t('accountManagement.port'), type: LowCodeFieldType.number, value: 993, required: true },
    { name: 'loginName', label: t('accountManagement.loginName'), required: true },
    {
      name: 'password',
      label: isPasswordRequired ? t('accountManagement.password') : t('accountManagement.newPassword'),
      type: LowCodeFieldType.password,
      required: isPasswordRequired
    },
    {
      name: 'connectionSecurity',
      label: t('accountManagement.connectionSecurity'),
      type: LowCodeFieldType.selectOne,
      value: ConnectionSecurity.SSL,
      options: [
        { label: 'None', value: ConnectionSecurity.None },
        { label: 'SSL', value: ConnectionSecurity.SSL },
        { label: 'TLS', value: ConnectionSecurity.TLS },
        { label: 'StartTLS', value: ConnectionSecurity.StartTLS }
      ],
      mapOptions: true,
      emitValue: true,
      required: true
    }
  ]
}

function microsoftApplicationFields(): ILowCodeField[] {
  return [
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
    {
      name: 'tenantId',
      label: 'Tenant ID',
      visible: (values) => Number(values.applicationSource) === OAuthApplicationSource.Custom,
      required: true
    },
    {
      name: 'clientId',
      label: 'Client ID',
      visible: (values) => Number(values.applicationSource) === OAuthApplicationSource.Custom,
      required: true
    },
    {
      name: 'clientSecret',
      label: 'Client Secret',
      type: LowCodeFieldType.password,
      visible: (values) => Number(values.applicationSource) === OAuthApplicationSource.Custom
    }
  ]
}

interface IReceivingIdentityForm {
  email: string
  name?: string
  description?: string
  remark?: string
  contentRetentionDays: number
  senderAccountIds: number[]
  primarySenderAccountId?: number
}

interface IImapCredentialForm {
  host: string
  port: number
  loginName: string
  password?: string
  connectionSecurity: number
}

interface IMicrosoftApplicationForm {
  applicationSource: number
  tenantId?: string
  clientId?: string
  clientSecret?: string
}

type IBasicReceivingForm = IReceivingIdentityForm & IImapCredentialForm
type IMicrosoftGraphReceivingForm = IReceivingIdentityForm & IMicrosoftApplicationForm
type IReceivingMetadataForm = Omit<IReceivingIdentityForm, 'email'>

function toReceivingBase(values: IReceivingIdentityForm) {
  return {
    email: values.email,
    name: values.name,
    description: values.description,
    remark: values.remark,
    contentRetentionDays: Number(values.contentRetentionDays),
    senderAccountIds: toNumberList(values.senderAccountIds),
    primarySenderAccountId: toOptionalNumber(values.primarySenderAccountId)
  }
}

function toCredential(values: IImapCredentialForm): IImapCredentialWrite {
  return {
    host: values.host,
    port: Number(values.port),
    loginName: values.loginName,
    password: values.password,
    connectionSecurity: Number(values.connectionSecurity) as ConnectionSecurityValue
  }
}

function toMicrosoftApplication(values: IMicrosoftApplicationForm): IMicrosoftGraphApplicationWrite {
  return {
    applicationSource: Number(values.applicationSource) as OAuthApplicationSourceValue,
    tenantId: values.tenantId,
    clientId: values.clientId,
    clientSecret: values.clientSecret
  }
}

function toNumberList(values: number[] | undefined): number[] {
  return (values ?? []).map(Number)
}

function toOptionalNumber(value: number | undefined): number | undefined {
  return value === undefined || value === null ? undefined : Number(value)
}

function replaceRow(updated: IReceivingAccount) {
  const index = rows.value.findIndex((row) => row.id === updated.id)
  if (index >= 0) rows.value.splice(index, 1, updated)
}
</script>

<style lang="scss" scoped></style>
