import logger from 'loglevel'
import { debounce } from 'quasar'
import { ConnectionSecurity, type ConnectionSecurity as ConnectionSecurityValue } from 'src/api/accountEnums'
import { EmailAccountConfigurationKind, type IEmailAccount, type IEmailAccountWrite } from 'src/api/emailAccounts'
import { guessImapInfoGet } from 'src/api/imapInfo'
import { guessSmtpInfoGet } from 'src/api/smtpInfo'
import { t, translateGlobal } from 'src/i18n/helpers'
import { isEmail } from 'src/utils/validator'
import {
  CredentialField,
  MailProtocol,
  createEmailAccountDialogForm,
  type CredentialField as CredentialFieldValue,
  type IProtocolCredentialForm,
  type MailProtocol as MailProtocolValue
} from './emailAccountDialogTypes'

interface IUseEmailAccountDialogFormOptions {
  emailGroupId: Ref<number>
  configurationKind: Ref<EmailAccountConfigurationKind>
  account: Ref<IEmailAccount | undefined>
}

const defaultPorts: Record<MailProtocolValue, Record<ConnectionSecurityValue, number>> = {
  [MailProtocol.Smtp]: {
    [ConnectionSecurity.None]: 25,
    [ConnectionSecurity.SSL]: 465,
    [ConnectionSecurity.TLS]: 465,
    [ConnectionSecurity.StartTLS]: 587
  },
  [MailProtocol.Imap]: {
    [ConnectionSecurity.None]: 143,
    [ConnectionSecurity.SSL]: 993,
    [ConnectionSecurity.TLS]: 993,
    [ConnectionSecurity.StartTLS]: 143
  }
}

