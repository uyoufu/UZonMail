<template>
  <q-dialog ref="dialogRef" @hide="onDialogHide" :persistent="persistent">
    <q-card style="min-width: 250px;">
      <!--
        ... 内容
        ... 用q-card-section来做？
      -->
      <div v-if="title" class="text-subtitle1 text-primary text-bold q-mx-md q-mt-sm">{{ title }}</div>

      <div class="q-py-md q-px-xs justify-start items-center" :class="getContainerClass()">
        <template v-for="field in validFields" :key="field.name">
          <q-input v-if="isMatchedType(field, commonInputTypes)" outlined class="q-mb-sm low-code__field q-px-sm"
            standout dense v-model="fieldsModel[field.name]" :type="(field.type as any)" :label="field.label"
            :placeholder="field.placeholder" :disable="field.disable">
            <template v-if="field.icon" v-slot:prepend>
              <q-icon :name="field.icon" />
            </template>

            <AsyncTooltip :tooltip="field.tooltip" />
          </q-input>

          <q-input v-if="isMatchedType(field, ['textarea'])" outlined class="q-mb-sm low-code__field q-px-sm" standout
            dense v-model="fieldsModel[field.name]" type="textarea" autogrow :label="field.label"
            :disable="field.disable" :placeholder="field.placeholder || '可 Enter 换行'">
            <template v-if="field.icon" v-slot:prepend>
              <q-icon :name="field.icon" />
            </template>

            <AsyncTooltip :tooltip="field.tooltip" />
          </q-input>

          <PasswordInput v-if="isMatchedType(field, 'password')" class="q-mb-sm low-code__field q-px-sm" no-icon
            :label="field.label" v-model="fieldsModel[field.name]" dense>
            <AsyncTooltip :tooltip="field.tooltip" />
          </PasswordInput>

          <q-select v-if="isMatchedType(field, ['selectOne', 'selectMany'])" class="q-mb-sm low-code__field q-px-sm"
            outlined v-model="fieldsModel[field.name]" :options="field.options" :label="field.label"
            :disable="field.disable" dense :option-label="field.optionLabel" :option-value="field.optionValue"
            options-dense :multiple="isMatchedType(field, 'selectMany')" :map-options="field.mapOptions"
            :emit-value="field.emitValue">
            <AsyncTooltip :tooltip="field.tooltip" />
            <template v-slot:option="{ itemProps, opt, selected, toggleOption }">
              <q-item v-bind="itemProps">
                <q-item-section>
                  {{ getSelectionItemLabel(itemProps, opt, field) }}
                </q-item-section>
                <q-item-section side>
                  <q-toggle color="secondary" :model-value="selected" @update:model-value="toggleOption(opt)" dense />
                </q-item-section>
                <AsyncTooltip :tooltip="getSelectionItemTooltip(itemProps, opt, field)" />
              </q-item>
            </template>
          </q-select>

          <q-checkbox v-if="isMatchedType(field, 'boolean')" class="q-mb-sm low-code__field q-px-sm" dense keep-color
            v-model="fieldsModel[field.name]" :label="field.label">
            <AsyncTooltip anchor="bottom left" self="top start" :tooltip="field.tooltip" />
          </q-checkbox>

          <div v-if="isMatchedType(field, 'editor')" class="q-mb-sm low-code__field q-px-sm">
            <q-editor v-model="fieldsModel[field.name]" :definitions="editorDefinitions" :toolbar="editorToolbar"
              max-height="300px" placeholder="在此处输入模板内容, 变量使用 {{ }} 号包裹, 例如 {{ variableName }}">
            </q-editor>
          </div>
        </template>
      </div>

      <!-- 按钮的例子 -->
      <q-card-actions align="right">
        <CommonBtn v-for="btn in customBtns" :key="btn.label" @click="onCustomBottonClicked(btn)" :label="btn.label"
          :color="btn.color" />
        <CancelBtn v-if="!disableDefaultBtns.includes('cancel')" @click="onDialogCancel"></CancelBtn>
        <OkBtn v-if="!disableDefaultBtns.includes('ok')" :loading="okBtnLoading" @click="onOKClick"></OkBtn>
      </q-card-actions>
    </q-card>
  </q-dialog>
