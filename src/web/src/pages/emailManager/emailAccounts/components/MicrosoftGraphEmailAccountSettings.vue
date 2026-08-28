<template>
  <div class="q-pa-sm">
    <div class="q-mb-sm">
      <div class="row items-center justify-between q-mb-sm">
        <div class="text-subtitle2 q-ml-xs text-primary">Microsoft Graph</div>
        <q-toggle
          v-if="props.isEditing"
          v-model="replaceApplication"
          dense
          color="primary"
          :label="t('accountManagement.emailAccount.updateApplicationOnSave')"
        />
      </div>

      <div v-if="shouldConfigureApplication" class="row q-col-gutter-sm">
        <div class="col-12">
          <q-select
            v-model="application.applicationSource"
            :label="t('accountManagement.applicationSource')"
            hide-bottom-space
            :options="applicationSourceOptions"
            emit-value
            map-options
            outlined
            dense
            options-dense
          />
        </div>
        <template v-if="isCustomApplication">
          <div class="col-12 col-sm-6">
            <q-input
              v-model.trim="application.tenantId"
              label="Tenant ID"
              hide-bottom-space
              outlined
              dense
              :rules="[props.requiredRule]"
            />
          </div>
          <div class="col-12 col-sm-6">
            <q-input
              v-model.trim="application.clientId"
              label="Client ID"
              hide-bottom-space
              outlined
              dense
              :rules="[props.requiredRule]"
            />
          </div>
          <div class="col-12">
            <q-input
              v-model="application.clientSecret"
              label="Client Secret"
              hide-bottom-space
              type="password"
              outlined
              dense
            />
          </div>
        </template>
      </div>
    </div>

    <div class="text-subtitle2 q-mb-sm q-ml-xs text-primary">
      {{ t('accountManagement.emailAccount.senderSettings') }}
    </div>
    <div class="row q-col-gutter-sm q-mb-sm">
      <div class="col-12 col-sm-6">
        <q-input
          v-model.number="sender.maxSendCountPerDay"
          :label="t('accountManagement.dailyLimit')"
          hide-bottom-space
          type="number"
          min="0"
          outlined
          dense
        />
      </div>
      <div class="col-12 col-sm-6">
        <q-input
          v-model.number="sender.weight"
          :label="t('accountManagement.weight')"
          hide-bottom-space
          type="number"
          min="1"
          outlined
          dense
        />
      </div>
      <div class="col-12">
        <q-input
          v-model.trim="sender.replyToEmails"
          :label="t('accountManagement.replyTo')"
          hide-bottom-space
          outlined
          dense
        />
      </div>
    </div>

    <div class="text-subtitle2 q-mb-sm q-ml-xs text-primary">
      {{ t('accountManagement.emailAccount.receivingSettings') }}
    </div>
    <div class="row q-col-gutter-sm">
      <div class="col-12 col-sm-6">
        <q-input
          v-model.number="receiving.contentRetentionDays"
          :label="t('accountManagement.retentionDays')"
          hide-bottom-space
          type="number"
          min="1"
          max="3650"
          outlined
          dense
        />
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { OAuthApplicationSource } from 'src/api/accountEnums'
import { t } from 'src/i18n/helpers'
import type { EmailAccountFormRule, IEmailAccountDialogForm } from '../emailAccountDialogTypes'

defineOptions({ name: 'MicrosoftGraphEmailAccountSettings' })

const sender = defineModel<IEmailAccountDialogForm['sender']>('sender', { required: true })
const receiving = defineModel<IEmailAccountDialogForm['receiving']>('receiving', { required: true })
const application = defineModel<IEmailAccountDialogForm['microsoftGraphApplication']>('application', { required: true })
const replaceApplication = defineModel<boolean>('replaceApplication', { required: true })
const props = defineProps<{
  isEditing: boolean
  requiredRule: EmailAccountFormRule
}>()

const shouldConfigureApplication = computed(() => !props.isEditing || replaceApplication.value)
const isCustomApplication = computed(() => application.value.applicationSource === OAuthApplicationSource.Custom)
const applicationSourceOptions = computed(() => [
  { label: t('accountManagement.systemApplication'), value: OAuthApplicationSource.System },
  { label: t('accountManagement.customApplication'), value: OAuthApplicationSource.Custom }
])
</script>

<style lang="scss" scoped></style>
