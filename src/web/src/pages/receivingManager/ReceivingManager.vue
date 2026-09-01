<template>
  <div class="receiving-workspace">
    <aside v-show="!isCompact || mobileView === 'list'" class="account-pane">
      <div class="pane-toolbar">
        <q-select v-model="selectedAccountId" dense outlined emit-value map-options clearable class="col"
          :label="t('pages.receivingManagement.account')" :options="accountOptions" />
        <CommonBtn icon="sync" flat :loading="syncing" :tooltip="t('pages.receivingManagement.sync')" @click="onSync" />
      </div>
      <div class="pane-toolbar">
        <q-input v-model="filter" dense outlined clearable debounce="300" class="col" :placeholder="t('pages.receivingManagement.search')">
          <template #prepend><q-icon name="search" /></template>
        </q-input>
        <q-btn-toggle v-model="unreadOnly" dense unelevated toggle-color="primary" :options="unreadOptions" />
      </div>
      <q-list separator class="conversation-list">
        <q-item v-for="conversation in conversations" :key="conversation.id" clickable
          :active="conversation.id === selectedConversation?.id" active-class="bg-blue-1 text-primary" @click="onConversationSelect(conversation)">
          <q-item-section avatar><q-avatar color="grey-3" text-color="primary" icon="person" /></q-item-section>
          <q-item-section>
            <q-item-label class="text-weight-medium ellipsis">{{ conversation.displayTitle || participantTitle(conversation) }}</q-item-label>
            <q-item-label caption lines="1">{{ conversation.lastMessagePreview || t('pages.receivingManagement.noPreview') }}</q-item-label>
            <div class="row q-gutter-xs q-mt-xs"><q-badge v-for="tag in conversationTags(conversation)" :key="tag.id" :style="{ backgroundColor: tag.color }">{{ tag.name }}</q-badge></div>
          </q-item-section>
          <q-item-section side><q-badge v-if="conversation.unreadCount" rounded color="negative">{{ conversation.unreadCount }}</q-badge><span class="text-caption q-mt-xs">{{ shortDate(conversation.lastMessageAtUtc) }}</span></q-item-section>
        </q-item>
      </q-list>
      <div v-if="!loadingConversations && conversations.length === 0" class="empty-state"><q-icon name="mark_email_read" size="40px" /><span>{{ t('pages.receivingManagement.empty') }}</span></div>
    </aside>

    <main v-show="!isCompact || mobileView === 'conversation'" class="conversation-pane">
      <template v-if="selectedConversation">
        <header class="conversation-header">
          <CommonBtn v-if="isCompact" icon="arrow_back" flat :tooltip="t('common.back')" @click="mobileView = 'list'" />
          <div class="col min-width-0"><div class="text-subtitle1 text-weight-medium ellipsis">{{ selectedConversation.displayTitle || participantTitle(selectedConversation) }}</div><div class="text-caption text-grey-7">{{ selectedConversation.emailAccount }}</div></div>
          <CommonBtn icon="label" flat :tooltip="t('pages.receivingManagement.tags')" @click="showTagDialog = true" />
          <CommonBtn icon="playlist_add" flat :disable="selectedMessageIds.length === 0" :tooltip="t('pages.receivingManagement.createTodo')" @click="openTodoDialog" />
        </header>
        <q-scroll-area ref="messageScroll" class="message-scroll">
          <div class="message-stream">
            <article v-for="message in messages" :key="message.id" class="message-row" :class="message.direction === MailMessageDirection.Outgoing ? 'outgoing' : 'incoming'">
              <q-checkbox v-model="selectedMessageIds" :val="message.id" dense class="message-select" />
              <div class="message-bubble" @click="expandedMessageIds.add(message.id)">
                <div class="row items-center no-wrap q-gutter-sm"><span class="text-weight-medium ellipsis">{{ message.subject || t('pages.receivingManagement.noSubject') }}</span><span class="text-caption text-grey-7">{{ longDate(message.occurredAtUtc) }}</span></div>
                <div class="text-caption text-grey-7 ellipsis">{{ addressLine(message) }}</div>
                <MailBody v-if="expandedMessageIds.has(message.id)" :message-id="message.id" class="q-mt-sm" />
                <div v-else class="expand-hint"><q-icon name="expand_more" /> {{ t('pages.receivingManagement.expand') }}</div>
                <div v-if="message.attachments.length" class="row q-gutter-xs q-mt-sm"><q-chip v-for="attachment in message.attachments" :key="attachment.id" dense square icon="attach_file">{{ attachment.fileName }}</q-chip></div>
                <div class="bubble-actions"><CommonBtn icon="reply" flat dense :tooltip="t('pages.receivingManagement.reply')" @click.stop="startReply(message)" /></div>
              </div>
            </article>
          </div>
        </q-scroll-area>
        <section class="composer">
          <div class="row items-center q-gutter-sm q-mb-sm">
            <q-input v-model="draftSubject" dense outlined class="col" :label="t('pages.receivingManagement.subject')" />
            <q-btn-toggle v-model="replyMode" dense unelevated toggle-color="primary" :options="replyOptions" />
            <CommonBtn icon="article" flat :tooltip="t('pages.receivingManagement.insertTemplate')" @click="showTemplateDialog = true" />
          </div>
          <q-expansion-item v-if="quotedContent" v-model="isQuoteExpanded" dense icon="format_quote" :label="t('pages.receivingManagement.quotedContent')" header-class="text-caption">
            <q-input v-model="quotedContent" outlined type="textarea" autogrow class="q-mb-sm"><template #append><CommonBtn icon="close" flat dense :tooltip="t('pages.receivingManagement.removeQuote')" @click="quotedContent = ''" /></template></q-input>
          </q-expansion-item>
          <q-editor v-model="draftBody" min-height="110px" :placeholder="t('pages.receivingManagement.writeReply')" :toolbar="editorToolbar" />
          <div class="row justify-end q-mt-sm"><CommonBtn icon="send" :label="t('pages.receivingManagement.send')" :loading="sending" @click="onSend" /></div>
        </section>
      </template>
      <div v-else class="empty-state"><q-icon name="forum" size="48px" /><span>{{ t('pages.receivingManagement.selectConversation') }}</span></div>
    </main>

    <q-dialog v-model="showTemplateDialog"><q-card class="dialog-card"><q-card-section class="text-subtitle1">{{ t('pages.receivingManagement.insertTemplate') }}</q-card-section><q-list separator><q-item v-for="template in templates" :key="template.id" clickable v-close-popup @click="draftBody += template.content"><q-item-section>{{ template.name }}</q-item-section></q-item></q-list></q-card></q-dialog>
    <q-dialog v-model="showTagDialog"><q-card class="dialog-card"><q-card-section class="text-subtitle1">{{ t('pages.receivingManagement.tags') }}</q-card-section><q-card-section><div class="row items-center q-gutter-sm q-mb-md"><q-input v-model="newTagName" dense outlined class="col" :label="t('pages.receivingManagement.newTag')" /><input v-model="newTagColor" type="color" class="tag-color" :aria-label="t('pages.receivingManagement.tagColor')" /><CommonBtn icon="add" flat :tooltip="t('pages.receivingManagement.addTag')" @click="onCreateTag" /></div><q-separator class="q-mb-md" /><div v-for="contact in selectedConversation?.participants" :key="contact.id" class="q-mb-md"><div class="text-weight-medium q-mb-xs">{{ contact.displayName || contact.email }}</div><q-option-group v-model="contactTagSelection[contact.id]" :options="tagOptions" type="checkbox" color="primary" /></div></q-card-section><q-card-actions align="right"><CommonBtn icon="save" :label="t('common.save')" @click="onSaveTags" /></q-card-actions></q-card></q-dialog>
    <q-dialog v-model="showTodoDialog"><q-card class="dialog-card"><q-card-section class="text-subtitle1">{{ t('pages.receivingManagement.createTodo') }}</q-card-section><q-card-section class="q-gutter-md"><q-input v-model="todoTitle" dense outlined :label="t('pages.todoManagement.title')" /><q-input v-model="todoDescription" dense outlined type="textarea" :label="t('pages.todoManagement.description')" /><q-input v-model="todoDueAt" dense outlined type="datetime-local" :label="t('pages.todoManagement.dueAt')" /></q-card-section><q-card-actions align="right"><CommonBtn icon="add_task" :label="t('common.create')" @click="onCreateTodo" /></q-card-actions></q-card></q-dialog>
  </div>
