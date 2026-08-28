<template>
  <q-dialog ref="dialogRef" persistent @hide="onDialogHide">
    <q-card class="column no-wrap q-pa-sm height-0 select-account-dialog">
      <div class="col row items-start">
        <EmailGroupList
          v-show="!isCollapseGroupList"
          v-model="selectedGroup"
          v-model:selected="selectedGroups"
          :extra-items="categoryTopItems"
          :group-category="accountCategory"
          readonly
          selectable
          class="q-mr-sm card-like full-height"
        />

        <q-table
          ref="accountTableRef"
          v-model:pagination="pagination"
          v-model:selected="selectedAccounts"
          class="col full-height"
          :rows="rows"
          :columns="columns"
          row-key="selectionKey"
          virtual-scroll
          dense
          :loading="loading"
          :filter="filter"
          binary-state-sort
          selection="multiple"
          @request="onTableRequest"
        >
          <template #top-left>
            <CreateBtn
              v-if="shouldShowTemporaryRecipientButton"
              :tooltip="t('pages.sendingTask.newTemporaryRecipient')"
              @click="onNewTemporaryRecipientClick"
            />
          </template>
          <template #top-right><SearchInput v-model="filter" /></template>
          <template #body-cell-index="tableCell"><QTableIndex :props="tableCell" /></template>
        </q-table>

        <CollapseLeft v-model="isCollapseGroupList" :style="collapseStyleRef" />
      </div>

      <div class="row justify-end items-center q-mt-md">
        <CancelBtn class="q-mr-sm" @click="onDialogCancel" />
        <OkBtn @click="onConfirmClick" />
      </div>
    </q-card>
  </q-dialog>
</template>

<script lang="ts" setup>
import type { QTableColumn } from 'quasar'
import { QTable, useDialogPluginComponent } from 'quasar'
import { EmailGroupCategory, type EmailGroupCategory as EmailGroupCategoryValue } from 'src/api/emailGroup'
import type { IRecipientContactSelection, ISenderAccountSelection } from 'src/api/emailSending'
import { getRecipientContacts } from 'src/api/recipientContacts'
import { getSenderEmailAccountOptions } from 'src/api/emailAccounts'
import { useTableCollapseLeft } from 'src/components/collapseIcon/useCollapseLeft'
import CancelBtn from 'src/components/buttons/CancelBtn.vue'
import OkBtn from 'src/components/buttons/OkBtn.vue'
import type { IRequestPagination, TTableFilterObject } from 'src/compositions/types'
import { useQTable, useQTableIndex } from 'src/compositions/qTableUtils'
import { t } from 'src/i18n/helpers'
import EmailGroupList from 'src/pages/emailManager/components/EmailGroupList.vue'
import type { IEmailGroupListItem } from 'src/pages/emailManager/components/types'
import { notifyError } from 'src/utils/dialog'
import { LowCodeFieldType, type IPopupDialogParams } from 'src/components/lowCode/types'
import { showDialog } from 'src/components/lowCode/PopupDialog'

type AccountSelection = ISenderAccountSelection | IRecipientContactSelection
type SelectableAccount = AccountSelection & { selectionKey: string }

const props = defineProps({
  accountCategory: {
    type: Number as PropType<EmailGroupCategoryValue>,
    default: EmailGroupCategory.EmailAccount
  },
  initialAccounts: { type: Array as PropType<AccountSelection[]>, default: () => [] },
  initialGroups: { type: Array as PropType<IEmailGroupListItem[]>, default: () => [] }
})

defineEmits([...useDialogPluginComponent.emits])
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent()

const selectedGroup = ref<IEmailGroupListItem>({
  name: 'selected', label: t('pages.sendingTask.selected'), order: 0
})
const categoryTopItems = ref<IEmailGroupListItem[]>([{
  name: 'selected', label: t('pages.sendingTask.selectedMailboxes'), order: -1,
  icon: 'task_alt', selectable: false
}])
const selectedAccounts = ref<SelectableAccount[]>(props.initialAccounts.map(toSelectableAccount))
const selectedGroups = ref<IEmailGroupListItem[]>([...props.initialGroups])
const shouldShowTemporaryRecipientButton = computed(() =>
  props.accountCategory === EmailGroupCategory.RecipientEmail && selectedGroup.value.name === 'selected'
)

