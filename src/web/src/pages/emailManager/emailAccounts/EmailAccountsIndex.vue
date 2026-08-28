<template>
  <div class="full-height full-width row items-start no-wrap">
    <EmailGroupList
      ref="groupListRef"
      v-show="!isCollapseGroupList"
      v-model="selectedGroup"
      :group-category="EmailGroupCategory.EmailAccount"
      :context-menu-items="groupContextMenuItems"
      class="q-card q-mr-sm full-height"
      style="min-width: 188px"
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
        <div class="row items-center q-gutter-sm">
          <q-fab
            v-model="isCreateMenuOpen"
            color="primary"
            icon="add"
            direction="down"
            vertical-actions-align="left"
            padding="xs"
            :disable="!selectedGroup.id"
          >
            <q-fab-action
              square
              padding="xs"
              color="primary"
              icon="dns"
              :label="t('accountManagement.emailAccount.basic')"
              @click="onCreate(EmailAccountConfigurationKind.Basic)"
            />
            <q-fab-action
              square
              padding="xs"
              color="secondary"
              icon="cloud"
              label="Microsoft Graph"
              @click="onCreate(EmailAccountConfigurationKind.MicrosoftGraph)"
            />
            <q-tooltip v-if="!selectedGroup.id">{{ t('accountManagement.groupRequired') }}</q-tooltip>
          </q-fab>
          <ExportBtn label="" :tooltip="exportTemplateTooltip" @click="onExportTemplate" />
          <ImportBtn
            label=""
            :tooltip="importExcelTooltip"
            :disable="!selectedGroup.id"
            @click="onImportExcel"
          />
          <ImportBtn
            label=""
            icon="description"
            :tooltip="importTextTooltip"
            :disable="!selectedGroup.id"
            @click="onImportText"
          />
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
          <q-chip
            v-for="protocol in accountProtocols(tableCell.row)"
            :key="protocol"
            dense
            square
            color="blue-grey-1"
            text-color="blue-grey-9"
            >{{ protocol }}</q-chip
          >
        </q-td>
      </template>
      <template #body-cell-validation="tableCell">
        <q-td :props="tableCell" class="q-gutter-xs">
          <StatusChip
            v-if="tableCell.row.sender"
            :status="senderStatusLabel(tableCell.row.sender.status)"
            :status-styles="senderValidationStatusStyles(tableCell.row.sender.protocol, tableCell.row.sender.status)"
          >
            <AsyncTooltip :cache="false" :tooltip="tableCell.row.sender.validationFailureReason" />
          </StatusChip>
          <StatusChip
            v-if="tableCell.row.receiving"
            :status="receivingStatusLabel(tableCell.row.receiving.status)"
            :status-styles="
              receivingValidationStatusStyles(tableCell.row.receiving.protocol, tableCell.row.receiving.status)
            "
          >
            <AsyncTooltip :cache="false" :tooltip="tableCell.row.receiving.lastError" />
          </StatusChip>
        </q-td>
      </template>
      <template #body-cell-credential="tableCell">
        <q-td :props="tableCell">
          {{
            hasConfiguredCredential(tableCell.row)
              ? t('accountManagement.configured')
              : t('accountManagement.notConfigured')
          }}
        </q-td>
      </template>
    </q-table>

    <CollapseLeft v-model="isCollapseGroupList" :style="collapseStyleRef" />
  </div>
</template>

