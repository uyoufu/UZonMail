<template>
  <q-table class="full-height" :rows="rows" :columns="columns" row-key="id" virtual-scroll
    v-model:pagination="pagination" dense :loading="loading" :filter="filter" binary-state-sort
    @request="onTableRequest">
    <template v-slot:top-left>
      <CommonBtn :label="t('pages.ipWarmUp.warmUp')" icon="autorenew" :tooltip="t('pages.ipWarmUp.addWarmUpPlan')" @click="onIpWarmUpClick" />
    </template>

    <template v-slot:top-right>
      <SearchInput v-model="filter" />
    </template>

    <template v-slot:body-cell-index="props">
      <QTableIndex :props="props" />
      <ContextMenu :items="ipWarmUpContextMenuItems" :value="props.row"></ContextMenu>
    </template>

    <template v-slot:body-cell-status="props">
      <q-td :props="props">
        <StatusChip :status="props.value">
        </StatusChip>
      </q-td>
    </template>
  </q-table>
</template>

<script lang="ts" setup>
import type { QTableColumn } from 'quasar'
import { useQTable, useQTableIndex } from 'src/compositions/qTableUtils'
import type { IRequestPagination, TTableFilterObject } from 'src/compositions/types'
import SearchInput from 'src/components/searchInput/SearchInput.vue'
import CommonBtn from 'src/components/quasarWrapper/buttons/CommonBtn.vue'
import StatusChip from 'src/components/statusChip/StatusChip.vue'
import { formatDate } from 'src/utils/format'
import { IpWarmUpUpStatus } from 'src/api/pro/ipWarmUp'
import { t } from 'src/i18n/helpers'

// 不进行缓存
defineOptions({
  name: 'IpWarmUpManager'
})

const { indexColumn, QTableIndex } = useQTableIndex()
const columns = computed<QTableColumn[]>(() => [
  indexColumn,
  {
    name: 'name',
    required: true,
    label: t('pages.ipWarmUp.name'),
    align: 'left',
    field: 'name',
    sortable: true
  },
  {
    name: 'subjects',
    required: true,
    label: t('pages.ipWarmUp.subjectCount'),
    align: 'left',
    field: 'subjects',
    format: (val: string[]) => String(val.length),
    sortable: true
  },
  {
    name: 'templateIds',
    required: true,
    label: t('pages.ipWarmUp.templateCount'),
    align: 'left',
    field: 'templateIds',
    format: (val: number[]) => String(val.length),
    sortable: true
  },
  {
    name: 'outboxIds',
    required: true,
    label: t('pages.ipWarmUp.outboxCount'),
    align: 'left',
    field: 'outboxIds',
    format: (val: number[]) => String(val.length),
    sortable: true
  },
  {
    name: 'inboxIds',
    required: true,
    label: t('pages.ipWarmUp.inboxCount'),
    align: 'left',
    field: 'inboxIds',
    format: (val: number[]) => String(val.length),
    sortable: true
  },
  {
    name: 'startDate',
    required: false,
    label: t('pages.ipWarmUp.startDate'),
    align: 'left',
    field: 'startDate',
    format: formatDate, // format 需要的 value 是 string
    sortable: true
  },
  {
    name: 'endDate',
    required: false,
    label: t('pages.ipWarmUp.endDate'),
    align: 'left',
    field: 'endDate',
    format: formatDate, // format 需要的 value 是 string
    sortable: true
  },
  {
    name: 'tasksCount',
    required: false,
    label: t('pages.ipWarmUp.totalSendRounds'),
    align: 'left',
    field: 'tasksCount',
    sortable: true
  },
  {
    name: 'status',
    required: false,
    label: t('pages.ipWarmUp.status'),
    align: 'left',
    field: 'status',
    format: v => IpWarmUpUpStatus[v] as string,
    sortable: true
  },
  {
    name: 'createDate',
    required: false,
    label: t('pages.ipWarmUp.createdAt'),
    align: 'left',
    field: 'createDate',
    format: formatDate, // format 需要的 value 是 string
    sortable: true
  }
])
import { getIpWarmUpPlanCount, getIpWarmUpPlanData } from 'src/api/pro/ipWarmUp'

async function getRowsNumberCount (filterObj: TTableFilterObject) {
  const { data } = await getIpWarmUpPlanCount(filterObj.filter)
  return data || 0
}

async function onRequest (filterObj: TTableFilterObject, pagination: IRequestPagination) {
  const { data } = await getIpWarmUpPlanData(filterObj.filter, pagination)
  return data || []
}

const { pagination, rows, filter, onTableRequest, loading, deleteRowById } = useQTable({
  getRowsNumberCount,
  onRequest
})

// #region 新增
import { useRouter } from 'vue-router'
const router = useRouter()
async function onIpWarmUpClick () {
  await router.push({
    name: 'SendingTask', query: {
      type: 'ipWarmUp',
      tagName: t('pages.ipWarmUp.warmUp')
    }
  })
}
// #endregion

// #region 右键菜单
import { useIpWarmIpContext } from './compositions/useIpWarmIpContext'
const { ipWarmUpContextMenuItems } = useIpWarmIpContext(deleteRowById)
// #endregion
</script>

<style lang="scss" scoped></style>