</template>

<script setup lang="ts">
import dayjs from 'dayjs'
import { useQuasar } from 'quasar'
import CommonBtn from 'src/components/buttons/CommonBtn.vue'
import MailBody from './components/MailBody.vue'
import { getEmailTemplatesData, type IEmailTemplate } from 'src/api/emailTemplate'
import { createMailTodoTask, TodoTaskPriority, TodoTaskStatus } from 'src/api/todoTask'
import { createMailTag, getMailConversations, getMailMessages, getMailTags, getReceivingAccounts, markMailConversationRead, MailMessageDirection, MailReplyMode, sendMailConversationMessage, setMailContactTags, synchronizeReceivingAccount, type IMailConversation, type IMailMessage, type IMailTag, type IReceivingAccount } from 'src/api/mailConversation'
import { notifyError, notifySuccess } from 'src/utils/dialog'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()
const $q = useQuasar()
const isCompact = computed(() => $q.screen.lt.md)
const mobileView = ref<'list' | 'conversation'>('list')
const accounts = ref<IReceivingAccount[]>([])
const selectedAccountId = ref<number>()
const conversations = ref<IMailConversation[]>([])
const selectedConversation = ref<IMailConversation>()
const messages = ref<IMailMessage[]>([])
const tags = ref<IMailTag[]>([])
const templates = ref<IEmailTemplate[]>([])
const filter = ref('')
const unreadOnly = ref(false)
const loadingConversations = ref(false)
const syncing = ref(false)
const sending = ref(false)
const expandedMessageIds = ref(new Set<number>())
const selectedMessageIds = ref<number[]>([])
const replyToMessageId = ref<number>()
const replyMode = ref(MailReplyMode.Reply)
const draftSubject = ref('')
const draftBody = ref('')
const quotedContent = ref('')
const isQuoteExpanded = ref(false)
const showTemplateDialog = ref(false)
const showTagDialog = ref(false)
const showTodoDialog = ref(false)
const contactTagSelection = ref<Record<number, number[]>>({})
const todoTitle = ref('')
const todoDescription = ref('')
const todoDueAt = ref('')
const newTagName = ref('')
const newTagColor = ref('#1976d2')
const editorToolbar = [['bold', 'italic', 'underline'], ['unordered', 'ordered'], ['link'], ['undo', 'redo']]
const accountOptions = computed(() => accounts.value.map(account => ({
  label: `${account.name || account.email} (${account.email})${account.isSupported ? '' : ` - ${t('pages.receivingManagement.unsupported')}`}`,
  value: account.emailAccountId,
  disable: !account.isSupported
})))
const unreadOptions = computed(() => [{ label: t('pages.receivingManagement.all'), value: false }, { label: t('pages.receivingManagement.unread'), value: true }])
const replyOptions = computed(() => [{ label: t('pages.receivingManagement.reply'), value: MailReplyMode.Reply }, { label: t('pages.receivingManagement.replyAll'), value: MailReplyMode.ReplyAll }])
const tagOptions = computed(() => tags.value.map(tag => ({ label: tag.name, value: tag.id, color: tag.color })))

