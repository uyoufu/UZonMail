<template>
  <q-card class="column" style="max-width: none; height: 60%">
    <q-table row-key="_id" :data="data" :columns="columns" :pagination.sync="pagination" :loading="loading"
      :filter="filter" dense binary-state-sort virtual-scroll class="col-grow" @request="initQuasarTable_onRequest">
      <template #top>
        <q-space />
        <q-input v-model="filter" dense debounce="300" :placeholder="$t('search')" color="primary">
          <template #append>
            <q-icon name="search" />
          </template>
        </q-input>
      </template>

      <template #body-cell-index="props">
        <q-td :props="props" class="row justify-end">
          {{ props.rowIndex + 1 }}
        </q-td>
      </template>

      <template #body-cell-sendMessage="props">
        <q-td :props="props">
          {{ props.value }}
          <q-tooltip>
            {{ props.value }}
          </q-tooltip>
        </q-td>
      </template>

      <template #body-cell-operations="props">
        <q-td :props="props" class="row justify-end">
          <!-- <q-btn
            :size="btn_detail.size"
            :color="btn_detail.color"
            :label="btn_detail.label"
            :dense="btn_detail.dense"
            @click="openShowDetailDialog(props.row._id)"
          /> -->
        </q-td>
      </template>
    </q-table>
    <div class="row justify-end q-pa-sm">
      <q-btn v-if="isShowResent" :label="$t('resend')" color="teal" size="sm" class="q-mr-sm" :class="resentClass"
        :disable="disableResend" :loading="isResending" @click="resend">
        <template #loading>
          <q-spinner-hourglass class="on-left" />
          {{ resendLabel }}
        </template>
      </q-btn>

      <q-btn :disable="disableCancle" :label="$t('cancel')" color="negative" size="sm" @click="closeHistoryDetail" />
    </div>
  </q-card>
</template>

<script>
import {
  getSendItemsCountByHistoryId,
  getSendItemsListByHistoryId,
  getHistoryGroupSendResult
} from '@/api/history'
import { startSending, getCurrentStatus, getSendingInfo } from '@/api/send'

import { table } from '@/themes/index'

import moment from 'moment'
import { notifyError, notifySuccess, okCancle } from '@/components/iPrompt'

import mixin_initQTable from '@/mixins/initQtable.vue'

