<template>
  <div class="reply-composer">
    <div v-if="replyComposerState === ReplyComposerState.collapsed" ref="collapsedReplyElement" class="row justify-end q-pa-sm">
      <CommonBtn icon="reply" flat color="primary" :tooltip="t('pages.receivingManagement.reply')" @click="onRestoreReply" />
    </div>

    <section v-if="replyComposerState === ReplyComposerState.expanded" ref="replyComposerElement" class="q-pa-sm">
      <MailReplyEditor ref="replyEditor" v-model:subject="draftSubject" v-model:html-body="draftBody" :is-sending="isSending" @submit="onSend">
        <template #actions>
        <q-fab v-if="quotedMessage" v-model="isQuoteActionsOpen" color="primary" icon="format_quote" direction="down"
          vertical-actions-align="right" padding="xs">
          <q-fab-action icon="remove_circle_outline" :label="t('pages.receivingManagement.removeQuote')" @click="onRemoveQuote" />
          <q-fab-action icon="visibility" :label="t('pages.receivingManagement.viewQuotedMessage')" @click="onViewQuotedMessage" />
          <AsyncTooltip :tooltip="getQuotePreviewTooltip" :cache="false" anchor="bottom middle" self="top middle" />
        </q-fab>
        <q-btn-toggle v-model="replyMode" dense unelevated toggle-color="primary" :options="replyOptions" />
        <CommonBtn icon="article" flat :tooltip="t('pages.receivingManagement.insertTemplate')" @click="onInsertTemplate" />
        <CommonBtn icon="keyboard_arrow_down" flat :tooltip="t('pages.receivingManagement.collapseReply')" @click="onCollapseReply" />
        </template>
      </MailReplyEditor>
    </section>
  </div>
</template>

<script setup lang="ts">
import { morph } from 'quasar'
import AsyncTooltip from 'src/components/asyncTooltip/AsyncTooltip.vue'
import CommonBtn from 'src/components/buttons/CommonBtn.vue'
import MailReplyEditor from 'src/components/mailMessage/MailReplyEditor.vue'
import { createManualQuoteHtml, createReplySubject, hasEditorContent } from 'src/components/mailMessage/mailReplyContent'
import { useMailMessageDraftCache } from 'src/components/mailMessage/useMailMessageDraftCache'
import TemplatePickerDialog from './TemplatePickerDialog.vue'
import { createFullMessageQuoteHtml, getMailContentPlainText } from './mailQuote'
import { getMailContent, MailReplyMode, sendMailConversationMessage, type IMailConversation, type IMailMessage } from 'src/api/mailConversation'
import { showComponentDialog, notifyError, notifySuccess } from 'src/utils/dialog'
import { useI18n } from 'vue-i18n'

const ReplyComposerState = {
  hidden: 'hidden',
  expanded: 'expanded',
  collapsed: 'collapsed'
} as const

type ReplyComposerState = typeof ReplyComposerState[keyof typeof ReplyComposerState]

interface IReplyDraft {
  state: ReplyComposerState
  subject: string
  body: string
  replyMode: MailReplyMode
  quotedMessage?: IMailMessage
}

const props = defineProps<{
  conversation: IMailConversation
  messages: readonly IMailMessage[]
}>()
const emit = defineEmits<{
  sent: []
  activated: []
  'view-quoted-message': [message: IMailMessage]
}>()
const { t } = useI18n()
const replyEditor = ref<InstanceType<typeof MailReplyEditor>>()
const replyComposerElement = ref<HTMLElement>()
const collapsedReplyElement = ref<HTMLElement>()
const replyComposerState = ref<ReplyComposerState>(ReplyComposerState.hidden)
const activeReplyMessageId = ref<number>()
const quotePreviewByMessageId = new Map<number, string[]>()
const draftSubject = ref('')
const draftBody = ref('')
const replyMode = ref(MailReplyMode.Reply)
const quotedMessage = ref<IMailMessage>()
const isSending = ref(false)
const isQuoteActionsOpen = ref(false)
const replyDraftCache = useMailMessageDraftCache(copyReplyDraft)
const replyOptions = computed(() => [
  { label: t('pages.receivingManagement.reply'), value: MailReplyMode.Reply },
  { label: t('pages.receivingManagement.replyAll'), value: MailReplyMode.ReplyAll }
])
async function startReply(message: IMailMessage): Promise<void> {
  saveCurrentReplyDraft()
  activeReplyMessageId.value = message.id
  replyDraftCache.saveLastMessageId(props.conversation.id, message.id)
  const cachedDraft = replyDraftCache.getDraft(message.id)
  if (cachedDraft) restoreReplyDraft(cachedDraft)
  else initializeReplyDraft(message)

  replyComposerState.value = ReplyComposerState.expanded
  emit('activated')
  await nextTick()
  replyEditor.value?.focus()
}

async function insertManualQuote(message: IMailMessage, selectedText: string): Promise<void> {
  if (!selectedText.trim()) return

  await startReply(message)
  replyEditor.value?.insertHtml(createManualQuoteHtml(selectedText))
  replyEditor.value?.focus()
}

function onCollapseReply() {
  morphReplyComposer(ReplyComposerState.collapsed)
}

function onRestoreReply() {
  emit('activated')
  morphReplyComposer(ReplyComposerState.expanded)
}

function morphReplyComposer(nextState: ReplyComposerState) {
  const currentState = replyComposerState.value
  if (currentState === nextState || currentState === ReplyComposerState.hidden) return

  morph({
    from: currentState === ReplyComposerState.expanded ? getReplyComposerElement : getCollapsedReplyElement,
    to: currentState === ReplyComposerState.expanded ? getCollapsedReplyElement : getReplyComposerElement,
    onToggle: () => {
      replyComposerState.value = nextState
      saveCurrentReplyDraft()
    },
    duration: 300
  })
}

