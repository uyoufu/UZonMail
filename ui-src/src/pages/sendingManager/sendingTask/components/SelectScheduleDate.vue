<template>
  <q-dialog ref='dialogRef' @hide="onDialogHide" :persistent='true'>
    <q-card class='send-schedule-container column items-center q-pa-md no-wrap'>
      <div class="text-subtitle1 q-mb-sm">
        <span>指定发送时间: </span>
        <span class="text-primary">{{ modelValue }}</span>
      </div>

      <div class="q-gutter-md row items-start justify-center">
        <q-date v-model="modelValue" mask="YYYY-MM-DD HH:mm" color="primary" />
        <q-time v-model="modelValue" mask="YYYY-MM-DD HH:mm" color="secondary" format24h />
      </div>

      <div class="row justify-end items-center q-mt-md full-width">
        <CancelBtn @click="onDialogCancel" />
        <OkBtn class="q-ml-sm" @click="onOkClick" />
      </div>
    </q-card>
  </q-dialog>
</template>

<script lang='ts' setup>
/**
 * warning: 该组件是一个弹窗的示例，不可直接使用
 * 参考：http://www.quasarchs.com/quasar-plugins/dialog#composition-api-variant
 */

import { useDialogPluginComponent } from 'quasar'
defineEmits([
  // 必需；需要指定一些事件
  // （组件将通过useDialogPluginComponent()发出）
  ...useDialogPluginComponent.emits
])
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent()

import dayjs from 'dayjs'
const modelValue = ref(dayjs().format('YYYY-MM-DD HH:mm'))

import OkBtn from 'src/components/quasarWrapper/buttons/OkBtn.vue'
import CancelBtn from 'src/components/quasarWrapper/buttons/CancelBtn.vue'
import { notifyError } from 'src/utils/dialog'
function onOkClick () {
  // 验证日期是否大于当前日期
  if (dayjs(modelValue.value).isBefore(dayjs().add(1, 'minute'))) {
    notifyError('指定发送时间至少推迟 1分钟')
    return
  }

  onDialogOK(modelValue.value)
}
</script>

<style lang='scss' scoped>
.send-schedule-container {
  :deep(.q-time__header) {
    padding-top: 0px;
    padding-bottom: 0px;
    height: 60px;
    min-height: 60px;
  }

  :deep(.q-date__header) {
    padding-top: 2px;
    padding-bottom: 2px;
    height: 60px;
    min-height: 60px;
  }
}
</style>
