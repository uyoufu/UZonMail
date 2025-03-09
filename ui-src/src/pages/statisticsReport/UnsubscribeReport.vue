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
import type { QTableColumn } from 'quasar'
import { useQTable, useQTableIndex } from 'src/compositions/qTableUtils'
import type { IRequestPagination, TTableFilterObject } from 'src/compositions/types'
import SearchInput from 'src/components/searchInput/SearchInput.vue'
import { formatDate } from 'src/utils/format'

const { indexColumn, QTableIndex } = useQTableIndex()
const columns: QTableColumn[] = [
  indexColumn,
  {
    name: 'email',
    required: true,
    label: '退订邮箱',
    align: 'left',
    field: 'email',
    sortable: true
  },
  {
    name: 'host',
    required: false,
    label: '退订时所在 IP',
    align: 'left',
    field: 'host',
    sortable: true
  },
  {
    name: 'createDate',
    required: true,
    label: '日期',
    align: 'left',
    field: 'createDate',
    sortable: true,
    format: v => formatDate(v)
  }
]

import { getUnsubscribesCount, getUnsubscribesData } from 'src/api/pro/unsubscribe'

async function getRowsNumberCount (filterObj: TTableFilterObject) {
  const { data } = await getUnsubscribesCount(filterObj.filter)
  return data || 0
}

async function onRequest (filterObj: TTableFilterObject, pagination: IRequestPagination) {
  const { data } = await getUnsubscribesData(filterObj.filter, pagination)
  return data || []
}

const { pagination, rows, filter, onTableRequest, loading } = useQTable({
  getRowsNumberCount,

  onRequest
})
</script>

<style lang="scss" scoped></style>
