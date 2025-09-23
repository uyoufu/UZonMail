<template>
  <q-dialog ref='dialogRef' @hide="onDialogHide" :persistent='true'>
    <q-card class='column items-center items-start'>
      <q-date v-model="modelValue" range :mask="dateMask" color="primary" />
      <div class="row justify-end items-center q-mb-sm q-pr-sm full-width">
        <CancelBtn />
        <OkBtn class="q-ml-sm" />
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
const { dialogRef, onDialogHide/* , onDialogOK, onDialogCancel */ } = useDialogPluginComponent()


// #region 时期范围选择
import dayjs from 'dayjs'
import logger from 'loglevel'
const dateMask = 'YYYY-MM-DD'
const modelValue = ref<{ from: string, to: string }>({ from: dayjs().format(dateMask), to: dayjs().add(1, 'month').format(dateMask) })
// #endregion
</script>

<style lang='scss' scoped></style>
