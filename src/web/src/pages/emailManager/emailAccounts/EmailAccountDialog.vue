<template>
  <q-dialog v-model="isOpen" persistent>
    <q-card class="email-account-dialog">
      <q-card-section class="row items-center q-pb-sm">
        <div class="text-subtitle1">{{ dialogTitle }}</div>
        <q-space />
        <q-btn v-close-popup flat round dense icon="close">
          <q-tooltip>{{ t('global.close') }}</q-tooltip>
        </q-btn>
      </q-card-section>

      <q-separator />
      <q-form @submit.prevent="onSubmit">
        <q-card-section class="q-gutter-md">
          <div class="row q-col-gutter-md">
            <div class="col-12 col-sm-6">
              <q-input v-model.trim="form.email" :disable="isEditing" :label="t('accountManagement.email')" type="email" outlined dense :rules="[requiredRule]" />
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

        <q-tabs v-model="activeTab" align="left" dense class="text-primary" active-color="primary" indicator-color="primary">
          <q-tab name="sender" icon="send" :label="t('accountManagement.emailAccount.senderSettings')" />
          <q-tab name="receiving" icon="move_to_inbox" :label="t('accountManagement.emailAccount.receivingSettings')" />
        </q-tabs>
        <q-separator />

        <q-tab-panels v-model="activeTab" animated>
          <q-tab-panel name="sender" class="q-gutter-md">
            <div class="row items-center justify-between">
              <div class="text-subtitle2">{{ senderProtocolLabel }}</div>
              <q-toggle v-model="form.sender.isEnabled" color="primary" :label="t('accountManagement.emailAccount.enableSender')" />
            </div>
            <template v-if="form.sender.isEnabled">
              <div class="row q-col-gutter-md">
                <div class="col-12 col-sm-4">
                  <q-input v-model.number="form.sender.maxSendCountPerDay" :label="t('accountManagement.dailyLimit')" type="number" min="0" outlined dense />
                </div>
                <div class="col-12 col-sm-4">
                  <q-input v-model.number="form.sender.weight" :label="t('accountManagement.weight')" type="number" min="1" outlined dense />
                </div>
                <div v-if="isBasic" class="col-12 col-sm-4">
                  <q-input v-model.number="form.sender.proxyId" :label="t('accountManagement.emailAccount.proxyId')" type="number" min="1" outlined dense clearable />
                </div>
                <div class="col-12">
                  <q-input v-model.trim="form.sender.replyToEmails" :label="t('accountManagement.replyTo')" outlined dense />
                </div>
              </div>

              <template v-if="isBasic">
                <q-separator />
                <div class="text-caption text-grey-7">{{ t('accountManagement.emailAccount.smtpCredential') }}</div>
                <div class="row q-col-gutter-md">
                  <div class="col-12 col-sm-6">
                    <q-input v-model.trim="form.sender.host" :label="t('accountManagement.host')" outlined dense :rules="smtpHostRules" />
                  </div>
                  <div class="col-12 col-sm-3">
                    <q-input v-model.number="form.sender.port" :label="t('accountManagement.port')" type="number" min="1" max="65535" outlined dense :rules="smtpPortRules" />
                  </div>
                  <div class="col-12 col-sm-3">
                    <q-select v-model="form.sender.connectionSecurity" :label="t('accountManagement.connectionSecurity')" :options="connectionSecurityOptions" emit-value map-options outlined dense />
                  </div>
                  <div class="col-12 col-sm-6">
                    <q-input v-model.trim="form.sender.loginName" :label="t('accountManagement.loginName')" outlined dense :rules="smtpLoginRules" />
                  </div>
                  <div class="col-12 col-sm-6">
                    <q-input v-model="form.sender.password" :label="smtpPasswordLabel" type="password" outlined dense :rules="smtpPasswordRules" />
                  </div>
                </div>
              </template>
            </template>
          </q-tab-panel>

          <q-tab-panel name="receiving" class="q-gutter-md">
            <div class="row items-center justify-between">
              <div class="text-subtitle2">{{ receivingProtocolLabel }}</div>
              <q-toggle v-model="form.receiving.isEnabled" color="primary" :label="t('accountManagement.emailAccount.enableReceiving')" />
            </div>
            <template v-if="form.receiving.isEnabled">
              <div class="row q-col-gutter-md">
                <div class="col-12 col-sm-4">
                  <q-input v-model.number="form.receiving.contentRetentionDays" :label="t('accountManagement.retentionDays')" type="number" min="1" max="3650" outlined dense />
                </div>
              </div>
              <template v-if="isBasic">
                <q-separator />
                <div class="text-caption text-grey-7">{{ t('accountManagement.emailAccount.imapCredential') }}</div>
                <div class="row q-col-gutter-md">
                  <div class="col-12 col-sm-6">
                    <q-input v-model.trim="form.receiving.host" :label="t('accountManagement.host')" outlined dense :rules="imapHostRules" />
                  </div>
                  <div class="col-12 col-sm-3">
                    <q-input v-model.number="form.receiving.port" :label="t('accountManagement.port')" type="number" min="1" max="65535" outlined dense :rules="imapPortRules" />
                  </div>
                  <div class="col-12 col-sm-3">
                    <q-select v-model="form.receiving.connectionSecurity" :label="t('accountManagement.connectionSecurity')" :options="connectionSecurityOptions" emit-value map-options outlined dense />
                  </div>
                  <div class="col-12 col-sm-6">
                    <q-input v-model.trim="form.receiving.loginName" :label="t('accountManagement.loginName')" outlined dense :rules="imapLoginRules" />
                  </div>
                  <div class="col-12 col-sm-6">
                    <q-input v-model="form.receiving.password" :label="imapPasswordLabel" type="password" outlined dense :rules="imapPasswordRules" />
                  </div>
                </div>
              </template>
            </template>
          </q-tab-panel>
        </q-tab-panels>

        <q-card-section v-if="isMicrosoftGraph" class="q-pt-none q-gutter-md">
          <q-separator />
          <div class="row items-center justify-between">
            <div class="text-subtitle2">{{ t('accountManagement.applicationSource') }}</div>
            <q-toggle v-if="isEditing" v-model="form.replaceMicrosoftGraphApplication" color="primary" :label="t('accountManagement.emailAccount.updateApplicationOnSave')" />
          </div>
          <div v-if="shouldSendMicrosoftGraphApplication" class="row q-col-gutter-md">
            <div class="col-12 col-sm-4">
              <q-select v-model="form.microsoftGraphApplication.applicationSource" :label="t('accountManagement.applicationSource')" :options="applicationSourceOptions" emit-value map-options outlined dense />
            </div>
            <template v-if="form.microsoftGraphApplication.applicationSource === OAuthApplicationSource.Custom">
              <div class="col-12 col-sm-4">
                <q-input v-model.trim="form.microsoftGraphApplication.tenantId" label="Tenant ID" outlined dense :rules="[requiredRule]" />
              </div>
              <div class="col-12 col-sm-4">
                <q-input v-model.trim="form.microsoftGraphApplication.clientId" label="Client ID" outlined dense :rules="[requiredRule]" />
              </div>
              <div class="col-12">
                <q-input v-model="form.microsoftGraphApplication.clientSecret" label="Client Secret" type="password" outlined dense />
              </div>
            </template>
          </div>
        </q-card-section>

        <q-card-actions align="right" class="q-pa-md">
          <q-btn v-close-popup flat :label="t('buttons.cancel')" />
          <q-btn type="submit" color="primary" :loading="isSaving" :label="t('buttons.save')" />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-dialog>
