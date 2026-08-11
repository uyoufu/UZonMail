<template>
  <q-dialog ref="dialogRef" @hide="onDialogHide">
    <q-card class="email-detail-dialog column no-wrap">
      <q-card-section class="row items-center no-wrap q-py-sm">
        <q-icon name="mail" color="primary" size="sm" />
        <div class="col row justify-between items-center q-ml-sm">
          <div class="text-subtitle1 text-weight-medium">
            {{ emailDetail?.subject || t('sendDetail.noSubject') }}
          </div>
          <div v-if="emailDetail" class="text-caption text-grey-7 q-mr-md">
            {{ t('sendDetail.sentAt') }}: {{ formatDate(emailDetail.sentAt) }}
          </div>
        </div>
        <CommonBtn flat dense icon="close" color="grey-7" :tooltip="t('sendDetail.close')" @click="onDialogCancel" />
      </q-card-section>

      <q-separator />

      <div v-if="isLoading" class="email-detail-dialog__state column items-center justify-center text-grey-7">
        <q-spinner-dots size="32px" color="primary" />
        <div class="q-mt-sm">{{ t('sendDetail.loadingEmail') }}</div>
      </div>

      <div v-else-if="hasLoadFailed" class="email-detail-dialog__state column items-center justify-center text-grey-7">
        <q-icon name="error_outline" color="negative" size="36px" />
        <div class="q-mt-sm">{{ t('sendDetail.loadEmailFailed') }}</div>
        <CommonBtn class="q-mt-md" icon="refresh" :label="t('sendDetail.retry')" :tooltip="t('sendDetail.retry')"
          @click="onLoadEmailDetail" />
      </div>

      <template v-else-if="emailDetail">
        <q-card-section class="email-detail-dialog__metadata q-py-sm">
          <div class="email-detail-dialog__metadata-row">
            <div class="email-detail-dialog__metadata-label">
              <q-icon name="outgoing_mail" />
              <span>{{ t('sendDetail.sender') }}</span>
            </div>
            <div class="email-detail-dialog__metadata-value">{{ emailDetail.fromEmail || t('sendDetail.none') }}</div>
          </div>
          <div class="email-detail-dialog__metadata-row">
            <div class="email-detail-dialog__metadata-label">
              <q-icon name="person" />
              <span>{{ t('sendDetail.recipients') }}</span>
            </div>
            <div class="email-detail-dialog__metadata-value">{{ formatAddresses(emailDetail.recipients) }}</div>
          </div>
          <div class="email-detail-dialog__metadata-row">
            <div class="email-detail-dialog__metadata-label">
              <q-icon name="group" />
              <span>{{ t('sendDetail.ccRecipients') }}</span>
            </div>
            <div class="email-detail-dialog__metadata-value">{{ formatAddresses(emailDetail.ccRecipients) }}</div>
          </div>
          <div class="email-detail-dialog__metadata-row">
            <div class="email-detail-dialog__metadata-label">
              <q-icon name="visibility_off" />
              <span>{{ t('sendDetail.bccRecipients') }}</span>
            </div>
            <div class="email-detail-dialog__metadata-value">{{ formatAddresses(emailDetail.bccRecipients) }}</div>
          </div>
        </q-card-section>

        <q-separator />

        <q-card-section class="email-detail-dialog__body column no-wrap q-pa-none">
          <div class="email-detail-dialog__section-title q-px-md q-py-sm">
            <q-icon name="article" class="q-mr-xs" />
            {{ t('sendDetail.emailContent') }}
          </div>
          <iframe v-if="emailDetail.content" class="email-detail-dialog__frame" :title="t('sendDetail.emailContent')"
            :srcdoc="emailDocument" sandbox="allow-popups allow-popups-to-escape-sandbox"
            referrerpolicy="no-referrer" />
          <div v-else class="email-detail-dialog__empty row items-center justify-center text-grey-7">
            {{ t('sendDetail.emptyContent') }}
          </div>
        </q-card-section>

        <q-separator />

        <q-card-section class="q-py-sm">
          <div class="email-detail-dialog__section-title q-mb-xs">
            <q-icon name="attachment" class="q-mr-xs" />
            {{ t('sendDetail.attachments') }}
            <span class="text-caption text-grey-7">({{ emailDetail.attachments.length }})</span>
          </div>
          <q-list v-if="emailDetail.attachments.length" dense separator class="email-detail-dialog__attachments">
            <q-item v-for="attachment in emailDetail.attachments" :key="attachment.id" class="q-px-none">
              <q-item-section avatar>
                <q-icon name="draft" color="grey-7" />
              </q-item-section>
              <q-item-section>
                <q-item-label class="ellipsis">{{ attachment.displayName }}</q-item-label>
                <q-item-label caption>{{ format.humanStorageSize(attachment.size) }}</q-item-label>
              </q-item-section>
              <q-item-section side>
                <CommonBtn flat dense icon="download" :loading="downloadingAttachmentIds.has(attachment.id)"
                  :tooltip="t('sendDetail.downloadAttachment')" @click="onAttachmentDownloadClick(attachment)" />
              </q-item-section>
            </q-item>
          </q-list>
          <div v-else class="text-caption text-grey-7 q-py-xs">{{ t('sendDetail.noAttachments') }}</div>
        </q-card-section>
      </template>
    </q-card>
  </q-dialog>
