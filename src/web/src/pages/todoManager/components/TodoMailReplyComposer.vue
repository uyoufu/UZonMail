<template>
  <MailReplyEditor ref="replyEditor" v-model:subject="draftSubject" v-model:html-body="draftBody" :is-sending="isSending" @submit="onSend">
    <template #actions>
      <q-btn-toggle v-model="replyMode" dense unelevated toggle-color="primary" :options="replyOptions" />
    </template>
  </MailReplyEditor>
</template>

<script setup lang="ts">
import MailReplyEditor from 'src/components/mailMessage/MailReplyEditor.vue'
import { createManualQuoteHtml, createReplySubject, hasEditorContent } from 'src/components/mailMessage/mailReplyContent'
import { useMailMessageDraftCache } from 'src/components/mailMessage/useMailMessageDraftCache'
import { MailReplyMode, type IMailMessage } from 'src/api/mailConversation'
import { sendTodoMailMessage } from 'src/api/todoTask'
import { notifyError, notifySuccess } from 'src/utils/dialog'
import { useI18n } from 'vue-i18n'

interface ITodoReplyDraft {
  subject: string
  body: string
  replyMode: MailReplyMode
}

interface ITodoUntargetedDraft {
  subject: string
  body: string
}

const props = defineProps<{
  taskId: number
  branchSubject: string
}>()
const emit = defineEmits<{
  sent: []
}>()
const { t } = useI18n()
const replyEditor = ref<InstanceType<typeof MailReplyEditor>>()
const draftSubject = ref('')
const draftBody = ref('')
const replyMode = ref(MailReplyMode.ReplyAll)
const activeReplyMessageId = ref<number>()
const isSending = ref(false)
const untargetedDraftsByTaskId = new Map<number, ITodoUntargetedDraft>()
const replyDraftCache = useMailMessageDraftCache(copyReplyDraft)
const replyOptions = computed(() => [
  { label: t('pages.receivingManagement.reply'), value: MailReplyMode.Reply },
  { label: t('pages.receivingManagement.replyAll'), value: MailReplyMode.ReplyAll }
])

/** Inserts a selected mail-body quote and makes that mail the reply target. */
async function insertManualQuote(message: IMailMessage, selectedText: string): Promise<void> {
  if (!selectedText.trim()) return

  saveCurrentDraft(props.taskId)
  activateReplyTarget(message)
  draftBody.value = appendReplyHtml(draftBody.value, createManualQuoteHtml(selectedText))
  saveCurrentDraft(props.taskId)
  await nextTick()
  replyEditor.value?.focus()
}

/** Activates a mail-targeted draft, merging an existing untargeted task draft when required. */
function activateReplyTarget(message: IMailMessage): void {
  if (activeReplyMessageId.value === message.id) return

  const previousReplyMessageId = activeReplyMessageId.value
  activeReplyMessageId.value = message.id
  replyDraftCache.saveLastMessageId(props.taskId, message.id)
  if (previousReplyMessageId) {
    restoreOrInitializeTargetDraft(message)
    return
  }

  const untargetedDraft = untargetedDraftsByTaskId.get(props.taskId)
  const targetDraft = replyDraftCache.getDraft(message.id)
  applyTargetDraft(mergeUntargetedDraft(targetDraft, untargetedDraft, message))
  untargetedDraftsByTaskId.delete(props.taskId)
}

/** Sends either a mail-targeted reply or the retained unassociated branch composition. */
async function onSend(): Promise<void> {
  if (!draftSubject.value.trim() || !hasEditorContent(draftBody.value)) {
    notifyError(t('pages.receivingManagement.completeReply'))
    return
  }

  isSending.value = true
  const replyMessageId = activeReplyMessageId.value
  try {
    await sendTodoMailMessage(props.taskId, {
      replyToMessageId: replyMessageId,
      replyMode: replyMode.value,
      subject: draftSubject.value.trim(),
      htmlBody: draftBody.value,
      attachmentFileUsageIds: []
    })
    clearSentDraft(replyMessageId)
    notifySuccess(t('pages.receivingManagement.sent'))
    emit('sent')
  } catch {
    notifyError(t('pages.receivingManagement.sendFailed'))
  } finally {
    isSending.value = false
  }
}

/** Persists the editor into its target-mail cache or its temporary task draft. */
function saveCurrentDraft(taskId: number): void {
  const replyMessageId = activeReplyMessageId.value
  if (!replyMessageId) {
    untargetedDraftsByTaskId.set(taskId, createUntargetedDraft())
    return
  }

  replyDraftCache.saveDraft(replyMessageId, createTargetDraft())
  replyDraftCache.saveLastMessageId(taskId, replyMessageId)
}