</template>

<script lang="ts" setup>
import { t, translateGlobal } from 'src/i18n/helpers'
import {
  ConnectionSecurity,
  OAuthApplicationSource,
  type ConnectionSecurity as ConnectionSecurityValue,
  type OAuthApplicationSource as OAuthApplicationSourceValue
} from 'src/api/accountEnums'
import {
  EmailAccountConfigurationKind,
  createBasicEmailAccount,
  createMicrosoftGraphEmailAccount,
  updateEmailAccount,
  type IEmailAccount,
  type IEmailAccountWrite
} from 'src/api/emailAccounts'
import { notifyError, notifySuccess } from 'src/utils/dialog'

const isOpen = defineModel<boolean>({ default: false })
const props = defineProps({
  emailGroupId: { type: Number, required: true },
  configurationKind: { type: Number as PropType<EmailAccountConfigurationKind>, required: true },
  account: { type: Object as PropType<IEmailAccount | undefined>, default: undefined }
})
const emit = defineEmits<{ saved: [account: IEmailAccount] }>()

const activeTab = ref<'sender' | 'receiving'>('sender')
const isSaving = ref(false)
const form = reactive(createEmptyForm())
const isEditing = computed(() => props.account !== undefined)
const isBasic = computed(() => props.configurationKind === EmailAccountConfigurationKind.Basic)
const isMicrosoftGraph = computed(() => !isBasic.value)
const senderProtocolLabel = computed(() => isBasic.value ? 'SMTP' : 'Microsoft Graph')
const receivingProtocolLabel = computed(() => isBasic.value ? 'IMAP' : 'Microsoft Graph')
const dialogTitle = computed(() => isEditing.value
  ? t('accountManagement.emailAccount.edit')
  : t('accountManagement.emailAccount.create'))