</template>

<script lang="ts" setup>
import { format, useDialogPluginComponent } from 'quasar'
import {
  getSendingItemDetail,
  type IEmailAddress,
  type ISendingItemAttachment,
  type ISendingItemDetail
} from 'src/api/sendingItem'
import CommonBtn from 'src/components/quasarWrapper/buttons/CommonBtn.vue'
import { useFileUsageDownload } from 'src/compositions/useFileUsageDownload'
import { formatDate } from 'src/utils/format'
import { useI18n } from 'vue-i18n'

/** 展示当前用户已发送邮件的完整只读内容。 */
defineOptions({ name: 'SendingItemDetailDialog' })

const props = defineProps<{ sendingItemId: number }>()
defineEmits([...useDialogPluginComponent.emits])

const { dialogRef, onDialogCancel, onDialogHide } = useDialogPluginComponent()
const { t } = useI18n()
const { downloadFileUsage } = useFileUsageDownload()
const emailDetail = ref<ISendingItemDetail>()
const isLoading = ref(true)
const hasLoadFailed = ref(false)
const downloadingAttachmentIds = ref(new Set<number>())

const emailDocument = computed(() => createSandboxedEmailDocument(emailDetail.value?.content ?? ''))

function formatAddresses(addresses: IEmailAddress[]): string {
  if (addresses.length === 0) return t('sendDetail.none')
  return addresses.map(address => {
    if (!address.name || address.name === address.email) return address.email
    return `${address.name} <${address.email}>`
  }).join('; ')
}

function createSandboxedEmailDocument(content: string): string {
  if (!content) return ''

  const emailDocument = new DOMParser().parseFromString(content, 'text/html')
  Array.from(emailDocument.querySelectorAll('script, form, iframe, object, embed, meta[http-equiv="refresh" i]'))
    .forEach(element => element.parentNode?.removeChild(element))
  Array.from(emailDocument.querySelectorAll('base'))
    .forEach(element => element.parentNode?.removeChild(element))

  const linkTarget = emailDocument.createElement('base')
  linkTarget.target = '_blank'
  emailDocument.head.prepend(linkTarget)

  const responsiveStyles = emailDocument.createElement('style')
  responsiveStyles.textContent = 'html,body{max-width:100%;overflow-wrap:anywhere}img,video{max-width:100%;height:auto}'
  emailDocument.head.append(responsiveStyles)
  return `<!doctype html>${emailDocument.documentElement.outerHTML}`
}

async function onLoadEmailDetail(): Promise<void> {
  isLoading.value = true
  hasLoadFailed.value = false
  try {
    const { data } = await getSendingItemDetail(props.sendingItemId)
    emailDetail.value = data
  } catch {
    hasLoadFailed.value = true
  } finally {
    isLoading.value = false
  }
}

async function onAttachmentDownloadClick(attachment: ISendingItemAttachment): Promise<void> {
  if (downloadingAttachmentIds.value.has(attachment.id)) return

  downloadingAttachmentIds.value = new Set(downloadingAttachmentIds.value).add(attachment.id)
  try {
    await downloadFileUsage(attachment)
  } finally {
    const remainingIds = new Set(downloadingAttachmentIds.value)
    remainingIds.delete(attachment.id)
    downloadingAttachmentIds.value = remainingIds
  }
}

onMounted(() => {
  void onLoadEmailDetail()
})
</script>

<style lang="scss" scoped>
.email-detail-dialog {
  width: min(960px, 94vw);
  height: min(90vh, 860px);
  max-width: 94vw;
}

.email-detail-dialog__subject,
.email-detail-dialog__metadata-value {
  overflow-wrap: anywhere;
}

.email-detail-dialog__state {
  flex: 1;
  min-height: 320px;
}

.email-detail-dialog__metadata {
  display: grid;
  gap: 6px;
}

.email-detail-dialog__metadata-row {
  display: grid;
  grid-template-columns: 116px minmax(0, 1fr);
  gap: 12px;
  align-items: start;
}

.email-detail-dialog__metadata-label {
  display: flex;
  gap: 6px;
  align-items: center;
  color: $grey-7;
}

.email-detail-dialog__body {
  flex: 1;
  min-height: 220px;
}

.email-detail-dialog__section-title {
  display: flex;
  align-items: center;
  font-weight: 500;
}

.email-detail-dialog__frame {
  flex: 1;
  width: 100%;
  min-height: 220px;
  border: 0;
  background: white;
}

.email-detail-dialog__empty {
  flex: 1;
  min-height: 220px;
}

.email-detail-dialog__attachments {
  max-height: 144px;
  overflow-y: auto;
}

@media (max-width: 599px) {
  .email-detail-dialog {
    width: 100vw;
    height: 100dvh;
    max-width: 100vw;
    border-radius: 0;
  }

  .email-detail-dialog__metadata-row {
    grid-template-columns: 1fr;
    gap: 2px;
  }
}
</style>
