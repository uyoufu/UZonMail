<template>
  <q-table v-bind="$attrs" class="full-height" :rows="rows" :columns="columns" row-key="id" virtual-scroll
    v-model:pagination="pagination" dense :loading="loading" :filter="filter" binary-state-sort selection="multiple"
    @request="onTableRequest">
    <template v-slot:top-left>
      <div class="text-primary text-subtitle1">选择代理</div>
    </template>

    <template v-slot:top-right>
      <SearchInput v-model="filter" />
    </template>

    <template v-slot:body-cell-index="props">
      <QTableIndex :props="props" />
    </template>

    <template v-slot:body-cell-url="props">
      <q-td :props="props">
        <EllipsisContent :content="props.value" :max-length="40" />
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

import EllipsisContent from 'src/components/ellipsisContent/EllipsisContent.vue'

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
  }
]

import { getEnabledProxiesCount, getEnabledProxiesData } from 'src/api/proxy'

async function getRowsNumberCount (filterObj: TTableFilterObject) {
  const { data } = await getEnabledProxiesCount(filterObj.filter)
  return data || 0
}

async function onRequest (filterObj: TTableFilterObject, pagination: IRequestPagination) {
  const { data } = await getEnabledProxiesData(filterObj.filter, pagination)
  return data || []
}

const { pagination, rows, filter, onTableRequest, loading } = useQTable({
  getRowsNumberCount,

  onRequest
})
</script>

<style lang="scss" scoped></style>
