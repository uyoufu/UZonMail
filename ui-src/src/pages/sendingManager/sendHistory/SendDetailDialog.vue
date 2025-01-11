<template>
  <q-dialog ref='dialogRef' @hide="onDialogHide">
    <q-card class='column items-center items-start'>
      <SendingDetailTable :sending-group-id="props.sendingGroupId" />
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
const { dialogRef, onDialogHide /* , onDialogOK, onDialogCancel */ } = useDialogPluginComponent()

const props = defineProps({
  sendingGroupId: {
    type: Number,
    required: true
  }
})
import SendingDetailTable from './SendDetailTable.vue'
</script>

<style lang='scss' scoped></style>
