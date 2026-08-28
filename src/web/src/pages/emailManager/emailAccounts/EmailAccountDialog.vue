<template>
  <q-dialog ref="dialogRef" persistent @hide="onDialogHide">
    <q-card class="email-account-dialog column no-wrap">
      <TitleBar :title="dialogTitle" @close="onDialogCancel" />

      <q-form class="col column no-wrap" @submit.prevent="onSubmit">
        <div class="col scroll">
          <q-card-section class="q-pa-sm">
            <div class="row q-col-gutter-sm">
              <div class="col-12 col-sm-6">
                <q-input v-model.trim="form.email" :disable="isEditing" hide-bottom-space
                  :label="t('accountManagement.email')" type="email" outlined dense :rules="[requiredRule]" />
              </div>
              <div class="col-12 col-sm-6">
                <q-input v-model.trim="form.name" :label="t('accountManagement.name')" outlined dense />
              </div>
              <div class="col-12 col-sm-6">
                <q-input v-model.trim="form.description" :label="t('accountManagement.description')" outlined dense />
              </div>
              <div class="col-12 col-sm-6">
                <q-input v-model.trim="form.remark" :label="t('accountManagement.remark')" outlined dense />
              </div>
            </div>
          </q-card-section>

          <UTabs v-model="activeTab" :tabs="tabs" />

          <q-tab-panels v-model="activeTab" animated>
            <q-tab-panel :name="EmailAccountTab.Sender" class="q-pa-sm">
              <div v-if="isBasic" class="q-mb-sm">
                <div class="text-subtitle2 q-mb-sm q-ml-xs text-primary">{{
                  t('accountManagement.emailAccount.smtpCredential') }}</div>

                <div class="row q-col-gutter-sm">
                  <div class="col-12 col-sm-6">
                    <q-input v-model.trim="form.sender.host" :label="t('accountManagement.host')" hide-bottom-space
                      outlined dense :rules="smtpHostRules"
                      @update:model-value="onCredentialFieldChanged(MailProtocol.Smtp, CredentialField.Host)" />
                  </div>
                  <div class="col-12 col-sm-3">
                    <q-input v-model.number="form.sender.port" :label="t('accountManagement.port')" hide-bottom-space
                      type="number" min="1" max="65535" outlined dense :rules="smtpPortRules"
                      @update:model-value="onCredentialFieldChanged(MailProtocol.Smtp, CredentialField.Port)" />
                  </div>
                  <div class="col-12 col-sm-3">
                    <q-select v-model="form.sender.connectionSecurity"
                      :label="t('accountManagement.connectionSecurity')" hide-bottom-space
                      :options="connectionSecurityOptions" emit-value map-options outlined dense options-dense
                      @update:model-value="onConnectionSecurityChanged(MailProtocol.Smtp)" />
                  </div>
                  <div class="col-12 col-sm-6">
                    <q-input v-model.trim="form.sender.loginName" :label="t('accountManagement.loginName')"
                      hide-bottom-space outlined dense :rules="smtpLoginRules"
                      @update:model-value="onCredentialFieldChanged(MailProtocol.Smtp, CredentialField.LoginName)" />
                  </div>
                  <div class="col-12 col-sm-6">
                    <q-input v-model="form.sender.password" :label="smtpPasswordLabel" hide-bottom-space type="password"
                      outlined dense :rules="smtpPasswordRules"
                      @update:model-value="onCredentialFieldChanged(MailProtocol.Smtp, CredentialField.Password)" />
                  </div>
                </div>
              </div>

              <div class="text-subtitle2 q-mb-sm q-ml-xs text-primary">{{ senderProtocolLabel }}</div>

              <div class="row q-col-gutter-sm">
                <div class="col-12 col-sm-4">
                  <q-input v-model.number="form.sender.maxSendCountPerDay" :label="t('accountManagement.dailyLimit')"
                    type="number" min="0" outlined dense />
                </div>
                <div class="col-12 col-sm-4">
                  <q-input v-model.number="form.sender.weight" :label="t('accountManagement.weight')" type="number"
                    min="1" outlined dense />
                </div>
                <div v-if="isBasic" class="col-12 col-sm-4">
                  <q-input v-model.number="form.sender.proxyId" :label="t('accountManagement.emailAccount.proxyId')"
                    type="number" min="1" outlined dense clearable />
                </div>
                <div class="col-12">
                  <q-input v-model.trim="form.sender.replyToEmails" :label="t('accountManagement.replyTo')" outlined
                    dense />
                </div>
              </div>
            </q-tab-panel>

            <q-tab-panel :name="EmailAccountTab.Receiving" class="q-pa-sm q-gutter-sm">
              <div class="text-subtitle2">{{ receivingProtocolLabel }}</div>
              <div class="row q-col-gutter-sm">
                <div class="col-12 col-sm-4">
                  <q-input v-model.number="form.receiving.contentRetentionDays"
                    :label="t('accountManagement.retentionDays')" type="number" min="1" max="3650" outlined dense />
                </div>
              </div>
              <div v-if="isBasic" class="q-mb-sm">
                <div class="text-caption text-grey-7">{{ t('accountManagement.emailAccount.imapCredential') }}</div>
                <div class="row q-col-gutter-sm">
                  <div class="col-12 col-sm-6">
                    <q-input v-model.trim="form.receiving.host" :label="t('accountManagement.host')" outlined dense
                      :rules="imapHostRules"
                      @update:model-value="onCredentialFieldChanged(MailProtocol.Imap, CredentialField.Host)" />
                  </div>
                  <div class="col-12 col-sm-3">
                    <q-input v-model.number="form.receiving.port" :label="t('accountManagement.port')" type="number"
                      min="1" max="65535" outlined dense :rules="imapPortRules"
                      @update:model-value="onCredentialFieldChanged(MailProtocol.Imap, CredentialField.Port)" />
                  </div>
                  <div class="col-12 col-sm-3">
                    <q-select v-model="form.receiving.connectionSecurity"
                      :label="t('accountManagement.connectionSecurity')" :options="connectionSecurityOptions" emit-value
                      map-options outlined dense options-dense
                      @update:model-value="onConnectionSecurityChanged(MailProtocol.Imap)" />
                  </div>
                  <div class="col-12 col-sm-6">
                    <q-input v-model.trim="form.receiving.loginName" :label="t('accountManagement.loginName')" outlined
                      dense :rules="imapLoginRules"
                      @update:model-value="onCredentialFieldChanged(MailProtocol.Imap, CredentialField.LoginName)" />
                  </div>
                  <div class="col-12 col-sm-6">
                    <q-input v-model="form.receiving.password" :label="imapPasswordLabel" type="password" outlined dense
                      :rules="imapPasswordRules"
                      @update:model-value="onCredentialFieldChanged(MailProtocol.Imap, CredentialField.Password)" />
                  </div>
                </div>
              </div>
            </q-tab-panel>
          </q-tab-panels>

          <q-card-section v-if="isMicrosoftGraph" class="q-pa-sm q-gutter-sm">
            <div class="row items-center justify-between">
              <div class="text-subtitle2">{{ t('accountManagement.applicationSource') }}</div>
              <q-toggle v-if="isEditing" v-model="form.replaceMicrosoftGraphApplication" dense color="primary"
                :label="t('accountManagement.emailAccount.updateApplicationOnSave')" />
            </div>
            <div v-if="shouldSendMicrosoftGraphApplication" class="row q-col-gutter-sm">
              <div class="col-12 col-sm-4">
                <q-select v-model="form.microsoftGraphApplication.applicationSource"
                  :label="t('accountManagement.applicationSource')" :options="applicationSourceOptions" emit-value
                  map-options outlined dense options-dense />
              </div>
              <template v-if="form.microsoftGraphApplication.applicationSource === OAuthApplicationSource.Custom">
                <div class="col-12 col-sm-4">
                  <q-input v-model.trim="form.microsoftGraphApplication.tenantId" label="Tenant ID" outlined dense
                    :rules="[requiredRule]" />
                </div>
                <div class="col-12 col-sm-4">
                  <q-input v-model.trim="form.microsoftGraphApplication.clientId" label="Client ID" outlined dense
                    :rules="[requiredRule]" />
                </div>
                <div class="col-12">
                  <q-input v-model="form.microsoftGraphApplication.clientSecret" label="Client Secret" type="password"
                    outlined dense />
                </div>
              </template>
            </div>
          </q-card-section>
        </div>

        <q-card-actions align="right" class="q-pa-sm q-gutter-sm">
          <CancelBtn @click="onDialogCancel" />
          <OkBtn type="submit" :loading="isSaving" />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script lang="ts" setup>
