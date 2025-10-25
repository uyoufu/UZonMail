<template>
  <q-table class="full-height full-width" :rows="rows" :columns="columns" row-key="id" virtual-scroll
    v-model:pagination="pagination" dense :loading="loading" :filter="filter" binary-state-sort
    @request="onTableRequest">
    <template v-slot:top-left>
      <div class="row justify-start items-center q-gutter-sm">
        <q-btn dense icon="west" class="q-mr-sm" flat size="sm" @click="goBackToSendHistory">
          <q-tooltip>
            返回到历史发件
          </q-tooltip>
        </q-btn>

        <q-tabs class="border-radius-6" v-model="statusTab" dense active-color="secondary" indicator-color="primary"
          align="left">
          <q-tab class="q-px-xs border-radius-6" v-for="statusTabOption in statusTabOptions"
            :key="statusTabOption.value" :name="statusTabOption.value" :label="statusTabOption.label" />
        </q-tabs>

        <!-- <div class="text-subtitle1">发件明细</div> -->
        <ExportBtn flat tooltip="导出当前数据" @click="onExportCurrentSendingItems"></ExportBtn>
      </div>
    </template>

    <template v-slot:top-right>
      <LinearProgress class="q-mr-sm" v-if="showProgressBar" :value="sendingGroupProgressValue" :width="160">
      </LinearProgress>

      <SearchInput v-model="filter" />
    </template>

    <template v-slot:body-cell-index="props">
      <QTableIndex :props="props" />
      <ContextMenu :items="sendDetailContextItems" :value="props.row"></ContextMenu>
    </template>

    <template v-slot:body-cell-status="props">
      <q-td :props="props">
        <q-circular-progress v-if="props.value === 1" indeterminate size="sm" :thickness="0.4" color="secondary"
          track-color="grey-3" center-color="primary" />
        <StatusChip :status="props.value">
          <AsyncTooltip v-if="props.value !== SendingItemStatus[SendingItemStatus.Success]"
            :tooltip="props.row.sendResult"></AsyncTooltip>
        </StatusChip>
      </q-td>
    </template>
  </q-table>
</template>

<script lang="ts" setup>
import { getSendingItemsCount, getSendingItemsData, SendingItemStatus } from 'src/api/sendingItem'

// 定义 props
const vueProps = defineProps({
  sendingGroupId: {
    type: Number,
    required: false
  }
})
const statusTab = ref(-1)
const statusTabOptions = [
  { label: '全部', value: -1 },
  { label: '发送中', value: SendingItemStatus.Pending },
  { label: '未发送', value: SendingItemStatus.Created },
  { label: '成功', value: SendingItemStatus.Success },
  { label: '失败', value: SendingItemStatus.Failed }
]
watch(statusTab, () => {
  refreshTable()
})

const sendingGroupId = ref(vueProps.sendingGroupId)

import type { QTableColumn } from 'quasar'
import { formatDate } from 'src/utils/format'

import { useQTable, useQTableIndex } from 'src/compositions/qTableUtils'
import type { IRequestPagination, TTableFilterObject } from 'src/compositions/types'
import SearchInput from 'src/components/searchInput/SearchInput.vue'

const { indexColumn, QTableIndex } = useQTableIndex()
const columns: QTableColumn[] = [
  indexColumn,
  {
    name: 'subject',
    required: true,
    label: '主题',
    align: 'left',
    field: 'subject',
    sortable: true
  },
  {
    name: 'fromEmail',
    required: true,
    label: '发件箱',
    align: 'left',
    field: 'fromEmail',
    sortable: true
  },
  {
    name: 'inboxes',
    required: true,
    label: '收件箱',
    align: 'left',
    field: 'inboxes',
    sortable: true,
    format: v => {
      return v.map((e: { email: string }) => e.email).join(';')
    }
  },
  {
    name: 'status',
    required: true,
    label: '状态',
    align: 'left',
    field: 'status',
    sortable: true,
    format: v => SendingItemStatus[v]
  },
  {
    name: 'sendDate',
    required: false,
    label: '发送日期',
    align: 'left',
    field: 'sendDate',
    format: v => formatDate(v),
    sortable: true
  }
]


async function getRowsNumberCount (filterObj: TTableFilterObject) {
  const { data } = await getSendingItemsCount(sendingGroupId.value as number, filterObj.filter, statusTab.value)
  return data || 0
}