export default {
  mixins: [mixin_initQTable],

  props: {
    historyId: {
      type: String,
      default: ''
    }
  },
  data() {
    return {
      columns: [],
      disableResend: false,
      disableCancle: false,
      isResending: false,
      resendLabel: this.$t('start_label'),
      dateFormat: 'YYYY-MM-DD',
    }
  },
  computed: {
    btn_detail() {
      return table.btn_detail
    },
    // 是否显示重新发送功能
    isShowResent() {
      const result = this.data.some(d => !d.isSent)
      console.log('isShowResent:', result)
      return result
    },

    resentClass() {
      if (this.isResending) return 'resend-runing'

      return 'resend-normal'
    }
  },
  watch: {
    '$i18n.locale'() {
      this.init() // 语言切换时重新初始化
    },
    data(val) {
      this.disableResend = val.every(d => d.isSent)
    }
  },
  async mounted() {
    // 获取数据
    if (!this.historyId) return
    this.init()
    // 获取当前状态，可能处于发送中
    const statusRes = await getCurrentStatus()
    if (statusRes.data & 1 && statusRes.data & 8) {
      // 打开发送框
      this.disableResend = true
      this.disableCancle = true
      this.isResending = true

      this.getProgressInfo()
    }
  },

  methods: {
    init() {
      if (this.$i18n.locale === 'it') {
        this.dateFormat = "DD/MM/YYYY"
      }
      this.columns = [
        {
          name: 'index',
          required: true,
          label: this.$t('table.index'),
          align: 'left'
        },
        {
          name: 'createDate',
          required: true,
          label: this.$t('table.createDate'),
          align: 'left',
          field: 'createDate',
          format: val => moment(val).format(this.dateFormat),
          sortable: true
        },
        {
          name: 'senderName',
          required: true,
          label: this.$t('table.senderName'),
          align: 'left',
          field: 'senderName',
          format: val => val || '-',
          sortable: true
        },
        {
          name: 'senderEmail',
          required: true,
          label: this.$t('table.senderEmail'),
          align: 'left',
          field: 'senderEmail',
          format: val => val || '-',
          sortable: true
        },
        {
          name: 'receiverName',
          required: true,
          label: this.$t('table.receiverName'),
          align: 'left',
          field: 'receiverName',
          sortable: true
        },
        {
          name: 'receiverEmail',
          required: true,
          label: this.$t('table.receiverEmail'),
          align: 'left',
          field: 'receiverEmail',
          sortable: true
        },
        // 状态
        {
          name: 'isSent',
          required: true,
          label: this.$t('table.isSent'),
          align: 'left',
          field: 'isSent',
          format: val => (val ? this.$t('table.sent_state_success') : this.$t('table.sent_state_fail')),
          sortable: true
        },
        {
          name: 'sendMessage',
          required: true,
          label: this.$t('table.sendMessage'),
          align: 'left',
          field: 'sendMessage',
          format: val => this.$t(val),
          sortable: true,
          style: 'max-width:200px;text-overflow: ellipsis;overflow: hidden'
        },
        {
          name: 'tryCount',
          required: true,
          label: this.$t('table.tryCount'),
          align: 'left',
          field: 'tryCount',
          sortable: true
        }
        // {
        //   name: 'operations',
        //   required: true,
        //   label: '操作',
        //   align: 'right'
        // }
      ]
    },
    // 获取筛选的数量
    // 重载 mixin 中的方法
    async initQuasarTable_getFilterCount(filterObj) {
      const res = await getSendItemsCountByHistoryId(this.historyId, filterObj)
      return res.data || 0
    },

    // 重载 mixin 中的方法
    // 获取筛选结果
    async initQuasarTable_getFilterList(filterObj, pagination) {
      const res = await getSendItemsListByHistoryId(
        this.historyId,
        filterObj,
        pagination
      )
      return res.data || []
    },

    async resend() {
      const ids = this.data.filter(d => !d.isSent).map(d => d._id)

      // 提示
      const ok = await okCancle(this.$t('resend_confirmation', { count: ids.length }))
      if (!ok) return

      if (!ids || ids.length < 1) {
        notifyError(this.$t('no_items_to_send'))
        return
      }

      // 开始发送
      await startSending(this.historyId)

      // 关闭重发,关闭取消
      this.disableResend = true
      this.disableCancle = true

      this.getProgressInfo()
    },

    // 轮询获取进度
    getProgressInfo() {
      setTimeout(async () => {
        // 获取更新数据
        const res = await getSendingInfo()
        // console.log('getProgressInfo:', res.data)
        const sendingInfo = res.data

        // 显示进度
        this.resendLabel =
          ((sendingInfo.index * 100) / (sendingInfo.total || 1)).toFixed(1) +
          ' %'

        if (sendingInfo.index < sendingInfo.total) {
          this.getProgressInfo()
        } else {
          // 获取发送结果
          const msgRes = await getHistoryGroupSendResult(sendingInfo.historyId)

          if (msgRes.data.ok) notifySuccess(msgRes.data.message)
          else notifyError(msgRes.data.message)

          // 重新更新数据
          this.initQuasarTable_onRequest()

          // 打开按钮锁定
          this.disableCancle = false
          this.isResending = false
          // 恢复重发初始值
          this.resendLabel = this.$t('start_label')
        }
      }, 800)
    },

    // 关掉窗口后，主程序需要更新当前数据
    closeHistoryDetail() {
      this.$emit('close', this.historyId)
    }
  }
}
</script>

<style lang='scss'>
.resend-runing {
  width: 80px;
}
</style>
