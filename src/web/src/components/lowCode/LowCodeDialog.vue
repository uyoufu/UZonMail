<template>
  <q-dialog ref="dialogRef" @hide="onDialogHide" :persistent="persistent">
    <q-card>
      <div v-if="title" class="text-subtitle1 text-primary text-bold q-mx-sm q-mt-sm">{{ title }}</div>

      <LowCodeForm :title="title" :fields="fields" :dataSet="dataSet" :validate="validate" :oneColumn="oneColumn"
        :disableDefaultBtns="disableDefaultBtns" :customBtns="customBtns" :onOkMain="onOkMain" :onSetup="onSetup"
        @ok="onFormOk" @cancel="onFormCancel" />
    </q-card>
  </q-dialog>
</template>

<script lang="ts" setup>
import { useDialogPluginComponent } from 'quasar'

import LowCodeForm from './LowCodeForm.vue'

import type { PropType } from 'vue'
import type { ICustomPopupButton, ILowCodeField, IOnSetupParams } from './types'
import type { IFunctionResult } from 'src/types'

defineProps({
  title: {
    type: String,
    default: ''
  },
  // 字段定义
  fields: {
    type: Array as PropType<Array<ILowCodeField>>,
    required: true,
    default: () => { return [] }
  },
  // 数据源
  dataSet: {
    type: Object,
    required: false,
    default: () => { return {} }
  },

  // 用于数据验证
  validate: {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    type: Function as PropType<(params: Record<string, any>) => Promise<IFunctionResult>>,
    required: false
  },

  // 窗体保持
  persistent: {
    type: Boolean,
    default: true
  },

  // ok 单击后，调用的函数
  // 在该函数中，可以向服务器发送请求，若不需要关闭窗体时，可以返回 false
  onOkMain: {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    type: Function as PropType<(params: Record<string, any>) => Promise<void | boolean>>
  },

  // 仅有一列
  oneColumn: {
    type: Boolean,
    default: false
  },

  // 禁用默认按钮
  disableDefaultBtns: {
    type: Array as PropType<Array<'ok' | 'cancel'>>,
    default: () => []
  },

  // 自定义按钮
  customBtns: {
    type: Array as PropType<Array<ICustomPopupButton>>,
    default: () => []
  },

  // 在初始化化时，调用
  onSetup: {
    type: Function as PropType<(params: IOnSetupParams) => void>,
    required: false
  }
})

defineEmits([
  // 必需；需要指定一些事件
  // （组件将通过useDialogPluginComponent()发出）
  ...useDialogPluginComponent.emits
])

const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent()

// 处理表单的 ok 事件
function onFormOk(data: Record<string, unknown>) {
  onDialogOK(data)
}

// 处理表单的 cancel 事件
function onFormCancel() {
  onDialogCancel()
}
</script>
