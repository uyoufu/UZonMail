<template>
  <div>
    <div class="q-mb-sm">
      <div class="text-subtitle2 q-mb-sm q-ml-xs text-primary">
        {{ t('accountManagement.emailAccount.smtpCredential') }}
      </div>

      <div class="row q-col-gutter-sm">
        <div class="col-12 col-sm-6">
          <q-input
            v-model.trim="sender.host"
            :label="t('accountManagement.host')"
            hide-bottom-space
            outlined
            dense
            :rules="props.hostRules"
            @update:model-value="emit('credentialFieldChanged', CredentialField.Host)"
          />
        </div>
        <div class="col-12 col-sm-3">
          <q-input
            v-model.number="sender.port"
            :label="t('accountManagement.port')"
            hide-bottom-space
            type="number"
            min="1"
            max="65535"
            outlined
            dense
            :rules="props.portRules"
            @update:model-value="emit('credentialFieldChanged', CredentialField.Port)"
          />
        </div>
        <div class="col-12 col-sm-3">
          <q-select
            v-model="sender.connectionSecurity"
            :label="t('accountManagement.connectionSecurity')"
            hide-bottom-space
            :options="connectionSecurityOptions"
            emit-value
            map-options
            outlined
            dense
            options-dense
            @update:model-value="emit('connectionSecurityChanged')"
          />
        </div>
        <div class="col-12 col-sm-6">
          <q-input
            v-model.trim="sender.loginName"
            :label="t('accountManagement.loginName')"
            hide-bottom-space
            outlined
            dense
            @update:model-value="emit('credentialFieldChanged', CredentialField.LoginName)"
          />
        </div>
        <div class="col-12 col-sm-6">
          <q-input
            v-model="sender.password"
            :label="props.passwordLabel"
            hide-bottom-space
            type="password"
            outlined
            dense
            :rules="props.passwordRules"
            @update:model-value="emit('credentialFieldChanged', CredentialField.Password)"
          />
        </div>
      </div>
    </div>

    <div class="text-subtitle2 q-mb-sm q-ml-xs text-primary">SMTP</div>

    <div class="row q-col-gutter-sm">
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
        <q-select
          v-model="sender.proxyId"
          :label="t('accountManagement.emailAccount.proxy')"
          :options="proxyOptions"
          option-label="name"
          option-value="id"
          emit-value
          map-options
          clearable
          outlined
          dense
          options-dense
          hide-bottom-space
          :loading="isLoadingProxyOptions"
          @popup-show="onProxyPopupShow"
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
  </div>
</template>

<script lang="ts" setup>
import { t } from 'src/i18n/helpers'
import {
  CredentialField,
  connectionSecurityOptions,
  type CredentialField as CredentialFieldValue,
  type EmailAccountFormRule,
  type IEmailAccountDialogForm
} from '../emailAccountDialogTypes'
import { useEmailAccountProxyOptions } from '../useEmailAccountProxyOptions'

defineOptions({ name: 'SmtpEmailAccountSettings' })

const sender = defineModel<IEmailAccountDialogForm['sender']>('sender', { required: true })
const props = defineProps<{
  passwordLabel: string
  hostRules: EmailAccountFormRule[]
  portRules: EmailAccountFormRule[]
  passwordRules: EmailAccountFormRule[]
}>()
const emit = defineEmits<{
  credentialFieldChanged: [field: CredentialFieldValue]
  connectionSecurityChanged: []
}>()

const { proxyOptions, isLoadingProxyOptions, onProxyPopupShow } = useEmailAccountProxyOptions()
</script>

<style lang="scss" scoped></style>
