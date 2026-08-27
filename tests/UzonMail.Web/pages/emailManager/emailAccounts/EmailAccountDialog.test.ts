import { mount } from '@vue/test-utils'
import { computed, reactive, ref, watch } from 'vue'
import { afterAll, beforeAll, describe, expect, it, vi } from 'vitest'
import { EmailAccountConfigurationKind } from 'src/api/emailAccounts'
import EmailAccountDialog from 'src/pages/emailManager/emailAccounts/EmailAccountDialog.vue'

vi.mock('src/i18n/helpers', () => ({
  t: (key: string) => key,
  translateGlobal: (key: string) => key
}))
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

const dialogStubs = {
  QCard: { template: '<div><slot /></div>' },
  QCardActions: { template: '<div><slot /></div>' },
  QCardSection: { template: '<section><slot /></section>' },
  QDialog: { props: ['modelValue'], template: '<div v-if="modelValue"><slot /></div>' },
  QForm: { template: '<form><slot /></form>' },
  QInput: { inheritAttrs: false, props: ['label', 'modelValue'], template: '<input :aria-label="label" />' },
  QSelect: { inheritAttrs: false, props: ['label', 'modelValue'], template: '<select :aria-label="label" />' },
  QSeparator: true,
  QSpace: true,
  QToggle: { inheritAttrs: false, props: ['label', 'modelValue'], template: '<label>{{ label }}</label>' },
  QBtn: true,
  QTooltip: true,
  QTabs: { template: '<div><slot /></div>' },
  QTab: { props: ['label'], template: '<button>{{ label }}</button>' },
  QTabPanels: { template: '<div><slot /></div>' },
  QTabPanel: { template: '<section><slot /></section>' }
}

describe('EmailAccountDialog', () => {
  beforeAll(() => {
    vi.stubGlobal('computed', computed)
    vi.stubGlobal('reactive', reactive)
    vi.stubGlobal('ref', ref)
    vi.stubGlobal('watch', watch)
  })

  afterAll(() => {
    vi.unstubAllGlobals()
  })

  it('keeps Basic sender and receiving setup in one tabbed dialog', () => {
    const wrapper = mount(EmailAccountDialog, {
      props: {
        modelValue: true,
        emailGroupId: 10,
        configurationKind: EmailAccountConfigurationKind.Basic
      },
      global: { stubs: dialogStubs }
    })

    expect(wrapper.text()).toContain('accountManagement.emailAccount.senderSettings')
    expect(wrapper.text()).toContain('accountManagement.emailAccount.receivingSettings')
    expect(wrapper.text()).toContain('accountManagement.emailAccount.enableSender')
    expect(wrapper.text()).toContain('accountManagement.emailAccount.enableReceiving')
  })
})
