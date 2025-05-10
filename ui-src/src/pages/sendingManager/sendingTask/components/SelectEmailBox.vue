<template>
  <q-field v-bind="$attrs" v-model="fieldModelValue" tag="div" dense @focus="isActive = true" @blur="isActive = false">
    <template v-slot:before>
      <q-icon :name="icon" :color="iconColor" />
    </template>

    <template v-slot:control>
      <div class="full-width no-outline">
        <div class="text-grey-7"> {{ fieldText }}</div>
      </div>
    </template>

    <template v-slot:append>
      <div class="row justify-end">
        <q-btn round dense flat icon="add" class="q-ml-sm" @click.prevent="onSelectOutboxes" color="grey-7">
          <q-tooltip>
            选择邮箱
          </q-tooltip>
        </q-btn>
      </div>
    </template>
  </q-field>
</template>

<script lang="ts" setup>
import logger from 'loglevel'

const props = defineProps({
  icon: {
    type: String,
    default: ''
  },

  // 图标颜色
  iconColor: {
    type: String,
    default: 'primary'
  },

  placeholder: {
    type: String,
    default: ''
  },

  // 邮箱类型
  // 0: 发件箱
  // 1: 收件箱
  emailBoxType: {
    type: Number as PropType<0 | 1>,
    default: 0
  }
})

import type { IInbox } from 'src/api/emailBox'
import type { IEmailGroupListItem } from 'src/pages/emailManager/components/types'

// placeholder 显示
import { useCustomQField } from '../helper'
const { isActive, fieldModelValue, fieldText } = useCustomQField(props.placeholder)

// v-model 定义
const modelValue = defineModel({
  type: Array as PropType<IInbox[]>,
  default: () => []
})
const selectedGroupsModelValue = defineModel('selectedGroups', {
  type: Array as PropType<IEmailGroupListItem[]>,
  default: () => []
})

import { showComponentDialog } from 'src/components/popupDialog/PopupDialog'
import { createAbstractLabel } from 'src/utils/labelHelper'
import SelectEmailBoxDialog from './SelectEmailBoxDialog.vue'

async function onSelectOutboxes () {
  logger.debug('[SelectEmailBox] onSelectOutboxes', modelValue.value, selectedGroupsModelValue.value)

  const { ok, data } = await showComponentDialog<{
    selectedEmails: IInbox[],
    selectedGroups: IEmailGroupListItem[]
  }>(SelectEmailBoxDialog, {
    initEmailBoxes: modelValue.value,
    initEmailGroups: selectedGroupsModelValue.value,
    emailBoxType: props.emailBoxType
  })
  if (!ok) return

  // 更新选择的数据
  const selectedEmailsResults = data.selectedEmails.map(x => {
    return {
      id: x.id,
      email: x.email,
      name: x.name
    } as IInbox
  })
  modelValue.value = selectedEmailsResults
  const selectedGroupsResults = data.selectedGroups.map(x => {
    return {
      id: x.id,
      name: x.name
    } as IEmailGroupListItem
  })
  selectedGroupsModelValue.value = selectedGroupsResults

  formatFieldLabel()
}

// #region 格式化显示
function formatFieldLabel () {
  let unitLabel = '个邮箱'
  if (selectedGroupsModelValue.value.length > 0) {
    unitLabel = '个分组和邮箱'
  }
  const labels = selectedGroupsModelValue.value.map(x => `组-${x.name}`)
  if (modelValue.value.length > 0) {
    labels.push(...modelValue.value.map(x => x.email))
  }

  const label = createAbstractLabel(labels, 5, unitLabel)
  fieldModelValue.value = label
}
watch(modelValue, () => {
  formatFieldLabel()
})
watch(selectedGroupsModelValue, () => {
  formatFieldLabel()
})
// #endregion
</script>

<style lang="scss" scoped></style>
