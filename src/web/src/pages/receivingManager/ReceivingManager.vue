<template>
  <PageContainer>
    <q-splitter v-if="!isCompact" v-model="splitterSize" unit="%" :limits="[22, 50]" class="full-height">
      <template #before>
        <ReceivingConversationList v-model:selected-account-id="selectedAccountId" v-model:filter="filter" v-model:unread-only="unreadOnly"
          :accounts="accounts" :conversations="conversations" :selected-conversation-id="selectedConversation?.id"
          :is-loading="isLoadingConversations" :is-syncing="isSyncing" @select="onConversationSelect" @sync="onSync" />
      </template>
      <template #after>
        <ConversationTimeline :conversation="selectedConversation" :messages="messages" :is-loading-messages="isLoadingMessages"
          :is-loading-older-messages="isLoadingOlderMessages" :has-older-messages="hasOlderMessages" :is-compact="false"
          @message-sent="onMessageSent" @request-older-messages="onLoadOlderMessages" @message-loaded="onTimelineMessageLoaded" @tags-saved="onTagsSaved" />
      </template>
    </q-splitter>
    <template v-else>
      <ReceivingConversationList v-show="mobileView === 'list'" v-model:selected-account-id="selectedAccountId" v-model:filter="filter"
        v-model:unread-only="unreadOnly" :accounts="accounts" :conversations="conversations" :selected-conversation-id="selectedConversation?.id"
        :is-loading="isLoadingConversations" :is-syncing="isSyncing" @select="onConversationSelect" @sync="onSync" />
      <ConversationTimeline v-show="mobileView === 'conversation'" :conversation="selectedConversation" :messages="messages"
        :is-loading-messages="isLoadingMessages" :is-loading-older-messages="isLoadingOlderMessages"
        :has-older-messages="hasOlderMessages" :is-compact="true" @back="mobileView = 'list'" @message-sent="onMessageSent"
        @request-older-messages="onLoadOlderMessages" @message-loaded="onTimelineMessageLoaded" @tags-saved="onTagsSaved" />
    </template>
  </PageContainer>
</template>

<script setup lang="ts">
import PageContainer from 'src/components/pageContainer/PageContainer.vue'
import ConversationTimeline from './components/ConversationTimeline.vue'
import ReceivingConversationList from './components/ReceivingConversationList.vue'
import { getMailConversations, getReceivingAccounts, markMailConversationRead, synchronizeReceivingAccount, type IMailConversation, type IMailMessage, type IReceivingAccount } from 'src/api/mailConversation'
import { notifyError, notifySuccess } from 'src/utils/dialog'
import { useI18n } from 'vue-i18n'
import { restoreReceivingAccountSelection, saveReceivingAccountSelection } from './receivingAccountPreference'
import { useConversationMessageHistory } from './compositions/useConversationMessageHistory'

const { t } = useI18n()
const $q = useQuasar()
const isCompact = computed(() => $q.screen.lt.md)
const splitterSize = ref(32)
const mobileView = ref<'list' | 'conversation'>('list')
const accounts = ref<IReceivingAccount[]>([])
const selectedAccountId = ref<number>()
const conversations = ref<IMailConversation[]>([])
const selectedConversation = ref<IMailConversation>()
const filter = ref('')
const unreadOnly = ref(false)
const isLoadingConversations = ref(false)
const isSyncing = ref(false)
let conversationLoadSequence = 0
const {
  messages,
  isLoadingInitialMessages: isLoadingMessages,
  isLoadingOlderMessages,
  hasOlderMessages,
  loadLatestMessages,
  loadOlderMessages,
  mergeMessage,
  refreshLatestMessages,
  clearMessageHistory
} = useConversationMessageHistory()

async function loadConversations() {
  isLoadingConversations.value = true
  try {
    conversations.value = (await getMailConversations({
      emailAccountId: selectedAccountId.value,
      filter: filter.value.trim() || undefined,
      unreadOnly: unreadOnly.value,
      limit: 100
    })).data
    if (selectedConversation.value && !conversations.value.some(value => value.id === selectedConversation.value?.id)) {
      selectedConversation.value = undefined
      clearMessageHistory()
      mobileView.value = 'list'
    }
  } finally {
    isLoadingConversations.value = false
  }
}

async function onConversationSelect(conversation: IMailConversation) {
  const currentLoadSequence = ++conversationLoadSequence
  if (isCompact.value) mobileView.value = 'conversation'
  selectedConversation.value = conversation
  await loadLatestMessages(conversation.id)
  if (currentLoadSequence !== conversationLoadSequence) return
  if (conversation.unreadCount) {
    conversation.unreadCount = 0
    void markMailConversationRead(conversation.id)
  }
}

async function onSync() {
  const receivingAccounts = selectedAccountId.value
    ? accounts.value.filter(account => account.emailAccountId === selectedAccountId.value && account.isSupported)
    : accounts.value.filter(account => account.isSupported)
  if (receivingAccounts.length === 0) {
    notifyError(t('pages.receivingManagement.configurationRequired'))
    return
  }

  isSyncing.value = true
  try {
    const syncResults = await Promise.allSettled(receivingAccounts.map(account => synchronizeReceivingAccount(account.receivingAccountId)))
    const hasFailure = syncResults.some(result => result.status === 'rejected')
    await loadConversations()
    if (hasFailure) notifyError(t('pages.receivingManagement.syncFailed'))
    else notifySuccess(t('pages.receivingManagement.syncComplete'))
  } finally {
    isSyncing.value = false
  }
}

async function onMessageSent() {
  if (!selectedConversation.value) return
  await refreshLatestMessages(selectedConversation.value.id)
  await loadConversations()
}

/** Loads another history page when the virtual timeline reaches its top threshold. */
async function onLoadOlderMessages(): Promise<void> {
  if (!selectedConversation.value) return
  await loadOlderMessages(selectedConversation.value.id)
}

function onTimelineMessageLoaded(message: IMailMessage) {
  mergeMessage(message)
}

async function onTagsSaved() {
  await loadConversations()
}

watch(selectedAccountId, saveReceivingAccountSelection)
watch([selectedAccountId, filter, unreadOnly], () => void loadConversations())

onMounted(async () => {
  accounts.value = (await getReceivingAccounts()).data
  selectedAccountId.value = restoreReceivingAccountSelection(accounts.value)
  if (!selectedAccountId.value) await loadConversations()
})
</script>
