<template>
  <q-dialog ref="dialogRef" @hide="onDialogHide">
    <q-card class="mail-metadata-dialog full-width q-ma-sm">
      <TitleBar :title="t('pages.receivingManagement.mailInformation')" @close="onDialogCancel" />
      <q-card-section class="q-pa-sm relative-position">
        <div v-if="metadata" class="q-gutter-y-sm">
          <div v-for="field in metadataFields" :key="field.label" class="mail-metadata-dialog__field">
            <div class="text-caption text-grey-7">{{ field.label }}</div>
            <div class="text-body2 text-break">{{ field.value || t('pages.receivingManagement.metadataUnavailable') }}</div>
          </div>
          <div class="mail-metadata-dialog__field">
            <div class="text-caption text-grey-7">{{ t('pages.receivingManagement.deliveryRoute') }}</div>
            <div v-if="metadata.deliveryHops.length" class="q-gutter-y-sm">
              <div v-for="(hop, index) in metadata.deliveryHops" :key="`${hop.receivedHeader}-${index}`" class="text-body2 text-break">
                <div>{{ hop.receivedHeader }}</div>
                <div class="text-caption text-grey-7">{{ hop.ipAddresses.join(', ') || t('pages.receivingManagement.metadataUnavailable') }}</div>
              </div>
            </div>
            <div v-else class="text-body2">{{ t('pages.receivingManagement.metadataUnavailable') }}</div>
          </div>
        </div>
        <q-inner-loading :showing="isLoading" color="primary" />
      </q-card-section>
    </q-card>
  </q-dialog>
</template>

<script setup lang="ts">
import { useDialogPluginComponent } from 'quasar'
import TitleBar from 'src/components/windowLike/TitleBar.vue'
import { getMailMessageMetadata, type IMailMessageMetadata } from 'src/api/mailConversation'
import { formatDate } from 'src/utils/format'
import { useI18n } from 'vue-i18n'

const props = defineProps<{
  messageId: number
}>()
defineEmits([...useDialogPluginComponent.emits])

const { t } = useI18n()
const { dialogRef, onDialogHide, onDialogCancel } = useDialogPluginComponent()
const metadata = ref<IMailMessageMetadata>()
const isLoading = ref(false)
const metadataFields = computed(() => {
  const metadataValue = metadata.value
  if (!metadataValue) return []

  return [
    { label: t('pages.receivingManagement.subject'), value: metadataValue.subject },
    { label: t('pages.receivingManagement.from'), value: formatAddresses(metadataValue.from) },
    { label: t('pages.receivingManagement.sender'), value: formatAddresses(metadataValue.sender) },
    { label: t('pages.receivingManagement.replyTo'), value: formatAddresses(metadataValue.replyTo) },
    { label: t('pages.receivingManagement.to'), value: formatAddresses(metadataValue.to) },
    { label: t('pages.receivingManagement.cc'), value: formatAddresses(metadataValue.cc) },
    { label: t('pages.receivingManagement.sentAt'), value: formatDate(metadataValue.sentAtUtc) },
    { label: t('pages.receivingManagement.receivedAt'), value: formatDate(metadataValue.receivedAtUtc) },
    { label: t('pages.receivingManagement.messageSize'), value: formatSize(metadataValue.size) },
    { label: t('pages.receivingManagement.senderTimeZone'), value: metadataValue.senderTimeZoneOffset },
    { label: t('pages.receivingManagement.messageId'), value: metadataValue.internetMessageId },
    { label: t('pages.receivingManagement.inReplyTo'), value: metadataValue.inReplyToMessageIds.join(', ') },
    { label: t('pages.receivingManagement.references'), value: metadataValue.referenceMessageIds.join(', ') }
  ]
})

function formatAddresses(addresses: IMailMessageMetadata['from']): string {
  return addresses.map(address => address.displayName ? `${address.displayName} <${address.email}>` : address.email).join(', ')
}

function formatSize(size?: number): string {
  if (size === undefined) return ''
  if (size < 1024) return `${size} B`
  if (size < 1024 * 1024) return `${(size / 1024).toFixed(1)} KB`
  return `${(size / (1024 * 1024)).toFixed(1)} MB`
}

async function loadMetadata(): Promise<void> {
  isLoading.value = true
  try {
    metadata.value = (await getMailMessageMetadata(props.messageId)).data
  } finally {
    isLoading.value = false
  }
}

onMounted(() => void loadMetadata())
</script>

<style lang="scss" scoped>
.mail-metadata-dialog {
  max-width: 860px;
}

.mail-metadata-dialog__field {
  overflow-wrap: anywhere;
}
</style>
