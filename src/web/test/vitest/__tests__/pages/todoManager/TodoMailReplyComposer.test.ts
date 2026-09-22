import { flushPromises, mount } from '@vue/test-utils'
import { computed, defineComponent, h, nextTick, ref, watch } from 'vue'
import { afterAll, beforeAll, beforeEach, describe, expect, it, vi } from 'vitest'
import type * as TodoTaskModule from 'src/api/todoTask'
import type * as VueI18nModule from 'vue-i18n'
import { MailMessageDirection, type IMailMessage } from 'src/api/mailConversation'
import { sendTodoMailMessage } from 'src/api/todoTask'
import TodoMailReplyComposer from 'src/pages/todoManager/components/TodoMailReplyComposer.vue'

vi.mock('vue-i18n', async (importOriginal) => {
  const vueI18n = await importOriginal<typeof VueI18nModule>()
  return {
    ...vueI18n,
    useI18n: () => ({ t: (key: string) => key })
  }
})

vi.mock('src/api/todoTask', async (importOriginal) => {
  const todoTask = await importOriginal<typeof TodoTaskModule>()
  return {
    ...todoTask,
    sendTodoMailMessage: vi.fn()
  }
})

vi.mock('src/utils/dialog', () => ({
  notifyError: vi.fn(),
  notifySuccess: vi.fn()
}))

const sendTodoMailMessageMock = vi.mocked(sendTodoMailMessage)

const EditorStub = defineComponent({
  props: {
    subject: { type: String, required: true },
    htmlBody: { type: String, required: true }
  },
  emits: ['update:subject', 'update:htmlBody', 'submit'],
  setup(_, { emit, expose }) {
    expose({ focus: vi.fn() })
    return () => h('button', { class: 'editor-submit', onClick: () => emit('submit') })
  }
})

const SlotStub = defineComponent({
  setup(_, { slots }) {
    return () => h('div', slots.default?.())
  }
})

const sourceMessage: IMailMessage = {
  id: 88,
  direction: MailMessageDirection.Incoming,
  subject: 'Customer question',
  occurredAtUtc: '2026-09-12T00:00:00Z',
  isRead: true,
  hasThreadReplies: false,
  from: [],
  to: [],
  cc: [],
  attachments: []
}

describe('TodoMailReplyComposer', () => {
  beforeAll(() => {
    vi.stubGlobal('computed', computed)
    vi.stubGlobal('nextTick', nextTick)
    vi.stubGlobal('ref', ref)
    vi.stubGlobal('watch', watch)
  })

  beforeEach(() => vi.clearAllMocks())
  afterAll(() => vi.unstubAllGlobals())

  it('associates an unassociated draft with the selected mail and sends the mail reply target', async () => {
    sendTodoMailMessageMock.mockResolvedValue({} as never)
    const wrapper = mount(TodoMailReplyComposer, {
      props: { taskId: 3, branchSubject: 'Follow up' },
      global: {
        stubs: {
          MailReplyEditor: EditorStub,
          QBtnToggle: SlotStub
        }
      }
    })
    const editor = wrapper.findComponent(EditorStub)
    editor.vm.$emit('update:htmlBody', '<p>Draft already written</p>')
    await flushPromises()

    await (wrapper.vm as unknown as {
      insertManualQuote: (message: IMailMessage, selectedText: string) => Promise<void>
    }).insertManualQuote(sourceMessage, 'Selected customer text')
    await wrapper.get('.editor-submit').trigger('click')
    await flushPromises()

    expect(sendTodoMailMessageMock).toHaveBeenCalledWith(3, expect.objectContaining({
      replyToMessageId: sourceMessage.id,
      subject: 'Re: Customer question',
      htmlBody: expect.stringContaining('Draft already written')
    }))
    expect(sendTodoMailMessageMock.mock.calls[0]?.[1].htmlBody).toContain('Selected customer text')
  })
})
