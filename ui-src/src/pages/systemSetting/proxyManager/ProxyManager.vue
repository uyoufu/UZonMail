<template>
  <q-table class="full-height" :rows="rows" :columns="columns" row-key="id" virtual-scroll
    v-model:pagination="pagination" dense :loading="loading" :filter="filter" binary-state-sort
    @request="onTableRequest">
    <template v-slot:top-left>
      <CreateBtn @click="onCreateProxy" />
    </template>

    <template v-slot:top-right>
      <SearchInput v-model="filter" />
    </template>

    <template v-slot:body-cell-index="props">
      <QTableIndex :props="props" />
      <ContextMenu :items="proxyContextMenuItems" :value="props.row" />
    </template>

    <template v-slot:body-cell-isShared="props">
      <q-td :props="props">
        <q-toggle :disable="disableShareToggle(props.row)" color="secondary" dense v-model="props.row.isShared"
          @click="onToggleShareProxy(props.row)">
          <q-tooltip>
            {{ getProxyShareTooltip(props.row) }}
          </q-tooltip>
        </q-toggle>
      </q-td>
    </template>
  </q-table>
</template>

<script lang="ts" setup>
import { QTableColumn } from 'quasar'
import { useQTable, useQTableIndex } from 'src/compositions/qTableUtils'
import { IRequestPagination, TTableFilterObject } from 'src/compositions/types'
import SearchInput from 'src/components/searchInput/SearchInput.vue'

// #region 表格定义
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
    name: 'url',
    label: '代理',
    align: 'left',
    field: 'url',
    sortable: true
  },
  {
    name: 'matchRegex',
    label: '匹配规则',
    align: 'left',
    field: 'matchRegex',
    sortable: true
  },
  {
    name: 'priority',
    label: '优先级',
    align: 'left',
    field: 'priority',
    sortable: true
  },
  {
    name: 'isActive',
    label: '启用',
    align: 'left',
    field: 'isActive',
    sortable: true,
    format: v => v ? '是' : '否'
  }
]

import { useUserInfoStore } from 'src/stores/user'
const userInfoStore = useUserInfoStore()
if (userInfoStore.isAdmin) {
  columns.push({
    name: 'isShared',
    label: '共享',
    align: 'left',
    field: 'isShared',
    sortable: true
  })
}

import { getProxiesCount, getProxiesData } from 'src/api/proxy'
// eslint-disable-next-line @typescript-eslint/no-unused-vars
async function getRowsNumberCount (filterObj: TTableFilterObject) {
  const { data } = await getProxiesCount(filterObj.filter)
  return data || 0
}
// eslint-disable-next-line @typescript-eslint/no-unused-vars
async function onRequest (filterObj: TTableFilterObject, pagination: IRequestPagination) {
  const { data } = await getProxiesData(filterObj.filter, pagination)
  return data || []
}

const { pagination, rows, filter, onTableRequest, loading, addNewRow, deleteRowById } = useQTable({
  getRowsNumberCount,
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  onRequest
})
// #endregion

// #region 代理的新建、编辑、删除
import { useHeaderFunctions } from './headerFuncs'
const { onCreateProxy, onToggleShareProxy } = useHeaderFunctions(addNewRow)

import ContextMenu from 'src/components/contextMenu/ContextMenu.vue'
import { useContextMenu } from './contextMenu'
const { proxyContextMenuItems } = useContextMenu(deleteRowById)
// #endregion

// #region toggle 控制
import { useShareToggle } from './compositions/useShareToggle'
const { disableShareToggle, getProxyShareTooltip } = useShareToggle()
// #endregion
</script>

<style lang="scss" scoped></style>
