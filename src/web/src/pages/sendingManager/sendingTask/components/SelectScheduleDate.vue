<template>
  <q-dialog ref='dialogRef' @hide="onDialogHide" :persistent='true'>
    <q-card class='send-schedule-container column items-center q-pa-md no-wrap'>
      <div class="text-subtitle q-mb-sm row items-center no-wrap">
        <span>定时发送: </span>
        <span v-if="isDesktop" class="text-primary">{{ modelValue }}</span>
        <div v-else>
          <q-btn flat dense color="primary" :label="dateStr" @click="onShowDateSelector" />
          <q-btn flat dense color="primary" :label="timeStr" @click="onShowTimeSelector" />
          <span class="text-caption">单击切换</span>
        </div>
      </div>

      <div class="q-gutter-md row items-start justify-center">
        <q-date v-show="isShowDateSelector" v-model="modelValue" mask="YYYY-MM-DD HH:mm" color="primary" />
        <q-time v-show="isShowTimeSelector" v-model="modelValue" mask="YYYY-MM-DD HH:mm" color="secondary" format24h />
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

// #region 移动端优化
import { Platform } from 'quasar'
const isDesktop = Platform.is.desktop
const dateStr = computed(() => {
  return dayjs(modelValue.value).format('YYYY-MM-DD')
})
const timeStr = computed(() => {
  return dayjs(modelValue.value).format('HH:mm')
})
const isShowDateSelector = ref(true)
const isShowTimeSelector = ref(isDesktop)
function onShowDateSelector () {
  isShowDateSelector.value = true
  isShowTimeSelector.value = false
}
function onShowTimeSelector () {
  isShowDateSelector.value = false
  isShowTimeSelector.value = true
}
// #endregion
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