async function loadConversations () {
  loadingConversations.value = true
  try { conversations.value = (await getMailConversations({ emailAccountId: selectedAccountId.value, filter: filter.value || undefined, unreadOnly: unreadOnly.value, limit: 100 })).data } finally { loadingConversations.value = false }
}
async function onConversationSelect (conversation: IMailConversation) {
  selectedConversation.value = conversation
  selectedMessageIds.value = []
  expandedMessageIds.value = new Set()
  messages.value = (await getMailMessages(conversation.id)).data
  if (conversation.unreadCount) { await markMailConversationRead(conversation.id); conversation.unreadCount = 0 }
  if (isCompact.value) mobileView.value = 'conversation'
}
async function onSync () {
  const account = accounts.value.find(value => value.emailAccountId === selectedAccountId.value)
  if (!account?.receivingAccountId) { notifyError(account?.lastError || t('pages.receivingManagement.configurationRequired')); return }
  syncing.value = true
  try { await synchronizeReceivingAccount(account.receivingAccountId); await loadConversations(); notifySuccess(t('pages.receivingManagement.syncComplete')) } finally { syncing.value = false }
}
function startReply (message: IMailMessage) { replyToMessageId.value = message.id; draftSubject.value = message.subject?.startsWith('Re:') ? message.subject : `Re: ${message.subject || ''}`; quotedContent.value = `${longDate(message.occurredAtUtc)} ${addressLine(message)}` }
async function onSend () {
  if (!selectedConversation.value || !draftSubject.value.trim() || !draftBody.value.trim()) { notifyError(t('pages.receivingManagement.completeReply')); return }
  sending.value = true
  try {
    await sendMailConversationMessage(selectedConversation.value.id, { replyToMessageId: replyToMessageId.value, replyMode: replyMode.value, subject: draftSubject.value, htmlBody: `${draftBody.value}${quotedContent.value ? `<blockquote>${quotedContent.value}</blockquote>` : ''}`, attachmentFileUsageIds: [] })
    draftBody.value = ''; quotedContent.value = ''; messages.value = (await getMailMessages(selectedConversation.value.id)).data; notifySuccess(t('pages.receivingManagement.sent'))
  } finally { sending.value = false }
}
function openTodoDialog () { todoTitle.value = selectedConversation.value?.displayTitle || ''; todoDescription.value = ''; todoDueAt.value = ''; showTodoDialog.value = true }
async function onCreateTodo () {
  if (!selectedConversation.value || !todoTitle.value.trim()) return
  await createMailTodoTask({ title: todoTitle.value, description: todoDescription.value, status: TodoTaskStatus.Pending, priority: TodoTaskPriority.Normal, dueAtUtc: todoDueAt.value ? new Date(todoDueAt.value).toISOString() : undefined, sourceConversationId: selectedConversation.value.id, sourceMessageIds: selectedMessageIds.value, branchSubject: todoTitle.value })
  showTodoDialog.value = false; notifySuccess(t('pages.receivingManagement.todoCreated'))
}
async function onSaveTags () { for (const contact of selectedConversation.value?.participants || []) await setMailContactTags(contact.id, contactTagSelection.value[contact.id] || []); showTagDialog.value = false; await loadConversations() }
async function onCreateTag () {
  if (!newTagName.value.trim()) return
  tags.value.push((await createMailTag(newTagName.value, newTagColor.value)).data)
  newTagName.value = ''
}
function participantTitle (conversation: IMailConversation) { return conversation.participants.map(contact => contact.displayName || contact.email).join(', ') }
function conversationTags (conversation: IMailConversation) { return [...new Map(conversation.participants.flatMap(contact => contact.tags).map(tag => [tag.id, tag])).values()] }
function addressLine (message: IMailMessage) { const addresses = message.direction === MailMessageDirection.Incoming ? message.from : message.to; return addresses.map(address => address.displayName || address.email).join(', ') }
function shortDate (date: string) { return dayjs(date).format('MM-DD') }
function longDate (date: string) { return dayjs(date).format('YYYY-MM-DD HH:mm') }

