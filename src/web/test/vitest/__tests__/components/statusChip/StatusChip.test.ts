import { mount } from '@vue/test-utils'
import { QChip } from 'quasar'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { i18n } from 'src/boot/i18n'
import StatusChip from 'src/components/statusChip/StatusChip.vue'
import type { IStatusChipItem, StatusChipValue } from 'src/components/statusChip/types'

vi.mock('#q-app/wrappers', () => ({
  defineBoot: <T>(bootCallback: T): T => bootCallback
}))

const initialLocale = i18n.global.locale.value

function mountStatusChip(status: StatusChipValue, statusStyles: readonly IStatusChipItem[] = []) {
  return mount(StatusChip, {
    props: { status, statusStyles }
  })
}

describe('StatusChip', () => {
  afterEach(() => {
    i18n.global.locale.value = initialLocale
  })

  it('updates the built-in label when locale changes', async () => {
    i18n.global.locale.value = 'zh-CN'
    const wrapper = mountStatusChip('WaitingForQuotaReset')

    expect(wrapper.text()).toContain('等待额度重置')

    i18n.global.locale.value = 'en-US'
    await wrapper.vm.$nextTick()

    expect(wrapper.text()).toContain('Waiting for quota reset')
  })

  it.each([
    ['Paused', 'Paused'],
    ['Completed', 'Completed'],
    ['Canceled', 'Canceled'],
    [true, 'Yes'],
    [false, 'No']
  ] as const)('translates the active status %s', (status, expectedLabel) => {
    i18n.global.locale.value = 'en-US'

    expect(mountStatusChip(status).text()).toContain(expectedLabel)
  })

  it('normalizes status keys, merges styles and does not mutate the caller configuration', () => {
    i18n.global.locale.value = 'en-US'
    const customStyle = Object.freeze({
      status: 'Forbidden Login',
      color: 'info'
    } satisfies IStatusChipItem)
    const customStyles = Object.freeze([customStyle])

    const wrapper = mountStatusChip('forbidden_login', customStyles)

    expect(wrapper.text()).toContain('Disabled')
    expect(wrapper.findComponent(QChip).props('color')).toBe('info')
    expect(customStyle).toEqual({ status: 'Forbidden Login', color: 'info' })
  })

  it('prefers labelKey over label and forwards the configured icon', () => {
    i18n.global.locale.value = 'en-US'
    const wrapper = mountStatusChip('custom_status', [
      {
        status: 'CustomStatus',
        label: 'Literal label',
        labelKey: 'components.statusChip.success',
        icon: 'check'
      }
    ])
    const chip = wrapper.findComponent(QChip)

    expect(wrapper.text()).toContain('Success')
    expect(wrapper.text()).not.toContain('Literal label')
    expect(chip.props('icon')).toBe('check')
  })

  it('allows a literal label to override a built-in translation', () => {
    i18n.global.locale.value = 'en-US'

    const wrapper = mountStatusChip('Success', [{ status: 'success', label: 'Custom result' }])

    expect(wrapper.text()).toContain('Custom result')
  })

  it('uses the original label and a stable visible color for unknown statuses', () => {
    const firstWrapper = mountStatusChip('Vendor_State_42')
    const secondWrapper = mountStatusChip('vendor-state-42')
    const firstColor = firstWrapper.findComponent(QChip).props('color')
    const secondColor = secondWrapper.findComponent(QChip).props('color')

    expect(firstWrapper.text()).toContain('Vendor_State_42')
    expect(firstColor).toBe(secondColor)
    expect(firstColor).not.toBe('white')
  })

  it('preserves forwarded attributes and default slot content', () => {
    const wrapper = mount(StatusChip, {
      attrs: { title: 'status details' },
      props: { status: 'Success' },
      slots: { default: '<span class="status-details">details</span>' }
    })

    expect(wrapper.findComponent(QChip).attributes('title')).toBe('status details')
    expect(wrapper.find('.status-details').text()).toBe('details')
  })
})
