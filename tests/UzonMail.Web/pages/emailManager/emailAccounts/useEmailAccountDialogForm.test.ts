import { flushPromises, mount } from '@vue/test-utils'
import { computed, defineComponent, h, onScopeDispose, ref, watch, type Ref } from 'vue'
import { afterAll, afterEach, beforeAll, beforeEach, describe, expect, it, vi } from 'vitest'
import { ConnectionSecurity } from 'src/api/accountEnums'
import { EmailAccountConfigurationKind, type IEmailAccount } from 'src/api/emailAccounts'
import { CredentialField, MailProtocol } from 'src/pages/emailManager/emailAccounts/emailAccountDialogTypes'
import { useEmailAccountDialogForm } from 'src/pages/emailManager/emailAccounts/useEmailAccountDialogForm'

const mocks = vi.hoisted(() => ({
  guessImapInfoGet: vi.fn(),
  guessSmtpInfoGet: vi.fn()
}))

vi.mock('src/api/smtpInfo', () => ({
  guessSmtpInfoGet: mocks.guessSmtpInfoGet
}))
vi.mock('src/api/imapInfo', () => ({
  guessImapInfoGet: mocks.guessImapInfoGet
}))
vi.mock('src/i18n/helpers', () => ({
  t: (key: string) => key,
  translateGlobal: (key: string) => key,
  translateButton: (key: string) => key
}))

type DialogFormApi = ReturnType<typeof useEmailAccountDialogForm>

function mountDialogForm(configurationKind: EmailAccountConfigurationKind, account?: IEmailAccount) {
  let dialogFormApi: DialogFormApi | undefined
  const host = defineComponent({
    setup() {
      dialogFormApi = useEmailAccountDialogForm({
        emailGroupId: ref(10),
        configurationKind: ref(configurationKind),
        account: ref(account) as Ref<IEmailAccount | undefined>
      })
      return () => h('div')
    }
  })
  const wrapper = mount(host)
  if (!dialogFormApi) throw new Error('Dialog form was not initialized')
  return { wrapper, dialogFormApi }
}

