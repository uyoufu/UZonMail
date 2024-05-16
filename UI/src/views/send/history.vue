<template>
  <div class="history-container">
    <q-table
      row-key="_id"
      :data="data"
      :columns="columns"
      :pagination.sync="pagination"
      :loading="loading"
      :filter="filter"
      dense
      binary-state-sort
      virtual-scroll
      class="full-height"
      @request="initQuasarTable_onRequest"
    >
      <template #top>
        <q-space />
        <q-input
          v-model="filter"
          dense
          debounce="300"
          placeholder="搜索"
          color="primary"
        >
          <template #append>
            <q-icon name="search" />
          </template>
        </q-input>
      </template>

      <template #body-cell-operations="props">
        <q-td :props="props" class="row justify-end">
          <q-btn
            :size="btn_detail.size"
            :color="btn_detail.color"
            :label="btn_detail.label"
            :dense="btn_detail.dense"
            class="q-mr-sm"
            @click="openShowDetailDialog(props.row._id)"
          />
          <q-btn
            :size="btn_delete.size"
            :color="btn_delete.color"
            :label="btn_delete.label"
            :dense="btn_delete.dense"
            @click="deleteHistoryGroup(props.row._id)"
          />
        </q-td>
      </template>
    </q-table>

    <q-dialog v-model="isShowDetailDialog" persistent>
      <HistoryDetail :history-id="toShowId" @close="closeHistoryDetail" />
    </q-dialog>
  </div>
</template>

<script>
import {
  getHistoriesCount,
  getHistoriesList,
  deleteHistoryGroup,
  getHistoryById
} from '@/api/history'
import moment from 'moment'

import { table } from '@/themes/index'
const { btn_detail, btn_delete } = table

import HistoryDetail from './components/historyDetail.vue'
import { notifyError, notifySuccess, okCancle } from '@/components/iPrompt'

import mixin_initQTable from '@/mixins/initQtable.vue'

export default {
  components: { HistoryDetail },
  mixins: [mixin_initQTable],
  data() {
    return {
      btn_detail,
      btn_delete,

      columns: [
        {
          name: 'createDate',
          required: true,
          label: '日期',
          align: 'left',
          field: 'createDate',
          format: val => moment(val).format('YYYY-MM-DD'),
          sortable: true
        },
        {
          name: 'subject',
          required: true,
          label: '主题',
          align: 'left',
          field: 'subject',
          sortable: true
        },
        {
          name: 'template',
          required: true,
          label: '模板',
          align: 'left',
          field: 'templateName',
          sortable: true
        },
        {
          name: 'senderIdsLength',
          required: true,
          label: '发件箱总数',
          align: 'left',
          field: 'senderIds',
          format: val => val.length,
          sortable: false
        },
        {
          name: 'receiverIdsLength',
          required: true,
          label: '收件箱总数',
          align: 'left',
          field: 'receiverIds',
          format: val => val.length,
          sortable: false
        },
        // 状态
        {
          name: 'status',
          required: true,
          label: '状态',
          align: 'left',
          field: row => row,
          format: this.formatStatus,
          sortable: false
        },
        {
          name: 'operations',
          required: true,
          label: '操作',
          align: 'right'
        }
      ],

      toShowId: '',
      isShowDetailDialog: false
    }
  },

  methods: {
    // 获取筛选的数量
    // 重载 mixin 中的方法
    async initQuasarTable_getFilterCount(filterObj) {
      const res = await getHistoriesCount(filterObj)
      return res.data || 0
    },

    // 重载 mixin 中的方法
    // 获取筛选结果
    async initQuasarTable_getFilterList(filterObj, pagination) {
      const res = await getHistoriesList(filterObj, pagination)
      return res.data || []
    },

    // 格式化状态
    formatStatus(val) {
      const status = []

      if (val.sendStatus & 1) {
        status.push(
          `发送结束，成功：${val.successCount}/${val.receiverIds.length}`
        )
      }

      if (val.sendStatus & 2) {
        status.push(`已初始化，但未发送`)
      }

      if (val.sendStatus & 4) {
        status.push(`发送中...`)
      }

      if (val.sendStatus & 8) {
        status.push(`重发中...`)
      }

      if (val.sendStatus & 16) {
        status.push(`暂停`)
      }

      if (val.sendStatus & 32) {
        status.push(`正在发送图片`)
      }

      if (val.sendStatus & 32) {
        status.push(`正在发送html`)
      }

      return status.join()
    },

    openShowDetailDialog(id) {
      this.toShowId = id
      this.isShowDetailDialog = true
    },

    // 删除发件
    async deleteHistoryGroup(historyId) {
      // 提醒
      const ok = await okCancle('是否删除该项发送记录？')
      if (!ok) return

      // 开始删除
      await deleteHistoryGroup(historyId)

      // 删除现有数据
      const index = this.data.findIndex(d => d._id === historyId)
      if (index > -1) {
        this.data.splice(index, 1)
      }

      // 提示成功
      notifySuccess('删除成功')
    },

    // 关闭详细面板
    async closeHistoryDetail(historyId) {
      // 从服务器拉取该条数据
      const res = await getHistoryById(historyId)
      if (!res.data) {
        this.isShowDetailDialog = false
        notifyError('更新历史记录失败')
        return
      }

      const index = this.data.findIndex(d => d._id === historyId)
      if (index > -1) this.data.splice(index, 1, res.data)

      this.isShowDetailDialog = false
    }
  }
}
</script>

<style lang="scss">
.history-container {
  position: absolute;
  top: 0;
  right: 0;
  bottom: 0;
  left: 0;
}
</style>
