<template>
  <q-field v-bind="$attrs" v-model="fieldModelValue" tag="div" dense @focus="isActive = true" @blur="isActive = false">
    <template #before>
      <q-icon :name="icon" :color="iconColor" />
    </template>

    <template #control>
      <div class="full-width no-outline" @dblclick="onSelectAccounts">
        <div class="text-grey-7">{{ fieldText }}</div>
      </div>
    </template>

    <template #append>
      <q-btn round dense flat icon="add" class="q-ml-sm" color="grey-7" @click.stop="onSelectAccounts">
        <q-tooltip>{{ t('pages.sendingTask.selectMailbox') }}</q-tooltip>
      </q-btn>
    </template>
  </q-field>
</template>

<script lang="ts" setup>
import logger from 'loglevel'
import { EmailGroupCategory, type EmailGroupCategory as EmailGroupCategoryValue } from 'src/api/emailGroup'
import type { IRecipientContactSelection, ISenderAccountSelection } from 'src/api/emailSending'
import { showComponentDialog } from 'src/components/lowCode/PopupDialog'
import { t } from 'src/i18n/helpers'
import type { IEmailGroupListItem } from 'src/pages/emailManager/components/types'
import { createAbstractLabel } from 'src/utils/labelHelper'
import { useCustomQField } from '../helper'
import SelectAccountDialog from './SelectAccountDialog.vue'

type AccountSelection = ISenderAccountSelection | IRecipientContactSelection

const props = defineProps({
  icon: { type: String, default: '' },
  iconColor: { type: String, default: 'primary' },
  placeholder: { type: String, default: '' },
  accountCategory: {
    type: Number as PropType<EmailGroupCategoryValue>,
    default: EmailGroupCategory.EmailAccount
  }
})

const { isActive, fieldModelValue, fieldText } = useCustomQField(props.placeholder)
const modelValue = defineModel<AccountSelection[]>({ default: () => [] })
const selectedGroupsModelValue = defineModel<IEmailGroupListItem[]>('selectedGroups', { default: () => [] })

async function onSelectAccounts () {
  logger.debug('[SelectAccount] open selector', modelValue.value, selectedGroupsModelValue.value)
  const { ok, data } = await showComponentDialog<{
    selectedAccounts: AccountSelection[]
    selectedGroups: IEmailGroupListItem[]
  }>(SelectAccountDialog, {
    initialAccounts: modelValue.value,
    initialGroups: selectedGroupsModelValue.value,
    accountCategory: props.accountCategory
  })
  if (!ok) return

  modelValue.value = data.selectedAccounts.map(account => ({
    id: account.id,
    email: account.email,
    name: account.name,
    description: account.description
  }))
  selectedGroupsModelValue.value = data.selectedGroups.map(group => ({
    id: group.id,
    name: group.name,
    label: group.label,
    order: group.order
  }))
  formatFieldLabel()
}

function formatFieldLabel () {
  let unitLabel = t('pages.sendingTask.mailboxUnit')
  if (selectedGroupsModelValue.value.length > 0) {
    unitLabel = t('pages.sendingTask.groupAndMailboxUnit')
  }
  const labels = selectedGroupsModelValue.value.map(group => t('pages.sendingTask.groupLabel', { name: group.name }))
  labels.push(...modelValue.value.map(account => account.email))
  fieldModelValue.value = createAbstractLabel(labels, 5, unitLabel)
}

watch(modelValue, formatFieldLabel, { deep: true })
watch(selectedGroupsModelValue, formatFieldLabel, { deep: true })
</script>