<script lang="ts" setup>
import type { QTable, QTableColumn } from 'quasar'
import { t, translateEmailGroup, translateGlobal } from 'src/i18n/helpers'
import { EmailGroupCategory, getEmailGroups } from 'src/api/emailGroup'
import { ReceivingProtocol, SendingProtocol } from 'src/api/accountEnums'
import {
  EmailAccountConfigurationKind,
  deleteEmailAccount,
  deleteInvalidSenderCapabilities,
  getEmailAccounts,
  moveEmailAccounts,
  startMicrosoftAuthorization,
  validateEmailAccounts,
  type IEmailAccount
} from 'src/api/emailAccounts'
import type { IEmailGroupListItem } from '../components/types'
import EmailGroupList from '../components/EmailGroupList.vue'
import EmailAccountDialog from './EmailAccountDialog.vue'
import SearchInput from 'src/components/searchInput/SearchInput.vue'
import ContextMenu from 'src/components/contextMenu/ContextMenu.vue'
import StatusChip from 'src/components/statusChip/StatusChip.vue'
import AsyncTooltip from 'src/components/asyncTooltip/AsyncTooltip.vue'
import ExportBtn from 'src/components/buttons/ExportBtn.vue'
import ImportBtn from 'src/components/buttons/ImportBtn.vue'
import { ContextMenuIcon, type IContextMenuItem } from 'src/components/contextMenu/types'
import { useQTableIndex } from 'src/compositions/qTableUtils'
import { useTableCollapseLeft } from 'src/components/collapseIcon/useCollapseLeft'
import { LowCodeFieldType } from 'src/components/lowCode/types'
import { showComponentDialog } from 'src/components/lowCode/PopupDialog'
import { confirmOperation, notifyError, notifyUntil, showDialog } from 'src/utils/dialog'
import {
  accountProtocols,
  accountValidationText,
  receivingStatusLabel,
  receivingValidationStatusStyles,
  senderStatusLabel,
  senderValidationStatusStyles
} from './emailAccountPresentation'
import { useEmailAccountImportExport } from './useEmailAccountImportExport'

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

const columns = computed<QTableColumn[]>(() => [
  indexColumn,
  { name: 'email', label: t('accountManagement.email'), align: 'left', field: 'email', sortable: true },
  { name: 'name', label: t('accountManagement.emailAccount.senderName'), align: 'left', field: 'name', sortable: true },
  { name: 'type', label: t('accountManagement.emailAccount.type'), align: 'left', field: accountProtocols },
  { name: 'credential', label: t('accountManagement.credential'), align: 'left', field: hasConfiguredCredential },
  {
    name: 'validation',
    label: t('accountManagement.emailAccount.validationResult'),
    align: 'left',
    field: accountValidationText
  },
  {
    name: 'sentTotalToday',
    label: t('accountManagement.sentToday'),
    align: 'right',
    field: (account) => account.sender?.sentTotalToday ?? '-'
  },
  {
    name: 'maxSendCountPerDay',
    label: t('accountManagement.dailyLimit'),
    align: 'right',
    field: (account) => account.sender?.maxSendCountPerDay ?? '-'
  }
])

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
    selectedRows.value = selectedRows.value.filter((selected) => data.some((account) => account.id === selected.id))
  } finally {
    isLoading.value = false
  }
}

async function onCreate(configurationKind: EmailAccountConfigurationKind) {
  if (!selectedGroup.value.id) return
  isCreateMenuOpen.value = false
  const result = await showComponentDialog<IEmailAccount>(EmailAccountDialog, {
    emailGroupId: selectedGroup.value.id,
    configurationKind
  })
  if (result.ok) await onAccountSaved()
}

async function onEdit(account: IEmailAccount) {
  const result = await showComponentDialog<IEmailAccount>(EmailAccountDialog, {
    emailGroupId: account.emailGroupId,
    configurationKind: accountConfigurationKind(account),
    account
  })
  if (result.ok) await onAccountSaved()
}

async function onAccountSaved() {
  await Promise.all([loadRows(), groupListRef.value?.reloadGroups()])
}

const currentGroupId = computed(() => selectedGroup.value.id)
const {
  exportTemplateTooltip,
  importExcelTooltip,
  importTextTooltip,
  exportGroupTooltip,
  onExportTemplate,
  onExportGroup,
  onImportExcel,
  onImportText
} = useEmailAccountImportExport(currentGroupId, onAccountSaved)

