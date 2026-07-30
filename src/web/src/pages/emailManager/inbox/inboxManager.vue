<template>
  <div class="full-height full-width row items-start">
    <EmailGroupList
      ref="emailGroupListRef"
      v-show="!isCollapseGroupList"
      v-model="emailGroupRef"
      :groupType="2"
      class="q-card q-mr-sm full-height"
      style="min-width: 160px"
      :contextMenuItems="groupCtxMenuItems"
    />

    <q-table
      ref="inboxTableRef"
      class="col full-height"
      :rows="rows"
      :columns="columns"
      row-key="id"
      virtual-scroll
      selection="multiple"
      v-model:selected="selectedInboxes"
      v-model:pagination="pagination"
      dense
      :loading="loading"
      :filter="filter"
      binary-state-sort
      @request="onTableRequest"
    >
      <template v-slot:top-left>
        <div class="row justify-start q-gutter-sm">
          <CreateBtn
            :tooltip="translateInboxManager('newInbox')"
            @click="onNewInboxClick"
            :disable="!isValidEmailGroup"
            :tooltip-when-disabled="translateInboxManager('addGroupFirst')"
          />
          <ExportBtn
            label=""
            :tooltip="translateInboxManager('exportInboxTemplate')"
            @click="onExportInboxTemplateClick"
          />
          <ImportBtn
            label=""
            :tooltip="translateInboxManager('importInbox')"
            @click="onImportInboxClick()"
            :disable="!isValidEmailGroup"
            :tooltip-when-disabled="translateInboxManager('addGroupFirst')"
          />
          <ImportBtn
            label=""
            icon="description"
            :tooltip="importFromTxtTooltip"
            @click="onImportInboxFromTxt()"
            :disable="!isValidEmailGroup"
            :tooltip-when-disabled="translateInboxManager('addGroupFirst')"
          />
          <ImportBtn
            label=""
            icon="block"
            :tooltip="translateInboxManager('importInvalidInboxes')"
            @click="onImportInvalidInboxes"
            :disable="!isValidEmailGroup"
            :tooltip-when-disabled="translateInboxManager('addGroupFirst')"
          />
        </div>
      </template>

      <template v-slot:top-right>
        <SearchInput v-model="filter" />
      </template>

      <template v-slot:body-cell-index="props">
        <QTableIndex :props="props" />
      </template>

      <template v-slot:body-cell-email="props">
        <q-td :props="props">
          {{ props.value }}
        </q-td>
        <ContextMenu :items="inboxContextMenuItems" :value="props.row" v-model:selected-values="selectedInboxes" />
      </template>

      <template v-slot:body-cell-status="props">
        <q-td :props="props">
          <StatusChip :status="props.value">
            <AsyncTooltip :cache="false" :tooltip="props.row.validFailReason" />
          </StatusChip>
        </q-td>
      </template>
    </q-table>

    <CollapseLeft v-model="isCollapseGroupList" :style="collapseStyleRef" />
  </div>
</template>

<script lang="ts" setup>
import { translateGlobal, translateInboxManager } from 'src/i18n/helpers'

import type { QTableColumn } from 'quasar'
import { QTable } from 'quasar'
import { formatDate } from 'src/utils/format'

import SearchInput from 'src/components/searchInput/SearchInput.vue'
import CreateBtn from 'src/components/quasarWrapper/buttons/CreateBtn.vue'
import ImportBtn from 'src/components/quasarWrapper/buttons/ImportBtn.vue'
import ExportBtn from 'src/components/quasarWrapper/buttons/ExportBtn.vue'
import EmailGroupList from '../components/EmailGroupList.vue'
import ContextMenu from 'components/contextMenu/ContextMenu.vue'
import StatusChip from 'src/components/statusChip/StatusChip.vue'
import AsyncTooltip from 'src/components/asyncTooltip/AsyncTooltip.vue'

import { useQTable, useQTableIndex } from 'src/compositions/qTableUtils'
import type { IRequestPagination, TTableFilterObject } from 'src/compositions/types'
import { getInboxesCount, getInboxesData, InboxStatus } from 'src/api/emailBox'
import type { IInbox } from 'src/api/emailBox'
import type { IEmailGroupListItem } from '../components/types'

// 左侧分组开关
import { useTableCollapseLeft } from 'src/components/collapseIcon/useCollapseLeft'
const inboxTableRef = ref<InstanceType<typeof QTable> | undefined>()
const { CollapseLeft, collapseStyleRef, isCollapseGroupList } = useTableCollapseLeft(inboxTableRef)

const { indexColumn, QTableIndex } = useQTableIndex()
// 菜单项
const emailGroupRef: Ref<IEmailGroupListItem> = ref({
  id: 0,
  name: 'all',
  label: '',
  order: 0
})
const isValidEmailGroup = computed(() => emailGroupRef.value.id)
const selectedInboxes = ref<IInbox[]>([])
const emailGroupListRef = ref<{ reloadGroups: () => Promise<void> }>()
const inboxStatusChipValues = {
  [InboxStatus.Unverified]: 'unverified',
  [InboxStatus.Invalid]: 'invalid',
  [InboxStatus.Unknown]: 'unknown',
  [InboxStatus.Valid]: 'valid'
} as const satisfies Record<InboxStatus, string>

