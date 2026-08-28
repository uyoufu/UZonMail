<template>
  <div class="low-code_form-container" @keydown.enter="onEnterKeyPress">
    <div class="q-py-xs q-px-xs row justify-start items-center" :class="getContainerClass()">
      <template v-for="field in validFields" :key="field.name">
        <q-input v-if="isMatchedType(field, commonInputTypes)" outlined class="q-mb-sm low-code__field q-px-xs"
          :class="[fieldClass, field.classes]" standout dense v-model="fieldsModel[field.name]"
          :type="(field.type as any)" :label="field.label" :placeholder="field.placeholder" :hint="field.hint"
          :autofocus="field.autofocus" :disable="field.disable">
          <template v-if="field.icon" v-slot:prepend>
            <q-icon :name="field.icon" />
          </template>
          <AsyncTooltip :tooltip="field.tooltip" />
        </q-input>

        <q-input v-if="isMatchedType(field, ['number'])" outlined class="q-mb-sm low-code__field q-px-xs"
          :class="[fieldClass, field.classes]" standout dense v-model.number="fieldsModel[field.name]" type="number"
          :label="field.label" :placeholder="field.placeholder" :hint="field.hint" :autofocus="field.autofocus"
          :disable="field.disable">
          <template v-if="field.icon" v-slot:prepend>
            <q-icon :name="field.icon" />
          </template>
          <AsyncTooltip :tooltip="field.tooltip" />
        </q-input>

        <q-input v-if="isMatchedType(field, ['textarea'])" outlined class="q-mb-sm low-code__field q-px-xs"
          :class="[fieldClass, field.classes]" standout dense v-model="fieldsModel[field.name]" type="textarea"
          :autogrow="!field.disableAutoGrow" :label="field.label" :hint="field.hint" :autofocus="field.autofocus"
          :disable="field.disable" :placeholder="field.placeholder || '按 Enter 可换行'">
          <template v-if="field.icon" v-slot:prepend>
            <q-icon :name="field.icon" />
          </template>
          <AsyncTooltip :tooltip="field.tooltip" />
        </q-input>

        <PasswordInput v-if="isMatchedType(field, 'password')" class="q-mb-sm low-code__field q-px-xs"
          :class="[fieldClass, field.classes]" no-icon :label="field.label" :placeholder="field.placeholder"
          v-model="fieldsModel[field.name]" dense>
          <AsyncTooltip :tooltip="field.tooltip" />
        </PasswordInput>

        <q-select v-if="isMatchedType(field, ['selectOne', 'selectMany'])" class="q-mb-sm low-code__field q-px-xs"
          :class="[fieldClass, field.classes]" outlined v-model="fieldsModel[field.name]" :options="field.options"
          :label="field.label" :hint="field.hint" :autofocus="field.autofocus" :disable="field.disable" dense
          options-dense :option-label="field.optionLabel" :option-value="field.optionValue"
          :multiple="isMatchedType(field, 'selectMany')" :map-options="field.mapOptions" :emit-value="field.emitValue">
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

        <q-checkbox v-if="isMatchedType(field, 'boolean')" class="q-mb-sm low-code__field q-px-xs"
          :class="[fieldClass, field.classes]" dense keep-color v-model="fieldsModel[field.name]" :label="field.label">
          <AsyncTooltip anchor="bottom left" self="top start" :tooltip="field.tooltip" />
        </q-checkbox>

        <div v-if="isMatchedType(field, 'editor')" class="q-mb-sm low-code__field q-px-sm" :class="field.classes">
          <q-editor v-model="fieldsModel[field.name]" :definitions="editorDefinitions" :toolbar="editorToolbar"
            max-height="300px" placeholder="在此处输入模板内容, 变量使用 {{ }} 号包裹, 例如 {{ variableName }}">
          </q-editor>
        </div>
      </template>
    </div>

    <!-- 按钮的例子 -->
    <div v-if="customBtns.length > 0 || disableDefaultBtns.length < 2" class="q-pa-sm"
      style="display: flex; justify-content: flex-end; gap: 8px;">
      <CommonBtn v-for="btn in customBtns" :key="btn.label" @click="onCustomButtonClicked(btn)" :label="btn.label"
        :color="btn.color" />
      <CancelBtn v-if="!disableDefaultBtns.includes('cancel')" @click="onCancel"></CancelBtn>
      <OkBtn v-if="!disableDefaultBtns.includes('ok')" :loading="okBtnLoading" @click="onOKClick"></OkBtn>
    </div>
  </div>