import { useDialogPluginComponent } from 'quasar'
import { ConnectionSecurity, OAuthApplicationSource } from 'src/api/accountEnums'
import {
  createBasicEmailAccount,
  createMicrosoftGraphEmailAccount,
  updateEmailAccount,
  type EmailAccountConfigurationKind,
  type IEmailAccount
} from 'src/api/emailAccounts'
import UTabs from 'src/components/utabs/UTabs.vue'
import type { IUTabOptions } from 'src/components/utabs/types'
import TitleBar from 'src/components/windowLike/TitleBar.vue'
import { t } from 'src/i18n/helpers'
import { notifyError, notifySuccess } from 'src/utils/dialog'
import { CredentialField, EmailAccountTab, MailProtocol } from './emailAccountDialogTypes'
import { useEmailAccountDialogForm } from './useEmailAccountDialogForm'

defineOptions({ name: 'EmailAccountDialog' })

const props = defineProps<{
  emailGroupId: number
  configurationKind: EmailAccountConfigurationKind
  account?: IEmailAccount
}>()
defineEmits([...useDialogPluginComponent.emits])

const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent<IEmailAccount>()
const { emailGroupId, configurationKind, account } = toRefs(props)
const activeTab = ref(EmailAccountTab.Sender)
const isSaving = ref(false)
const {
  form,
  isEditing,
  isBasic,
  isMicrosoftGraph,
  shouldSendMicrosoftGraphApplication,
  smtpPasswordLabel,
  imapPasswordLabel,
  requiredRule,
  smtpHostRules,
  smtpLoginRules,
  smtpPortRules,
  smtpPasswordRules,
  imapHostRules,
  imapLoginRules,
  imapPortRules,
  imapPasswordRules,
  onCredentialFieldChanged,
  onConnectionSecurityChanged,
  hasValidCredentials,
  toWriteRequest
} = useEmailAccountDialogForm({ emailGroupId, configurationKind, account })

