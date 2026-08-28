import logger from 'loglevel'
import { debounce } from 'quasar'
import { computed, onScopeDispose, ref, watch, type Ref } from 'vue'
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

interface IProtocolCredentialRecommendation {
  host: string
  port: number
  connectionSecurity: ConnectionSecurityValue
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
  const form = ref(createEmailAccountDialogForm())
  const manuallyChangedFields = {
    [MailProtocol.Smtp]: ref(new Set<CredentialFieldValue>()),
    [MailProtocol.Imap]: ref(new Set<CredentialFieldValue>())
  }
  const credentialRecommendations = {
    [MailProtocol.Smtp]: ref<IProtocolCredentialRecommendation>(),
    [MailProtocol.Imap]: ref<IProtocolCredentialRecommendation>()
  }
  let latestGuessSequence = 0

  const isEditing = computed(() => options.account.value !== undefined)
  const isBasic = computed(() => options.configurationKind.value === EmailAccountConfigurationKind.Basic)
  const isMicrosoftGraph = computed(() => !isBasic.value)
  const hasExistingSenderCapability = computed(() => options.account.value?.sender !== undefined)
  const hasExistingReceivingCapability = computed(() => options.account.value?.receiving !== undefined)
  const hasExistingSmtpCredential = computed(() => options.account.value?.sender?.hasCredential === true)
  const hasExistingImapCredential = computed(() => options.account.value?.receiving?.hasCredential === true)
  const hasSmtpChanges = computed(() => manuallyChangedFields[MailProtocol.Smtp].value.size > 0)
  const hasImapChanges = computed(() => manuallyChangedFields[MailProtocol.Imap].value.size > 0)
  const hasSmtpServer = computed(() => Boolean(form.value.sender.host.trim()))
  const hasImapServer = computed(() => Boolean(form.value.receiving.host.trim()))
  const isSmtpCredentialComplete = computed(() =>
    isCredentialComplete(form.value.sender, hasExistingSmtpCredential.value)
  )
  const isImapCredentialComplete = computed(() =>
    isCredentialComplete(form.value.receiving, hasExistingImapCredential.value)
  )
  const isSenderEnabled = computed(() =>
    isMicrosoftGraph.value
      ? !isEditing.value || hasExistingSenderCapability.value
      : hasSmtpServer.value &&
        (hasExistingSmtpCredential.value || (hasSmtpChanges.value && isSmtpCredentialComplete.value))
  )
  const isReceivingEnabled = computed(() =>
    isMicrosoftGraph.value
      ? !isEditing.value || hasExistingReceivingCapability.value
      : hasImapServer.value &&
        (hasExistingImapCredential.value || (hasImapChanges.value && isImapCredentialComplete.value))
  )
  const shouldValidateSmtpDraft = computed(() => hasSmtpChanges.value && hasSmtpServer.value)
  const shouldValidateImapDraft = computed(() => hasImapChanges.value && hasImapServer.value)
  const shouldWriteSmtpCredential = computed(() => isBasic.value && isSenderEnabled.value && hasSmtpChanges.value)
  const shouldWriteImapCredential = computed(() => isBasic.value && isReceivingEnabled.value && hasImapChanges.value)
  const shouldSendMicrosoftGraphApplication = computed(
    () => !isEditing.value || form.value.replaceMicrosoftGraphApplication
  )
  const smtpPasswordLabel = computed(() =>
    hasExistingSmtpCredential.value ? t('accountManagement.newPassword') : t('accountManagement.password')
  )
  const imapPasswordLabel = computed(() =>
    hasExistingImapCredential.value ? t('accountManagement.newPassword') : t('accountManagement.password')
  )

  const requiredRule = (value: unknown) => Boolean(value) || translateGlobal('required')
  const smtpHostRules = computed(() => (shouldValidateSmtpDraft.value ? [requiredRule] : []))
  const smtpPortRules = computed(() => (shouldValidateSmtpDraft.value ? [validPortRule] : []))
  const smtpPasswordRules = computed(() =>
    shouldValidateSmtpDraft.value && !hasExistingSmtpCredential.value ? [requiredRule] : []
  )
  const imapHostRules = computed(() => (shouldValidateImapDraft.value ? [requiredRule] : []))
  const imapPortRules = computed(() => (shouldValidateImapDraft.value ? [validPortRule] : []))
  const imapPasswordRules = computed(() =>
    shouldValidateImapDraft.value && !hasExistingImapCredential.value ? [requiredRule] : []
  )

  initializeForm()

  const onEmailGuess = debounce((email: string) => {
    void guessProtocolCredentials(email)
  }, 500)

  watch(
    () => form.value.email,
    (email) => {
      credentialRecommendations[MailProtocol.Smtp].value = undefined
      credentialRecommendations[MailProtocol.Imap].value = undefined
      if (!isBasic.value || !isEmail(email)) return
      onEmailGuess(email)
    }
  )
  onScopeDispose(() => onEmailGuess.cancel())

  if (isBasic.value && isEmail(form.value.email)) void guessProtocolCredentials(form.value.email)