</template>

<script lang="ts" setup>
import { useDialogPluginComponent } from 'quasar'
import dayjs from 'dayjs'

import CommonBtn from '../quasarWrapper/buttons/CommonBtn.vue'
import OkBtn from 'src/components/quasarWrapper/buttons/OkBtn.vue'
import CancelBtn from 'src/components/quasarWrapper/buttons/CancelBtn.vue'
import AsyncTooltip from 'src/components/asyncTooltip/AsyncTooltip.vue'
import PasswordInput from '../passwordInput/PasswordInput.vue'

import type { PropType } from 'vue'
import type { ICustomPopupButton, IPopupDialogField } from './types';
import { PopupDialogFieldType } from './types'
import { notifyError } from 'src/utils/dialog'
import type { IFunctionResult } from 'src/types'

const props = defineProps({
  title: {
    type: String,
    default: ''
  },
  // 字段定义
  fields: {
    type: Array as PropType<Array<IPopupDialogField>>,
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
  }
})

// 是否为匹配到的类型
const commonInputTypes = ["text", "email", "search", "tel", "file", "number", "url", "time", "date", "datetime-local"]
function isMatchedType (field: IPopupDialogField, types: string | string[]): boolean {
  if (Array.isArray(types)) return types.includes(field.type as string)
  // eslint-disable-next-line @typescript-eslint/no-unsafe-enum-comparison
  return field.type === types
}

function getContainerClass () {
  return {
    'low-code__container_1': props.oneColumn,
    'low-code__container_2': !props.oneColumn,
    row: !props.oneColumn,
    column: props.oneColumn
  }
}

// #region 编辑器
// 编辑器配置
import { useWysiwygEditor } from 'src/pages/templateManager/compositions'
const { editorDefinitions, editorToolbar } = useWysiwygEditor()
// #endregion

// #region 数据初始化
const { fields, dataSet } = toRefs(props)
// eslint-disable-next-line @typescript-eslint/no-explicit-any
const dataSetRef: Ref<Record<string, any>> = ref({})
// eslint-disable-next-line @typescript-eslint/no-explicit-any
const fieldsModel: Ref<Record<string, any>> = ref({})
async function pullDateSet () {
  // 获取数据源
  for (const key of Object.keys(dataSet.value)) {
    const value = dataSet.value[key]
    switch (typeof value) {
      case 'function':
        // 函数
        dataSetRef.value[key] = await value()
        break
      default:
        dataSetRef.value[key] = value
    }
  }
}
function initFieldsModel () {
  // 生成初始值
  for (const field of fields.value) {
    // 根据不同的类型，生成不同的初始值
    switch (field.type) {
      case PopupDialogFieldType.text:
        fieldsModel.value[field.name] = field.value || ''
        break
      case PopupDialogFieldType.date:
        fieldsModel.value[field.name] = field.value ? dayjs(field.value as string).format('YYYY-MM-DD') : ''
        break
      case PopupDialogFieldType.number:
        fieldsModel.value[field.name] = field.value || 0
        break
      default:
        fieldsModel.value[field.name] = (field.value === undefined || field.value === null) ? '' : field.value
    }
  }
}
initFieldsModel()
console.log('fieldsModel:', fieldsModel.value, fields)

onMounted(async () => {
  // 初始化数据源
  await pullDateSet()
})

// 有效的字段
const validFields = computed(() => {
  const results = []
  for (const field of fields.value) {
    // 过滤没有 name 和 type 的字段
    if (!field.name || !field.type) {
      continue
    }

    // 对字段内容进行格式化，比如单选可能需要从数据源中获取

    results.push(field)
  }

  return results
})
// #endregion

// #region 单选或多选
// import logger from 'loglevel'