</template>

<script lang="ts" setup>
import dayjs from 'dayjs'

import CommonBtn from '../buttons/CommonBtn.vue'
import OkBtn from 'src/components/buttons/OkBtn.vue'
import CancelBtn from 'src/components/buttons/CancelBtn.vue'
import AsyncTooltip from 'src/components/asyncTooltip/AsyncTooltip.vue'
import PasswordInput from '../passwordInput/PasswordInput.vue'

import type { PropType } from 'vue'
import type { ICustomPopupButton, ILowCodeField, IOnSetupParams } from './types'
import { LowCodeFieldType } from './types'
import { notifyError } from 'src/utils/notification'
import type { IFunctionResult } from 'src/types'

const props = defineProps({
  // 字段定义
  fields: {
    type: Array as PropType<Array<ILowCodeField>>,
    required: true,
    default: () => { return [] }
  },

  // 是否同时值到 fields 中的 value 字段上
  syncValue: {
    type: Boolean,
    default: false
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

const emit = defineEmits<{
  ok: [data: Record<string, unknown>]
  cancel: []
}>()

// 字段的基础样式类，用于控制布局
const fieldClass = computed(() => {
  if (props.oneColumn) return 'col-12'
  // 使用 col-grow 配合 CSS 中的 min-width，可以实现按父级宽度动态显示列
  return 'col-grow'
})

// 是否为匹配到的类型
const commonInputTypes = ["text", "email", "search", "tel", "file", "url", "time", "date", "datetime-local"]
function isMatchedType(field: ILowCodeField, types: string | string[]): boolean {
  if (Array.isArray(types)) return types.includes(field.type as string)
  // eslint-disable-next-line @typescript-eslint/no-unsafe-enum-comparison
  return field.type === types
}

function getContainerClass() {
  return {
    'low-code__container_1': props.oneColumn,
    'low-code__container_2': !props.oneColumn,
    row: !props.oneColumn,
    column: props.oneColumn
  }
}
// #endregion

// #region 编辑器
import { useWysiwygEditor } from './useWysiwygEditor'
const { editorDefinitions, editorToolbar } = useWysiwygEditor()
// #endregion

// #region 数据初始化
const { fields, dataSet } = toRefs(props)
// eslint-disable-next-line @typescript-eslint/no-explicit-any
const dataSetRef: Ref<Record<string, any>> = ref({})
// eslint-disable-next-line @typescript-eslint/no-explicit-any
const fieldsModel: Ref<Record<string, any>> = ref({})
async function pullDateSet() {
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
function initFieldsModel() {
  // 生成初始值
  for (const field of fields.value) {
    // 根据不同的类型，生成不同的初始值
    switch (field.type) {
      case LowCodeFieldType.text:
        fieldsModel.value[field.name] = field.value || ''
        break
      case LowCodeFieldType.date:
        fieldsModel.value[field.name] = field.value ? dayjs(field.value as string).format('YYYY-MM-DD') : ''
        break
      case LowCodeFieldType.number:
        fieldsModel.value[field.name] = field.value || 0
        break
      default:
        fieldsModel.value[field.name] = (field.value === undefined || field.value === null) ? '' : field.value
    }
  }
}
initFieldsModel()

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

    if (field.visible === false) {
      continue
    }

    if (typeof field.visible === 'function') {
      const isVisible = field.visible(fieldsModel.value)
      if (!isVisible) {
        continue
      }
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
function getSelectionItemLabel(itemProps: any, opt: any, field: ILowCodeField) {
  const labelField = field.optionLabel || 'label'

  if (!field || !opt) return opt
  if (typeof opt !== 'object') return opt

  return opt[labelField]
}
// eslint-disable-next-line @typescript-eslint/no-explicit-any
function getSelectionItemTooltip(itemProps: any, opt: any, field: ILowCodeField) {
  // logger.debug('[lowCode] getSelectionItemTooltip:', opt, field)
  if (!field || !field.optionTooltip || !opt) return ''
  if (typeof opt !== 'object') return opt
  return opt[field.optionTooltip]
}
// #endregion

// ok 逻辑
const okBtnLoading = ref(false)
async function onOKClick() {
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
        const fieldVdResult = await field.validate(fieldValue, fieldsModel.value[field.name], fieldsModel.value)
        if (!fieldVdResult.ok) {
          // 恢复失败项
          fieldsModel.value[field.name] = fieldValue // 恢复原值
          // 提示错误
          notifyError(fieldVdResult.message ? `${fieldVdResult.message}` : `${field.label} 数据格式错误`)
          return
        }
      }
    }

    // 对所有的结果进行转换
    // 后期有需要再增加

    // 根据结果，调用全局验证函数
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

  // 发出 ok 事件
  emit('ok', fieldsModel.value)
}
// #endregion

// #region 自定义按钮
async function onCustomButtonClicked(btn: ICustomPopupButton) {
  // 调用
  if (typeof btn.onClick !== 'function') {
    notifyError('自定义按钮没有注册 onClick 函数')
  }

  await btn.onClick(fieldsModel.value)
}
// #endregion

// #region 取消
function onCancel() {
  emit('cancel')
}
// #endregion

// #region 外部 setup 函数
if (props.onSetup) {
  // 调用函数
  props.onSetup({
    fieldsModel: fieldsModel,
    fields: validFields
  })
}
// #endregion

// #region Enter 快捷键
async function onEnterKeyPress(event: KeyboardEvent) {
  // 如果 target 是 textArea，则不处理
  if (event.target && (event.target as HTMLElement).nodeName === 'TEXTAREA') {
    return
  }
  await onOKClick()
}
// #endregion

// #region 进行值同步
const previousFieldsModel = ref({ ...fieldsModel.value })

async function notifyFieldValueChanged() {
  // 使用快照获取旧值，避免 deep watch 中 oldValue 与 newValue 共用引用
  const oldFieldsModel = previousFieldsModel.value
  for (const field of fields.value) {
    const fieldName = field.name
    const newValue = fieldsModel.value[fieldName]
    const oldValue = oldFieldsModel[fieldName]

    if (Object.is(newValue, oldValue)) {
      continue
    }

    if (typeof field.onChanged === 'function') {
      await field.onChanged(newValue, oldValue, fieldsModel.value, fields.value)
    }
  }

  // 回调可能联动修改其它字段，这里记录最终表单值作为下一次比较基准
  previousFieldsModel.value = { ...fieldsModel.value }
}

function syncFieldValueToDefinition() {
  // 将表单模型同步回字段定义，便于外层执行器收集默认值
  if (!props.syncValue) return
  for (const field of fields.value) {
    field.value = fieldsModel.value[field.name]
  }
}

watch(fieldsModel, async () => {
  await notifyFieldValueChanged()
  syncFieldValueToDefinition()
}, { deep: true })
// #endregion
</script>

<style lang="scss" scoped>
.low-code_form-container {
  min-width: 300px;
  display: flex;
  flex-direction: column;
}


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

:deep(.low-code__field textarea) {
  max-height: 300px;
  overflow-y: auto;
}
</style>