  function initializeForm() {
    form.value = createEmailAccountDialogForm()
    const account = options.account.value
    if (!account) return

    form.value.email = account.email
    form.value.name = account.name ?? ''
    form.value.description = account.description ?? ''
    form.value.remark = account.remark ?? ''
    if (account.sender) {
      form.value.sender.proxyId = account.sender.proxyId ?? null
      form.value.sender.maxSendCountPerDay = account.sender.maxSendCountPerDay
      form.value.sender.replyToEmails = account.sender.replyToEmails ?? ''
      Object.assign(form.value.sender, account.sender.smtpCredential)
    }
    if (account.receiving) {
      form.value.receiving.contentRetentionDays = account.receiving.contentRetentionDays
      Object.assign(form.value.receiving, account.receiving.imapCredential)
    }
    if (account.oAuthApplicationSource !== undefined) {
      form.value.microsoftGraphApplication.applicationSource = account.oAuthApplicationSource
    }
  }

  async function guessProtocolCredentials(email: string) {
    const guessSequence = ++latestGuessSequence
    const guesses = await Promise.allSettled([guessSmtpInfoGet(email), guessImapInfoGet(email)])
    if (guessSequence !== latestGuessSequence || email !== form.value.email) return

    const [smtpGuess, imapGuess] = guesses
    if (smtpGuess.status === 'fulfilled') {
      credentialRecommendations[MailProtocol.Smtp].value = smtpGuess.value.data
      applyServerGuess(MailProtocol.Smtp, form.value.sender, smtpGuess.value.data)
    } else if (smtpGuess.status === 'rejected') {
      logger.warn('[EmailAccountDialog] SMTP 参数推断失败', smtpGuess.reason)
    }

    if (imapGuess.status === 'fulfilled') {
      credentialRecommendations[MailProtocol.Imap].value = imapGuess.value.data
      applyServerGuess(MailProtocol.Imap, form.value.receiving, imapGuess.value.data)
    } else if (imapGuess.status === 'rejected') {
      logger.warn('[EmailAccountDialog] IMAP 参数推断失败', imapGuess.reason)
    }
  }

  function applyServerGuess(
    protocol: MailProtocolValue,
    credential: IProtocolCredentialForm,
    guess: IProtocolCredentialRecommendation
  ) {
    const changedFields = manuallyChangedFields[protocol]
    if (!changedFields.value.has(CredentialField.Host) || !credential.host.trim()) credential.host = guess.host
    if (!changedFields.value.has(CredentialField.Port) || !isValidPort(credential.port)) credential.port = guess.port
    if (!changedFields.value.has(CredentialField.ConnectionSecurity)) {
      credential.connectionSecurity = guess.connectionSecurity
    }
  }

  function onCredentialFieldChanged(protocol: MailProtocolValue, field: CredentialFieldValue) {
    const changedFields = manuallyChangedFields[protocol]
    if (!changedFields.value.has(field)) changedFields.value = new Set([...changedFields.value, field])

    const recommendation = credentialRecommendations[protocol].value
    if (!recommendation || field === CredentialField.LoginName || field === CredentialField.Password) return
    const hasExistingCredential =
      protocol === MailProtocol.Smtp ? hasExistingSmtpCredential.value : hasExistingImapCredential.value
    if (hasExistingCredential) return
    const credential = protocol === MailProtocol.Smtp ? form.value.sender : form.value.receiving
    applyServerGuess(protocol, credential, recommendation)
  }

  function onConnectionSecurityChanged(protocol: MailProtocolValue) {
    const credential = protocol === MailProtocol.Smtp ? form.value.sender : form.value.receiving
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
      email: isEditing.value ? undefined : form.value.email,
      emailGroupId: options.emailGroupId.value,
      name: form.value.name || undefined,
      description: form.value.description || undefined,
      remark: form.value.remark || undefined,
      configurationKind: options.configurationKind.value,
      sender: {
        isEnabled: isSenderEnabled.value,
        proxyId: form.value.sender.proxyId ?? undefined,
        maxSendCountPerDay: Number(form.value.sender.maxSendCountPerDay) || 0,
        replyToEmails: form.value.sender.replyToEmails || undefined,
        smtpCredential: shouldWriteSmtpCredential.value ? toCredentialWrite(form.value.sender) : undefined
      },
      receiving: {
        isEnabled: isReceivingEnabled.value,
        contentRetentionDays: Math.max(1, Number(form.value.receiving.contentRetentionDays) || 30),
        imapCredential: shouldWriteImapCredential.value ? toCredentialWrite(form.value.receiving) : undefined
      },
      microsoftGraphApplication:
        isMicrosoftGraph.value && shouldSendMicrosoftGraphApplication.value
          ? {
              applicationSource: form.value.microsoftGraphApplication.applicationSource,
              tenantId: form.value.microsoftGraphApplication.tenantId || undefined,
              clientId: form.value.microsoftGraphApplication.clientId || undefined,
              clientSecret: form.value.microsoftGraphApplication.clientSecret || undefined
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
    smtpPortRules,
    smtpPasswordRules,
    imapHostRules,
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
  return Boolean(credential.host && (credential.password || hasExistingPassword) && isValidPort(port))
}

function validPortRule(value: unknown) {
  return isValidPort(value) || t('accountManagement.emailAccount.portInvalid')
}

function isValidPort(value: unknown): boolean {
  const port = Number(value)
  return port > 0 && port <= 65535
}

function toCredentialWrite(credential: IProtocolCredentialForm) {
  return {
    host: credential.host,
    port: Number(credential.port),
    loginName: credential.loginName || undefined,
    password: credential.password || undefined,
    connectionSecurity: credential.connectionSecurity
  }
}