/** 内聚邮箱账户弹窗的初始化、能力派生、凭据推断和写入映射。 */
export function useEmailAccountDialogForm(options: IUseEmailAccountDialogFormOptions) {
  const form = reactive(createEmailAccountDialogForm())
  const manuallyChangedFields = {
    [MailProtocol.Smtp]: reactive(new Set<CredentialFieldValue>()),
    [MailProtocol.Imap]: reactive(new Set<CredentialFieldValue>())
  }
  let latestGuessSequence = 0

  const isEditing = computed(() => options.account.value !== undefined)
  const isBasic = computed(() => options.configurationKind.value === EmailAccountConfigurationKind.Basic)
  const isMicrosoftGraph = computed(() => !isBasic.value)
  const hasExistingSenderCapability = computed(() => options.account.value?.sender !== undefined)
  const hasExistingReceivingCapability = computed(() => options.account.value?.receiving !== undefined)
  const hasExistingSmtpCredential = computed(() => options.account.value?.sender?.hasCredential === true)
  const hasExistingImapCredential = computed(() => options.account.value?.receiving?.hasCredential === true)
  const hasSmtpChanges = computed(() => manuallyChangedFields[MailProtocol.Smtp].size > 0)
  const hasImapChanges = computed(() => manuallyChangedFields[MailProtocol.Imap].size > 0)
  const isSmtpCredentialComplete = computed(() => isCredentialComplete(form.sender, hasExistingSmtpCredential.value))
  const isImapCredentialComplete = computed(() => isCredentialComplete(form.receiving, hasExistingImapCredential.value))
  const isSenderEnabled = computed(() =>
    isMicrosoftGraph.value
      ? !isEditing.value || hasExistingSenderCapability.value
      : hasExistingSenderCapability.value || (hasSmtpChanges.value && isSmtpCredentialComplete.value)
  )
  const isReceivingEnabled = computed(() =>
    isMicrosoftGraph.value
      ? !isEditing.value || hasExistingReceivingCapability.value
      : hasExistingReceivingCapability.value || (hasImapChanges.value && isImapCredentialComplete.value)
  )
  const shouldValidateSmtpDraft = computed(
    () => hasSmtpChanges.value || (hasExistingSenderCapability.value && !hasExistingSmtpCredential.value)
  )
  const shouldValidateImapDraft = computed(
    () => hasImapChanges.value || (hasExistingReceivingCapability.value && !hasExistingImapCredential.value)
  )
  const shouldWriteSmtpCredential = computed(() => isBasic.value && isSenderEnabled.value && hasSmtpChanges.value)
  const shouldWriteImapCredential = computed(() => isBasic.value && isReceivingEnabled.value && hasImapChanges.value)
  const shouldSendMicrosoftGraphApplication = computed(() => !isEditing.value || form.replaceMicrosoftGraphApplication)
  const smtpPasswordLabel = computed(() =>
    hasExistingSmtpCredential.value ? t('accountManagement.newPassword') : t('accountManagement.password')
  )
  const imapPasswordLabel = computed(() =>
    hasExistingImapCredential.value ? t('accountManagement.newPassword') : t('accountManagement.password')
  )

  const requiredRule = (value: unknown) => Boolean(value) || translateGlobal('required')
  const smtpHostRules = computed(() => (shouldValidateSmtpDraft.value ? [requiredRule] : []))
  const smtpLoginRules = computed(() => (shouldValidateSmtpDraft.value ? [requiredRule] : []))
  const smtpPortRules = computed(() => (shouldValidateSmtpDraft.value ? [validPortRule] : []))
  const smtpPasswordRules = computed(() =>
    shouldValidateSmtpDraft.value && !hasExistingSmtpCredential.value ? [requiredRule] : []
  )
  const imapHostRules = computed(() => (shouldValidateImapDraft.value ? [requiredRule] : []))
  const imapLoginRules = computed(() => (shouldValidateImapDraft.value ? [requiredRule] : []))
  const imapPortRules = computed(() => (shouldValidateImapDraft.value ? [validPortRule] : []))
  const imapPasswordRules = computed(() =>
    shouldValidateImapDraft.value && !hasExistingImapCredential.value ? [requiredRule] : []
  )

  initializeForm()

  const onEmailGuess = debounce((email: string) => {
    void guessProtocolCredentials(email)
  }, 500)

  watch(
    () => form.email,
    (email) => {
      if (!isBasic.value || !isEmail(email)) return
      onEmailGuess(email)
    }
  )
  onScopeDispose(() => onEmailGuess.cancel())

  if (isBasic.value && isEmail(form.email)) void guessProtocolCredentials(form.email)

  function initializeForm() {
    Object.assign(form, createEmailAccountDialogForm())
    const account = options.account.value
    if (!account) return

    form.email = account.email
    form.name = account.name ?? ''
    form.description = account.description ?? ''
    form.remark = account.remark ?? ''
    if (account.sender) {
      form.sender.proxyId = account.sender.proxyId
      form.sender.maxSendCountPerDay = account.sender.maxSendCountPerDay
      form.sender.replyToEmails = account.sender.replyToEmails ?? ''
      form.sender.weight = account.sender.weight
    }
    if (account.receiving) form.receiving.contentRetentionDays = account.receiving.contentRetentionDays
    if (account.oAuthApplicationSource !== undefined) {
      form.microsoftGraphApplication.applicationSource = account.oAuthApplicationSource
    }
  }

  async function guessProtocolCredentials(email: string) {
    const guessSequence = ++latestGuessSequence
    const guesses = await Promise.allSettled([guessSmtpInfoGet(email), guessImapInfoGet(email)])
    if (guessSequence !== latestGuessSequence || email !== form.email) return

    const [smtpGuess, imapGuess] = guesses
    if (smtpGuess.status === 'fulfilled' && !hasExistingSmtpCredential.value) {
      applyServerGuess(MailProtocol.Smtp, form.sender, smtpGuess.value.data, email)
    } else if (smtpGuess.status === 'rejected') {
      logger.warn('[EmailAccountDialog] SMTP 参数推断失败', smtpGuess.reason)
    }

    if (imapGuess.status === 'fulfilled' && !hasExistingImapCredential.value) {
      applyServerGuess(MailProtocol.Imap, form.receiving, imapGuess.value.data, email)
    } else if (imapGuess.status === 'rejected') {
      logger.warn('[EmailAccountDialog] IMAP 参数推断失败', imapGuess.reason)
    }
  }

  function applyServerGuess(
    protocol: MailProtocolValue,
    credential: IProtocolCredentialForm,
    guess: { host: string; port: number; connectionSecurity: ConnectionSecurityValue },
    email: string
  ) {
    const changedFields = manuallyChangedFields[protocol]
    if (!changedFields.has(CredentialField.Host)) credential.host = guess.host
    if (!changedFields.has(CredentialField.Port)) credential.port = guess.port
    if (!changedFields.has(CredentialField.LoginName)) credential.loginName = email
    if (!changedFields.has(CredentialField.ConnectionSecurity)) {
      credential.connectionSecurity = guess.connectionSecurity
    }
  }

  function onCredentialFieldChanged(protocol: MailProtocolValue, field: CredentialFieldValue) {
    manuallyChangedFields[protocol].add(field)
  }

  function onConnectionSecurityChanged(protocol: MailProtocolValue) {
    const credential = protocol === MailProtocol.Smtp ? form.sender : form.receiving
    onCredentialFieldChanged(protocol, CredentialField.ConnectionSecurity)
    onCredentialFieldChanged(protocol, CredentialField.Port)
    credential.port = defaultPorts[protocol][credential.connectionSecurity]
  }

  function hasValidCredentials(): boolean {
    if (shouldValidateSmtpDraft.value && !isSmtpCredentialComplete.value) return false
    if (shouldValidateImapDraft.value && !isImapCredentialComplete.value) return false
    return isSenderEnabled.value || isReceivingEnabled.value
  }

  function toWriteRequest(): IEmailAccountWrite {
    return {
      email: isEditing.value ? undefined : form.email,
      emailGroupId: options.emailGroupId.value,
      name: form.name || undefined,
      description: form.description || undefined,
      remark: form.remark || undefined,
      configurationKind: options.configurationKind.value,
      sender: {
        isEnabled: isSenderEnabled.value,
        proxyId: form.sender.proxyId || undefined,
        maxSendCountPerDay: Number(form.sender.maxSendCountPerDay) || 0,
        replyToEmails: form.sender.replyToEmails || undefined,
        weight: Math.max(1, Number(form.sender.weight) || 1),
        smtpCredential: shouldWriteSmtpCredential.value ? toCredentialWrite(form.sender) : undefined
      },
      receiving: {
        isEnabled: isReceivingEnabled.value,
        contentRetentionDays: Math.max(1, Number(form.receiving.contentRetentionDays) || 30),
        imapCredential: shouldWriteImapCredential.value ? toCredentialWrite(form.receiving) : undefined
      },
      microsoftGraphApplication:
        isMicrosoftGraph.value && shouldSendMicrosoftGraphApplication.value
          ? {
              applicationSource: form.microsoftGraphApplication.applicationSource,
              tenantId: form.microsoftGraphApplication.tenantId || undefined,
              clientId: form.microsoftGraphApplication.clientId || undefined,
              clientSecret: form.microsoftGraphApplication.clientSecret || undefined
            }
          : undefined
    }
  }

  return {
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
  }
}

function isCredentialComplete(credential: IProtocolCredentialForm, hasExistingPassword: boolean): boolean {
  const port = Number(credential.port)
  return Boolean(
    credential.host && credential.loginName && (credential.password || hasExistingPassword) && port > 0 && port <= 65535
  )
}

function validPortRule(value: unknown) {
  const port = Number(value)
  return (port > 0 && port <= 65535) || t('accountManagement.emailAccount.portInvalid')
}

function toCredentialWrite(credential: IProtocolCredentialForm) {
  return {
    host: credential.host,
    port: Number(credential.port),
    loginName: credential.loginName,
    password: credential.password || undefined,
    connectionSecurity: credential.connectionSecurity
  }
}