const shouldSendMicrosoftGraphApplication = computed(() => !isEditing.value || form.replaceMicrosoftGraphApplication)
const shouldWriteSmtpCredential = computed(() => Boolean(
  form.sender.host || form.sender.loginName || form.sender.password
))
const shouldWriteImapCredential = computed(() => Boolean(
  form.receiving.host || form.receiving.loginName || form.receiving.password
))
const hasExistingSmtpCredential = computed(() => props.account?.sender?.hasCredential === true)
const hasExistingImapCredential = computed(() => props.account?.receiving?.hasCredential === true)
const smtpPasswordLabel = computed(() => hasExistingSmtpCredential.value ? t('accountManagement.newPassword') : t('accountManagement.password'))
const imapPasswordLabel = computed(() => hasExistingImapCredential.value ? t('accountManagement.newPassword') : t('accountManagement.password'))
const requiredRule = (value: unknown) => Boolean(value) || translateGlobal('required')
const smtpHostRules = computed(() => requiresSmtpCredential() ? [requiredRule] : [])
const smtpLoginRules = computed(() => requiresSmtpCredential() ? [requiredRule] : [])
const smtpPortRules = computed(() => requiresSmtpCredential() ? [validPortRule] : [])
const smtpPasswordRules = computed(() => requiresNewSmtpPassword() ? [requiredRule] : [])
const imapHostRules = computed(() => requiresImapCredential() ? [requiredRule] : [])
const imapLoginRules = computed(() => requiresImapCredential() ? [requiredRule] : [])
const imapPortRules = computed(() => requiresImapCredential() ? [validPortRule] : [])
const imapPasswordRules = computed(() => requiresNewImapPassword() ? [requiredRule] : [])

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

watch(isOpen, (isDialogOpen) => {
  if (isDialogOpen) initializeForm()
})

function createEmptyForm () {
  return {
    email: '',
    name: '',
    description: '',
    remark: '',
    sender: {
      isEnabled: true,
      proxyId: undefined as number | undefined,
      maxSendCountPerDay: 0,
      replyToEmails: '',
      weight: 1,
      host: '',
      port: 465,
      loginName: '',
      password: '',
      connectionSecurity: ConnectionSecurity.SSL as ConnectionSecurityValue
    },
    receiving: {
      isEnabled: false,
      contentRetentionDays: 30,
      host: '',
      port: 993,
      loginName: '',
      password: '',
      connectionSecurity: ConnectionSecurity.SSL as ConnectionSecurityValue
    },
    replaceMicrosoftGraphApplication: false,
    microsoftGraphApplication: {
      applicationSource: OAuthApplicationSource.System as OAuthApplicationSourceValue,
      tenantId: '',
      clientId: '',
      clientSecret: ''
    }
  }
}

