<template>
  <div class="full-height full-width row items-start no-wrap">
    <EmailGroupList
      v-show="!isCollapseGroupList"
      v-model="selectedGroup"
      :group-category="EmailGroupCategory.Recipient"
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
        <div class="row q-gutter-sm">
          <CreateBtn
            :tooltip="t('accountManagement.recipient.create')"
            :disable="!selectedGroup.id"
            :tooltip-when-disabled="t('accountManagement.groupRequired')"
            @click="onCreateClick"
          />
          <ImportBtn
            label=""
            :tooltip="t('accountManagement.recipient.import')"
            :disable="!selectedGroup.id"
            :tooltip-when-disabled="t('accountManagement.groupRequired')"
            @click="onImportClick"
          />
        </div>
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
      <template #body-cell-validationStatus="props">
        <q-td :props="props">
          <StatusChip :status="validationStatusLabels[props.value as RecipientValidationStatusValue]">
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
  RecipientValidationStatus,
  type RecipientValidationStatus as RecipientValidationStatusValue
} from 'src/api/accountEnums'
import {
  createRecipientContact,
  createRecipientContacts,
  deleteRecipientContact,
  getRecipientContacts,
  updateRecipientContact,
  updateRecipientValidationStatus,
  type IRecipientContact,
  type IRecipientContactWrite
} from 'src/api/recipientContacts'
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
import { confirmOperation, notifySuccess, showDialog } from 'src/utils/dialog'

const tableRef = ref<InstanceType<typeof QTable>>()
const { CollapseLeft, collapseStyleRef, isCollapseGroupList } = useTableCollapseLeft(tableRef)
const { indexColumn, QTableIndex } = useQTableIndex()
const selectedGroup = ref<IEmailGroupListItem>({ id: 0, name: '', label: '', order: 0 })
const rows = ref<IRecipientContact[]>([])
const selectedRows = ref<IRecipientContact[]>([])
const filter = ref('')
const isLoading = ref(false)

const validationStatusLabels: Record<RecipientValidationStatusValue, string> = {
  [RecipientValidationStatus.Unverified]: 'unverified',
  [RecipientValidationStatus.Invalid]: 'invalid',
  [RecipientValidationStatus.Unknown]: 'unknown',
  [RecipientValidationStatus.Valid]: 'valid'
}

const columns = computed<QTableColumn[]>(() => [
  indexColumn,
  { name: 'email', label: t('accountManagement.email'), align: 'left', field: 'email', sortable: true },
  { name: 'name', label: t('accountManagement.name'), align: 'left', field: 'name', sortable: true },
  { name: 'description', label: t('accountManagement.description'), align: 'left', field: 'description' },
  {
    name: 'minimumCooldownHours',
    label: t('accountManagement.cooldownHours'),
    align: 'right',
    field: 'minimumCooldownHours',
    sortable: true
  },
  {
    name: 'validationStatus',
    label: t('accountManagement.status'),
    align: 'left',
    field: 'validationStatus',
    sortable: true
  }
])

watch(() => selectedGroup.value.id, onReload, { immediate: true })

async function onReload() {
  if (!selectedGroup.value.id) {
    rows.value = []
    return
  }
  isLoading.value = true
  try {
    const response = await getRecipientContacts(selectedGroup.value.id)
    rows.value = response.data
  } finally {
    isLoading.value = false
  }
}

function getWriteFields(contact?: IRecipientContact) {
  return [
    {
      name: 'email',
      label: t('accountManagement.email'),
      type: LowCodeFieldType.email,
      value: contact?.email,
      required: true
    },
    { name: 'name', label: t('accountManagement.name'), value: contact?.name },
    {
      name: 'description',
      label: t('accountManagement.description'),
      type: LowCodeFieldType.textarea,
      value: contact?.description
    },
    { name: 'remark', label: t('accountManagement.remark'), value: contact?.remark },
    {
      name: 'minimumCooldownHours',
      label: t('accountManagement.cooldownHours'),
      type: LowCodeFieldType.number,
      value: contact?.minimumCooldownHours ?? -1
    }
  ]
}