// eslint-disable-next-line @typescript-eslint/no-explicit-any
function getSelectionItemLabel (itemProps: any, opt: any, field: IPopupDialogField) {
  const labelField = field.optionLabel || 'label'

  if (!field || !opt) return opt
  if (typeof opt !== 'object') return opt

  return opt[labelField]
}
// eslint-disable-next-line @typescript-eslint/no-explicit-any
function getSelectionItemTooltip (itemProps: any, opt: any, field: IPopupDialogField) {
  // logger.debug('[popupDialog] getSelectionItemTooltip:', opt, field)
  if (!field || !field.optionTooltip || !opt) return ''
  if (typeof opt !== 'object') return opt
  return opt[field.optionTooltip]
}
// #endregion

// #region quasar 弹窗逻辑
defineEmits([
  // 必需；需要指定一些事件
  // （组件将通过useDialogPluginComponent()发出）
  ...useDialogPluginComponent.emits
])

const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent()
// dialogRef - 应用于QDialog的Vue引用。
// onDialogHide - 用作QDialog上@hide的处理函数。
// onDialogOK - 调用函数来处理结果为"确定"的对话框。
//              例子: onDialogOK() - 没有有效载荷
//              例子: onDialogOK({ /*...*/ }) -- 有有效载荷
// onDialogCancel - 调用函数来处理结果为"取消"的对话框。

// ok 逻辑
const okBtnLoading = ref(false)
async function onOKClick () {
  okBtnLoading.value = true
  try {
    // 验证单个值
    for (const field of validFields.value) {
      const fieldValue = fieldsModel.value[field.name]
      // 若是必须的，则验证是否为 null 或者 undefined
      if (field.required) {
        if (!fieldValue && fieldValue !== false) {
          // 提示错误
          notifyError(`${field.label} 不能为空`)
          return
        }
      }

      // 对结果进行转换
      if (typeof field.parser === 'function') {
        fieldsModel.value[field.name] = await field.parser(fieldValue)
      }

      // 验证函数
      if (typeof field.validate === 'function') {
        const fieldVdResult = await field.validate(fieldValue, fieldsModel.value[field.name])
        if (!fieldVdResult.ok) {
          // 提示错误
          notifyError(fieldVdResult.message ? `${fieldVdResult.message}` : `${field.label} 数据格式错误`)
          return
        }
      }
    }

    // 对所有的结果进行转换
    // 后期有需要再增加

    // 根据结果，调用所有参数的验证函数
    if (typeof props.validate === 'function') {
      const modelVdResult = await props.validate(fieldsModel.value)
      if (!modelVdResult.ok) {
        notifyError(modelVdResult.message)
        return
      }
    }

    // 验证完成后，若有，则调用 onOkClick 函数
    if (typeof props.onOkMain === 'function') {
      const mainResult = await props.onOkMain(fieldsModel.value)
      if (mainResult === false) return
    }
  } finally {
    okBtnLoading.value = false
  }

  // 在"确定"时，它必须要
  // 调用onDialogOK（带可选的有效载荷）。
  onDialogOK(fieldsModel.value)
  // 或使用有效载荷：onDialogOK({ ... })
  // ...它还会自动隐藏对话框
}
// #endregion

// #region 自定义按钮
async function onCustomBottonClicked (btn: ICustomPopupButton) {
  // 调用
  if (typeof btn.onClick !== 'function') {
    notifyError('自定义按钮没有注册 onClick 函数')
  }

  await btn.onClick(fieldsModel.value)
}
// #endregion
</script>

<style lang="scss" scoped>
.low-code__container_2 {
  display: flex;
  flex-wrap: wrap;

  .low-code__field {
    flex: 1 1 100%;

    @media screen and (min-width: 600px) {
      flex: 1 1 50%;
      max-width: 50%;
    }
  }
}

.low-code__container_1 {
  .low-code__field {
    width: 100%;
    flex: 1 1 100%;
    min-width: 300px;
  }
}
</style>
