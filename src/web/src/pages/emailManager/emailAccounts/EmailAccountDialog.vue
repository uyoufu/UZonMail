<template>
  <q-dialog ref="dialogRef" persistent @hide="onDialogHide">
    <q-card class="email-account-dialog column no-wrap">
      <TitleBar :title="dialogTitle" @close="onDialogCancel" />

      <q-form class="col column no-wrap" @submit.prevent="onSubmit">
        <div class="col scroll">
          <q-card-section class="q-pa-sm">
            <div class="row q-col-gutter-sm">
              <div class="col-12 col-sm-6">
                <q-input
                  v-model.trim="form.email"
                  :disable="isEditing"
                  hide-bottom-space
                  :label="t('accountManagement.email')"
                  type="email"
                  outlined
                  dense
                  :rules="[requiredRule]"
                />
              </div>
              <div class="col-12 col-sm-6">
                <q-input
                  v-model.trim="form.name"
                  :label="t('accountManagement.emailAccount.senderName')"
                  outlined
                  dense
                />
              </div>
              <div class="col-12 col-sm-6">
                <q-input v-model.trim="form.description" :label="t('accountManagement.description')" outlined dense />
              </div>
              <div class="col-12 col-sm-6">
                <q-input v-model.trim="form.remark" :label="t('accountManagement.remark')" outlined dense />
              </div>
            </div>
          </q-card-section>

          <template v-if="isBasic">
            <UTabs v-model="activeTab" :tabs="tabs" />

            <q-tab-panels v-model="activeTab" animated>
              <q-tab-panel :name="EmailAccountTab.Sender" class="q-pa-sm">
                <SmtpEmailAccountSettings
                  v-model:sender="form.sender"
                  :password-label="smtpPasswordLabel"
                  :host-rules="smtpHostRules"
                  :port-rules="smtpPortRules"
                  :password-rules="smtpPasswordRules"
                  @credential-field-changed="onCredentialFieldChanged(MailProtocol.Smtp, $event)"
                  @connection-security-changed="onConnectionSecurityChanged(MailProtocol.Smtp)"
                />
              </q-tab-panel>

              <q-tab-panel :name="EmailAccountTab.Receiving" class="q-pa-sm">
                <ImapEmailAccountSettings
                  v-model:receiving="form.receiving"
                  :password-label="imapPasswordLabel"
                  :host-rules="imapHostRules"
                  :port-rules="imapPortRules"
                  :password-rules="imapPasswordRules"
                  @credential-field-changed="onCredentialFieldChanged(MailProtocol.Imap, $event)"
                  @connection-security-changed="onConnectionSecurityChanged(MailProtocol.Imap)"
                />
              </q-tab-panel>
            </q-tab-panels>
          </template>

          <MicrosoftGraphEmailAccountSettings
            v-else
            v-model:sender="form.sender"
            v-model:receiving="form.receiving"
            v-model:application="form.microsoftGraphApplication"
            v-model:replace-application="form.replaceMicrosoftGraphApplication"
            :is-editing="isEditing"
            :required-rule="requiredRule"
          />
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
import ImapEmailAccountSettings from './components/ImapEmailAccountSettings.vue'
import MicrosoftGraphEmailAccountSettings from './components/MicrosoftGraphEmailAccountSettings.vue'
import SmtpEmailAccountSettings from './components/SmtpEmailAccountSettings.vue'
import { EmailAccountTab, MailProtocol } from './emailAccountDialogTypes'
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
  smtpPasswordLabel,
  imapPasswordLabel,
  requiredRule,
  smtpHostRules,
  smtpPortRules,
  smtpPasswordRules,
  imapHostRules,
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
const dialogTitle = computed(() =>
  isEditing.value ? t('accountManagement.emailAccount.edit') : t('accountManagement.emailAccount.create')
)
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
