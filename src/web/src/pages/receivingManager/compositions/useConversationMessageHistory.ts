import { getMailMessages, type IMailMessage } from 'src/api/mailConversation'

/** The fixed number of messages loaded for the newest page and every history page. */
export const ConversationMessagePageSize = 10

const emptyMessageHistory: readonly IMailMessage[] = Object.freeze([])

/** Manages cursor-based, immutable message history for the receiving conversation timeline. */
export function useConversationMessageHistory() {
  const messages = shallowRef<readonly IMailMessage[]>(emptyMessageHistory)
  const isLoadingInitialMessages = ref(false)
  const isLoadingOlderMessages = ref(false)
  const hasOlderMessages = ref(false)
  let activeConversationId: number | undefined
  let historyGeneration = 0

  /** Replaces the active history with the latest page for a newly selected conversation. */
  async function loadLatestMessages(conversationId: number): Promise<void> {
    const requestGeneration = ++historyGeneration
    activeConversationId = conversationId
    messages.value = emptyMessageHistory
    hasOlderMessages.value = false
    isLoadingOlderMessages.value = false
    isLoadingInitialMessages.value = true

    try {
      const page = (await getMailMessages(conversationId, { limit: ConversationMessagePageSize })).data
      if (!isActiveRequest(conversationId, requestGeneration)) return

      messages.value = freezeMessageHistory(page)
      hasOlderMessages.value = page.length === ConversationMessagePageSize
    } finally {
      if (isActiveRequest(conversationId, requestGeneration)) isLoadingInitialMessages.value = false
    }
  }

  /** Prepends the page immediately before the oldest message currently held in memory. */
  async function loadOlderMessages(conversationId: number): Promise<void> {
    if (!canLoadOlderMessages(conversationId)) return

    const oldestMessage = messages.value[0]
    if (!oldestMessage) {
      hasOlderMessages.value = false
      return
    }

    const requestGeneration = historyGeneration
    isLoadingOlderMessages.value = true
    try {
      const page = (await getMailMessages(conversationId, {
        beforeAtUtc: oldestMessage.occurredAtUtc,
        beforeId: oldestMessage.id,
        limit: ConversationMessagePageSize
      })).data
      if (!isActiveRequest(conversationId, requestGeneration)) return

      messages.value = freezeMessageHistory([...page, ...messages.value])
      hasOlderMessages.value = page.length === ConversationMessagePageSize
    } finally {
      if (isActiveRequest(conversationId, requestGeneration)) isLoadingOlderMessages.value = false
    }
  }

  /** Merges a message discovered through thread navigation into the current history. */
  function mergeMessage(message: IMailMessage): void {
    if (activeConversationId === undefined) return
    messages.value = freezeMessageHistory([...messages.value, message])
  }

  /** Refreshes the newest page after sending while retaining older pages already loaded by the user. */
  async function refreshLatestMessages(conversationId: number): Promise<void> {
    const requestGeneration = historyGeneration
    const latestMessages = (await getMailMessages(conversationId, { limit: ConversationMessagePageSize })).data
    if (!isActiveRequest(conversationId, requestGeneration)) return

    messages.value = freezeMessageHistory([...messages.value, ...latestMessages])
    if (messages.value.length === latestMessages.length) {
      hasOlderMessages.value = latestMessages.length === ConversationMessagePageSize
    }
  }

  /** Clears state and invalidates responses that belong to the previously selected conversation. */
  function clearMessageHistory(): void {
    historyGeneration++
    activeConversationId = undefined
    messages.value = emptyMessageHistory
    isLoadingInitialMessages.value = false
    isLoadingOlderMessages.value = false
    hasOlderMessages.value = false
  }

  /** Checks that a request still belongs to the active conversation history. */
  function isActiveRequest(conversationId: number, requestGeneration: number): boolean {
    return activeConversationId === conversationId && historyGeneration === requestGeneration
  }

  /** Checks whether loading another older page is currently meaningful. */
  function canLoadOlderMessages(conversationId: number): boolean {
    return activeConversationId === conversationId
      && hasOlderMessages.value
      && !isLoadingInitialMessages.value
      && !isLoadingOlderMessages.value
  }

  return {
    messages,
    isLoadingInitialMessages,
    isLoadingOlderMessages,
    hasOlderMessages,
    loadLatestMessages,
    loadOlderMessages,
    mergeMessage,
    refreshLatestMessages,
    clearMessageHistory
  }
}

/** Creates a sorted, de-duplicated and non-reactive message snapshot for virtual rendering. */
function freezeMessageHistory(messages: readonly IMailMessage[]): readonly IMailMessage[] {
  const messagesById = new Map(messages.map(message => [message.id, message]))
  return Object.freeze([...messagesById.values()].sort(compareMessages))
}

/** Sorts messages chronologically and resolves timestamp ties with their database IDs. */
function compareMessages(left: IMailMessage, right: IMailMessage): number {
  const occurredAtDifference = new Date(left.occurredAtUtc).getTime() - new Date(right.occurredAtUtc).getTime()
  return occurredAtDifference || left.id - right.id
}
