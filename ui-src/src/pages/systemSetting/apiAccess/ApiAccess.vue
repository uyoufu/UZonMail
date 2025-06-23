<template>
  <q-table class="full-height" :rows="rows" :columns="columns" row-key="id" virtual-scroll
    v-model:pagination="pagination" dense :loading="loading" :filter="filter" binary-state-sort
    @request="onTableRequest">
    <template v-slot:top-left>
      <CreateBtn @click="onAddApiAccess" />
    </template>

    <template v-slot:top-right>
      <SearchInput v-model="filter" />
    </template>

    <template v-slot:body-cell-index="props">
      <QTableIndex :props="props" />
      <ContextMenu :items="contextMenuItems" :value="props.row" />
    </template>

    <template v-slot:body-cell-enable="props">
      <q-td :props="props">
        <StatusChip :status="props.value"></StatusChip>
      </q-td>
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

import ContextMenu from 'src/components/contextMenu/ContextMenu.vue'
import StatusChip from 'src/components/statusChip/StatusChip.vue'

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
    name: 'description',
    label: '描述',
    align: 'left',
    field: 'description',
    sortable: true
  },
  {
    name: 'expireDate',
    required: false,
    label: '有效期',
    align: 'left',
    field: 'expireDate',
    format: formatDate, // format 需要的 value 是 string
    sortable: true
  },
  {
    name: 'enable',
    label: '状态',
    align: 'left',
    field: 'enable',
    sortable: true
  },
]

import { getApiAccessCount, getApiAccessData } from 'src/api/pro/apiAccess'

async function getRowsNumberCount (filterObj: TTableFilterObject) {
  const { data } = await getApiAccessCount(filterObj.filter)
  return data || 0
}

async function onRequest (filterObj: TTableFilterObject, pagination: IRequestPagination) {
  const { data } = await getApiAccessData(filterObj.filter, pagination)
  return data || []
}

const { pagination, rows, filter, onTableRequest, loading, addNewRow, deleteRowById } = useQTable({
  getRowsNumberCount,
  onRequest
})

// #region 功能菜单
import { useApiAccessContext } from './compositions/useApiAccessContext'
const { onAddApiAccess, contextMenuItems } = useApiAccessContext(addNewRow, deleteRowById)
// #endregion
</script>

<style lang="scss" scoped></style>