function accountConfigurationKind(account: IEmailAccount): EmailAccountConfigurationKind {
  return account.sender?.protocol === SendingProtocol.MicrosoftGraph ||
    account.receiving?.protocol === ReceivingProtocol.MicrosoftGraph
    ? EmailAccountConfigurationKind.MicrosoftGraph
    : EmailAccountConfigurationKind.Basic
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
    vif: (account) => accountConfigurationKind(account) === EmailAccountConfigurationKind.MicrosoftGraph,
    onClick: onAuthorize
  },
  {
    name: 'validate',
    label: t('accountManagement.validate'),
    icon: ContextMenuIcon.verified,
    onClick: (_account, context) => onValidate(context.targetValues)
  },
  {
    name: 'move',
    label: translateGlobal('move'),
    icon: ContextMenuIcon.driveFileMove,
    onClick: (_account, context) => onMove(context.targetValues)
  },
  {
    name: 'delete',
    label: t('accountManagement.delete'),
    icon: ContextMenuIcon.delete,
    color: 'negative',
    onClick: (_account, context) => onDelete(context.targetValues, context.clearSelection)
  }
])

const groupContextMenuItems = computed<IContextMenuItem<IEmailGroupListItem>[]>(() => [
  {
    name: 'importExcel',
    label: t('accountManagement.emailAccount.importExcel'),
    tooltip: importExcelTooltip.value,
    icon: ContextMenuIcon.uploadFile,
    onClick: (group) => onImportExcel(group.id)
  },
  {
    name: 'importText',
    label: t('accountManagement.emailAccount.importText'),
    tooltip: importTextTooltip.value,
    icon: ContextMenuIcon.uploadFile,
    onClick: (group) => onImportText(group.id)
  },
  {
    name: 'export',
    label: translateGlobal('export'),
    tooltip: exportGroupTooltip.value,
    icon: ContextMenuIcon.download,
    onClick: (group) => onExportGroup(group.id, group.name)
  },
  {
    name: 'deleteInvalid',
    label: t('accountManagement.emailAccount.deleteInvalidSenders'),
    icon: ContextMenuIcon.deleteSweep,
    color: 'negative',
    onClick: (group) => onDeleteInvalidSenders(group.id)
  }
])

async function onAuthorize(account: IEmailAccount) {
  const { data: authorizationUrl } = await startMicrosoftAuthorization(account.id)
  window.open(authorizationUrl, '_blank', 'popup,width=720,height=760')
}

async function onValidate(accounts: readonly IEmailAccount[]) {
  if (accounts.length === 0) return
  await notifyUntil(
    () => validateEmailAccounts(accounts.map((account) => account.id)),
    t('accountManagement.validating'),
    t('accountManagement.validate')
  )
  await loadRows()
}

async function onMove(accounts: readonly IEmailAccount[]) {
  if (accounts.length === 0) return
  const { data: groups } = await getEmailGroups(EmailGroupCategory.EmailAccount)
  const targetGroups = groups.filter((group) => group.id !== selectedGroup.value.id)
  if (targetGroups.length === 0) {
    notifyError(translateEmailGroup('noAvailableTargetGroup'))
    return
  }
  const result = await showDialog<{ targetEmailGroupId: number }>({
    title: translateGlobal('move'),
    oneColumn: true,
    fields: [
      {
        name: 'targetEmailGroupId',
        label: translateEmailGroup('selectTargetGroup'),
        type: LowCodeFieldType.selectOne,
        options: targetGroups.map((group) => ({ label: group.name, value: group.id })),
        mapOptions: true,
        emitValue: true,
        required: true
      }
    ]
  })
  if (!result.ok) return
  await moveEmailAccounts(
    accounts.map((account) => account.id),
    Number(result.data.targetEmailGroupId)
  )
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

</script>

<style lang="scss" scoped></style>
