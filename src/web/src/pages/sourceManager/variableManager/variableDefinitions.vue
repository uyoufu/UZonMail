<template>
  <q-table class="full-height" :rows="rows" :columns="columns" row-key="id" virtual-scroll
    v-model:pagination="pagination" selection="multiple" v-model:selected="selectedRows" dense :loading="loading"
    :filter="filter" binary-state-sort @request="onTableRequest">
    <template v-slot:top-left>
      <CreateBtn @click="onNewVariableDefinition" />
    </template>

    <template v-slot:top-right>
      <SearchInput v-model="filter" />
    </template>

    <template v-slot:body-cell-index="props">
      <QTableIndex :props="props" />

      <ContextMenu :items="dataSourceContextMenuItems" :value="props.row" />
    </template>

    <template v-slot:body-cell-value="props">
      <q-td :props="props">
        <EllipsisContent :content="props.value" :max-length="40" />
      </q-td>
    </template>
  </q-table>
</template>

<script lang="ts" setup>
import type { QTableColumn } from 'quasar'
import { useQTable, useQTableIndex } from 'src/compositions/qTableUtils'
import type { IRequestPagination, TTableFilterObject } from 'src/compositions/types'
import SearchInput from 'src/components/searchInput/SearchInput.vue'
import EllipsisContent from 'src/components/ellipsisContent/EllipsisContent.vue'

import { formatDate } from 'src/utils/format'

const { indexColumn, QTableIndex } = useQTableIndex()
const columns: QTableColumn[] = [
  indexColumn,
  {
    name: 'name',
    required: true,
    label: '变量名',
    align: 'left',
    field: 'name',
    sortable: true
  },
  {
    name: 'description',
    required: true,
    label: '描述',
    align: 'left',
    field: 'description',
    sortable: true
  },
  {
    name: 'functionBody',
    required: true,
    label: '表达式',
    align: 'left',
    field: 'functionBody',
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

import { getJsFunctionDefinitionsCount, getJsFunctionDefinitionsData } from 'src/api/pro/jsFunctionDefinition'


async function getRowsNumberCount (filterObj: TTableFilterObject) {
  const { data } = await getJsFunctionDefinitionsCount(filterObj.filter)
  return data || 0
}

async function onRequest (filterObj: TTableFilterObject, pagination: IRequestPagination) {
  const { data } = await getJsFunctionDefinitionsData(filterObj.filter, pagination)
  return data || []
}

const { pagination, rows, filter, onTableRequest, loading, selectedRows,
  addNewRow, getSelectedRows, deleteRowById } = useQTable({
    getRowsNumberCount,
    onRequest
  })

// #region 右键菜单
import ContextMenu from 'src/components/contextMenu/ContextMenu.vue'
import { useVariableDefinitionContext } from './useVariableDefinitionContext'
const { dataSourceContextMenuItems, onNewVariableDefinition } = useVariableDefinitionContext(addNewRow, getSelectedRows, deleteRowById)
// #endregion
</script>

<style lang="scss" scoped></style>
