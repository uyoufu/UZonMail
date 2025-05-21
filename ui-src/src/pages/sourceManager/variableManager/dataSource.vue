<template>
  <q-table class="full-height" :rows="rows" :columns="columns" row-key="id" virtual-scroll
    v-model:pagination="pagination" dense :loading="loading" :filter="filter" binary-state-sort
    @request="onTableRequest">
    <template v-slot:top-left>
      <CreateBtn />
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
    label: '数据名',
    align: 'left',
    field: 'name',
    sortable: true
  },
  {
    name: 'remark',
    required: true,
    label: '备注',
    align: 'left',
    field: 'remark',
    sortable: true
  },
  {
    name: 'value',
    required: true,
    label: '数据值',
    align: 'left',
    field: 'value',
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
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  onRequest
})
</script>

<style lang="scss" scoped></style>