describe('useEmailAccountDialogForm', () => {
  beforeAll(() => {
    vi.stubGlobal('computed', computed)
    vi.stubGlobal('onScopeDispose', onScopeDispose)
    vi.stubGlobal('ref', ref)
    vi.stubGlobal('watch', watch)
  })

  afterAll(() => vi.unstubAllGlobals())

  beforeEach(() => {
    vi.clearAllMocks()
    vi.useFakeTimers()
    mocks.guessSmtpInfoGet.mockResolvedValue({
      data: {
        domain: 'example.com',
        host: 'smtp.provider.test',
        port: 587,
        connectionSecurity: ConnectionSecurity.StartTLS
      }
    })
    mocks.guessImapInfoGet.mockResolvedValue({
      data: {
        domain: 'example.com',
        host: 'imap.provider.test',
        port: 993,
        connectionSecurity: ConnectionSecurity.SSL
      }
    })
  })

  afterEach(() => vi.useRealTimers())

  it('fills SMTP and IMAP settings but enables only the completed capability', async () => {
    const { wrapper, dialogFormApi } = mountDialogForm(EmailAccountConfigurationKind.Basic)
    dialogFormApi.form.value.email = 'owner@example.com'
    await vi.advanceTimersByTimeAsync(500)
    await flushPromises()

    expect(dialogFormApi.form.value.sender).toMatchObject({
      host: 'smtp.provider.test',
      port: 587,
      loginName: 'owner@example.com',
      connectionSecurity: ConnectionSecurity.StartTLS
    })
    expect(dialogFormApi.form.value.receiving).toMatchObject({
      host: 'imap.provider.test',
      port: 993,
      loginName: 'owner@example.com',
      connectionSecurity: ConnectionSecurity.SSL
    })

    dialogFormApi.form.value.sender.password = 'smtp-secret'
    dialogFormApi.onCredentialFieldChanged(MailProtocol.Smtp, CredentialField.Password)
    const senderOnlyRequest = dialogFormApi.toWriteRequest()
    expect(senderOnlyRequest.sender.isEnabled).toBe(true)
    expect(senderOnlyRequest.receiving.isEnabled).toBe(false)
    expect(senderOnlyRequest.sender.smtpCredential?.host).toBe('smtp.provider.test')

    dialogFormApi.form.value.receiving.password = 'imap-secret'
    dialogFormApi.onCredentialFieldChanged(MailProtocol.Imap, CredentialField.Password)
    const completeRequest = dialogFormApi.toWriteRequest()
    expect(completeRequest.receiving.isEnabled).toBe(true)
    expect(completeRequest.receiving.imapCredential?.host).toBe('imap.provider.test')
    wrapper.unmount()
  })

  it('does not overwrite a manually changed host with a delayed guess', async () => {
    const { wrapper, dialogFormApi } = mountDialogForm(EmailAccountConfigurationKind.Basic)
    dialogFormApi.form.value.sender.host = 'smtp.custom.test'
    dialogFormApi.onCredentialFieldChanged(MailProtocol.Smtp, CredentialField.Host)
    dialogFormApi.form.value.email = 'owner@example.com'
    await vi.advanceTimersByTimeAsync(500)
    await flushPromises()

    expect(dialogFormApi.form.value.sender.host).toBe('smtp.custom.test')
    expect(dialogFormApi.form.value.receiving.host).toBe('imap.provider.test')
    wrapper.unmount()
  })

  it('enables both Microsoft Graph capabilities for a new account', () => {
    const { wrapper, dialogFormApi } = mountDialogForm(EmailAccountConfigurationKind.MicrosoftGraph)
    dialogFormApi.form.value.email = 'owner@example.com'

    const request = dialogFormApi.toWriteRequest()
    expect(request.sender.isEnabled).toBe(true)
    expect(request.receiving.isEnabled).toBe(true)
    expect(request.microsoftGraphApplication).toBeDefined()
    wrapper.unmount()
  })

  it('returns non-sensitive existing connection settings without rewriting untouched credentials', () => {
    const account: IEmailAccount = {
      id: 20,
      emailGroupId: 10,
      email: 'owner@example.com',
      hasOAuthAuthorization: false,
      sender: {
        id: 21,
        protocol: 0,
        status: 0,
        maxSendCountPerDay: 100,
        sentTotalToday: 0,
        weight: 1,
        hasCredential: true,
        smtpCredential: {
          host: 'smtp.custom.test',
          port: 465,
          connectionSecurity: ConnectionSecurity.SSL,
          loginName: 'owner@example.com'
        }
      }
    }
    const { wrapper, dialogFormApi } = mountDialogForm(EmailAccountConfigurationKind.Basic, account)

    expect(dialogFormApi.form.value.sender).toMatchObject({
      host: 'smtp.custom.test',
      port: 465,
      loginName: 'owner@example.com',
      password: ''
    })

    const request = dialogFormApi.toWriteRequest()
    expect(request.sender.isEnabled).toBe(true)
    expect(request.sender.smtpCredential).toBeUndefined()
    dialogFormApi.form.value.sender.proxyId = 42
    expect(dialogFormApi.toWriteRequest().sender.proxyId).toBe(42)
    dialogFormApi.form.value.sender.proxyId = null
    expect(dialogFormApi.toWriteRequest().sender.proxyId).toBeUndefined()
    expect(request.receiving.isEnabled).toBe(false)
    wrapper.unmount()
  })

  it('disables an existing SMTP capability when its server is cleared', () => {
    const account: IEmailAccount = {
      id: 20,
      emailGroupId: 10,
      email: 'owner@example.com',
      hasOAuthAuthorization: false,
      sender: {
        id: 21,
        protocol: 0,
        status: 0,
        maxSendCountPerDay: 100,
        sentTotalToday: 0,
        weight: 1,
        hasCredential: true,
        smtpCredential: {
          host: 'smtp.custom.test',
          port: 465,
          connectionSecurity: ConnectionSecurity.SSL,
          loginName: 'owner@example.com'
        }
      },
      receiving: {
        id: 22,
        protocol: 0,
        status: 0,
        contentRetentionDays: 30,
        hasCredential: true,
        imapCredential: {
          host: 'imap.custom.test',
          port: 993,
          connectionSecurity: ConnectionSecurity.SSL,
          loginName: 'owner@example.com'
        }
      }
    }
    const { wrapper, dialogFormApi } = mountDialogForm(EmailAccountConfigurationKind.Basic, account)
    dialogFormApi.form.value.sender.host = ''
    dialogFormApi.onCredentialFieldChanged(MailProtocol.Smtp, CredentialField.Host)

    const request = dialogFormApi.toWriteRequest()
    expect(request.sender.isEnabled).toBe(false)
    expect(request.sender.smtpCredential).toBeUndefined()
    expect(request.receiving.isEnabled).toBe(true)
    expect(dialogFormApi.hasValidCredentials()).toBe(true)
    wrapper.unmount()
  })
})