const tabs = computed<IUTabOptions[]>(() => [
  {
    name: EmailAccountTab.Sender,
    icon: 'outbox',
    label: t('accountManagement.emailAccount.senderSettings')
  },
  {
    name: EmailAccountTab.Receiving,
    icon: 'move_to_inbox',
    label: t('accountManagement.emailAccount.receivingSettings')
  }
])
const senderProtocolLabel = computed(() => (isBasic.value ? 'SMTP' : 'Microsoft Graph'))
const receivingProtocolLabel = computed(() => (isBasic.value ? 'IMAP' : 'Microsoft Graph'))
const dialogTitle = computed(() =>
  isEditing.value ? t('accountManagement.emailAccount.edit') : t('accountManagement.emailAccount.create')
)
const connectionSecurityOptions = [
  { label: 'None', value: ConnectionSecurity.None },
  { label: 'SSL', value: ConnectionSecurity.SSL },
  { label: 'TLS', value: ConnectionSecurity.TLS },
  { label: 'StartTLS', value: ConnectionSecurity.StartTLS }
]
const applicationSourceOptions = [
  { label: t('accountManagement.systemApplication'), value: OAuthApplicationSource.System },
  { label: t('accountManagement.customApplication'), value: OAuthApplicationSource.Custom }
]

async function onSubmit() {
  if (!hasValidCredentials()) {
    notifyError(t('accountManagement.emailAccount.credentialRequired'))
    return
  }

  isSaving.value = true
  try {
    const request = toWriteRequest()
    const response = isEditing.value
      ? await updateEmailAccount(props.account!.id, request)
      : isBasic.value
        ? await createBasicEmailAccount(request)
        : await createMicrosoftGraphEmailAccount(request)
    notifySuccess(isEditing.value ? t('accountManagement.updated') : t('accountManagement.created'))
    onDialogOK(response.data)
  } finally {
    isSaving.value = false
  }
}
</script>

<style lang="scss" scoped>
.email-account-dialog {
  width: min(760px, calc(100vw - 16px));
  max-width: calc(100vw - 16px);
  max-height: calc(100dvh - 16px);
}
</style>
