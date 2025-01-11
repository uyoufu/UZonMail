<template>
  <q-table class="full-height" :rows="rows" :columns="columns" row-key="id" virtual-scroll
    v-model:pagination="pagination" dense :loading="loading" :filter="filter" binary-state-sort
    @request="onTableRequest">
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
import { QTableColumn } from 'quasar'
import { useQTable, useQTableIndex } from 'src/compositions/qTableUtils'
import { IRequestPagination, TTableFilterObject } from 'src/compositions/types'
import SearchInput from 'src/components/searchInput/SearchInput.vue'
import { formatDate } from 'src/utils/format'

const { indexColumn, QTableIndex } = useQTableIndex()
const columns: QTableColumn[] = [
  indexColumn,
  {
    name: 'outboxEmail',
    required: true,
    label: '发件箱',
    align: 'left',
    field: 'outboxEmail',
    sortable: true
  },
  {
    name: 'inboxEmails',
    required: true,
    label: '收件箱',
    align: 'left',
    field: 'inboxEmails',
    sortable: true
  },
  {
    name: 'visitedCount',
    required: true,
    label: '打开次数',
    align: 'left',
    field: 'visitedCount',
    sortable: true
  },
  {
    name: 'firstVisitDate',
    required: true,
    label: '首次阅读',
    align: 'left',
    field: 'firstVisitDate',
    sortable: true,
    format: v => formatDate(v)
  },
  {
    name: 'lastVisitDate',
    required: true,
    label: '最近阅读',
    align: 'left',
    field: 'lastVisitDate',
    sortable: true,
    format: v => formatDate(v)
  }
]

import { getEmailAnchorsCount, getEmailAnchorsData } from 'src/api/pro/emailTracker'
// eslint-disable-next-line @typescript-eslint/no-unused-vars
async function getRowsNumberCount (filterObj: TTableFilterObject) {
  const { data } = await getEmailAnchorsCount(filterObj.filter)
  return data || 0
}
// eslint-disable-next-line @typescript-eslint/no-unused-vars
async function onRequest (filterObj: TTableFilterObject, pagination: IRequestPagination) {
  const { data } = await getEmailAnchorsData(filterObj.filter, pagination)
  return data || []
}

const { pagination, rows, filter, onTableRequest, loading } = useQTable({
  getRowsNumberCount,
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  onRequest
})
</script>

<style lang="scss" scoped></style>