const { indexColumn, QTableIndex } = useQTableIndex()
const columns = computed<QTableColumn[]>(() => [
  indexColumn,
  { name: 'email', label: t('pages.sendingTask.mailbox'), align: 'left', field: 'email', sortable: true },
  { name: 'description', label: t('global.description'), align: 'left', field: 'description', sortable: true }
])

let lastLoadedAccounts: SelectableAccount[] = []
async function loadAccounts (filterText: string): Promise<SelectableAccount[]> {
  if (selectedGroup.value.name === 'selected') {
    const normalizedFilter = filterText.trim().toLowerCase()
    return selectedAccounts.value.filter(account => !normalizedFilter || account.email.toLowerCase().includes(normalizedFilter))
  }

  const groupId = selectedGroup.value.id
  const response = props.accountCategory === EmailGroupCategory.EmailAccount
    ? await getSenderEmailAccountOptions()
    : await getRecipientContacts(groupId, filterText)
  return response.data.map(toSelectableAccount)
}

async function getRowsNumberCount (filterObject: TTableFilterObject) {
  lastLoadedAccounts = await loadAccounts(filterObject.filter || '')
  return lastLoadedAccounts.length
}

function onRequest (_filterObject: TTableFilterObject, requestPagination: IRequestPagination) {
  return Promise.resolve(lastLoadedAccounts.slice(requestPagination.skip, requestPagination.skip + requestPagination.limit))
}

const { pagination, rows, filter, onTableRequest, loading, refreshTable, addNewRow } = useQTable<SelectableAccount>({
  getRowsNumberCount,
  onRequest
})
watch(selectedGroup, refreshTable)

function toSelectableAccount (account: AccountSelection): SelectableAccount {
  return { ...account, selectionKey: account.id ? `persisted-${account.id}` : `temporary-${account.email.toLowerCase()}` }
}

async function onNewTemporaryRecipientClick () {
  const dialogParams: IPopupDialogParams = {
    title: t('pages.sendingTask.temporaryRecipient'),
    oneColumn: true,
    fields: [
      { name: 'email', label: t('pages.sendingTask.mailbox'), type: LowCodeFieldType.text, required: true },
      { name: 'name', label: t('global.name'), type: LowCodeFieldType.text }
    ]
  }
  const result = await showDialog<{ email: string, name?: string }>(dialogParams)
  if (!result.ok) return

  const email = result.data.email.trim()
  if (!email.includes('@')) {
    notifyError(t('pages.sendingTask.duplicateMailbox'))
    return
  }
  if (selectedAccounts.value.some(account => account.email.toLowerCase() === email.toLowerCase())) {
    notifyError(t('pages.sendingTask.duplicateMailbox'))
    return
  }

  const temporaryRecipient = toSelectableAccount({ email, name: result.data.name?.trim() })
  selectedAccounts.value.push(temporaryRecipient)
  addNewRow(temporaryRecipient, 'selectionKey')
}

function onConfirmClick () {
  onDialogOK({
    selectedAccounts: selectedAccounts.value.map(toAccountSelection),
    selectedGroups: selectedGroups.value
  })
}

function toAccountSelection (account: SelectableAccount): AccountSelection {
  return {
    id: account.id,
    email: account.email,
    name: account.name,
    description: account.description
  }
}

const accountTableRef = ref<InstanceType<typeof QTable>>()
const { CollapseLeft, collapseStyleRef, isCollapseGroupList } = useTableCollapseLeft(accountTableRef, 14)
isCollapseGroupList.value = false
</script>

<style lang="scss" scoped>
.select-account-dialog {
  min-width: min(800px, 100%);
  min-height: min(600px, 100%);
}
</style>
