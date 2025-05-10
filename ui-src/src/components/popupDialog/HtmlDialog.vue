<template>
  <q-dialog ref='dialogRef' @hide="onDialogHide" :persistent="false">
    <q-card class='column items-start q-pa-sm'>
      <div class="text-subtitle1 text-primary text-bold full-width">{{ title }}</div>
      <div v-html="html" class="flex"></div>
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

defineProps({
  // 标题
  title: {
    type: String,
    default: () => 'HTML'
  },

  // 内容
  html: {
    type: String,
    required: true
  }
})
</script>

<style lang='scss' scoped></style>
