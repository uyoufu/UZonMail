import { flushPromises, mount } from '@vue/test-utils'
import { computed, defineComponent, h, onMounted, ref } from 'vue'
import { afterAll, beforeAll, describe, expect, it, vi } from 'vitest'
import ReceivingAccountsIndex from 'src/pages/emailManager/receivingAccounts/ReceivingAccountsIndex.vue'

vi.mock('src/i18n/helpers', () => ({
  t: (key: string) => key
}))

vi.mock('src/api/receivingAccounts', () => ({
  createBasicReceivingAccount: vi.fn(),
  createMicrosoftGraphReceivingAccount: vi.fn(),
  deleteReceivingAccount: vi.fn(),
  getReceivingAccounts: vi.fn().mockResolvedValue({ data: [] }),
  startReceivingMicrosoftAuthorization: vi.fn(),
  updateImapCredential: vi.fn(),
  updateReceivingAccount: vi.fn(),
  updateReceivingMicrosoftGraphApplication: vi.fn(),
  validateReceivingAccount: vi.fn()
}))

vi.mock('src/api/senderAccounts', () => ({
  getSenderAccounts: vi.fn().mockResolvedValue({ data: [] })
}))

vi.mock('src/utils/dialog', () => ({
  confirmOperation: vi.fn(),
  notifySuccess: vi.fn(),
  notifyUntil: vi.fn(),
  showDialog: vi.fn()
}))

const QTableStub = defineComponent({
  name: 'QTable',
  setup(_props, { slots }) {
    return () => h('div', { 'data-testid': 'receiving-table' }, slots['top-left']?.())
  }
})

const QFabStub = defineComponent({
  name: 'QFab',
  setup(_props, { slots }) {
    return () => h('div', { 'data-testid': 'create-receiving-account' }, slots.default?.())
  }
})

const QFabActionStub = defineComponent({
  name: 'QFabAction',
  props: {
    label: { type: String, default: '' },
    disable: { type: Boolean, default: false }
  },
  setup(props, { slots }) {
    return () => h('button', { disabled: props.disable, 'data-label': props.label }, slots.default?.())
  }
})

describe('ReceivingAccountsIndex', () => {
  beforeAll(() => {
    vi.stubGlobal('computed', computed)
    vi.stubGlobal('onMounted', onMounted)
    vi.stubGlobal('ref', ref)
  })

  afterAll(() => {
    vi.unstubAllGlobals()
  })

  it('offers Basic and Microsoft Graph while keeping generic OAuth disabled', async () => {
    const wrapper = mount(ReceivingAccountsIndex, {
      global: {
        stubs: {
          QTable: QTableStub,
          QFab: QFabStub,
          QFabAction: QFabActionStub,
          QTooltip: true,
          SearchInput: true,
          ContextMenu: true,
          StatusChip: true,
          AsyncTooltip: true
        }
      }
    })
    await flushPromises()

    const actions = wrapper.findAllComponents(QFabActionStub)
    expect(actions.map(action => action.props('label'))).toEqual([
      'accountManagement.receiving.basic',
      'accountManagement.receiving.microsoftGraph',
      'accountManagement.receiving.oauth'
    ])
    expect(actions.map(action => action.props('disable'))).toEqual([false, false, true])
  })
})
