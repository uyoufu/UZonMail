<template>
  <aside class="column full-height min-width-0">
    <div class="row no-wrap items-center q-pa-sm q-gutter-sm">
      <q-avatar color="primary" text-color="white" size="32px" class="cursor-pointer" tabindex="0">
        {{ receivingAccountLabel }}
        <q-menu auto-close>
          <q-list dense class="q-pa-xs">
            <q-item clickable :active="selectedAccountId === undefined" active-class="bg-blue-1 text-primary"
              @click="selectedAccountId = undefined">
              <q-item-section avatar><q-icon name="all_inbox" /></q-item-section>
              <q-item-section>{{ t('pages.receivingManagement.allAccounts') }}</q-item-section>
            </q-item>
            <q-item v-for="account in accounts" :key="account.emailAccountId" clickable :disable="!account.isSupported"
              :active="account.emailAccountId === selectedAccountId" active-class="bg-blue-1 text-primary"
              @click="selectedAccountId = account.emailAccountId">
              <q-item-section avatar><q-icon :name="account.isSupported ? 'mail' : 'error_outline'" /></q-item-section>
              <q-item-section>
                <q-item-label>{{ account.name || account.email }}</q-item-label>
                <q-item-label caption>{{ account.email }}</q-item-label>
              </q-item-section>
            </q-item>
          </q-list>
        </q-menu>
      </q-avatar>
      <q-input v-model="filter" dense outlined hide-bottom-space clearable debounce="300" class="col"
        :placeholder="t('pages.receivingManagement.search')">
        <template #prepend><q-icon name="search" /></template>
      </q-input>
      <CommonBtn icon="sync" flat :loading="isSyncing" :tooltip="t('pages.receivingManagement.sync')" @click="emit('sync')" />
    </div>
    <div class="q-px-sm q-pb-sm">
      <q-btn-toggle v-model="unreadOnly" dense unelevated spread toggle-color="primary" :options="unreadOptions" />
    </div>
    <q-scroll-area class="col height-0">
      <q-list dense class="q-px-xs">
        <q-item v-for="conversation in conversations" :key="conversation.id" clickable class="border-radius-4 q-mb-xs"
          :active="conversation.id === selectedConversationId" active-class="bg-blue-1 text-primary" @click="emit('select', conversation)">
          <q-item-section avatar><q-avatar color="grey-3" text-color="primary" icon="person" /></q-item-section>
          <q-item-section class="min-width-0">
            <q-item-label class="text-weight-medium" lines="1">{{ conversation.displayTitle || participantTitle(conversation) }}</q-item-label>
            <q-item-label caption lines="2">{{ conversation.lastMessagePreview || t('pages.receivingManagement.noPreview') }}</q-item-label>
            <div v-if="conversationTags(conversation).length" class="row q-gutter-xs q-mt-xs">
              <q-badge v-for="tag in conversationTags(conversation)" :key="tag.id" outline color="primary">{{ tag.name }}</q-badge>
            </div>
          </q-item-section>
          <q-item-section side top>
            <q-badge v-if="conversation.unreadCount" rounded color="negative">{{ conversation.unreadCount }}</q-badge>
            <span class="text-caption text-grey-7 q-mt-xs">{{ formatDate(conversation.lastMessageAtUtc, 'MM-DD') }}</span>
          </q-item-section>
        </q-item>
        <q-item v-if="!isLoading && conversations.length === 0" class="text-grey-7">
          <q-item-section class="items-center q-py-xl">
            <q-icon name="mark_email_read" size="40px" class="q-mb-sm" />
            <q-item-label>{{ t('pages.receivingManagement.empty') }}</q-item-label>
          </q-item-section>
        </q-item>
      </q-list>
      <q-inner-loading :showing="isLoading" color="primary" />
    </q-scroll-area>
  </aside>
</template>

<script setup lang="ts">
import CommonBtn from 'src/components/buttons/CommonBtn.vue'
import type { IMailConversation, IReceivingAccount } from 'src/api/mailConversation'
import { formatDate } from 'src/utils/format'
import { useI18n } from 'vue-i18n'

const selectedAccountId = defineModel<number | undefined>('selectedAccountId')
const filter = defineModel<string>('filter', { default: '' })
const unreadOnly = defineModel<boolean>('unreadOnly', { default: false })
const props = defineProps<{
  accounts: IReceivingAccount[]
  conversations: IMailConversation[]
  selectedConversationId?: number
  isLoading: boolean
  isSyncing: boolean
}>()
const emit = defineEmits<{
  select: [conversation: IMailConversation]
  sync: []
}>()
const { t } = useI18n()

const unreadOptions = computed(() => [
  { label: t('pages.receivingManagement.all'), value: false },
  { label: t('pages.receivingManagement.unread'), value: true }
])
const receivingAccountLabel = computed(() => {
  const account = props.accounts.find(value => value.emailAccountId === selectedAccountId.value)
  const displayName = account?.name || account?.email || t('pages.receivingManagement.allAccounts')
  return displayName.slice(0, 1).toLocaleUpperCase()
})

function participantTitle(conversation: IMailConversation): string {
  return conversation.participants.map(contact => contact.displayName || contact.email).join(', ')
}

function conversationTags(conversation: IMailConversation) {
  return [...new Map(conversation.participants.flatMap(contact => contact.tags).map(tag => [tag.id, tag])).values()]
}
</script>
