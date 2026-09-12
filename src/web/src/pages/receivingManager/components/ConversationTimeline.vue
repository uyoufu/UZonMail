<template>
  <main v-if="conversation" ref="timelineElement" class="column full-height min-width-0 relative-position">
    <header class="row no-wrap items-center q-pa-sm q-gutter-sm">
      <CommonBtn v-if="isCompact" icon="arrow_back" flat :tooltip="t('common.back')" @click="emit('back')" />
      <q-avatar color="grey-3" text-color="primary" icon="person" size="32px" />
      <div class="col min-width-0">
        <div class="text-body2 text-weight-medium ellipsis">{{ conversation.displayTitle || participantTitle }}</div>
        <div class="text-caption text-grey-7 ellipsis">{{ headerAddressRoute }}</div>
      </div>
      <CommonBtn icon="label" flat :tooltip="t('pages.receivingManagement.tags')" @click="onManageTags" />
      <CommonBtn icon="playlist_add" flat :disable="selectedMessageIds.length === 0" :tooltip="t('pages.receivingManagement.createTodo')"
        @click="onCreateTodo" />
    </header>

    <q-scroll-area class="col height-0 bg-grey-2 relative-position">
      <div class="q-pa-sm q-gutter-y-sm">
        <article v-for="message in messages" :key="message.id" :data-message-id="message.id" class="row no-wrap items-start q-gutter-sm"
          :class="{ 'justify-end': message.direction === MailMessageDirection.Outgoing }">
          <q-avatar v-if="message.direction === MailMessageDirection.Incoming" color="grey-3" text-color="primary" size="36px">
            {{ getMessageAvatarLabel(message) }}
          </q-avatar>
          <div class="col-10 q-pa-sm border-radius-4 border" :class="message.direction === MailMessageDirection.Outgoing ? 'bg-blue-1' : 'bg-white'">
            <div class="row no-wrap items-center q-gutter-sm">
              <q-checkbox v-model="selectedMessageIds" dense :val="message.id" />
              <span class="col text-body2 text-weight-medium ellipsis">{{ message.subject || t('pages.receivingManagement.noSubject') }}</span>
              <span class="text-caption text-grey-7 no-wrap">{{ formatDate(message.occurredAtUtc) }}</span>
              <CommonBtn :icon="expandedMessageIds.has(message.id) ? 'expand_less' : 'expand_more'" flat
                :tooltip="t('pages.receivingManagement.expand')" @click="onToggleMessage(message.id)" />
              <CommonBtn icon="reply" flat :tooltip="t('pages.receivingManagement.reply')" @click="replyComposer?.startReply(message)" />
            </div>
            <div v-if="!expandedMessageIds.has(message.id)" class="text-body2 ellipsis-2-lines q-mt-sm">
              {{ getMessagePreview(message, t('pages.receivingManagement.noPreview')) }}
            </div>
            <MailBody v-else :message-id="message.id" class="q-mt-sm"
              @quote-selection-contextmenu="context => onMailBodyQuoteSelection(message, context)" />
            <div v-if="message.attachments.length" class="row q-gutter-xs q-mt-sm">
              <q-chip v-for="attachment in message.attachments" :key="attachment.id" dense square icon="attach_file">
                {{ attachment.fileName }}
              </q-chip>
            </div>
          </div>
          <q-avatar v-if="message.direction === MailMessageDirection.Outgoing" color="primary" text-color="white" size="36px">
            {{ getMessageAvatarLabel(message) }}
          </q-avatar>
        </article>
        <div v-if="messages.length === 0 && !isLoadingMessages" class="column items-center justify-center q-pa-xl text-grey-7">
          <q-icon name="mail_outline" size="40px" class="q-mb-sm" />
          <span>{{ t('pages.receivingManagement.empty') }}</span>
        </div>
      </div>
      <q-inner-loading :showing="isLoadingMessages" color="primary" />
    </q-scroll-area>

    <ReplyComposer ref="replyComposer" :conversation="conversation" :messages="messages" @sent="emit('message-sent')"
      @view-quoted-message="onViewQuotedMessage" />
    <q-menu ref="quoteMenu" touch-position no-focus>
      <q-list dense class="q-pa-xs">
        <q-item clickable @click="onInsertSelectionQuote">
          <q-item-section avatar><q-icon name="format_quote" /></q-item-section>
          <q-item-section>{{ t('pages.receivingManagement.quoteSelection') }}</q-item-section>
        </q-item>
      </q-list>
    </q-menu>
  </main>
  <div v-else class="full-height column items-center justify-center q-gutter-sm text-grey-7">
    <q-icon name="forum" size="48px" />
    <span>{{ t('pages.receivingManagement.selectConversation') }}</span>
  </div>
