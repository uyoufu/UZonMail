<template>
  <section class="relative-position q-pa-sm q-pb-xl">
    <div class="row items-center q-gutter-sm q-mb-sm">
      <q-input v-model="draftSubject" dense outlined hide-bottom-space class="col" :label="t('pages.receivingManagement.subject')" />
      <q-btn-toggle v-model="replyMode" dense unelevated toggle-color="primary" :options="replyOptions" />
      <CommonBtn icon="article" flat :tooltip="t('pages.receivingManagement.insertTemplate')" @click="onInsertTemplate" />
    </div>
    <div v-if="quotedMessage" class="row no-wrap items-center q-pa-xs q-mb-sm bg-grey-2 border-radius-4">
      <q-icon name="format_quote" color="primary" class="q-mr-xs" />
      <span class="col text-caption ellipsis">{{ quotedMessage.subject || t('pages.receivingManagement.noSubject') }}</span>
      <CommonBtn icon="close" flat :tooltip="t('pages.receivingManagement.removeQuote')" @click="quotedMessageId = undefined" />
    </div>
    <q-editor ref="editorRef" v-model="draftBody" min-height="110px" :placeholder="t('pages.receivingManagement.writeReply')"
      :toolbar="editorToolbar" />
    <CommonBtn icon="send" class="absolute-bottom-right q-ma-sm" :loading="isSending"
      :tooltip="t('pages.receivingManagement.send')" @click="onSend" />
  </section>
</template>

<script setup lang="ts">
import type { QEditor } from 'quasar'
import CommonBtn from 'src/components/buttons/CommonBtn.vue'
import TemplatePickerDialog from './TemplatePickerDialog.vue'
import { createFullMessageQuoteHtml, createManualQuoteHtml } from './mailQuote'
import { getMailContent, MailReplyMode, sendMailConversationMessage, type IMailConversation, type IMailMessage } from 'src/api/mailConversation'
import { showComponentDialog, notifyError, notifySuccess } from 'src/utils/dialog'
import { useI18n } from 'vue-i18n'

const props = defineProps<{
  conversation: IMailConversation
  messages: IMailMessage[]
}>()
const emit = defineEmits<{ sent: [] }>()
const { t } = useI18n()
const editorRef = ref<QEditor>()
const draftSubject = ref('')
const draftBody = ref('')
const replyMode = ref(MailReplyMode.Reply)
const replyToMessageId = ref<number>()
const quotedMessageId = ref<number>()
const isSending = ref(false)
const editorToolbar = [['bold', 'italic', 'underline'], ['unordered', 'ordered'], ['link'], ['undo', 'redo']]
const replyOptions = computed(() => [
  { label: t('pages.receivingManagement.reply'), value: MailReplyMode.Reply },
  { label: t('pages.receivingManagement.replyAll'), value: MailReplyMode.ReplyAll }
])
const quotedMessage = computed(() => props.messages.find(message => message.id === quotedMessageId.value))

function onConversationChanged() {
  const latestMessage = props.messages.at(-1)
  draftSubject.value = createReplySubject(latestMessage?.subject)
  draftBody.value = ''
  replyMode.value = MailReplyMode.Reply
  replyToMessageId.value = latestMessage?.id
  quotedMessageId.value = latestMessage?.id
}

function startReply(message: IMailMessage) {
  draftSubject.value = createReplySubject(message.subject)
  replyToMessageId.value = message.id
  quotedMessageId.value = message.id
  editorRef.value?.focus()
}

function insertManualQuote(selectedText: string) {
  editorRef.value?.runCmd('insertHTML', createManualQuoteHtml(selectedText))
  editorRef.value?.focus()
}

async function onInsertTemplate() {
  const result = await showComponentDialog<string>(TemplatePickerDialog)
  if (!result.ok || !result.data) return
  editorRef.value?.runCmd('insertHTML', result.data)
}

async function onSend() {
  if (!draftSubject.value.trim() || !hasEditorContent(draftBody.value)) {
    notifyError(t('pages.receivingManagement.completeReply'))
    return
  }

  isSending.value = true
  try {
    const htmlBody = await createOutgoingHtml()
    await sendMailConversationMessage(props.conversation.id, {
      replyToMessageId: replyToMessageId.value,
      replyMode: replyMode.value,
      subject: draftSubject.value.trim(),
      htmlBody,
      attachmentFileUsageIds: []
    })
    draftBody.value = ''
    replyToMessageId.value = undefined
    quotedMessageId.value = undefined
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

function createReplySubject(subject?: string): string {
  if (!subject) return 'Re: '
  return subject.startsWith('Re:') ? subject : `Re: ${subject}`
}

function hasEditorContent(html: string): boolean {
  return new DOMParser().parseFromString(html, 'text/html').body.textContent?.trim().length !== 0
}

watch(() => props.conversation.id, onConversationChanged, { immediate: true })
watch(() => props.messages, onConversationChanged)

defineExpose({ startReply, insertManualQuote })
</script>
