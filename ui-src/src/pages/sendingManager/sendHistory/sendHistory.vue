<template>
  <q-table class="full-height" :rows="rows" :columns="columns" row-key="id" virtual-scroll
    v-model:pagination="pagination" dense :loading="loading" :filter="filter" binary-state-sort selection="multiple"
    v-model:selected="selectedRows" @request="onTableRequest">
    <template v-slot:top-left>
      <div class="text-subtile1 text-primary">历史发件</div>
    </template>

    <template v-slot:top-right>
      <SearchInput v-model="filter" />
    </template>

    <template v-slot:body-cell-index="props">
      <QTableIndex :props="props" />
      <ContextMenu :items="sendingHistoryContextItems" :value="props.row" />
    </template>

    <template v-slot:body-cell-subjects="props">
      <q-td :props="props">
        <ClickableText :text="props.value" @click="openSendDetailDialog(props.row)" />
      </q-td>
    </template>

    <template v-slot:body-cell-status="props">
      <q-td :props="props">
        <StatusChip v-if="props.value !== 'Sending'" :status="props.value"></StatusChip>
        <LinearProgress class="full-width" v-else :value="props.row.progress" :width="60"></LinearProgress>
      </q-td>
    </template>

    <template v-slot:body-cell-sendingType="props">
      <q-td :props="props">
        <StatusChip :status="props.value"></StatusChip>
      </q-td>
    </template>
  </q-table>
</template>

<script lang="ts" setup>
import LinearProgress from 'src/components/Progress/LinearProgress.vue'
import StatusChip from 'src/components/statusChip/StatusChip.vue'
import ClickableText from 'src/components/clickableText/ClickableText.vue'

import { formatDate } from 'src/utils/format'

import type { QTableColumn } from 'quasar'
import { useQTable, useQTableIndex } from 'src/compositions/qTableUtils'
import type { IRequestPagination, TTableFilterObject } from 'src/compositions/types'
import SearchInput from 'src/components/searchInput/SearchInput.vue'

import type { ISendingGroupInfo } from 'src/api/sendingGroup'
import { getSendingGroupsCount, getEmailTemplatesData, SendingGroupStatus, SendingGroupType } from 'src/api/sendingGroup'

const { indexColumn, QTableIndex } = useQTableIndex()
// eslint-disable-next-line @typescript-eslint/no-explicit-any
function formatSuccessPercent (success: number, row: Record<string, any>) {
  if (!row.totalCount) return '0%'
  return ((success / row.totalCount) * 100).toFixed(0) + '%'
}
const columns: QTableColumn[] = [
  indexColumn,
  {
    name: 'id',
    label: 'ID',
    align: 'center',
    field: 'id',
    sortable: true
  },
  {
    name: 'subjects',
    required: true,
    label: '主题',
    align: 'left',
    field: 'subjects',
    sortable: true,
    format: v => v.replace('\n', ';')
  },
  {
    name: 'sendingType',
    required: true,
    label: '类型',
    align: 'left',
    field: 'sendingType',
    sortable: true,
    format: v => SendingGroupType[v] as string
  },
  {
    name: 'templatesCount',
    required: true,
    label: '模板数',
    align: 'left',
    field: 'templatesCount'
  },
  {
    name: 'outboxesCount',
    required: true,
    label: '发件箱数',
    align: 'left',
    field: 'outboxesCount'
  },
  {
    name: 'totalCount',
    required: true,
    label: '收件箱总数',
    align: 'left',
    field: 'totalCount',
    sortable: true
  },
  {
    name: 'sentCount',
    required: true,
    label: '已发送',
    align: 'left',
    field: 'sentCount',
    sortable: true
  },
  {
    name: 'successCount',
    required: true,
    label: '已成功',
    align: 'left',
    field: 'successCount',
    sortable: true
  },
  {
    name: 'successPercent',
    required: true,
    label: '成功率',
    align: 'left',
    field: 'successCount',
    sortable: false,
    format: formatSuccessPercent
  },
  {
    name: 'createDate',
    required: false,
    label: '开始日期',
    align: 'left',
    field: v => v,
    format: (value: ISendingGroupInfo) => {
      if (value.sendingType === SendingGroupType.Scheduled) return formatDate(value.scheduleDate)
      return formatDate(value.createDate)
    },
    sortable: true
  }, {
    name: 'status',
    required: true,
    label: '状态',
    align: 'center',
    field: 'status',
    sortable: true,
    format: v => SendingGroupStatus[v] as string
  }
]


async function getRowsNumberCount (filterObj: TTableFilterObject) {
  const { data } = await getSendingGroupsCount(filterObj.filter)
  return data || 0
}

async function onRequest (filterObj: TTableFilterObject, pagination: IRequestPagination) {
  const { data } = await getEmailTemplatesData(filterObj.filter, pagination)
  return data || []
}

const { pagination, rows, filter, onTableRequest, loading,
  selectedRows, getSelectedRows, deleteRowById
} = useQTable({
  sortBy: 'id',
  descending: true,
  getRowsNumberCount,
  onRequest
})

// 右键菜单
import ContextMenu from 'src/components/contextMenu/ContextMenu.vue'
import { useContextMenu } from './sendingHistoryContext'
const { openSendDetailDialog, sendingHistoryContextItems } = useContextMenu(getSelectedRows, deleteRowById)

/**
 * 进度与状态显示
 */

// 注册进度获取回调
import { subscribeOne } from 'src/signalR/signalR'
import type { ISendingGroupProgressArg } from 'src/signalR/types'
import { SendingGroupProgressType, UzonMailClientMethods } from 'src/signalR/types'

// 进度变化
function onSendingGroupProgressChanged (arg: ISendingGroupProgressArg) {
  const row = rows.value.find(r => r.id === arg.sendingGroupId)
  if (!row) return

  row.successCount = arg.successCount
  row.sentCount = arg.sentCount

  if (arg.progressType === SendingGroupProgressType.end) {
    row.status = SendingGroupStatus.Finish
    row.progress = 1
    // 不提示成功，因为全局进度条也在监听事件，由全局发出通知即可
    // notifySuccess(`邮件组 ${arg.sendingGroupId} 发送完成`)
    return
  }

  // 更新进度
  row.progress = arg.current * 1.0 / arg.total
}
subscribeOne(UzonMailClientMethods.sendingGroupProgressChanged, onSendingGroupProgressChanged)
</script>

<style lang="scss" scoped></style>
