<template>
  <main v-if="conversation" class="column full-height min-width-0 relative-position">
    <header class="row no-wrap items-center q-pa-sm q-gutter-sm">
      <CommonBtn v-if="isCompact" icon="arrow_back" flat :tooltip="t('common.back')" @click="emit('back')" />
      <q-avatar color="grey-3" text-color="primary" icon="person" size="32px" />
      <div class="col min-width-0">
        <div class="text-body2 text-weight-medium ellipsis">{{ conversation.displayTitle || participantTitle }}</div>
        <div class="text-caption text-grey-7 ellipsis">{{ headerAddressRoute }}</div>
      </div>
      <CommonBtn icon="label" flat :tooltip="t('pages.receivingManagement.tags')" @click="onManageTags" />
    </header>

    <section class="col height-0 bg-grey-2 relative-position">
      <q-virtual-scroll v-if="messages.length" ref="virtualScroll" :items="messages" class="full-height hover-scroll"
        :virtual-scroll-item-size="128" @virtual-scroll="onVirtualScroll">
        <template #before>
          <div v-if="isLoadingOlderMessages" class="q-virtual-scroll--skip row justify-center q-pa-sm">
            <q-spinner color="primary" size="24px" />
          </div>
        </template>
        <template #default="{ item: message }">
        <article :key="message.id" :data-message-id="message.id"
          class="row no-wrap items-start q-gutter-sm q-mx-sm q-my-sm"
          :class="{ 'justify-end': message.direction === MailMessageDirection.Outgoing }">
          <q-avatar v-if="message.direction === MailMessageDirection.Incoming" color="grey-3" text-color="primary"
            size="36px">
            {{ getMessageAvatarLabel(message) }}
          </q-avatar>
          <div class="col-10 q-pa-sm border-radius-4 border"
            :class="message.direction === MailMessageDirection.Outgoing ? 'bg-blue-1' : 'bg-white'">
            <div class="row no-wrap items-center">
              <span class="col text-body2 text-weight-medium ellipsis">{{ message.subject ||
                t('pages.receivingManagement.noSubject') }}</span>
              <span class="text-caption text-grey-7 no-wrap">{{ formatDate(message.occurredAtUtc) }}</span>
              <CommonBtn v-if="message.replyToMessageId" icon="arrow_upward" flat
                :tooltip="t('pages.receivingManagement.previousMessage')" @click="onPreviousMessageClick(message)" />
              <CommonBtn v-else icon="first_page" flat disable :tooltip="t('pages.receivingManagement.threadStart')" />
              <CommonBtn v-if="message.hasThreadReplies" icon="arrow_downward" flat
                :tooltip="t('pages.receivingManagement.nextMessage')" @click="onNextMessageClick(message)">
                <q-menu :ref="menu => onNextMessageMenuRef(message.id, menu)" no-parent-event>
                  <q-list dense class="q-pa-xs">
                    <q-item v-for="nextMessage in nextMessageOptions" :key="nextMessage.id" v-close-popup clickable
                      @click="onNavigateToMessage(nextMessage)">
                      <q-item-section><q-item-label lines="1">{{ nextMessage.subject ||
                        t('pages.receivingManagement.noSubject') }}</q-item-label>
                        <q-item-label caption>{{ formatDate(nextMessage.occurredAtUtc)
                          }}</q-item-label></q-item-section>
                    </q-item>
                  </q-list>
                </q-menu>
              </CommonBtn>
              <CommonBtn v-else icon="last_page" flat disable :tooltip="t('pages.receivingManagement.threadEnd')" />
              <CommonBtn icon="info" flat :tooltip="t('pages.receivingManagement.mailInformation')"
                @click="onShowMailMetadata(message)" />
              <CommonBtn icon="playlist_add" flat :tooltip="t('pages.receivingManagement.createTodo')"
                @click="onCreateTodo(message)" />
              <CommonBtn :icon="expandedMessageIds.has(message.id) ? 'expand_less' : 'expand_more'" flat
                :tooltip="t('pages.receivingManagement.expand')" @click="onToggleMessage(message.id)" />
              <CommonBtn icon="reply" flat :tooltip="t('pages.receivingManagement.reply')"
                @click="onStartReply(message)" />
            </div>
            <div v-if="!expandedMessageIds.has(message.id)" class="text-body2 ellipsis-2-lines q-mt-sm">
              {{ getMessagePreview(message, t('pages.receivingManagement.noPreview')) }}
            </div>
            <MailBody v-else :message-id="message.id" class="q-mt-sm"
              @body-pointerdown="quoteMenu?.hide()"
              @quote-selection-contextmenu="context => onMailBodyQuoteSelection(message, context)" />
            <div v-if="message.attachments.length" class="row q-gutter-xs q-mt-sm">
              <q-chip v-for="attachment in message.attachments" :key="attachment.id" dense square icon="attach_file">
                {{ attachment.fileName }}
              </q-chip>
            </div>
          </div>
          <q-avatar v-if="message.direction === MailMessageDirection.Outgoing" color="primary" text-color="white"
            size="36px">
            {{ getMessageAvatarLabel(message) }}
          </q-avatar>
        </article>
        </template>
      </q-virtual-scroll>
      <div v-else-if="!isLoadingMessages" class="full-height column items-center justify-center q-pa-xl text-grey-7">
        <q-icon name="mail_outline" size="40px" class="q-mb-sm" />
        <span>{{ t('pages.receivingManagement.empty') }}</span>
      </div>
      <q-inner-loading :showing="isLoadingMessages" color="primary" />
    </section>

    <ReplyComposer ref="replyComposer" :conversation="conversation" :messages="messages" @activated="quoteMenu?.hide()"
      @sent="emit('message-sent')" @view-quoted-message="onViewQuotedMessage" />
    <MailBodyQuoteMenu ref="quoteMenu" @insert-selection-quote="onInsertSelectionQuote" />
  </main>
  <div v-else class="full-height column items-center justify-center q-gutter-sm text-grey-7">
    <q-icon name="forum" size="48px" />
    <span>{{ t('pages.receivingManagement.selectConversation') }}</span>
  </div>
