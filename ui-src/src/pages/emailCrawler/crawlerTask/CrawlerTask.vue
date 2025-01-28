<template>
  <q-table class="full-height" :rows="rows" :columns="columns" row-key="id" virtual-scroll
    v-model:pagination="pagination" dense :loading="loading" :filter="filter" binary-state-sort
    @request="onTableRequest">
    <template v-slot:top-left>
      <CreateBtn @click="onCreateCrawlerTask" />
    </template>

    <template v-slot:top-right>
      <SearchInput v-model="filter" />
    </template>

    <template v-slot:body-cell-index="props">
      <QTableIndex :props="props" />
      <ContextMenu :items="contextMenuItems" :value="props.row" />
    </template>

    <template v-slot:body-cell-status="props">
      <q-td :props="props">
        <StatusChip :status="props.value">
        </StatusChip>
      </q-td>
    </template>
  </q-table>
</template>

<script lang="ts" setup>
import { QTableColumn } from 'quasar'
import { useQTable, useQTableIndex } from 'src/compositions/qTableUtils'
import { IRequestPagination, TTableFilterObject } from 'src/compositions/types'
import { useColumnsFormater } from './compositions/useColumnsFormater'

import SearchInput from 'src/components/searchInput/SearchInput.vue'
import StatusChip from 'src/components/statusChip/StatusChip.vue'

import { formatDate } from 'src/utils/format'

const { formatProxyId, formatCrawlerType, formatCrawlerStatus, formatDeviceId: formatTikTokDeviceId } = useColumnsFormater()
const { indexColumn, QTableIndex } = useQTableIndex()
const columns: QTableColumn[] = [
  indexColumn,
  {
    name: 'name',
    label: '名称',
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
    name: 'type',
    required: true,
    label: '爬虫类型',
    align: 'left',
    field: 'type',
    format: formatCrawlerType,
    sortable: true
  },
  {
    name: 'tikTokDeviceId',
    required: true,
    label: '设备',
    align: 'left',
    field: 'tikTokDeviceId',
    format: formatTikTokDeviceId,
    sortable: true
  },
  {
    name: 'proxyId',
    label: '代理',
    align: 'left',
    field: 'proxyId',
    format: formatProxyId,
    sortable: true
  },
  {
    name: 'deadline',
    required: false,
    label: '截止日期',
    align: 'left',
    field: 'deadline',
    format: formatDate,
    sortable: true
  },
  {
    name: 'status',
    required: true,
    label: '状态',
    align: 'left',
    field: 'status',
    format: formatCrawlerStatus,
    sortable: true
  },
  {
    name: 'count',
    required: true,
    label: '数量',
    align: 'left',
    field: 'count',
    sortable: true
  },
  {
    name: 'startDate',
    required: false,
    label: '开始日期',
    align: 'left',
    field: 'startDate',
    format: formatDate,
    sortable: true
  },
  {
    name: 'endDate',
    required: false,
    label: '结束日期',
    align: 'left',
    field: 'endDate',
    format: formatDate,
    sortable: true
  },
  {
    name: 'createDate',
    required: false,
    label: '创建日期',
    align: 'left',
    field: 'createDate',
    format: formatDate,
    sortable: true
  }
]

import { getCrawlerTaskInfosCount, getCrawlerTaskInfosData } from 'src/api/pro/crawlerTask'

// eslint-disable-next-line @typescript-eslint/no-unused-vars
async function getRowsNumberCount (filterObj: TTableFilterObject) {
  const { data } = await getCrawlerTaskInfosCount(filterObj.filter)
  return data || 0
}
// eslint-disable-next-line @typescript-eslint/no-unused-vars
async function onRequest (filterObj: TTableFilterObject, pagination: IRequestPagination) {
  const { data } = await getCrawlerTaskInfosData(filterObj.filter, pagination)
  return data || []
}

const { pagination, rows, filter, onTableRequest, loading, addNewRow, deleteRowById } = useQTable({
  getRowsNumberCount,
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  onRequest
})

// #region 头部相关的方法
import { useHeaderFunctions } from './compositions/useHeaderFunctions'
const { onCreateCrawlerTask } = useHeaderFunctions(addNewRow)
// #endregion

// #region 右键菜单
import { useContextMenu } from './compositions/useContextMenu'
const { contextMenuItems } = useContextMenu(addNewRow, deleteRowById)
// #endregion

// #region 自动更新正在运行任务的数量
import { useCountUpdator } from './compositions/useCountUpdator'
useCountUpdator(rows)
// #endregion
</script>

<style lang="scss" scoped></style>
