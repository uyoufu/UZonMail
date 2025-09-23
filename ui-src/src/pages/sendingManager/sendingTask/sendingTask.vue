<template>
  <div class="card-like column q-pa-md hover-scroll no-wrap">
    <q-input v-model="emailInfo.subjects" autogrow label="主题" dense
      placeholder="请输入邮件主题(若需要随机主题，多个主题之间请使用分号 ; 进行分隔或者单独一行)" class="email-subject q-mb-sm" style="max-height:200px">
      <template v-slot:before>
        <q-icon name="subject" color="primary" />
      </template>
    </q-input>

    <SelectEmailTemplate v-model="emailInfo.templates" class="q-mb-sm" />

    <SelectEmailData v-model="emailInfo.data" class="q-mb-sm" />

    <SelectEmailBox v-model="emailInfo.outboxes" v-model:selectedGroups="emailInfo.outboxGroups" :emailBoxType="0"
      icon="directions_run" label="发件人" class="q-mb-sm" icon-color="secondary" placeholder="请选择发件箱 (必须)" />

    <div class="q-mb-sm row justify-start items-center">
      <SelectEmailBox class="col" v-model="emailInfo.inboxes" v-model:selectedGroups="emailInfo.inboxGroups"
        :emailBoxType="1" icon="hail" label="收件人" placeholder="请选择收件箱 (必须)" />

      <q-checkbox dense keep-color v-model="emailInfo.sendBatch" label="合并" color="secondary"
        :disable="disableSendBatchCheckbox">
        <AsyncTooltip anchor="bottom left" self="top start" :tooltip="sendBatchTooltips" />
      </q-checkbox>
    </div>

    <SelectEmailBox v-model="emailInfo.ccBoxes" :emailBoxType="1" icon="settings_accessibility" label="抄送人"
      placeholder="请选择抄送人 (可选)" class="q-mb-sm" icon-color="secondary" />

    <TemplateEditor class="q-pa-xs q-ma-sm" v-model="emailInfo.body" :show-title-bar="false" :height="300">
    </TemplateEditor>

    <ObjectUploader v-model="emailInfo.attachments" v-model:need-upload="needUpload" label="附件" class="q-mx-sm q-mt-sm"
      style="width:auto" multiple />

    <div class="row justify-between items-center q-ma-sm q-mt-lg">
      <CommonBtn label="" :color="proxyBtnColor" icon="public" :tooltip="proxyBtnTooltip" @click="onProxyBtnClick"
        :cache-tip="false" />

      <div class="row justify-end items-center">
        <CommonBtn label="预览" color="primary" icon="view_carousel" tooltip="预览发件正文" @click="onPreviewClick" />
        <CommonBtn label="定时" class="q-ml-sm" color="secondary" icon="schedule" tooltip="定时发件"
          @click="onScheduleSendClick" />
        <CommonBtn v-if="enableIpWarmUpBtn" label="预热" class="q-ml-sm" color="secondary" icon="autorenew"
          tooltip="IP 预热发件" @click="onIpWarmUpClick" />
        <OkBtn label="发送" class="q-ml-sm" icon="alternate_email" tooltip="立即发件" @click="onSendNowClick" />
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

import type { IEmailCreateInfo } from 'src/api/emailSending'

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
const { needUpload, OkBtn, CommonBtn, onPreviewClick, onScheduleSendClick, onSendNowClick } = useBottomFunctions(emailInfo)

// 合并发送
const sendBatchTooltips = ['若有多个发件人,将其合并到一封邮件中发送', '启用后无法对单个发件箱进行重发', '一般不建议启用']
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
const { onIpWarmUpClick, enableIpWarmUpBtn } = useIpWarmUp()
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
