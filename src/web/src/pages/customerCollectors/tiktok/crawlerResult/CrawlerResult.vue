<template>
  <q-table class="full-height" :rows="rows" :columns="columns" row-key="id" virtual-scroll
    v-model:pagination="pagination" dense :loading="loading" :filter="filter" binary-state-sort
    @request="onTableRequest">
    <!-- <template v-slot:top-left>
      <CreateBtn />
    </template> -->

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
    name: 'nickname',
    label: '昵称',
    align: 'left',
    field: 'nickname',
    sortable: true
  },
  {
    name: 'email',
    required: true,
    label: '邮箱',
    align: 'left',
    field: 'email',
    sortable: true
  },
  {
    name: 'createDate',
    required: false,
    label: '获取日期',
    align: 'left',
    field: 'createDate',
    format: formatDate,
    sortable: true
  }
]

import { useRoute } from 'vue-router'
import { getCrawlerTaskResultsCount, getCrawlerTaskResultsData } from 'src/api/pro/crawlerTask'
// 从路由中获取id
const crawlerTaskId = ref(0)
const route = useRoute()
onMounted(() => {
  if (!route.params.id) return
  crawlerTaskId.value = Number(route.params.id)
  // 触发更新
  refreshTable()
})


async function getRowsNumberCount (filterObj: TTableFilterObject) {
  if (crawlerTaskId.value <= 0) return 0
  const { data } = await getCrawlerTaskResultsCount(crawlerTaskId.value, filterObj.filter)
  return data
}

async function onRequest (filterObj: TTableFilterObject, pagination: IRequestPagination) {
  const { data } = await getCrawlerTaskResultsData(crawlerTaskId.value, filterObj.filter, pagination)
  return data || []
}

const { pagination, rows, filter, onTableRequest, loading, refreshTable } = useQTable({
  getRowsNumberCount,

  onRequest
})
</script>

<style lang="scss" scoped></style>
