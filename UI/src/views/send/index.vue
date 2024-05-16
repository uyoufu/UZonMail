<template>
  <div class="q-pa-lg send-container q-gutter-md">
    <div class="receive-box row">
      <strong style="height: auto; align-self: center">
        主题：
        <q-tooltip anchor="center right" self="center left">
          发件主题，不能为空
        </q-tooltip>
      </strong>
      <input v-model="subject" type="text" class="send-input col-grow">
    </div>

    <div class="receive-box row justify-between">
      <div class="row col-grow">
        <strong style="height: auto; align-self: center">
          发件人：
          <q-tooltip anchor="center right" self="center left">
            <div>如果发件人为空，则会使用随机发件箱进行发件</div>
          </q-tooltip>
        </strong>
        <q-chip
          v-for="rec in senders"
          :key="rec.type + rec._id"
          removable
          :color="rec.type === 'group' ? 'orange' : 'primary'"
          size="sm"
          text-color="white"
          :label="rec.label"
          @remove="removeSender(rec)"
        />
        <input type="text" class="send-input col-grow">
      </div>
      <q-btn
        size="sm"
        dense
        class="self-center q-mb-sm"
        color="secondary"
        outline
        label="选择发件人"
        @click="openSelectSendersDialog"
      />
    </div>

    <div class="receive-box row justify-between">
      <div class="row col-grow">
        <strong style="height: auto; align-self: center">
          收件人：
          <q-tooltip anchor="center right" self="center left">
            如果按数据中定义的收件人来发件，则收件人栏必须为空
          </q-tooltip>
        </strong>
        <q-chip
          v-for="rec in receivers"
          :key="rec.type + rec._id"
          removable
          :color="rec.type === 'group' ? 'orange' : 'primary'"
          size="sm"
          text-color="white"
          :label="rec.label"
          @remove="removeReceiver(rec)"
        />
        <input type="text" class="send-input col-grow">
      </div>
      <q-btn
        size="sm"
        dense
        class="self-center q-mb-sm"
        color="secondary"
        outline
        label="选择收件人"
        @click="openSelectReceiversDialog"
      />
    </div>

    <div class="receive-box row justify-between">
      <div class="row col-grow">
        <strong style="height: auto; align-self: center">
          抄送人：
          <q-tooltip anchor="center right" self="center left">
            抄送人为空时，会从数据中读取 copyToEmails 中的用户名作为抄送对象
          </q-tooltip>
        </strong>
        <q-chip
          v-for="rec in copyToEmails"
          :key="rec.type + rec._id"
          removable
          :color="rec.type === 'group' ? 'orange' : 'primary'"
          size="sm"
          text-color="white"
          :label="rec.label"
          @remove="removeCopyTo(rec)"
        />
        <input type="text" class="send-input col-grow">
      </div>
      <q-btn
        size="sm"
        dense
        class="self-center q-mb-sm"
        color="secondary"
        outline
        label="选择抄送人"
        @click="openSelectCopyToDialog"
      />
    </div>

    <div class="row justify-between">
      <div class="row content-center">
        <strong style="height: auto; align-self: center">模板：</strong>
        <el-select
          v-model="selectedTemplate"
          placeholder="请选择"
          size="small"
          value-key="_id"
        >
          <el-tooltip
            v-for="item in options"
            :key="item._id"
            class="item"
            effect="light"
            placement="right"
          >
            <div slot="content">
              <q-img
                class="rounded-borders"
                :src="item.imageUrl"
                width="300px"
              />
            </div>
            <el-option :label="item.name" :value="item" />
          </el-tooltip>
        </el-select>
      </div>
      <div
        class="receive-box row justify-between content-center q-ml-md"
        style="flex: 1"
      >
        <div>
          <strong style="height: auto; align-self: center">数据：</strong>
          {{ selectedFileName }}
        </div>
        <input
          id="fileInput"
          type="file"
          style="display: none"
          accept="application/vnd.ms-excel,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
          @change="fileSelected"
        >
        <q-btn
          label="选择Excel"
          dense
          size="sm"
          outline
          color="secondary"
          class="q-mb-sm"
          @click="selectExcelFile"
        />
      </div>
    </div>

    <div class="receive-box row justify-between">
      <div class="row col-grow">
        <strong style="height: auto; align-self: center">附件：</strong>
        <q-chip
          v-for="att in attachments"
          :key="att"
          removable
          color="orange"
          size="sm"
          text-color="white"
          :label="getFileBaseName(att)"
          @remove="removeAttachment(att)"
        />
        <input type="text" class="send-input col-grow">
      </div>
      <q-btn
        size="sm"
        dense
        class="self-center q-mb-sm"
        color="secondary"
        outline
        label="选择附件"
        @click="sendSignToSelectAttachment"
      />
    </div>

    <div class="q-ma-md" v-html="selectedTemplate.html" />

    <div class="row justify-end preview-row q-mr-md">
      <q-btn
        label="预览"
        color="secondary"
        size="sm"
        @click="previewEmailBody"
      />

      <q-btn
        label="发送"
        color="primary"
        size="sm"
        class="q-ml-sm"
        :disable="disableSend"
        @click="startSending"
      />
      <q-btn
        v-if="false"
        label="定时"
        color="primary"
        class="q-ml-sm"
        size="sm"
        :disable="disableSend"
        @click="startSending"
      />
    </div>

    <q-dialog v-model="isShowSendingDialog" persistent>
      <SendingProgress @close="isShowSendingDialog = false" />
    </q-dialog>

    <q-dialog v-model="isShowPreviewDialog" persistent>
      <q-card style="max-width: none">
        <q-layout
          view="lHh lpr lFf"
          container
          style="height: 400px; width: 600px"
          class="shadow-2 rounded-borders"
        >
          <q-header elevated class="bg-primary">
            <div class="q-pa-sm text-subtitle1">
              发送给：{{ previewData.receiverName }}/{{
                previewData.receiverEmail
              }}
            </div>
          </q-header>

          <q-footer elevated class="bg-secondary">
            <div class="row justify-between q-pa-sm">
              <div>
                当前：{{ previewData.index + 1 }} / 合计：{{
                  previewData.total
                }}
              </div>
              <div class="row justify-end q-gutter-sm">
                <q-btn
                  label="上一条"
                  color="orange"
                  size="sm"
                  @click="previousItem"
                />
                <q-btn
                  label="下一条"
                  color="orange"
                  size="sm"
                  @click="nextItem"
                />
                <q-btn v-close-popup label="退出" color="negative" size="sm" />
              </div>
            </div>
          </q-footer>

          <q-page-container>
            <div v-html="previewData.html" />
          </q-page-container>
        </q-layout>
      </q-card>
    </q-dialog>
  </div>
