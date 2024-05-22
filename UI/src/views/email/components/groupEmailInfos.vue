<template>
  <div class="emails-table">
    <q-table row-key="_id" :data="data" :columns="columns" :pagination.sync="pagination" :loading="loading"
      :filter="filter" dense binary-state-sort virtual-scroll class="full-height" @request="initQuasarTable_onRequest">
      <template #top>
        <div class="row justify-center q-gutter-sm">
          <q-btn :label="$t('new')" dense size="sm" color="primary" class="q-pr-xs q-pl-xs"
            @click="openNewEmailDialog" />
          <q-btn :label="$t('importExcel')" dense size="sm" color="primary" class="q-pr-xs q-pl-xs"
            @click="selectExcelFile" />
          <span class="text-subtitle1 text-primary">{{ group.name }}</span>
          <input id="fileInput" type="file" style="display: none"
            accept="application/vnd.ms-excel,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            @change="fileSelected">
        </div>
        <q-space />
        <q-input v-model="filter" dense debounce="300" :placeholder="$t('search')" color="primary">
          <template #append>
            <q-icon :name="$t('search')" />
          </template>
        </q-input>
      </template>
      <template #header-cell-operation="props">
        <q-th :props="props">
          {{ props.col.label }}
          <q-btn v-if="data.length > 0" :size="btn_delete.size" color="secondary" :label="$t('clear')"
            :dense="btn_delete.dense" @click="clearGroup()" />
        </q-th>
      </template>

      <template #body-cell-operation="props">
        <q-td :props="props" class="row justify-end">
          <q-btn :size="btn_modify.size" :color="btn_modify.color" :label="btn_modify.label" :dense="btn_modify.dense"
            class="q-mr-sm" @click="showModifyEmailDialog(props.row)" />

          <q-btn v-if="columns.length > 3" :size="btn_modify.size" :color="btn_modify.color" :label="$t('btnSettings')"
            :dense="btn_modify.dense" class="q-mr-sm" @click="showUpdateSettings(props.row)" />

          <q-btn :size="btn_delete.size" :color="btn_delete.color" :label="btn_delete.label" :dense="btn_delete.dense"
            @click="deleteEmailInfo(props.row._id)" />
        </q-td>
      </template>
    </q-table>

    <q-dialog v-model="isShowNewEmailDialog" persistent>
      <DialogForm type="create" :init-params="initNewEmailParams" @createSuccess="addedNewEmail" />
    </q-dialog>

    <q-dialog v-model="isShowModifyEmailDialog" persistent>
      <DialogForm :init-params="initModifyEmailParams" type="update" @updateSuccess="modifiedEmail" />
    </q-dialog>

    <q-dialog v-model="isShowUpdateSettings" persistent>
      <DialogForm :init-params="initSettingParams" type="update" @updateSuccess="updatedSettings" />
    </q-dialog>
  </div>
</template>

<script>
import DialogForm from '@/components/DialogForm'

import NewEmail from '../mixins/newEmail.vue'
import NewEmails from '../mixins/newEmails.vue'
import ModifyEmail from '../mixins/modifyEmail.vue'
import UpdateSettings from '../mixins/updateSettings.vue'
import mixin_initQTable from '@/mixins/initQtable.vue'

import {
  getEmailsCount,
  getEmails,
  deleteEmail,
  deleteEmails
} from '@/api/group'

import { table } from '@/themes/index'
import { notifySuccess, okCancle } from '@/components/iPrompt'

export default {
  components: { DialogForm },
  mixins: [NewEmail, ModifyEmail, NewEmails, UpdateSettings, mixin_initQTable],

  props: {
    group: {
      type: Object,
      default() {
        return {
          groupType: 'send'
        }
      }
    }
  },

  data() {
    return {
    }
  },

  computed: {
    btn_modify() {
      return table.btn_modify
    },
    btn_delete() {
      return table.btn_delete
    },
    // 列定义
    columns() {
      if (this.group.groupType === 'send') {
        return [
          {
            name: 'userName',
            required: true,
            label: this.$t('table.userName'),
            align: 'left',
            field: row => row.userName,
            sortable: true
          },
          {
            name: 'email',
            required: true,
            label: this.$t('table.email'),
            align: 'left',
            field: row => row.email,
            sortable: true
          },
          {
            name: 'smtp',
            required: true,
            label: this.$t('table.smtp'),
            align: 'left',
            field: row => row.smtp,
            sortable: true
          },
          {
            name: 'password',
            required: true,
            label: this.$t('table.password'),
            align: 'left',
            field: row => row.password,
            sortable: true
          },
          {
            name: 'maxEmailsPerDay',
            label: this.$t('table.maxEmailsPerDay'),
            align: 'left',
            field: 'settings',
            format: val => {
              if (!val) return ''

              return val.maxEmailsPerDay
            },
            sortable: true
          },
          {
            name: 'operation',
            label: this.$t('table.operation'),
            align: 'right'
          }
        ]
      } else {
        return [
          {
            name: 'userName',
            required: true,
            label: this.$t('table.userName'),
            align: 'left',
            field: row => row.userName,
            sortable: true
          },
          {
            name: 'email',
            required: true,
            label: this.$t('table.email'),
            align: 'left',
            field: row => row.email,
            sortable: true
          },
          {
            name: 'operation',
            label: this.$t('table.operation'),
            align: 'right'
          }
        ]
      }
    }
  },

  methods: {
    // 获取筛选的数量
    // 重载 mixin 中的方法
    async initQuasarTable_getFilterCount(filterObj) {
      const res = await getEmailsCount(this.group._id, filterObj)
      return res.data || 0
    },

    // 重载 mixin 中的方法
    // 获取筛选结果
    async initQuasarTable_getFilterList(filterObj, pagination) {
      const res = await getEmails(this.group._id, filterObj, pagination)
      return res.data || []
    },

    // 删除邮箱
    async deleteEmailInfo(emailInfoId) {
      const ok = await okCancle(this.$t('deleteEmailInfoConfirm'))
      if (!ok) return

      // 开始删除
      await deleteEmail(emailInfoId)

      // 清空现有数据
      const index = this.data.findIndex(d => d._id === emailInfoId)
      if (index > -1) this.data.splice(index, 1)

      notifySuccess(this.$t('delete_success'))
    },

    // 清空组
    async clearGroup() {
      const ok = await okCancle(this.$t('clearGroupConfirm'))
      if (!ok) return

      await deleteEmails(this.group._id)

      this.data = []
      notifySuccess(this.$t('allClearSuccess'))
    }
  }
}
</script>

<style lang='scss'>
.emails-table {
  position: absolute;
  top: 0;
  bottom: 0;
  left: 0;
  right: 0;
}
</style>
