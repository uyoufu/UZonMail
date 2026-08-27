import { flushPromises, mount } from '@vue/test-utils'
import { computed, defineComponent, h, onMounted, ref, type ComputedRef } from 'vue'
import { createI18n } from 'vue-i18n'
import { afterAll, beforeAll, beforeEach, describe, expect, it, vi } from 'vitest'
import { SendingItemStatus, type ISendingItem } from 'src/api/sendingItem'
import type { IContextMenuItem } from 'src/components/contextMenu/types'
import SendingItemDetailDialog from 'src/pages/sendingManager/sendHistory/SendingItemDetailDialog.vue'
import { useContextMenu } from 'src/pages/sendingManager/sendHistory/sendDetailContext'

const mocks = vi.hoisted(() => ({
  downloadFileUsage: vi.fn(),
  getSendingItemDetail: vi.fn(),
  onDialogCancel: vi.fn(),
  onDialogHide: vi.fn(),
  resendSendingItem: vi.fn(),
  showComponentDialog: vi.fn()
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
        onDialogHide: mocks.onDialogHide
      }),
      { emits: ['hide'] }
    )
  }
})

vi.mock('src/api/sendingItem', async (importOriginal) => {
  const original = await importOriginal()
  if (!original || typeof original !== 'object') throw new Error('Sending item module mock initialization failed')
  return { ...original, getSendingItemDetail: mocks.getSendingItemDetail }
})
vi.mock('src/api/emailSending', () => ({ resendSendingItem: mocks.resendSendingItem }))
vi.mock('src/compositions/useFileUsageDownload', () => ({
  useFileUsageDownload: () => ({ downloadFileUsage: mocks.downloadFileUsage })
}))
vi.mock('src/utils/dialog', () => ({
  confirmOperation: vi.fn(),
  notifySuccess: vi.fn(),
  showComponentDialog: mocks.showComponentDialog
}))
vi.mock('src/utils/format', () => ({ formatDate: (value: string) => value }))

const dialogStubs = {
  QCard: { template: '<div><slot /></div>' },
  QCardSection: { template: '<section><slot /></section>' },
  QDialog: { template: '<div><slot /></div>' },
  QIcon: true,
  QItem: { template: '<div><slot /></div>' },
  QItemLabel: { template: '<div><slot /></div>' },
  QItemSection: { template: '<div><slot /></div>' },
  QList: { template: '<div><slot /></div>' },
  QSeparator: true,
  QSpinnerDots: true,
  CommonBtn: {
    props: {
      label: String,
      loading: Boolean,
      tooltip: String
    },
    emits: ['click'],
    template: '<button :data-tooltip="tooltip" :disabled="loading" @click="$emit(\'click\')">{{ label }}</button>'
  }
}

const completeEmail = {
  id: 81,
  subject: 'Quarterly update',
  fromEmail: 'sender@example.com',
  sentAt: '2026-08-11T02:30:00Z',
  recipients: [{ email: 'recipient@example.com', name: 'Recipient' }],
  ccRecipients: [{ email: 'copy@example.com', name: 'Copy' }],
  bccRecipients: [{ email: 'hidden@example.com', name: 'Hidden' }],
  content: '<html><head><meta http-equiv="refresh" content="0"><script type="application/json">unsafe</script></head><body><img src="https://cdn.example.com/banner.png"><form><input></form><p>Hello</p></body></html>',
  attachments: [{ id: 9, displayName: 'report.pdf', size: 4096 }]
}

const i18n = createI18n({
  legacy: false,
  locale: 'en-US',
  missingWarn: false,
  fallbackWarn: false,
  messages: { 'en-US': {} }
})

