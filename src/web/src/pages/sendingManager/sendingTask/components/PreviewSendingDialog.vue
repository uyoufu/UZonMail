<template>
  <q-dialog ref="dialogRef" @hide="onDialogHide">
    <q-card class="column justify-start q-pa-sm width-800 height-500">
      <div class="text-subtitle1 text-primary">
        {{ t('pages.sendingTask.subject') }}: {{ emailSubject }}
      </div>
      <div class="text-secondary">
        {{ t('pages.sendingTask.recipient') }}: {{ currentRecipientEmail }}
      </div>
      <q-separator class="q-mb-sm" />
      <div class="q-pb-md col hover-scroll full-width" v-html="emailBody"></div>
      <q-pagination
        v-if="pagesCount > 1"
        v-model="currentPage"
        class="self-center q-pt-sm"
        :max="pagesCount"
        :max-pages="6"
        boundary-numbers
        size="md"
        padding="0px"
      />
    </q-card>
  </q-dialog>
</template>

<script lang="ts" setup>
import logger from 'loglevel'
import { useDialogPluginComponent } from 'quasar'
import { useInstanceRequestCache } from 'src/api/base/httpCache'
import type {
  IEmailCreateInfo,
  IRecipientContactSelection,
  ISenderAccountSelection,
  ISendingItemPreview
} from 'src/api/emailSending'
import { previewSendingItem } from 'src/api/emailSending'
import { getEmailTemplateById, getEmailTemplateByIdOrName } from 'src/api/emailTemplate'
import { getRecipientContacts } from 'src/api/recipientContacts'
import { getEmailAccounts } from 'src/api/emailAccounts'
import { t } from 'src/i18n/helpers'

defineOptions({ name: 'PreviewSendingDialog' })
defineEmits([...useDialogPluginComponent.emits])
const { dialogRef, onDialogHide } = useDialogPluginComponent()

const props = defineProps({
  emailCreateInfo: { type: Object as PropType<IEmailCreateInfo>, required: true }
})

const currentPage = ref(1)
const pagesCount = ref(0)
const currentRecipientEmail = ref('')
const emailBody = ref('')
const emailSubject = ref('')
const recipients = ref<IRecipientContactSelection[]>([])
const senderAccounts = ref<ISenderAccountSelection[]>([])
const isInitialized = ref(false)
const cacheKey = useInstanceRequestCache()

const subjects = props.emailCreateInfo.subjects
  .replace(/;|；/gm, '\n')
  .split('\n')
  .filter(Boolean)

onMounted(async () => {
  const userDataRecipients = props.emailCreateInfo.data
    .filter(row => typeof row.recipientEmail === 'string' && row.recipientEmail)
    .map(row => ({
      email: String(row.recipientEmail),
      name: typeof row.recipientName === 'string' ? row.recipientName : undefined
    }))

  let recipientCandidates = [...props.emailCreateInfo.recipients, ...userDataRecipients]
  const recipientGroupIds = new Set(props.emailCreateInfo.recipientContactGroups.map(group => group.id))
  if (recipientGroupIds.size > 0) {
    const { data: persistedRecipients } = await getRecipientContacts()
    recipientCandidates = recipientCandidates.concat(
      persistedRecipients.filter(recipient => recipientGroupIds.has(recipient.emailGroupId))
    )
  }
  recipients.value = uniqueByEmail(recipientCandidates)

  let senderCandidates = [...props.emailCreateInfo.senderAccounts]
  const senderGroupIds = new Set(props.emailCreateInfo.senderAccountGroups.map(group => group.id))
  if (senderGroupIds.size > 0) {
    const { data: persistedAccounts } = await getEmailAccounts()
    senderCandidates = senderCandidates.concat(
      persistedAccounts
        .filter(account => account.sender && senderGroupIds.has(account.emailGroupId))
        .map(account => ({
          id: account.sender!.id,
          email: account.email,
          name: account.name,
          description: account.description
        }))
    )
  }
  senderAccounts.value = uniqueByEmail(senderCandidates)

  pagesCount.value = recipients.value.length
  isInitialized.value = true
  await refreshPreview()
})

watch(currentPage, refreshPreview)

function uniqueByEmail<TAccount extends { email: string }> (accounts: TAccount[]): TAccount[] {
  const accountsByEmail = new Map<string, TAccount>()
  for (const account of accounts) {
    if (!account.email) continue
    accountsByEmail.set(account.email.trim().toLowerCase(), account)
  }
  return [...accountsByEmail.values()]
}

async function refreshPreview () {
  if (!isInitialized.value || recipients.value.length === 0) return

  const recipientIndex = currentPage.value - 1
  const recipientEmail = recipients.value[recipientIndex]!.email
  const userData = props.emailCreateInfo.data.find(row => row.recipientEmail === recipientEmail)
  emailSubject.value = typeof userData?.subject === 'string'
    ? userData.subject
    : subjects[recipientIndex % subjects.length] || ''
  currentRecipientEmail.value = recipientEmail
  emailBody.value = await resolveEmailBody(recipientEmail, recipientIndex)

  const previewRequest: ISendingItemPreview = {
    recipientContact: recipientEmail,
    senderAccount: senderAccounts.value.length > 0
      ? senderAccounts.value[recipientIndex % senderAccounts.value.length]!.email
      : '',
    subject: emailSubject.value,
    body: emailBody.value,
    data: userData || {}
  }
  logger.debug('[PreviewSendingDialog] preview request', previewRequest)
  const { data: previewResult } = await previewSendingItem(previewRequest)
  emailSubject.value = previewResult.subject
  emailBody.value = previewResult.body
}

async function resolveEmailBody (recipientEmail: string, recipientIndex: number): Promise<string> {
  const userData = props.emailCreateInfo.data.find(row => row.recipientEmail === recipientEmail)
  let templateContent = ''
  if (typeof userData?.body === 'string') {
    templateContent = userData.body
  } else if (userData?.templateId || userData?.templateName) {
    const { data: template } = await getEmailTemplateByIdOrName(userData.templateId, userData.templateName, cacheKey)
    templateContent = template.content
  }

  if (!templateContent) templateContent = props.emailCreateInfo.body
  if (!templateContent && props.emailCreateInfo.templates.length > 0) {
    const templateIndex = recipientIndex % props.emailCreateInfo.templates.length
    const selectedTemplate = props.emailCreateInfo.templates[templateIndex]!
    const { data: template } = await getEmailTemplateById(selectedTemplate.id as number, cacheKey)
    templateContent = template.content
  }

  return applyVariablesToTemplate(userData, templateContent) || t('pages.sendingTask.emptyBody')
}

function applyVariablesToTemplate (variables?: Record<string, unknown>, templateContent = ''): string {
  if (!variables) return templateContent
  let resolvedContent = templateContent
  for (const [variableName, variableValue] of Object.entries(variables)) {
    const variablePattern = new RegExp(`{{\\s*${variableName}\\s*}}`, 'gm')
    const replacementValue = typeof variableValue === 'string'
      ? variableValue
      : JSON.stringify(variableValue) ?? ''
    resolvedContent = resolvedContent.replace(variablePattern, replacementValue)
  }
  return resolvedContent
}
</script>
