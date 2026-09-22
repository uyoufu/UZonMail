import { flushPromises, mount } from '@vue/test-utils'
import { computed, defineComponent, h, nextTick, ref, watch } from 'vue'
import { afterAll, beforeAll, describe, expect, it, vi } from 'vitest'
import type * as MailConversationModule from 'src/api/mailConversation'
import type * as QuasarModule from 'quasar'
import type * as VueI18nModule from 'vue-i18n'
import { MailMessageDirection, getMailContent, sendMailConversationMessage, type IMailConversation, type IMailMessage } from 'src/api/mailConversation'
import ReplyComposer from 'src/pages/receivingManager/components/ReplyComposer.vue'

vi.mock('quasar', async (importOriginal) => {
  const quasar = await importOriginal<typeof QuasarModule>()
  return {
    ...quasar,
    morph: ({ onToggle }: { onToggle: () => void }) => onToggle()
  }
})

vi.mock('vue-i18n', async (importOriginal) => {
  const vueI18n = await importOriginal<typeof VueI18nModule>()
  return {
    ...vueI18n,
    useI18n: () => ({ t: (key: string) => key })
  }
})

vi.mock('src/api/mailConversation', async (importOriginal) => {
  const mailConversation = await importOriginal<typeof MailConversationModule>()
  return {
    ...mailConversation,
    getMailContent: vi.fn(),
    sendMailConversationMessage: vi.fn()
  }
})

vi.mock('src/utils/dialog', () => ({
  notifyError: vi.fn(),
  notifySuccess: vi.fn(),
  showComponentDialog: vi.fn()
}))

const SlotStub = defineComponent({
  setup(_, { slots }) {
    return () => h('div', slots.default?.())
  }
})

const EditorStub = defineComponent({
  emits: ['update:modelValue', 'submit'],
  setup(_, { emit, expose }) {
    expose({ focus: vi.fn(), runCmd: vi.fn() })
    return () => h('button', { class: 'q-editor-stub', onClick: () => emit('submit') })
  }
})

const CommonButtonStub = defineComponent({
  props: {
    icon: {
      type: String,
      required: true
    }
  },
  emits: ['click'],
  setup(props, { emit }) {
    return () => h('button', {
      'data-icon': props.icon,
      onClick: () => emit('click')
    })
  }
})

const firstConversation: IMailConversation = {
  id: 1,
  emailAccountId: 1,
  emailAccount: 'sender@example.com',
  conversationType: 0,
  displayTitle: 'First conversation',
  lastMessageAtUtc: '2026-09-11T00:00:00Z',
  unreadCount: 0,
  participants: []
}

const secondConversation: IMailConversation = {
  ...firstConversation,
  id: 2,
  displayTitle: 'Second conversation'
}

const firstMessage: IMailMessage = {
  id: 101,
  direction: MailMessageDirection.Incoming,
  subject: 'First subject',
  occurredAtUtc: '2026-09-11T00:00:00Z',
  isRead: true,
  hasThreadReplies: false,
  from: [],
  to: [],
  cc: [],
  attachments: []
}

const secondMessage: IMailMessage = {
  ...firstMessage,
  id: 202,
  subject: 'Second subject'
}

function mountReplyComposer() {
  return mount(ReplyComposer, {
    props: {
      conversation: firstConversation,
      messages: [firstMessage]
    },
    global: {
      stubs: {
        AsyncTooltip: true,
        CommonBtn: CommonButtonStub,
        QBtnToggle: SlotStub,
        QEditor: EditorStub,
        QFab: SlotStub,
        QFabAction: SlotStub,
        QInput: SlotStub
      }
    }
  })
}

describe('ReplyComposer', () => {
  beforeAll(() => {
    vi.stubGlobal('computed', computed)
    vi.stubGlobal('nextTick', nextTick)
    vi.stubGlobal('ref', ref)
    vi.stubGlobal('watch', watch)
  })

  afterAll(() => vi.unstubAllGlobals())

  it('stays hidden until the user starts a reply', async () => {
    const wrapper = mountReplyComposer()

    expect(wrapper.find('.q-editor-stub').exists()).toBe(false)

    await (wrapper.vm as unknown as { startReply: (message: IMailMessage) => Promise<void> }).startReply(firstMessage)
    await flushPromises()

    expect(wrapper.find('.q-editor-stub').exists()).toBe(true)
  })

  it('restores the expanded reply draft after returning to its conversation', async () => {
    const wrapper = mountReplyComposer()
    const replyComposer = wrapper.vm as unknown as { startReply: (message: IMailMessage) => Promise<void> }

    await replyComposer.startReply(firstMessage)
    await wrapper.setProps({ conversation: secondConversation, messages: [secondMessage] })
    await flushPromises()
    expect(wrapper.find('.q-editor-stub').exists()).toBe(false)

    await wrapper.setProps({ conversation: firstConversation, messages: [firstMessage] })
    await flushPromises()

    expect(wrapper.find('.q-editor-stub').exists()).toBe(true)
  })

  it('collapses and restores the reply composer without discarding it', async () => {
    const wrapper = mountReplyComposer()
    const replyComposer = wrapper.vm as unknown as { startReply: (message: IMailMessage) => Promise<void> }

    await replyComposer.startReply(firstMessage)
    await wrapper.get('[data-icon="keyboard_arrow_down"]').trigger('click')
    await flushPromises()
    expect(wrapper.find('.q-editor-stub').exists()).toBe(false)

    await wrapper.get('[data-icon="reply"]').trigger('click')
    await flushPromises()
    expect(wrapper.find('.q-editor-stub').exists()).toBe(true)
  })

  it('restores drafts by reply target and sends using that message ID', async () => {
    vi.mocked(getMailContent).mockResolvedValue({ data: { conversationMessageId: firstMessage.id, textBody: 'Original content', attachments: [] } } as never)
    vi.mocked(sendMailConversationMessage).mockResolvedValue({} as never)
    const wrapper = mountReplyComposer()
    await wrapper.setProps({ messages: [firstMessage, secondMessage] })
    const replyComposer = wrapper.vm as unknown as { startReply: (message: IMailMessage) => Promise<void> }

    await replyComposer.startReply(firstMessage)
    wrapper.findComponent(EditorStub).vm.$emit('update:modelValue', '<p>First target draft</p>')
    await flushPromises()
    await replyComposer.startReply(secondMessage)
    wrapper.findComponent(EditorStub).vm.$emit('update:modelValue', '<p>Second target draft</p>')
    await flushPromises()
    await replyComposer.startReply(firstMessage)
    await wrapper.get('[data-icon="send"]').trigger('click')
    await flushPromises()

    expect(sendMailConversationMessage).toHaveBeenCalledWith(firstConversation.id, expect.objectContaining({
      replyToMessageId: firstMessage.id,
      htmlBody: expect.stringContaining('First target draft')
    }))
  })
})