function initializeForm () {
  Object.assign(form, createEmptyForm())
  const account = props.account
  if (!account) return

  form.email = account.email
  form.name = account.name ?? ''
  form.description = account.description ?? ''
  form.remark = account.remark ?? ''
  if (account.sender) {
    form.sender.isEnabled = true
    form.sender.proxyId = account.sender.proxyId
    form.sender.maxSendCountPerDay = account.sender.maxSendCountPerDay
    form.sender.replyToEmails = account.sender.replyToEmails ?? ''
    form.sender.weight = account.sender.weight
  } else {
    form.sender.isEnabled = false
  }
  if (account.receiving) {
    form.receiving.isEnabled = true
    form.receiving.contentRetentionDays = account.receiving.contentRetentionDays
  }
  if (account.oAuthApplicationSource !== undefined) {
    form.microsoftGraphApplication.applicationSource = account.oAuthApplicationSource
  }
}

function requiresSmtpCredential () {
  return isBasic.value && form.sender.isEnabled && (!hasExistingSmtpCredential.value || shouldWriteSmtpCredential.value)
}

function requiresNewSmtpPassword () {
  return requiresSmtpCredential() && !hasExistingSmtpCredential.value
}

function requiresImapCredential () {
  return isBasic.value && form.receiving.isEnabled && (!hasExistingImapCredential.value || shouldWriteImapCredential.value)
}

function requiresNewImapPassword () {
  return requiresImapCredential() && !hasExistingImapCredential.value
}

function validPortRule (value: unknown) {
  const port = Number(value)
  return (port > 0 && port <= 65535) || t('accountManagement.emailAccount.portInvalid')
}

async function onSubmit () {
  if (!form.sender.isEnabled && !form.receiving.isEnabled) {
    notifyError(t('accountManagement.emailAccount.capabilityRequired'))
    return
  }
  if (requiresSmtpCredential() && !form.sender.password && !hasExistingSmtpCredential.value) {
    notifyError(t('accountManagement.emailAccount.credentialRequired'))
    return
  }
  if (requiresImapCredential() && !form.receiving.password && !hasExistingImapCredential.value) {
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
    emit('saved', response.data)
    isOpen.value = false
    notifySuccess(isEditing.value ? t('accountManagement.updated') : t('accountManagement.created'))
  } finally {
    isSaving.value = false
  }
}

function toWriteRequest (): IEmailAccountWrite {
  return {
    email: isEditing.value ? undefined : form.email,
    emailGroupId: props.emailGroupId,
    name: form.name || undefined,
    description: form.description || undefined,
    remark: form.remark || undefined,
    configurationKind: props.configurationKind,
    sender: {
      isEnabled: form.sender.isEnabled,
      proxyId: form.sender.proxyId || undefined,
      maxSendCountPerDay: Number(form.sender.maxSendCountPerDay) || 0,
      replyToEmails: form.sender.replyToEmails || undefined,
      weight: Math.max(1, Number(form.sender.weight) || 1),
      smtpCredential: isBasic.value && shouldWriteSmtpCredential.value
        ? {
            host: form.sender.host,
            port: Number(form.sender.port),
            loginName: form.sender.loginName,
            password: form.sender.password || undefined,
            connectionSecurity: form.sender.connectionSecurity
          }
        : undefined
    },
    receiving: {
      isEnabled: form.receiving.isEnabled,
      contentRetentionDays: Math.max(1, Number(form.receiving.contentRetentionDays) || 30),
      imapCredential: isBasic.value && shouldWriteImapCredential.value
        ? {
            host: form.receiving.host,
            port: Number(form.receiving.port),
            loginName: form.receiving.loginName,
            password: form.receiving.password || undefined,
            connectionSecurity: form.receiving.connectionSecurity
          }
        : undefined
    },
    microsoftGraphApplication: isMicrosoftGraph.value && shouldSendMicrosoftGraphApplication.value
      ? {
          applicationSource: form.microsoftGraphApplication.applicationSource,
          tenantId: form.microsoftGraphApplication.tenantId || undefined,
          clientId: form.microsoftGraphApplication.clientId || undefined,
          clientSecret: form.microsoftGraphApplication.clientSecret || undefined
        }
      : undefined
  }
}
</script>

<style lang="scss" scoped>
.email-account-dialog {
  width: min(760px, calc(100vw - 32px));
  max-height: calc(100vh - 32px);
}
</style>
