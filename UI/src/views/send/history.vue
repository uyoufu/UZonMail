<template>
  <div class="history-container">
    <q-table
      :key="tableKey"
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
          :placeholder="$t('search')"
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

import HistoryDetail from './components/historyDetail.vue'
import { notifyError, notifySuccess, okCancle } from '@/components/iPrompt'

import mixin_initQTable from '@/mixins/initQtable.vue'

export default {
  components: { HistoryDetail },
  mixins: [mixin_initQTable],
  data() {
    return {
      columns: [],
      toShowId: '',
      isShowDetailDialog: false,
      tableKey: 0,
      dateFormat: 'YYYY-MM-DD',
    }
  },
  computed: {
    btn_detail() {
      return table.btn_detail
    },
    btn_delete() {
      return table.btn_delete
    },
  },
  watch: {
    '$i18n.locale'() {
      this.init() // 语言切换时重新初始化
      this.loadData() // 语言切换时重新加载数据
    }
  },
  async mounted() {
    this.init()
    this.loadData()
  },
  methods: {
    init() {
      if (this.$i18n.locale === 'it') {
        this.dateFormat="DD/MM/YYYY"
      }
      this.columns = [
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
          name: 'subject',
          required: true,
          label: this.$t('table.subject'),
          align: 'left',
          field: 'subject',
          sortable: true
        },
        {
          name: 'template',
          required: true,
          label: this.$t('table.templateName'),
          align: 'left',
          field: 'templateName',
          sortable: true
        },
        {
          name: 'senderIdsLength',
          required: true,
          label: this.$t('table.senderIdsLength'),
          align: 'left',
          field: 'senderIds',
          format: val => val.length,
          sortable: false
        },
        {
          name: 'receiverIdsLength',
          required: true,
          label: this.$t('table.receiverIdsLength'),
          align: 'left',
          field: 'receiverIds',
          format: val => val.length,
          sortable: false
        },
        // 状态
        {
          name: 'status',
          required: true,
          label: this.$t('table.status'),
          align: 'left',
          field: row => row,
          format: this.formatStatus,
          sortable: false
        },
        {
          name: 'operations',
          required: true,
          label: this.$t('table.operations'),
          align: 'right'
        }
      ]
    },
    async loadData() {
      // 强制刷新表格
      this.tableKey += 1
      // 调用mixin中的方法加载数据
      await this.initQuasarTable_onRequest()
    },
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
          `${this.$t('send_status1')}：${val.successCount}/${val.receiverIds.length}`
        )
      }

      if (val.sendStatus & 2) {
        status.push(this.$t('send_status2'))
      }

      if (val.sendStatus & 4) {
        status.push(this.$t('send_status4'))
      }

      if (val.sendStatus & 8) {
        status.push(this.$t('send_status8'))
      }

      if (val.sendStatus & 16) {
        status.push(this.$t('send_status16'))
      }

      if (val.sendStatus & 32) {
        status.push(this.$t('send_status32'))
      }

      if (val.sendStatus & 64) {
        status.push(this.$t('send_status64'))
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
      const ok = await okCancle(this.$t('delete_history_group_confirm'))
      if (!ok) return

      // 开始删除
      await deleteHistoryGroup(historyId)

      // 删除现有数据
      const index = this.data.findIndex(d => d._id === historyId)
      if (index > -1) {
        this.data.splice(index, 1)
      }

      // 提示成功
      notifySuccess(this.$t('delete_success'))
    },

    // 关闭详细面板
    async closeHistoryDetail(historyId) {
      // 从服务器拉取该条数据
      const res = await getHistoryById(historyId)
      if (!res.data) {
        this.isShowDetailDialog = false
        notifyError(this.$t('get_history_detail_error'))
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
