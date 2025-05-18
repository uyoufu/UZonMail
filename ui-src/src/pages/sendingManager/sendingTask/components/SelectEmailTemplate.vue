<template>
  <q-field v-model="fieldModelValue" tag="div" label="模板" dense @focus="isActive = true" @blur="isActive = false"
    @dbclick="onSelectEmailTemplates">
    <template v-slot:before>
      <q-icon name="article" color="secondary" />
    </template>

    <template v-slot:control>
      <div class="full-width no-outline" @dblclick="onSelectEmailTemplates">
        <div class="text-grey-7"> {{ fieldText }}</div>
      </div>
    </template>

    <template v-slot:append>
      <div class="row justify-end">
        <q-btn round dense flat icon="add" class="q-ml-sm" @click.stop="onSelectEmailTemplates" color="grey-7">
          <q-tooltip>
            选择模板
          </q-tooltip>
        </q-btn>
      </div>
    </template>
  </q-field>
</template>

<script lang="ts" setup>
import type { IEmailTemplate } from 'src/api/emailTemplate'
// 定义 v-model
const modelValue = defineModel({
  type: Array as PropType<IEmailTemplate[]>,
  default: () => ([])
})
import { createAbstractLabel } from 'src/utils/labelHelper'

// placeholder 显示
import { showComponentDialog } from 'src/components/popupDialog/PopupDialog'
import { useCustomQField } from '../helper'
const { isActive, fieldModelValue, fieldText } = useCustomQField('请选择模板 (模板与正文需至少有一个不为空)')

// 选择模板
import SelectEmailTemplateDialog from './SelectEmailTemplateDialog.vue'

async function onSelectEmailTemplates () {
  const { ok, data: templates } = await showComponentDialog<IEmailTemplate[]>(SelectEmailTemplateDialog, {
    initTemplates: modelValue.value
  })
  if (!ok) return

  // 更新选择的数据
  modelValue.value = templates
  const label = createAbstractLabel(templates.map(item => item.name), 5, '个模板')
  fieldModelValue.value = label
}
</script>

<style lang="scss" scoped></style>
