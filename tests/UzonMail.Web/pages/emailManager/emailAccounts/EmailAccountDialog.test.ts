import { mount } from '@vue/test-utils'
import { computed, onScopeDispose, ref, toRefs, watch } from 'vue'
import { afterAll, beforeAll, beforeEach, describe, expect, it, vi } from 'vitest'
import { EmailAccountConfigurationKind } from 'src/api/emailAccounts'
import EmailAccountDialog from 'src/pages/emailManager/emailAccounts/EmailAccountDialog.vue'

const mocks = vi.hoisted(() => ({
  onDialogCancel: vi.fn(),
  onDialogHide: vi.fn(),
  onDialogOK: vi.fn()
}))

vi.mock('quasar', async (importOriginal) => {
  const original = await importOriginal()
  if (!original || typeof original !== 'object') throw new Error('Quasar module mock initialization failed')
  return {
    ...original,
    useDialogPluginComponent: Object.assign(
      () => ({
        dialogRef: ref(null),
        onDialogCancel: mocks.onDialogCancel,
        onDialogHide: mocks.onDialogHide,
        onDialogOK: mocks.onDialogOK
      }),
      { emits: ['ok', 'hide'] }
    )
  }
})
vi.mock('src/i18n/helpers', () => ({
  t: (key: string) => key,
  translateGlobal: (key: string) => key,
  translateButton: (key: string) => key
}))
vi.mock('src/api/smtpInfo', () => ({ guessSmtpInfoGet: vi.fn() }))
vi.mock('src/api/imapInfo', () => ({ guessImapInfoGet: vi.fn() }))
vi.mock('src/api/emailAccounts', async (importOriginal) => {
  const original = await importOriginal<typeof import('src/api/emailAccounts')>()
  return {
    ...original,
    createBasicEmailAccount: vi.fn(),
    createMicrosoftGraphEmailAccount: vi.fn(),
    updateEmailAccount: vi.fn()
  }
})
vi.mock('src/utils/dialog', () => ({
  notifyError: vi.fn(),
  notifySuccess: vi.fn()
}))

const slotStub = { template: '<div><slot /></div>' }
const dialogStubs = {
  QCard: slotStub,
  QCardActions: slotStub,
  QCardSection: slotStub,
  QDialog: slotStub,
  QForm: { template: '<form><slot /></form>' },
  QInput: { inheritAttrs: false, template: '<input />' },
  QSelect: {
    props: {
      label: String,
      clearable: Boolean,
      loading: Boolean,
      optionLabel: String,
      optionValue: String,
      options: Array
    },
    emits: ['popup-show'],
    template:
      '<select :data-label="label" :data-clearable="clearable ? \'true\' : \'false\'" :data-option-label="optionLabel" :data-option-value="optionValue" />'
  },
  QSeparator: true,
  QTabPanels: slotStub,
  QTabPanel: slotStub,
  QToggle: true,
  CancelBtn: { template: '<button data-cancel-button />' },
  OkBtn: { template: '<button data-ok-button />' },
  TitleBar: {
    props: ['title'],
    emits: ['close'],
    template: '<button data-title-bar @click="$emit(\'close\')">{{ title }}</button>'
  },
  UTabs: {
    props: ['tabs', 'align'],
    template:
      '<div data-tabs :data-align="align"><span v-for="tab in tabs" :key="tab.name">{{ tab.label }}</span></div>'
  }
}

describe('EmailAccountDialog', () => {
  beforeAll(() => {
    vi.stubGlobal('computed', computed)
    vi.stubGlobal('onScopeDispose', onScopeDispose)
    vi.stubGlobal('ref', ref)
    vi.stubGlobal('toRefs', toRefs)
    vi.stubGlobal('watch', watch)
  })

  afterAll(() => vi.unstubAllGlobals())
  beforeEach(() => vi.clearAllMocks())

  it('uses the shared title, centered tabs, and a clearable proxy selector without capability toggles', async () => {
    const wrapper = mount(EmailAccountDialog, {
      props: {
        emailGroupId: 10,
        configurationKind: EmailAccountConfigurationKind.Basic
      },
      global: { stubs: dialogStubs }
    })

    expect(wrapper.get('[data-title-bar]').text()).toBe('accountManagement.emailAccount.create')
    expect(wrapper.get('[data-tabs]').attributes('data-align')).toBe('center')
    expect(wrapper.text()).toContain('accountManagement.emailAccount.senderSettings')
    expect(wrapper.text()).toContain('accountManagement.emailAccount.receivingSettings')
    expect(wrapper.text()).not.toContain('accountManagement.emailAccount.enableSender')
    expect(wrapper.text()).not.toContain('accountManagement.emailAccount.enableReceiving')
    const proxySelector = wrapper.find('select[data-label="accountManagement.emailAccount.proxy"]')
    expect(proxySelector.attributes('data-clearable')).toBe('true')
    expect(proxySelector.attributes('data-option-label')).toBe('name')
    expect(proxySelector.attributes('data-option-value')).toBe('id')
    expect(wrapper.find('[data-cancel-button]').exists()).toBe(true)
    expect(wrapper.find('[data-ok-button]').exists()).toBe(true)
  })
})