</template>

<script setup lang="ts">
import CommonBtn from 'src/components/buttons/CommonBtn.vue'
import CreateTodoDialog from './CreateTodoDialog.vue'
import MailBody from './MailBody.vue'
import ReplyComposer from './ReplyComposer.vue'
import TagManagerDialog from './TagManagerDialog.vue'
import { formatMailEmailRoute, getMessageAvatarLabel, getMessagePreview } from './mailMessagePresentation'
import { MailMessageDirection, type IMailConversation, type IMailMessage } from 'src/api/mailConversation'
import { showComponentDialog, notifySuccess } from 'src/utils/dialog'
import { formatDate } from 'src/utils/format'
import { useI18n } from 'vue-i18n'
import { useMailBodyQuoteMenu, type IMailBodyQuoteSelectionContext } from './useMailBodyQuoteMenu'

const props = defineProps<{
  conversation?: IMailConversation
  messages: IMailMessage[]
  isLoadingMessages: boolean
  isCompact: boolean
}>()
const emit = defineEmits<{
  back: []
  'message-sent': []
  'tags-saved': []
}>()
const { t } = useI18n()
const timelineElement = ref<HTMLElement>()
const selectedMessageIds = ref<number[]>([])
const expandedMessageIds = ref(new Set<number>())
const replyComposer = ref<InstanceType<typeof ReplyComposer>>()
const { quoteMenu, onInsertSelectionQuote, onQuoteSelection } = useMailBodyQuoteMenu(replyComposer)

const participantTitle = computed(() => props.conversation?.participants.map(contact => contact.displayName || contact.email).join(', ') || '')
const headerAddressRoute = computed(() => {
  const latestMessage = props.messages.at(-1)
  return latestMessage ? formatMailEmailRoute(latestMessage) : props.conversation?.emailAccount || ''
})

function onToggleMessage(messageId: number) {
  const expandedMessageIdsValue = new Set(expandedMessageIds.value)
  if (expandedMessageIdsValue.has(messageId)) expandedMessageIdsValue.delete(messageId)
  else expandedMessageIdsValue.add(messageId)
  expandedMessageIds.value = expandedMessageIdsValue
}

function onMailBodyQuoteSelection(message: IMailMessage, context: IMailBodyQuoteSelectionContext) {
  onQuoteSelection(message, context)
}

async function onViewQuotedMessage(messageId: number) {
  if (!props.messages.some(message => message.id === messageId)) return

  if (!expandedMessageIds.value.has(messageId)) onToggleMessage(messageId)
  await nextTick()
  timelineElement.value?.querySelector<HTMLElement>(`[data-message-id="${messageId}"]`)
    ?.scrollIntoView({ behavior: 'smooth', block: 'center' })
}

async function onManageTags() {
  if (!props.conversation) return
  const result = await showComponentDialog(TagManagerDialog, { conversation: props.conversation })
  if (result.ok) emit('tags-saved')
}

async function onCreateTodo() {
  if (!props.conversation || selectedMessageIds.value.length === 0) return
  const result = await showComponentDialog(CreateTodoDialog, {
    conversation: props.conversation,
    messageIds: selectedMessageIds.value
  })
  if (result.ok) notifySuccess(t('pages.receivingManagement.todoCreated'))
}

watch(() => props.conversation?.id, () => {
  selectedMessageIds.value = []
  expandedMessageIds.value = new Set()
})
</script>