function getReplyComposerElement() {
  return replyComposerElement.value
}

function getCollapsedReplyElement() {
  return collapsedReplyElement.value
}

function onRemoveQuote() {
  quotedMessage.value = undefined
  isQuoteActionsOpen.value = false
  saveCurrentReplyDraft()
}

function onViewQuotedMessage() {
  const message = quotedMessage.value
  if (!message) return

  isQuoteActionsOpen.value = false
  emit('view-quoted-message', message)
}

async function getQuotePreviewTooltip(): Promise<string[]> {
  const message = quotedMessage.value
  if (!message) return []

  const cachedPreview = quotePreviewByMessageId.get(message.id)
  if (cachedPreview) return cachedPreview

  try {
    const content = (await getMailContent(message.id)).data
    const previewText = getMailContentPlainText(content).trim()
    const truncatedPreview = previewText.length > 800 ? `${previewText.slice(0, 800)}...` : previewText
    const quotePreview = [message.subject || t('pages.receivingManagement.noSubject'), truncatedPreview].filter(Boolean)
    quotePreviewByMessageId.set(message.id, quotePreview)
    return quotePreview
  } catch {
    return [message.subject || t('pages.receivingManagement.noSubject')]
  }
}

async function onInsertTemplate() {
  const result = await showComponentDialog<string>(TemplatePickerDialog)
  if (!result.ok || !result.data) return
  replyEditor.value?.insertHtml(result.data)
}

async function onSend() {
  const replyToMessageId = activeReplyMessageId.value
  if (!replyToMessageId || !draftSubject.value.trim() || !hasEditorContent(draftBody.value)) {
    notifyError(t('pages.receivingManagement.completeReply'))
    return
  }

  isSending.value = true
  try {
    const htmlBody = await createOutgoingHtml()
    await sendMailConversationMessage(props.conversation.id, {
      replyToMessageId,
      replyMode: replyMode.value,
      subject: draftSubject.value.trim(),
      htmlBody,
      attachmentFileUsageIds: []
    })
    replyDraftCache.removeDraft(replyToMessageId)
    replyDraftCache.removeLastMessageId(props.conversation.id, replyToMessageId)
    clearReplyDraft()
    notifySuccess(t('pages.receivingManagement.sent'))
    emit('sent')
  } catch {
    notifyError(t('pages.receivingManagement.sendFailed'))
  } finally {
    isSending.value = false
  }
}

async function createOutgoingHtml(): Promise<string> {
  const quotedMessageValue = quotedMessage.value
  if (!quotedMessageValue) return draftBody.value
  const content = (await getMailContent(quotedMessageValue.id)).data
  return `${draftBody.value}${createFullMessageQuoteHtml(quotedMessageValue, content)}`
}

/** Persists the active mail draft while leaving unrelated mail drafts intact. */
function saveCurrentReplyDraft(conversationId = props.conversation.id): void {
  const messageId = activeReplyMessageId.value
  if (!messageId || replyComposerState.value === ReplyComposerState.hidden) return

  replyDraftCache.saveDraft(messageId, {
    state: replyComposerState.value,
    subject: draftSubject.value,
    body: draftBody.value,
    replyMode: replyMode.value,
    quotedMessage: quotedMessage.value
  })
  replyDraftCache.saveLastMessageId(conversationId, messageId)
}

/** Applies a draft retrieved from the mail-ID cache to the current editor view. */
function restoreReplyDraft(replyDraft: IReplyDraft): void {
  draftSubject.value = replyDraft.subject
  draftBody.value = replyDraft.body
  replyMode.value = replyDraft.replyMode
  quotedMessage.value = replyDraft.quotedMessage
  replyComposerState.value = replyDraft.state
}

/** Initializes an empty expanded reply targeting the supplied source message. */
function initializeReplyDraft(message: IMailMessage): void {
  draftSubject.value = createReplySubject(message.subject)
  draftBody.value = ''
  replyMode.value = MailReplyMode.Reply
  quotedMessage.value = message
}

function clearReplyDraft() {
  draftSubject.value = ''
  draftBody.value = ''
  replyMode.value = MailReplyMode.Reply
  activeReplyMessageId.value = undefined
  quotedMessage.value = undefined
  replyComposerState.value = ReplyComposerState.hidden
  isQuoteActionsOpen.value = false
}

watch(() => props.conversation.id, (conversationId, previousConversationId) => {
  if (previousConversationId !== undefined) saveCurrentReplyDraft(previousConversationId)
  restoreConversationReplyDraft(conversationId)
}, { immediate: true })

defineExpose({ startReply, insertManualQuote })

/** Restores the last mail-targeted draft used in a conversation, if it still exists. */
function restoreConversationReplyDraft(conversationId: number): void {
  const messageId = replyDraftCache.getLastMessageId(conversationId)
  if (!messageId) {
    clearReplyDraft()
    return
  }

  const replyDraft = replyDraftCache.getDraft(messageId)
  if (!replyDraft) {
    replyDraftCache.removeLastMessageId(conversationId)
    clearReplyDraft()
    return
  }

  activeReplyMessageId.value = messageId
  restoreReplyDraft(replyDraft)
}

/** Copies a mutable reply draft before it crosses the cache boundary. */
function copyReplyDraft(replyDraft: IReplyDraft): IReplyDraft {
  return { ...replyDraft }
}
</script>

<style lang="scss" scoped>
.reply-composer {
  border-top: 1px solid $grey-4;
}

.reply-composer__editor :deep(.q-editor__content) {
  padding-right: 52px;
  padding-bottom: 52px;
}

.reply-composer__send {
  position: absolute;
  right: 12px;
  bottom: 12px;
  z-index: 1;
}
</style>