/** Restores the last target for a task, or its unassociated composition when no target exists. */
function restoreTaskDraft(taskId: number): void {
  const replyMessageId = replyDraftCache.getLastMessageId(taskId)
  if (replyMessageId) {
    const targetDraft = replyDraftCache.getDraft(replyMessageId)
    if (targetDraft) {
      activeReplyMessageId.value = replyMessageId
      applyTargetDraft(targetDraft)
      return
    }
    replyDraftCache.removeLastMessageId(taskId)
  }

  activeReplyMessageId.value = undefined
  const untargetedDraft = untargetedDraftsByTaskId.get(taskId)
  if (untargetedDraft) {
    draftSubject.value = untargetedDraft.subject
    draftBody.value = untargetedDraft.body
  } else {
    draftSubject.value = props.branchSubject
    draftBody.value = ''
  }
  replyMode.value = MailReplyMode.ReplyAll
}

/** Restores an existing target draft or creates a default reply draft for the selected mail. */
function restoreOrInitializeTargetDraft(message: IMailMessage): void {
  const targetDraft = replyDraftCache.getDraft(message.id)
  if (targetDraft) {
    applyTargetDraft(targetDraft)
    return
  }

  applyTargetDraft({
    subject: createReplySubject(message.subject),
    body: '',
    replyMode: MailReplyMode.ReplyAll
  })
}

/** Merges an unassociated composition into a target draft without discarding either body. */
function mergeUntargetedDraft(
  targetDraft: ITodoReplyDraft | undefined,
  untargetedDraft: ITodoUntargetedDraft | undefined,
  message: IMailMessage
): ITodoReplyDraft {
  const baseDraft = targetDraft || {
    subject: createReplySubject(message.subject),
    body: '',
    replyMode: MailReplyMode.ReplyAll
  }
  if (!untargetedDraft) return baseDraft

  return {
    subject: hasCustomizedUntargetedSubject(untargetedDraft.subject) ? untargetedDraft.subject : baseDraft.subject,
    body: appendReplyHtml(baseDraft.body, untargetedDraft.body),
    replyMode: baseDraft.replyMode
  }
}

/** Applies a target draft to the editable reply state. */
function applyTargetDraft(replyDraft: ITodoReplyDraft): void {
  draftSubject.value = replyDraft.subject
  draftBody.value = replyDraft.body
  replyMode.value = replyDraft.replyMode
}

/** Clears only the successfully sent target draft and retains other mail targets. */
function clearSentDraft(replyMessageId: number | undefined): void {
  if (replyMessageId) {
    replyDraftCache.removeDraft(replyMessageId)
    replyDraftCache.removeLastMessageId(props.taskId, replyMessageId)
  } else {
    untargetedDraftsByTaskId.delete(props.taskId)
  }

  activeReplyMessageId.value = undefined
  draftSubject.value = props.branchSubject
  draftBody.value = ''
  replyMode.value = MailReplyMode.ReplyAll
}

/** Creates a target-mail draft from the current editor state. */
function createTargetDraft(): ITodoReplyDraft {
  return {
    subject: draftSubject.value,
    body: draftBody.value,
    replyMode: replyMode.value
  }
}

/** Creates the temporary unassociated task draft from the current editor state. */
function createUntargetedDraft(): ITodoUntargetedDraft {
  return {
    subject: draftSubject.value,
    body: draftBody.value
  }
}

/** Appends non-empty reply HTML with an editor paragraph boundary. */
function appendReplyHtml(currentHtml: string, appendedHtml: string): string {
  if (!hasEditorContent(currentHtml)) return appendedHtml
  if (!hasEditorContent(appendedHtml)) return currentHtml
  return `${currentHtml}<p><br></p>${appendedHtml}`
}

/** Determines whether the default task subject was intentionally changed by the user. */
function hasCustomizedUntargetedSubject(subject: string): boolean {
  return subject.trim().length > 0 && subject.trim() !== props.branchSubject.trim()
}

/** Copies drafts before they cross the mail-ID cache boundary. */
function copyReplyDraft(replyDraft: ITodoReplyDraft): ITodoReplyDraft {
  return { ...replyDraft }
}

watch(() => props.taskId, (taskId, previousTaskId) => {
  if (previousTaskId !== undefined) saveCurrentDraft(previousTaskId)
  restoreTaskDraft(taskId)
}, { immediate: true })

defineExpose({ insertManualQuote })
</script>