describe('sending item detail', () => {
  beforeAll(() => {
    vi.stubGlobal('computed', computed)
    vi.stubGlobal('onMounted', onMounted)
    vi.stubGlobal('ref', ref)
  })

  afterAll(() => {
    vi.unstubAllGlobals()
  })

  beforeEach(() => {
    vi.clearAllMocks()
    mocks.getSendingItemDetail.mockResolvedValue({ data: completeEmail })
    mocks.showComponentDialog.mockResolvedValue({ ok: false, data: {} })
  })

  it('uses localized menu labels and opens the complete email dialog', async () => {
    let sendDetailContextItems: ComputedRef<IContextMenuItem<ISendingItem>[]> | undefined
    const contextHarness = defineComponent({
      setup() {
        sendDetailContextItems = useContextMenu().sendDetailContextItems
        return () => h('div')
      }
    })
    mount(contextHarness, { global: { plugins: [i18n] } })
    if (!sendDetailContextItems) throw new Error('Context menu was not initialized')
    const viewEmailAction = sendDetailContextItems.value.find(action => action.name === 'viewEmail')!
    const sendingItem: ISendingItem = {
      id: 81,
      subject: 'Quarterly update',
      senderEmail: 'sender@example.com',
      recipients: completeEmail.recipients,
      sendDate: completeEmail.sentAt,
      status: SendingItemStatus.Success
    }

    expect(viewEmailAction.label).toBe('sendDetail.viewEmail')
    expect(viewEmailAction.tooltip).toBe('sendDetail.viewEmailTooltip')
    expect(viewEmailAction.vif?.(sendingItem)).toBe(true)
    expect(viewEmailAction.vif?.({ ...sendingItem, status: SendingItemStatus.Failed })).toBe(false)

    await viewEmailAction.onClick(sendingItem, { targetValues: [sendingItem], clearSelection: vi.fn() })

    expect(mocks.showComponentDialog).toHaveBeenCalledWith(
      SendingItemDetailDialog,
      { sendingItemId: sendingItem.id }
    )
  })

  it('renders complete headers, isolated HTML and downloadable attachments', async () => {
    const wrapper = mount(SendingItemDetailDialog, {
      props: { sendingItemId: completeEmail.id },
      global: { plugins: [i18n], stubs: dialogStubs }
    })
    await flushPromises()

    expect(wrapper.text()).toContain(completeEmail.subject)
    expect(wrapper.text()).toContain('Recipient <recipient@example.com>')
    expect(wrapper.text()).toContain('Copy <copy@example.com>')
    expect(wrapper.text()).toContain('Hidden <hidden@example.com>')
    expect(wrapper.text()).toContain('report.pdf')

    const emailFrame = wrapper.get('iframe')
    const sourceDocument = emailFrame.attributes('srcdoc')
    expect(emailFrame.attributes('sandbox')).toBe('allow-popups allow-popups-to-escape-sandbox')
    expect(sourceDocument).toContain('https://cdn.example.com/banner.png')
    expect(sourceDocument).not.toContain('<script')
    expect(sourceDocument).not.toContain('<form')
    expect(sourceDocument).not.toContain('http-equiv="refresh"')

    await wrapper.get('[data-tooltip="sendDetail.downloadAttachment"]').trigger('click')
    expect(mocks.downloadFileUsage).toHaveBeenCalledWith(completeEmail.attachments[0])
  })

  it('shows a retry action when loading fails', async () => {
    mocks.getSendingItemDetail
      .mockRejectedValueOnce(new Error('request failed'))
      .mockResolvedValueOnce({ data: completeEmail })
    const wrapper = mount(SendingItemDetailDialog, {
      props: { sendingItemId: completeEmail.id },
      global: { plugins: [i18n], stubs: dialogStubs }
    })
    await flushPromises()

    expect(wrapper.text()).toContain('sendDetail.loadEmailFailed')
    await wrapper.get('[data-tooltip="sendDetail.retry"]').trigger('click')
    await flushPromises()

    expect(mocks.getSendingItemDetail).toHaveBeenCalledTimes(2)
    expect(wrapper.text()).toContain(completeEmail.subject)
  })
})