</template>

<script setup lang="ts">
import type { ComponentPublicInstance } from 'vue'
import type { QVirtualScroll } from 'quasar'
import CommonBtn from 'src/components/buttons/CommonBtn.vue'
import MailBody from 'src/components/mailMessage/MailBody.vue'
import MailBodyQuoteMenu from 'src/components/mailMessage/MailBodyQuoteMenu.vue'
import MailMessageMetadataDialog from 'src/components/mailMessage/MailMessageMetadataDialog.vue'
import CreateTodoDialog from './CreateTodoDialog.vue'
import ReplyComposer from './ReplyComposer.vue'
import TagManagerDialog from './TagManagerDialog.vue'
import { formatMailEmailRoute, getMessageAvatarLabel, getMessagePreview } from './mailMessagePresentation'
import { getMailThreadNeighbors, MailMessageDirection, type IMailConversation, type IMailMessage, type IMailThreadNeighbors } from 'src/api/mailConversation'
import { showComponentDialog, notifyError, notifySuccess } from 'src/utils/dialog'
import { formatDate } from 'src/utils/format'
import { useI18n } from 'vue-i18n'
import type { IMailBodySelectionContext } from 'src/components/mailMessage/mailBodyInteraction'

const props = defineProps<{
  conversation?: IMailConversation
  messages: readonly IMailMessage[]
  isLoadingMessages: boolean
  isLoadingOlderMessages: boolean
  hasOlderMessages: boolean
  isCompact: boolean
}>()
const emit = defineEmits<{
  back: []
  'message-sent': []
  'request-older-messages': []
  'message-loaded': [message: IMailMessage]
  'tags-saved': []
}>()
const { t } = useI18n()
const expandedMessageIds = ref(new Set<number>())
const replyComposer = ref<InstanceType<typeof ReplyComposer>>()
const quoteMenu = ref<InstanceType<typeof MailBodyQuoteMenu>>()
const virtualScroll = ref<QVirtualScroll>()
const nextMessageOptions = ref<IMailMessage[]>([])
const threadNeighborsByMessageId = ref(new Map<number, IMailThreadNeighbors>())
const nextMessageMenus = new Map<number, { show: () => void }>()
const olderHistoryAnchorMessageId = ref<number>()
const pendingVisibleMessageId = ref<number>()
const shouldScrollToNewestMessage = ref(false)

const participantTitle = computed(() => props.conversation?.participants.map(contact => contact.displayName || contact.email).join(', ') || '')
const headerAddressRoute = computed(() => {
  const latestMessage = props.messages.at(-1)
  return latestMessage ? formatMailEmailRoute(latestMessage) : props.conversation?.emailAccount || ''
})

async function onToggleMessage(messageId: number): Promise<void> {
  quoteMenu.value?.hide()
  const expandedMessageIdsValue = new Set(expandedMessageIds.value)
  if (expandedMessageIdsValue.has(messageId)) expandedMessageIdsValue.delete(messageId)
  else expandedMessageIdsValue.add(messageId)
  expandedMessageIds.value = expandedMessageIdsValue
  await nextTick()
  virtualScroll.value?.refresh(-1)
}

function onMailBodyQuoteSelection(message: IMailMessage, context: IMailBodySelectionContext): void {
  quoteMenu.value?.showForSelection(message, context)
}

function onInsertSelectionQuote(message: IMailMessage, selectedText: string): void {
  quoteMenu.value?.hide()
  void replyComposer.value?.insertManualQuote(message, selectedText)
}

function onStartReply(message: IMailMessage): void {
  quoteMenu.value?.hide()
  void replyComposer.value?.startReply(message)
}

