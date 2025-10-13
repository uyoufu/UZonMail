<template>
  <q-table class="full-height" :rows="rows" :columns="columns" row-key="id" virtual-scroll
    v-model:pagination="pagination" dense :loading="loading" :filter="filter" binary-state-sort
    @request="onTableRequest">
    <template v-slot:top-left>
      <div class="text-primary">IP 预热计划</div>
    </template>

    <template v-slot:top-right>
      <SearchInput v-model="filter" />
    </template>

    <template v-slot:body-cell-index="props">
      <QTableIndex :props="props" />
    </template>

    <template v-slot:body-cell-userId="props">
      <q-td :props="props">
        {{ props.value }}
      </q-td>
    </template>
  </q-table>
</template>

<script lang="ts" setup>
import type { QTableColumn } from 'quasar'
import { useQTable, useQTableIndex } from 'src/compositions/qTableUtils'
import type { IRequestPagination, TTableFilterObject } from 'src/compositions/types'
import SearchInput from 'src/components/searchInput/SearchInput.vue'
import { formatDate } from 'src/utils/format'

const { indexColumn, QTableIndex } = useQTableIndex()
const columns: QTableColumn[] = [
  indexColumn,
  {
    name: 'name',
    required: true,
    label: '名称',
    align: 'left',
    field: 'name',
    sortable: true
  },
  {
    name: 'subjectsCount',
    required: true,
    label: '主题数',
    align: 'left',
    field: 'subjectsCount',
    sortable: true
  },
  {
    name: 'templatesCount',
    required: true,
    label: '模板数',
    align: 'left',
    field: 'templatesCount',
    sortable: true
  },
  {
    name: 'outboxesCount',
    required: true,
    label: '发件箱数',
    align: 'left',
    field: 'outboxesCount',
    sortable: true
  },
  {
    name: 'inboxesCount',
    required: true,
    label: '收件箱数',
    align: 'left',
    field: 'inboxesCount',
    sortable: true
  },
  {
    name: 'startDate',
    required: false,
    label: '开始日期',
    align: 'left',
    field: 'startDate',
    format: formatDate, // format 需要的 value 是 string
    sortable: true
  },
  {
    name: 'endDate',
    required: false,
    label: '截止日期',
    align: 'left',
    field: 'endDate',
    format: formatDate, // format 需要的 value 是 string
    sortable: true
  },
  {
    name: 'repeatRounds',
    required: false,
    label: '发件总轮数',
    align: 'left',
    field: 'repeatRounds',
    format: formatDate, // format 需要的 value 是 string
    sortable: true
  },
  {
    name: 'status',
    required: false,
    label: '状态',
    align: 'left',
    field: 'status',
    format: formatDate, // format 需要的 value 是 string
    sortable: true
  },
  {
    name: 'createDate',
    required: false,
    label: '创建日期',
    align: 'left',
    field: 'createDate',
    format: formatDate, // format 需要的 value 是 string
    sortable: true
  }
]
// eslint-disable-next-line @typescript-eslint/no-unused-vars
async function getRowsNumberCount (filterObj: TTableFilterObject) {
  return 0
}
// eslint-disable-next-line @typescript-eslint/no-unused-vars
async function onRequest (filterObj: TTableFilterObject, pagination: IRequestPagination) {
  return []
}

const { pagination, rows, filter, onTableRequest, loading } = useQTable({
  getRowsNumberCount,

  onRequest
})
</script>

<style lang="scss" scoped></style>
