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
import { t } from 'src/i18n/helpers'

const { indexColumn, QTableIndex } = useQTableIndex()
const columns = computed<QTableColumn[]>(() => [
  indexColumn,
  {
    name: 'outboxEmail',
    required: true,
    label: t('statisticsReport.outbox'),
    align: 'left',
    field: 'outboxEmail',
    sortable: true
  },
  {
    name: 'inboxEmails',
    required: true,
    label: t('statisticsReport.inbox'),
    align: 'left',
    field: 'inboxEmails',
    sortable: true
  },
  {
    name: 'visitedCount',
    required: true,
    label: t('statisticsReport.openCount'),
    align: 'left',
    field: 'visitedCount',
    sortable: true
  },
  {
    name: 'firstVisitDate',
    required: true,
    label: t('statisticsReport.firstRead'),
    align: 'left',
    field: 'firstVisitDate',
    sortable: true,
    format: v => formatDate(v)
  },
  {
    name: 'lastVisitDate',
    required: true,
    label: t('statisticsReport.lastRead'),
    align: 'left',
    field: 'lastVisitDate',
    sortable: true,
    format: v => formatDate(v)
  }
])

import { getEmailAnchorsCount, getEmailAnchorsData } from 'src/api/pro/emailTracker'

async function getRowsNumberCount (filterObj: TTableFilterObject) {
  const { data } = await getEmailAnchorsCount(filterObj.filter)
  return data || 0
}

async function onRequest (filterObj: TTableFilterObject, pagination: IRequestPagination) {
  const { data } = await getEmailAnchorsData(filterObj.filter, pagination)
  return data || []
}

const { pagination, rows, filter, onTableRequest, loading } = useQTable({
  getRowsNumberCount,

  onRequest
})
</script>

<style lang="scss" scoped></style>
