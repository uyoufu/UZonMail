<template>
  <q-dialog ref='dialogRef' @hide="onDialogHide">
    <q-card class='column items-start q-pa-sm'>
      <div class="text-subtitle1 text-primary">退订页面预览</div>
      <q-separator class="full-width" />
      <UnsubscribePage class="q-mt-sm" :html-content="htmlContent" />
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
  htmlContent: {
    type: String,
    required: true
  }
})

// #region 显示内容
import UnsubscribePage from './UnsubscribePage.vue'
// #endregion
</script>

<style lang='scss' scoped></style>
