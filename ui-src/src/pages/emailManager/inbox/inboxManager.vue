<template>
  <div class="full-height full-width row items-start">
    <EmailGroupList v-show="!isCollapseGroupList" v-model="emailGroupRef" :groupType="2"
      class="q-card q-mr-sm full-height" style="min-width: 160px" :contextMenuItems="groupCtxMenuItems" />

    <q-table ref="inboxTableRef" class="col full-height" :rows="rows" :columns="columns" row-key="id" virtual-scroll
      v-model:pagination="pagination" dense :loading="loading" :filter="filter" binary-state-sort
      @request="onTableRequest">
      <template v-slot:top-left>
        <div class="row justify-start q-gutter-sm">
          <CreateBtn tooltip="新增收件箱" @click="onNewInboxClick" :disable="!isValidEmailGroup"
            tooltip-when-disabled="请先添加组" />
          <ExportBtn label="" tooltip="导出收件箱模板" @click="onExportInboxTemplateClick" />
          <ImportBtn label="" tooltip="导入收件箱" @click="onImportInboxClick()" :disable="!isValidEmailGroup"
            tooltip-when-disabled="请先添加组" />
          <ImportBtn label="" icon="description" :tooltip="importFromTxtTooltip" @click="onImportInboxFromTxt()"
            :disable="!isValidEmailGroup" tooltip-when-disabled="请先添加组" />
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
        <ContextMenu :items="inboxContextMenuItems" :value="props.row" />
      </template>
    </q-table>

    <CollapseLeft v-model="isCollapseGroupList" :style="collapseStyleRef" />
  </div>
</template>

<script lang="ts" setup>
import type { QTableColumn } from 'quasar'
import { QTable } from 'quasar'
import { formatDate } from 'src/utils/format'

import SearchInput from 'src/components/searchInput/SearchInput.vue'
import CreateBtn from 'src/components/quasarWrapper/buttons/CreateBtn.vue'
import ImportBtn from 'src/components/quasarWrapper/buttons/ImportBtn.vue'
import ExportBtn from 'src/components/quasarWrapper/buttons/ExportBtn.vue'
import EmailGroupList from '../components/EmailGroupList.vue'
import ContextMenu from 'components/contextMenu/ContextMenu.vue'

import { useQTable, useQTableIndex } from 'src/compositions/qTableUtils'
import type { IRequestPagination, TTableFilterObject } from 'src/compositions/types'
import { getInboxesCount, getInboxesData } from 'src/api/emailBox'
import type { IEmailGroupListItem } from '../components/types'

// 左侧分组开关
// 左侧分组开关
import { useTableCollapseLeft } from 'src/components/collapseLeft/useCollapseLeft'
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

const columns: QTableColumn[] = [
  indexColumn,
  {
    name: 'email',
    label: '邮箱',
    align: 'left',
    field: 'email',
    sortable: true
  },
  {
    name: 'name',
    label: '名称(收件人姓名)',
    align: 'left',
    field: 'name',
    sortable: true
  },
  {
    name: 'description',
    label: '描述',
    align: 'left',
    field: 'description',
    sortable: true
  },
  {
    name: 'minInboxCooldownHours',
    label: '最小收件间隔(h)',
    align: 'left',
    field: 'minInboxCooldownHours',
    sortable: true
  },
  {
    name: 'lastSuccessDeliveryDate',
    label: '最近发件日期',
    align: 'left',
    field: 'lastSuccessDeliveryDate',
    format: (v) => formatDate(v),
    sortable: true
  }
]
async function getRowsNumberCount (filterObj: TTableFilterObject) {
  const { data } = await getInboxesCount(emailGroupRef.value.id, filterObj.filter)
  return data
}
async function onRequest (filterObj: TTableFilterObject, pagination: IRequestPagination) {
  const { data } = await getInboxesData(emailGroupRef.value.id, filterObj.filter, pagination)
  return data
}
const { pagination, rows, filter, onTableRequest, loading, refreshTable, addNewRow, deleteRowById } = useQTable({
  getRowsNumberCount,

  onRequest
})
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
const { inboxContextMenuItems } = useContextMenu(deleteRowById)
// #endregion

// #region 分组的右键菜单
import type { IContextMenuItem } from 'src/components/contextMenu/types'
import { notifyError } from 'src/utils/dialog'
const groupCtxMenuItems: Ref<IContextMenuItem[]> = ref([
  {
    name: 'import',
    label: '导入',
    tooltip: '向当前组中导入收件箱',
    onClick: (value) => onImportInboxClick(value.id)
  },
  {
    name: 'export',
    label: '导出',
    tooltip: '导出当前组中的收件箱',
    onClick: exportAllInboxesInThisGroup
  }
])

// 导出当前组中的所有的收件箱
import { writeExcel } from 'src/utils/file'
// eslint-disable-next-line @typescript-eslint/no-explicit-any
async function exportAllInboxesInThisGroup (group: Record<string, any>) {
  // 获取所有的收件箱
  const { data: count } = await getInboxesCount(group.id, '')
  if (!count) {
    notifyError('没有可导出项')
    return
  }
  const { data: dataRows } = await getInboxesData(group.id, '', {
    sortBy: 'id',
    descending: false,
    skip: 0,
    limit: count
  })
  await writeExcel(dataRows, {
    fileName: `${group.name}-收件箱.xlsx`,
    sheetName: group.name,
    mappers: getInboxExcelDataMapper(),
    strict: true
  })
}
// #endregion

// #region 从文本导入邮箱
import { useInboxImporter } from './useInboxImporter'
// eslint-disable-next-line @typescript-eslint/no-unused-vars
const { onImportInboxFromTxt, importFromTxtLable, importFromTxtTooltip } = useInboxImporter(emailGroupRef, addNewRow)
// #endregion
</script>

<style lang="scss" scoped></style>