const columns: ComputedRef<QTableColumn[]> = computed(() => [
  indexColumn,
  {
    name: 'email',
    label: translateInboxManager('col_email'),
    align: 'left',
    field: 'email',
    sortable: true
  },
  {
    name: 'name',
    label: translateInboxManager('col_inboxName'),
    align: 'left',
    field: 'name',
    sortable: true
  },
  {
    name: 'description',
    label: translateInboxManager('col_description'),
    align: 'left',
    field: 'description',
    sortable: true
  },
  {
    name: 'minInboxCooldownHours',
    label: translateInboxManager('col_minInboxCooldownHours'),
    align: 'left',
    field: 'minInboxCooldownHours',
    sortable: true
  },
  {
    name: 'lastSuccessDeliveryDate',
    label: translateInboxManager('col_lastSuccessDeliveryDate'),
    align: 'left',
    field: 'lastSuccessDeliveryDate',
    format: (v: string | undefined) => formatDate(v),
    sortable: true
  },
  {
    name: 'status',
    label: translateInboxManager('col_status'),
    align: 'left',
    field: (inbox: IInbox) => inbox.status ?? InboxStatus.Unverified,
    format: (status: InboxStatus) => inboxStatusChipValues[status],
    sortable: true
  }
])
async function getRowsNumberCount(filterObj: TTableFilterObject) {
  const { data } = await getInboxesCount(emailGroupRef.value.id, filterObj.filter)
  return data
}
async function onRequest(filterObj: TTableFilterObject, pagination: IRequestPagination) {
  const { data } = await getInboxesData(emailGroupRef.value.id, filterObj.filter, pagination)
  return data
}
const { pagination, rows, filter, onTableRequest, loading, refreshTable, addNewRow, deleteRowById } = useQTable<IInbox>(
  {
    getRowsNumberCount,
    onRequest
  }
)
watch(emailGroupRef, () => {
  // 组切换时，触发更新
  refreshTable()
})

// #region 表头功能
import { useHeaderFunction, getInboxExcelDataMapper } from './headerFunctions'
const { onNewInboxClick, onExportInboxTemplateClick, onImportInboxClick } = useHeaderFunction(emailGroupRef, addNewRow)
// #endregion

// #region 数据右键菜单
import { useContextMenu } from './contextMenu'
const { inboxContextMenuItems } = useContextMenu(deleteRowById, refreshTable, selectedInboxes)
// #endregion

// #region 分组的右键菜单
import { ContextMenuIcon, type IContextMenuItem } from 'src/components/contextMenu/types'
import { notifyError, notifySuccess, notifyUntil } from 'src/utils/dialog'
import { validateInboxGroup } from 'src/api/pro/emailVerify'
import { usePermission } from 'src/compositions/permission'
const { isProfession } = usePermission()
const groupCtxMenuItems: Ref<IContextMenuItem<IEmailGroupListItem>[]> = ref([
  {
    name: 'validate',
    label: translateGlobal('validate'),
    tooltip: translateInboxManager('validateAllInvalidInboxesInCurrentGroup'),
    icon: ContextMenuIcon.verified,
    vif: () => isProfession.value,
    onClick: onValidateInboxGroup
  },
  {
    name: 'import',
    label: translateInboxManager('ctx_import'),
    tooltip: translateInboxManager('ctx_importInboxToCurrentGroup'),
    icon: ContextMenuIcon.uploadFile,
    onClick: (value) => onImportInboxClick(value.id)
  },
  {
    name: 'export',
    label: translateInboxManager('ctx_export'),
    tooltip: translateInboxManager('ctx_exportInboxToCurrentGroup'),
    icon: ContextMenuIcon.download,
    onClick: exportAllInboxesInThisGroup
  }
])

/** 验证当前分类中的全部收件箱。 */
async function onValidateInboxGroup(group: IEmailGroupListItem) {
  const groupId = group.id
  if (groupId === undefined) return

  const result = await notifyUntil(
    () => validateInboxGroup(groupId),
    translateInboxManager('validateAllInvalidInboxesInCurrentGroup'),
    translateGlobal('validate')
  )
  if (!result) return

  await emailGroupListRef.value?.reloadGroups()
  refreshTable()
  notifySuccess(translateGlobal('updateSuccess'))
}

// 导出当前组中的所有的收件箱
import { writeExcel } from 'src/utils/file'
async function exportAllInboxesInThisGroup(group: IEmailGroupListItem) {
  // 获取所有的收件箱
  const { data: count } = await getInboxesCount(group.id, '')
  if (!count) {
    notifyError(translateInboxManager('noInboxesInThisGroupForExporting'))
    return
  }
  const { data: dataRows } = await getInboxesData(group.id, '', {
    sortBy: 'id',
    descending: false,
    skip: 0,
    limit: count
  })
  await writeExcel(dataRows, {
    fileName: `${group.name}-${translateInboxManager('inbox')}.xlsx`,
    sheetName: group.name,
    mappers: getInboxExcelDataMapper(),
    strict: true
  })
}
// #endregion

// #region 从文本导入邮箱
import { useInboxImporter } from './useInboxImporter'
// eslint-disable-next-line @typescript-eslint/no-unused-vars
const { onImportInboxFromTxt, onImportInvalidInboxes, importFromTxtLable, importFromTxtTooltip } = useInboxImporter(
  emailGroupRef,
  addNewRow
)
// #endregion
</script>

<style lang="scss" scoped></style>