</template>

<script>
import { getTemplates } from '@/api/template'
import {
  newPreview,
  getPreviewData,
  getCurrentStatus,
  newSendTask,
  startSending
} from '@/api/send'

import XLSX from 'js-xlsx'
import { notifyError } from '@/components/iPrompt'
import SendingProgress from './components/sendingProgress'

import SelectSender from './mixins/selectSender.vue'
import SelectReceiver from './mixins/selectReceiver.vue'
import SelectCopyTo from './mixins/selectCopyTo.vue'

import SelectAttachment from './mixins/selectAttachment.vue'

export default {
  components: { SendingProgress },
  mixins: [SelectSender, SelectReceiver, SelectCopyTo, SelectAttachment],
  data() {
    return {
      subject: '',

      selectedFileName: '',
      options: [],
      selectedTemplate: {},

      isShowSendingDialog: false,

      isShowPreviewDialog: false,
      previewData: {},

      sendingData: {
        progress: 0,
        label: '0.00%'
      },

      disableSend: false
    }
  },
  async mounted() {
    // 获取当前状态，可能处于发送中
    const statusRes = await getCurrentStatus()

    // 没有完成，且发送 html
    if (!(statusRes.data & 1)) {
      if (statusRes.data & 32) {
        // 打开发送框
        this.isShowSendingDialog = true
      } else {
        // 关闭发送按钮
        this.disableSend = true
      }
    }

    // 获取所有模板
    const res = await getTemplates()
    this.options = res.data

    // 设置默认选项
    if (res.data && res.data.length > 0) this.selectedTemplate = res.data[0]
  },
  methods: {
    // 选择文件
    selectExcelFile() {
      const elem = document.getElementById('fileInput')
      elem.click()
      elem.value = ''
    },

    async fileSelected(e) {
      console.log('fileSelected:', e)
      // 判断是否选择了文件
      if (e.target.files.length === 0) {
        return
      }

      // 获取选择的文件
      const file = e.target.files[0]

      this.excelData = await this.readExcelData(file)
      if (!this.excelData) return
      this.selectedFileName = file.name
    },

    async readExcelData(file) {
      return new Promise((resolve, reject) => {
        const reader = new FileReader()
        reader.onload = e => {
          const data = new Uint8Array(e.target.result)
          const workbook = XLSX.read(data, { type: 'array' })
          /* DO SOMETHING WITH workbook HERE */
          // 变成json
          const jsonObj = XLSX.utils.sheet_to_json(
            workbook.Sheets[workbook.SheetNames[0]]
          )
          resolve(jsonObj)
        }
        reader.onerror = () => {
          reject(false)
        }

        reader.readAsArrayBuffer(file)
      })
    },

    async checkData() {
      if (!this.subject) {
        notifyError('请输入主题')
        return false
      }

      // 判断收件人与数据是否至少有一个
      if ((!this.receivers || this.receivers.length < 1) && !this.excelData) {
        notifyError('请选择收件人或者录入数据（两者必须有其一）')
        return false
      }

      return true
    },

    // 预览邮箱
    async previewEmailBody() {
      // 判断数据输入
      const isNewTask = await this.checkData()
      if (!isNewTask) return

      // 新建预览
      await newPreview(
        this.subject,
        this.receivers,
        this.excelData || [],
        this.selectedTemplate._id,
        this.attachments,
        this.copyToEmails
      )

      // 打开预览窗体
      this.isShowPreviewDialog = true

      // 获取第一个预览
      const res = await getPreviewData('first')
      this.previewData = res.data
    },

    async previousItem() {
      const res = await getPreviewData('previous')
      this.previewData = res.data
    },

    async nextItem() {
      const res = await getPreviewData('next')
      this.previewData = res.data
    },

    // 开始发件，发html内容
    async startSending() {
      // 判断数据输入
      const isNewTask = await this.checkData()
      if (!isNewTask) return

      // 新建发件任务
      const res = await newSendTask(
        this.senders || [],
        this.subject,
        this.receivers,
        this.excelData || [],
        this.selectedTemplate._id,
        this.attachments,
        this.copyToEmails
      )

      const { data } = res
      if (!data.ok) {
        notifyError(data.message)
        return
      }

      const ok = await new Promise((resolve, reject) => {
        this.$q
          .dialog({
            title: '发件确认<em>!</em>',
            message: `<div>选择收件：${data.selectedReceiverCount} 个</div>
            <div>录入数据：${data.dataReceiverCount} 条</div>
            <div>实际发件：${data.acctualReceiverCount} 个</div>
            <div style="font-size: 1.375em; color: crimson;margin-top: 10px;">是否继续？</div>`,
            html: true,
            ok: {
              dense: true,
              color: 'primary'
            },
            cancel: {
              dense: true,
              color: 'negative'
            },
            persistent: true
          })
          .onOk(() => {
            resolve(true)
          })
          .onCancel(() => {
            reject(false)
          })
          .onDismiss(() => {
            reject(false)
          })
      })
      if (!ok) return

      // 开始发送
      await startSending(data.historyId)

      // 获取设置，判断是否是发送图文
      // const settingsRes = await getUserSettings()
      // 如果是图文混发，不打开此处的进度条
      // 一直不打开进度条，因为全局会响应
      // if (!settingsRes.data.sendWithImageAndHtml) {
      //   this.isShowSendingDialog = true
      // }
    },

    // 定时发件
    async scheduleSend() {}
  }
}
</script>

<style lang='scss'>
.send-container {
  position: absolute;
  top: 0;
  right: 0;
  bottom: 0;
  left: 0;
  overflow-y: auto;

  .receive-box {
    border-bottom: 1px solid gray;
  }

  .send-input {
    border: none;
    outline: none;
  }

  .preview-row {
    position: fixed;
    right: 20px;
    bottom: 20px;
  }
}
</style>