/** Requests an older page only after the user scrolls toward the history boundary. */
function onVirtualScroll(details: { index: number, direction: 'increase' | 'decrease' }): void {
  quoteMenu.value?.hide()
  if (details.direction !== 'decrease' || details.index > 2 || !props.hasOlderMessages || props.isLoadingOlderMessages) return

  olderHistoryAnchorMessageId.value = props.messages[details.index]?.id
  emit('request-older-messages')
}

async function onViewQuotedMessage(message: IMailMessage): Promise<void> {
  await onNavigateToMessage(message)
}

async function onManageTags() {
  if (!props.conversation) return
  const result = await showComponentDialog(TagManagerDialog, { conversation: props.conversation })
  if (result.ok) emit('tags-saved')
}

async function onCreateTodo(message: IMailMessage) {
  if (!props.conversation) return
  const result = await showComponentDialog(CreateTodoDialog, {
    conversation: props.conversation,
    message
  })
  if (result.ok) notifySuccess(t('pages.receivingManagement.todoCreated'))
}

function onNextMessageMenuRef(messageId: number, menu: Element | ComponentPublicInstance | null): void {
  const menuController = menu as unknown as { show?: () => void } | null
  if (typeof menuController?.show === 'function') {
    nextMessageMenus.set(messageId, menuController as { show: () => void })
    return
  }

  nextMessageMenus.delete(messageId)
}

async function onPreviousMessageClick(message: IMailMessage): Promise<void> {
  const neighbors = await getThreadNeighbors(message)
  if (neighbors?.previousMessage) await onNavigateToMessage(neighbors.previousMessage)
}

async function onNextMessageClick(message: IMailMessage): Promise<void> {
  const neighbors = await getThreadNeighbors(message)
  if (!neighbors || neighbors.nextMessages.length === 0) return
  if (neighbors.nextMessages.length === 1) {
    const [nextMessage] = neighbors.nextMessages
    if (nextMessage) await onNavigateToMessage(nextMessage)
    return
  }

  nextMessageOptions.value = neighbors.nextMessages
  await nextTick()
  nextMessageMenus.get(message.id)?.show()
}

async function getThreadNeighbors(message: IMailMessage): Promise<IMailThreadNeighbors | undefined> {
  const cachedNeighbors = threadNeighborsByMessageId.value.get(message.id)
  if (cachedNeighbors) return cachedNeighbors

  try {
    const neighbors = (await getMailThreadNeighbors(message.id)).data
    const nextNeighbors = new Map(threadNeighborsByMessageId.value)
    nextNeighbors.set(message.id, neighbors)
    threadNeighborsByMessageId.value = nextNeighbors
    return neighbors
  } catch {
    notifyError(t('pages.receivingManagement.threadNavigationFailed'))
  }
}

async function onNavigateToMessage(message: IMailMessage): Promise<void> {
  pendingVisibleMessageId.value = message.id
  if (!props.messages.some(value => value.id === message.id)) emit('message-loaded', message)
  await revealMessage(message.id)
}

async function onShowMailMetadata(message: IMailMessage): Promise<void> {
  await showComponentDialog(MailMessageMetadataDialog, { messageId: message.id })
}

watch(() => props.conversation?.id, () => {
  quoteMenu.value?.hide()
  expandedMessageIds.value = new Set()
  threadNeighborsByMessageId.value = new Map()
  olderHistoryAnchorMessageId.value = undefined
  pendingVisibleMessageId.value = undefined
  shouldScrollToNewestMessage.value = true
}, { immediate: true })

watch(() => props.messages, () => {
  const messageId = pendingVisibleMessageId.value
  if (messageId) void revealMessage(messageId)
  if (shouldScrollToNewestMessage.value && props.messages.length > 0) {
    void scrollToNewestMessage()
  }
})

watch(() => props.isLoadingOlderMessages, isLoading => {
  if (!isLoading && olderHistoryAnchorMessageId.value) {
    void restoreOlderHistoryAnchor()
  }
})

/** Restores the first visible mail after prepending an older cursor page. */
async function restoreOlderHistoryAnchor(): Promise<void> {
  const messageId = olderHistoryAnchorMessageId.value
  if (!messageId) return

  const messageIndex = props.messages.findIndex(message => message.id === messageId)
  if (messageIndex < 0) return

  await nextTick()
  virtualScroll.value?.refresh(messageIndex)
  olderHistoryAnchorMessageId.value = undefined
}

/** Expands and centers a message even when it was previously outside the virtual slice. */
async function revealMessage(messageId: number): Promise<void> {
  const messageIndex = props.messages.findIndex(message => message.id === messageId)
  if (messageIndex < 0) return

  if (!expandedMessageIds.value.has(messageId)) await onToggleMessage(messageId)
  await nextTick()
  virtualScroll.value?.scrollTo(messageIndex, 'center-force')
  pendingVisibleMessageId.value = undefined
}

/** Positions an initially loaded conversation at its most recent message. */
async function scrollToNewestMessage(): Promise<void> {
  await nextTick()
  virtualScroll.value?.scrollTo(props.messages.length - 1, 'end-force')
  shouldScrollToNewestMessage.value = false
}
</script>