async function onCreateClick() {
  if (!selectedGroup.value.id) return
  const result = await showDialog<IRecipientContactWrite>({
    title: t('accountManagement.recipient.create'),
    oneColumn: true,
    fields: getWriteFields()
  })
  if (!result.ok) return
  await createRecipientContact(toWriteRequest(result.data, selectedGroup.value.id))
  await onReload()
  notifySuccess(t('accountManagement.created'))
}

async function onImportClick() {
  if (!selectedGroup.value.id) return
  const result = await showDialog<{ addresses: string }>({
    title: t('accountManagement.recipient.import'),
    oneColumn: true,
    fields: [{
      name: 'addresses',
      label: t('accountManagement.recipient.addressList'),
      type: LowCodeFieldType.textarea,
      required: true
    }]
  })
  if (!result.ok) return

  const uniqueEmails = [...new Set(
    result.data.addresses
      .split(/[\r\n,;]+/)
      .map((email) => email.trim().toLowerCase())
      .filter(Boolean)
  )]
  if (uniqueEmails.length === 0) return
  await createRecipientContacts(uniqueEmails.map((email) => ({
    email,
    emailGroupId: selectedGroup.value.id!,
    minimumCooldownHours: -1
  })))
  await onReload()
  notifySuccess(t('accountManagement.recipient.imported', { count: uniqueEmails.length }))
}

const contextMenuItems = computed<IContextMenuItem<IRecipientContact>[]>(() => [
  { name: 'edit', label: t('accountManagement.edit'), icon: ContextMenuIcon.edit, when: 'onlySingle', onClick: onEdit },
  {
    name: 'markValid',
    label: t('accountManagement.recipient.markValid'),
    icon: ContextMenuIcon.checkCircle,
    onClick: async (_row, context) => onSetValidationStatus(context.targetValues, RecipientValidationStatus.Valid, context.clearSelection)
  },
  {
    name: 'markInvalid',
    label: t('accountManagement.recipient.markInvalid'),
    icon: ContextMenuIcon.block,
    color: 'negative',
    onClick: async (_row, context) => onSetValidationStatus(context.targetValues, RecipientValidationStatus.Invalid, context.clearSelection)
  },
  {
    name: 'delete',
    label: t('accountManagement.delete'),
    icon: ContextMenuIcon.delete,
    color: 'negative',
    onClick: async (_row, context) => onDelete(context.targetValues, context.clearSelection)
  }
])

async function onEdit(contact: IRecipientContact) {
  const result = await showDialog<IRecipientContactWrite>({
    title: t('accountManagement.recipient.edit'),
    oneColumn: true,
    fields: getWriteFields(contact)
  })
  if (!result.ok) return
  await updateRecipientContact(contact.id, toWriteRequest(result.data, contact.emailGroupId))
  await onReload()
  notifySuccess(t('accountManagement.updated'))
}

async function onSetValidationStatus(
  contacts: readonly IRecipientContact[],
  validationStatus: RecipientValidationStatusValue,
  clearSelection: () => void
) {
  await updateRecipientValidationStatus(contacts.map((contact) => contact.id), validationStatus)
  clearSelection()
  await onReload()
  notifySuccess(t('accountManagement.updated'))
}

async function onDelete(contacts: readonly IRecipientContact[], clearSelection: () => void) {
  const confirmed = await confirmOperation(
    t('accountManagement.delete'),
    t('accountManagement.deleteConfirm', { count: contacts.length })
  )
  if (!confirmed) return
  for (const contact of contacts) await deleteRecipientContact(contact.id)
  clearSelection()
  await onReload()
}

function toWriteRequest(values: IRecipientContactWrite, emailGroupId: number): IRecipientContactWrite {
  return {
    ...values,
    emailGroupId,
    minimumCooldownHours: Number(values.minimumCooldownHours)
  }
}
</script>

<style lang="scss" scoped></style>