async function onRequest (filterObj: TTableFilterObject, pagination: IRequestPagination) {
  const { data } = await getSendingItemsData(sendingGroupId.value as number, filterObj.filter, statusTab.value, pagination)
  return data || []
}

const { pagination, rows, filter, onTableRequest, loading, refreshTable } = useQTable({
  getRowsNumberCount,
  onRequest
})

import { removeHistory } from 'src/layouts/components/tags/routeHistories'
import type { IRouteHistory } from 'src/layouts/components/tags/types'
const router = useRouter()
async function goBackToSendHistory () {
  await removeHistory(router, route as unknown as IRouteHistory, '/send-management/history')
}

// 从路由中获取id
const route = useRoute()
onMounted(() => {
  if (!route.query.sendingGroupId) return
  sendingGroupId.value = Number(route.query.sendingGroupId)
  // 触发更新
  refreshTable()
})

// #region 发件进度相关
import LinearProgress from 'src/components/Progress/LinearProgress.vue'
import { subscribeOne } from 'src/signalR/signalR'
import type { ISendingGroupProgressArg, ISendingItemStatusChangedArg } from 'src/signalR/types'
import { UzonMailClientMethods, SendingGroupProgressType } from 'src/signalR/types'
import { getSendingGroupRunningInfo, SendingGroupStatus } from 'src/api/sendingGroup'
const sendingGroupProgressValue = ref(-1)
const showProgressBar = computed(() => sendingGroupProgressValue.value > 0 && sendingGroupProgressValue.value < 1)
onMounted(async () => {
  // 读取初始状态
  const { data } = await getSendingGroupRunningInfo(sendingGroupId.value as number)
  if (data.status !== SendingGroupStatus.Sending) return
  sendingGroupProgressValue.value = data.sentCount / data.totalCount
})
// 注册单个邮件发送进度回调
function onSendingItemStatusChanged (arg: ISendingItemStatusChangedArg) {
  // 更新单个进度
  const row = rows.value.find(r => r.id === arg.sendingItemId)
  if (!row) return

  // 更新参数
  Object.assign(row, arg)
}
subscribeOne(UzonMailClientMethods.sendingItemStatusChanged, onSendingItemStatusChanged)
// 注册由件组的发送进度
function onSendingGroupProgressChanged (arg: ISendingGroupProgressArg) {
  // 更新总进度
  if (arg.sendingGroupId !== sendingGroupId.value) return
  if (arg.progressType === SendingGroupProgressType.end) {
    if (arg.sendingGroupId !== sendingGroupId.value) return
    sendingGroupProgressValue.value = -1
    // 不提示成功，因为全局进度条也在监听事件，由全局发出通知即可
    // notifySuccess(`邮件组 ${arg.sendingGroupId} 发送完成`)
    return
  }
  sendingGroupProgressValue.value = arg.current / arg.total
}
subscribeOne(UzonMailClientMethods.sendingGroupProgressChanged, onSendingGroupProgressChanged)
// #endregion

// #region 发送状态
import StatusChip from 'src/components/statusChip/StatusChip.vue'
import AsyncTooltip from 'src/components/asyncTooltip/AsyncTooltip.vue'
// #endregion

/**
 * 右键菜单
 */
import { useContextMenu } from './sendDetailContext'
import { notifyError } from 'src/utils/dialog'
import type { IExcelColumnMapper, IExcelWriterParams } from 'src/utils/file'
import { writeExcel } from 'src/utils/file'
const { sendDetailContextItems, ContextMenu } = useContextMenu()

// 导出
async function onExportCurrentSendingItems () {
  if (rows.value.length === 0) {
    notifyError('数据为空')
    return
  }

  const { data: allRows } = await getSendingItemsData(sendingGroupId.value as number, '', statusTab.value, {
    sortBy: 'id',
    descending: false,
    skip: 0,
    limit: pagination.value.rowsNumber
  })
  // 生成 headerMaps
  const headerMaps: IExcelColumnMapper[] = columns.map(x => {
    return {
      headerName: x.label,
      fieldName: x.field as string,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      format: x.format as (v: any) => string
    }
  })
  headerMaps.push({
    headerName: '发送结果',
    fieldName: 'sendResult'
  })
  const statusLabel = statusTabOptions.find(x => x.value === statusTab.value)?.label
  const writerParams: IExcelWriterParams = {
    mappers: headerMaps,
    fileName: `${sendingGroupId.value}-发送明细-${statusLabel}.xlsx`,
    strict: true
  }
  await writeExcel(allRows, writerParams)
}
</script>

<style lang="scss" scoped></style>