watch([selectedAccountId, filter, unreadOnly], loadConversations)
watch(showTagDialog, visible => { if (visible) contactTagSelection.value = Object.fromEntries((selectedConversation.value?.participants || []).map(contact => [contact.id, contact.tags.map(tag => tag.id)])) })
onMounted(async () => {
  const [accountResult, tagResult, templateResult] = await Promise.all([getReceivingAccounts(), getMailTags(), getEmailTemplatesData(undefined, { sortBy: 'id', descending: true, skip: 0, limit: 100 })])
  accounts.value = accountResult.data; tags.value = tagResult.data; templates.value = templateResult.data; selectedAccountId.value = accounts.value[0]?.emailAccountId; await loadConversations()
})
</script>

<style scoped lang="scss">
.receiving-workspace { height: calc(100vh - 98px); min-height: 540px; display: grid; grid-template-columns: minmax(300px, 34%) 1fr; border: 1px solid $grey-4; background: white; overflow: hidden; }
.account-pane,.conversation-pane { min-width: 0; min-height: 0; display: flex; flex-direction: column; }
.account-pane { border-right: 1px solid $grey-4; }
.pane-toolbar,.conversation-header { min-height: 54px; display: flex; align-items: center; gap: 8px; padding: 8px 12px; border-bottom: 1px solid $grey-4; }
.conversation-list { overflow: auto; flex: 1; }
.message-scroll { flex: 1; min-height: 0; background: $grey-2; }
.message-stream { padding: 18px; display: flex; flex-direction: column; gap: 14px; }
.message-row { display: flex; align-items: flex-start; gap: 6px; max-width: 84%; }
.message-row.outgoing { align-self: flex-end; flex-direction: row-reverse; }
.message-bubble { position: relative; min-width: 240px; padding: 12px 42px 10px 12px; border: 1px solid $grey-4; border-radius: 6px; background: white; cursor: pointer; }
.outgoing .message-bubble { background: #e7f6ea; border-color: #b8dfc0; }
.bubble-actions { position: absolute; right: 4px; top: 4px; }
.expand-hint { color: $grey-7; font-size: 12px; margin-top: 6px; }
.composer { flex: 0 0 auto; padding: 10px 12px; border-top: 1px solid $grey-4; background: white; }
.empty-state { flex: 1; display: flex; flex-direction: column; align-items: center; justify-content: center; gap: 10px; color: $grey-7; }
.dialog-card { width: min(560px, 92vw); max-height: 80vh; overflow: auto; border-radius: 6px; }
.tag-color { flex: 0 0 58px; }
.min-width-0 { min-width: 0; }
@media (max-width: 1023px) { .receiving-workspace { grid-template-columns: 1fr; height: calc(100vh - 82px); }.account-pane { border-right: 0; }.message-row { max-width: 96%; } }
</style>
