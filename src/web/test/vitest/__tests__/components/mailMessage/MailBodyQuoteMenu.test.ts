import { flushPromises, mount } from '@vue/test-utils'
import { defineComponent, h, ref } from 'vue'
import { afterAll, beforeAll, describe, expect, it, vi } from 'vitest'
import type * as VueI18nModule from 'vue-i18n'
import { MailMessageDirection, type IMailMessage } from 'src/api/mailConversation'
import MailBodyQuoteMenu from 'src/components/mailMessage/MailBodyQuoteMenu.vue'

vi.mock('vue-i18n', async (importOriginal) => {
  const vueI18n = await importOriginal<typeof VueI18nModule>()
  return {
    ...vueI18n,
    useI18n: () => ({ t: (key: string) => key })
  }
})

const menuController = {
  show: vi.fn(),
  hide: vi.fn()
}

const QMenuStub = defineComponent({
  props: {
    target: { type: [Boolean, String, Object], default: undefined },
    noParentEvent: Boolean,
    touchPosition: Boolean,
    noFocus: Boolean
  },
  emits: ['hide'],
  setup(_, { expose, slots }) {
    expose(menuController)
    return () => h('div', { class: 'q-menu-stub' }, slots.default?.())
  }
})

const ClickableItemStub = defineComponent({
  emits: ['click'],
  setup(_, { emit, slots }) {
    return () => h('button', { class: 'quote-action', onClick: () => emit('click') }, slots.default?.())
  }
})

const SlotStub = defineComponent({
  setup(_, { slots }) {
    return () => h('div', slots.default?.())
  }
})

const sourceMessage: IMailMessage = {
  id: 7,
  direction: MailMessageDirection.Incoming,
  subject: 'Quoted mail',
  occurredAtUtc: '2026-09-12T00:00:00Z',
  isRead: true,
  hasThreadReplies: false,
  from: [],
  to: [],
  cc: [],
  attachments: []
}

describe('MailBodyQuoteMenu', () => {
  beforeAll(() => vi.stubGlobal('ref', ref))
  afterAll(() => vi.unstubAllGlobals())

  it('opens only through its explicit API and clears the selection after invoking the quote action', async () => {
    const wrapper = mount(MailBodyQuoteMenu, {
      global: {
        stubs: {
          QMenu: QMenuStub,
          QList: SlotStub,
          QItem: ClickableItemStub,
          QItemSection: SlotStub,
          QIcon: SlotStub
        }
      }
    })
    const quoteMenu = wrapper.vm as unknown as {
      showForSelection: (message: IMailMessage, context: { selectedText: string, clientX: number, clientY: number }) => void
      hide: () => void
    }

    quoteMenu.showForSelection(sourceMessage, { selectedText: ' Selected text ', clientX: 123, clientY: 456 })

    expect(menuController.show).toHaveBeenCalledTimes(1)
    expect(wrapper.findComponent(QMenuStub).props('noParentEvent')).toBe(true)
    await wrapper.get('.quote-action').trigger('click')
    await flushPromises()

    expect(menuController.hide).toHaveBeenCalled()
    expect(wrapper.emitted('insert-selection-quote')).toEqual([[sourceMessage, 'Selected text']])
  })
})
