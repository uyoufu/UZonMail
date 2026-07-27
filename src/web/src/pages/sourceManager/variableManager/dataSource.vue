<template>
  <q-table class="full-height" :rows="rows" :columns="columns" row-key="id" virtual-scroll
    v-model:pagination="pagination" selection="multiple" v-model:selected="selectedRows" dense :loading="loading"
    :filter="filter" binary-state-sort @request="onTableRequest">
    <template v-slot:top-left>
      <CreateBtn @click="onNewDataSource" />
    </template>

    <template v-slot:top-right>
      <SearchInput v-model="filter" />
    </template>

    <template v-slot:body-cell-index="props">
      <QTableIndex :props="props" />

      <ContextMenu v-model:selected-values="selectedRows" :items="dataSourceContextMenuItems" :value="props.row" />
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
import { t } from 'src/i18n/helpers'

const { indexColumn, QTableIndex } = useQTableIndex()
const columns = computed<QTableColumn[]>(() => [
  indexColumn,
  {
    name: 'name',
    required: true,
    label: t('pages.variableManager.dataName'),
    align: 'left',
    field: 'name',
    sortable: true
  },
  {
    name: 'description',
    required: true,
    label: t('pages.variableManager.description'),
    align: 'left',
    field: 'description',
    sortable: true
  },
  {
    name: 'value',
    required: true,
    label: t('pages.variableManager.dataValue'),
    align: 'left',
    field: 'value',
    format: (val) => JSON.stringify(val, null, 2) || '',
    sortable: true
  },
  {
    name: 'createDate',
    required: false,
    label: t('pages.variableManager.createDate'),
    align: 'left',
    field: 'createDate',
    format: formatDate, // format 需要的 value 是 string
    sortable: true
  }
])

import { getJsVariableSourcesCount, getJsVariableSourcesData } from 'src/api/pro/jsVariable'
import type { IJsVariableSource } from 'src/api/pro/jsVariable'


async function getRowsNumberCount (filterObj: TTableFilterObject) {
  const { data } = await getJsVariableSourcesCount(filterObj.filter)
  return data || 0
}

async function onRequest (filterObj: TTableFilterObject, pagination: IRequestPagination) {
  const { data } = await getJsVariableSourcesData(filterObj.filter, pagination)
  return data || []
}

const { pagination, rows, filter, onTableRequest, loading, selectedRows,
  addNewRow, deleteRowById } = useQTable<IJsVariableSource>({
    getRowsNumberCount,
    onRequest
  })

// #region 右键菜单
import ContextMenu from 'src/components/contextMenu/ContextMenu.vue'
import { useDataSourceContext } from './useDataSourceContext'
const { dataSourceContextMenuItems, onNewDataSource } = useDataSourceContext(addNewRow, deleteRowById)
// #endregion
</script>

<style lang="scss" scoped></style>
