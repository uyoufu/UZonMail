import { ref, shallowRef } from 'vue'
import { afterAll, beforeAll, beforeEach, describe, expect, it, vi } from 'vitest'
import type * as MailConversationModule from 'src/api/mailConversation'
import { MailMessageDirection, getMailMessages, type IMailMessage } from 'src/api/mailConversation'
import { ConversationMessagePageSize, useConversationMessageHistory } from 'src/pages/receivingManager/compositions/useConversationMessageHistory'

vi.mock('src/api/mailConversation', async (importOriginal) => {
  const mailConversation = await importOriginal<typeof MailConversationModule>()
  return {
    ...mailConversation,
    getMailMessages: vi.fn()
  }
})

const getMailMessagesMock = vi.mocked(getMailMessages)

function createMessage(id: number): IMailMessage {
  return {
    id,
    direction: MailMessageDirection.Incoming,
    subject: `Message ${id}`,
    occurredAtUtc: `2026-09-12T00:${String(id).padStart(2, '0')}:00Z`,
    isRead: true,
    hasThreadReplies: false,
    from: [],
    to: [],
    cc: [],
    attachments: []
  }
}

describe('useConversationMessageHistory', () => {
  beforeAll(() => {
    vi.stubGlobal('ref', ref)
    vi.stubGlobal('shallowRef', shallowRef)
  })

  beforeEach(() => vi.clearAllMocks())
  afterAll(() => vi.unstubAllGlobals())

  it('loads the latest ten messages before requesting older history with an exclusive cursor', async () => {
    const latestMessages = Array.from({ length: ConversationMessagePageSize }, (_, index) => createMessage(index + 11))
    const olderMessages = Array.from({ length: ConversationMessagePageSize }, (_, index) => createMessage(index + 1))
    getMailMessagesMock
      .mockResolvedValueOnce({ data: latestMessages } as never)
      .mockResolvedValueOnce({ data: olderMessages } as never)
    const history = useConversationMessageHistory()

    await history.loadLatestMessages(42)
    await history.loadOlderMessages(42)

    expect(getMailMessagesMock).toHaveBeenNthCalledWith(1, 42, { limit: 10 })
    expect(getMailMessagesMock).toHaveBeenNthCalledWith(2, 42, {
      beforeAtUtc: latestMessages[0]?.occurredAtUtc,
      beforeId: latestMessages[0]?.id,
      limit: 10
    })
    expect(history.messages.value.map(message => message.id)).toEqual(Array.from({ length: 20 }, (_, index) => index + 1))
    expect(history.hasOlderMessages.value).toBe(true)
  })

  it('stops requesting history after a short page and discards superseded conversation responses', async () => {
    let resolveFirstRequest: ((value: { data: IMailMessage[] }) => void) | undefined
    const activeConversationPage = Array.from({ length: ConversationMessagePageSize }, (_, index) => createMessage(index + 20))
    getMailMessagesMock
      .mockImplementationOnce(() => new Promise(resolve => { resolveFirstRequest = resolve }) as never)
      .mockResolvedValueOnce({ data: activeConversationPage } as never)
      .mockResolvedValueOnce({ data: [createMessage(19)] } as never)
    const history = useConversationMessageHistory()

    const firstLoad = history.loadLatestMessages(1)
    await history.loadLatestMessages(2)
    resolveFirstRequest?.({ data: [createMessage(1)] })
    await firstLoad
    await history.loadOlderMessages(2)

    expect(history.messages.value.map(message => message.id)).toEqual(Array.from({ length: 11 }, (_, index) => index + 19))
    expect(history.hasOlderMessages.value).toBe(false)
    expect(getMailMessagesMock).toHaveBeenCalledTimes(3)
  })
})
