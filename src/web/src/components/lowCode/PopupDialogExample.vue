<template>
  <q-dialog ref="dialogRef" :persistent="false">
    <q-card class="column items-center items-start">
      this is dialog example
    </q-card>
  </q-dialog>
</template>

<script lang="ts" setup>
/**
 * warning: 该组件一个弹窗的示例，不可直接使用
 * 参考：http://www.quasarchs.com/quasar-plugins/dialog#composition-api-variant
 */

import { useDialogPluginComponent } from 'quasar'
defineEmits([
  // 必需；需要指定一些事件
  // （组件将通过useDialogPluginComponent()发出）
  ...useDialogPluginComponent.emits
])
const { dialogRef /* , onDialogOK, onDialogCancel */ } = useDialogPluginComponent()
</script>

<style lang="scss" scoped></style>
