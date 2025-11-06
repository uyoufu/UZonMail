<template>
  <div class="card-like column q-pa-md hover-scroll no-wrap">
    <q-input v-model="emailInfo.subjects" autogrow :label="translateSendingTask('subject')" dense
      :placeholder="translateSendingTask('subjectPlaceholder')" class="email-subject q-mb-sm" style="max-height:200px">
      <template v-slot:before>
        <q-icon name="subject" color="primary" />
      </template>
    </q-input>

    <SelectEmailTemplate v-model="emailInfo.templates" class="q-mb-sm" />

    <SelectEmailData v-model="emailInfo.data" class="q-mb-sm" />

    <SelectEmailBox v-model="emailInfo.outboxes" v-model:selectedGroups="emailInfo.outboxGroups" :emailBoxType="0"
      icon="directions_run" :label="translateSendingTask('sender')" class="q-mb-sm" icon-color="secondary"
      :placeholder="translateSendingTask('senderPlaceholder')" />

    <div class="q-mb-sm row justify-start items-center">
      <SelectEmailBox class="col" v-model="emailInfo.inboxes" v-model:selectedGroups="emailInfo.inboxGroups"
        :emailBoxType="1" icon="hail" :label="translateSendingTask('recipients')"
        :placeholder="translateSendingTask('recipientsPlaceholder')" />

      <q-checkbox dense keep-color v-model="emailInfo.sendBatch" :label="translateSendingTask('mergeToSend')"
        color="secondary" :disable="disableSendBatchCheckbox">
        <AsyncTooltip anchor="bottom left" self="top start" :tooltip="sendBatchTooltips" />
      </q-checkbox>
    </div>

    <SelectEmailBox v-model="emailInfo.ccBoxes" :emailBoxType="1" icon="settings_accessibility"
      :label="translateSendingTask('ccRecipients')" :placeholder="translateSendingTask('ccRecipientsPlaceholder')"
      class="q-mb-sm" icon-color="secondary" />

    <TemplateEditor class="q-pa-xs q-ma-sm" v-model="emailInfo.body" :show-title-bar="false" :height="300">
    </TemplateEditor>

    <ObjectUploader v-model="emailInfo.attachments" v-model:need-upload="needUpload"
      :label="translateSendingTask('attachments')" class="q-mx-sm q-mt-sm" style="width:auto" multiple />

    <div class="row justify-between items-center q-ma-sm q-mt-lg">
      <CommonBtn label="" :color="proxyBtnColor" icon="public" :tooltip="proxyBtnTooltip" @click="onProxyBtnClick"
        :cache-tip="false" />

      <div class="row justify-end items-center">
        <CommonBtn :label="translateSendingTask('btn_preview')" color="primary" icon="view_carousel"
          :tooltip="translateSendingTask('btn_previewTooltip')" @click="onPreviewClick" />
        <CommonBtn :label="translateSendingTask('btn_schedule')" class="q-ml-sm" color="secondary" icon="schedule"
          :tooltip="translateSendingTask('btn_scheduleTooltip')" @click="onScheduleSendClick" />
        <OkBtn :label="translateSendingTask('btn_send')" class="q-ml-sm" icon="alternate_email"
          :tooltip="translateSendingTask('btn_sendTooltip')" @click="onSendNowClick" />
        <OkBtn v-if="enableIpWarmUpBtn" :label="translateSendingTask('btn_warmup')" class="q-ml-sm" icon="autorenew"
          :tooltip="translateSendingTask('btn_warmupTooltip')" @click="onIpWarmUpClick" />
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import SelectEmailTemplate from './components/SelectEmailTemplate.vue'
import SelectEmailBox from './components/SelectEmailBox.vue'
import SelectEmailData from './components/SelectEmailData.vue'
import ObjectUploader from 'components/uploader/ObjectUploader.vue'
import AsyncTooltip from 'components/asyncTooltip/AsyncTooltip.vue'
import TemplateEditor from 'src/pages/sourceManager/templateManager/templateEditor.vue'

import { useBottomFunctions } from './bottomFunctions'
import { translateSendingTask } from 'src/i18n/helpers'

import type { IEmailCreateInfo } from 'src/api/emailSending'

// 设置名称
defineOptions({
  name: 'SendingTask'
})

const emailInfo: Ref<IEmailCreateInfo> = ref({
  subjects: '', // 主题
  templates: [], // 模板 id
  data: [], // 用户发件数据
  inboxGroups: [],
  outboxes: [], // 发件人邮箱
  outboxGroups: [],
  inboxes: [], // 收件人邮箱
  ccBoxes: [], // 抄送人邮箱
  bccBoxes: [], // 密送人邮箱
  body: '', // 邮件正文
  // 附件必须先上传，此处保存的是附件的Id
  attachments: [], // 附件
  sendBatch: false,
  proxyIds: []
})

// 底部功能按钮
const {
  needUpload, OkBtn, CommonBtn,
  onPreviewClick, onScheduleSendClick, onSendNowClick, validateSendingTaskParams
} = useBottomFunctions(emailInfo)

// 合并发送
const sendBatchTooltips = translateSendingTask('sendBatchTooltips')
// 进行重置
// inboxes 数量太少时不批量
watch(() => emailInfo.value.inboxes, (newValue) => {
  if (newValue.length < 2) emailInfo.value.sendBatch = false
})
// 有数据时，不批量
watch(() => emailInfo.value.data, (newValue) => {
  if (newValue.length > 0) emailInfo.value.sendBatch = false
})
// 多个发件箱时，不批量
watch(() => emailInfo.value.outboxes, (newValue) => {
  if (newValue.length > 1) emailInfo.value.sendBatch = false
})
watch(() => emailInfo.value.outboxGroups, (newValue) => {
  if (newValue.length > 0) emailInfo.value.sendBatch = false
})
// 格式化 body，可能有一些无用的字符, 比如 \n, <br>
watch(() => emailInfo.value.body, (newValue) => {
  // 去除所有标签和空白字符
  const text = newValue.replace(/<[^>]*>/g, '').replace(/&nbsp;/g, '').trim()
  if (text.length === 0) emailInfo.value.body = ''
})

const disableSendBatchCheckbox = computed(() => {
  return emailInfo.value.data.length === 0 && emailInfo.value.inboxes.length < 2 && emailInfo.value.outboxes.length < 2
})

// #region 代理相关
import { useProxyAdder } from './compositions/useProxyAdder'
const { proxyBtnTooltip, proxyBtnColor, onProxyBtnClick } = useProxyAdder(emailInfo)
// #endregion

// #region 使用模板
import { useSendingGroupTemplate } from './compositions/useSendingGroupTemplate'
useSendingGroupTemplate(emailInfo)
// #endregion

// #region IP预热
import { useIpWarmUp } from './compositions/useIpWarmUp'
const { onIpWarmUpClick, enableIpWarmUpBtn } = useIpWarmUp(validateSendingTaskParams, emailInfo)
// #endregion
</script>

<style lang="scss" scoped>
.email-subject :deep(textarea) {
  max-height: 120px;
  overflow-y: auto;
}

.template-editor-container {
  height: 250px;
  max-height: 300px;
  border: 2px solid $grey-12
}
</style>
