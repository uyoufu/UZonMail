<template>
  <div class="relative-position full-width height-300">
    <iframe v-if="documentHtml" ref="bodyFrame" :title="t('pages.receivingManagement.mailBody')" :srcdoc="documentHtml"
      sandbox="allow-popups allow-popups-to-escape-sandbox allow-scripts" class="block full-width full-height no-border bg-white" />
    <div v-else-if="loadError" class="full-height column items-center justify-center q-gutter-sm text-grey-7">
      <span class="text-caption">{{ t('pages.receivingManagement.mailBodyLoadFailed') }}</span>
      <CommonBtn icon="refresh" flat :tooltip="t('global.retry')" @click="onRetryLoad" />
    </div>
    <q-inner-loading :showing="isLoading" color="primary" />
  </div>
</template>

<script setup lang="ts">
import CommonBtn from 'src/components/buttons/CommonBtn.vue'
import { getMailContent, type IMailContent } from 'src/api/mailConversation'
import { useI18n } from 'vue-i18n'

const mailBodySelectionEvent = 'uzonmail:mail-body-selection-contextmenu'

interface IMailBodySelectionContext {
  selectedText: string
  clientX: number
  clientY: number
}

interface IFrameSelectionContextPayload extends IMailBodySelectionContext {
  type: typeof mailBodySelectionEvent
  channelId: string
}

const props = defineProps<{ messageId: number }>()
const emit = defineEmits<{
  'quote-selection-contextmenu': [context: IMailBodySelectionContext]
}>()
const { t } = useI18n()
const bodyFrame = ref<HTMLIFrameElement>()
const content = ref<IMailContent>()
const isLoading = ref(false)
const loadError = ref(false)
const bodyChannelId = `mail-body-${Date.now()}-${Math.random().toString(36).slice(2)}`
const documentHtml = computed(() => content.value ? createDocumentHtml(content.value) : '')

async function loadContent() {
  isLoading.value = true
  loadError.value = false
  try {
    content.value = (await getMailContent(props.messageId)).data
  } catch {
    content.value = undefined
    loadError.value = true
  } finally {
    isLoading.value = false
  }
}

function onRetryLoad() {
  void loadContent()
}

function createDocumentHtml(mailContent: IMailContent): string {
  const sourceHtml = mailContent.htmlBody || createPlainTextHtml(mailContent.textBody || t('pages.receivingManagement.noBody'))
  const documentNode = new DOMParser().parseFromString(sourceHtml, 'text/html')
  documentNode.querySelectorAll('script, form, iframe, object, embed, meta[http-equiv="refresh" i]').forEach(node => node.remove())
  documentNode.querySelectorAll<HTMLElement>('*').forEach(node => {
    Array.from(node.attributes).forEach(attribute => {
      const attributeName = attribute.name.toLowerCase()
      if (attributeName.startsWith('on') || isUnsafeUrlAttribute(attributeName, attribute.value)) node.removeAttribute(attribute.name)
    })
  })
  documentNode.body.append(createSelectionBridge(documentNode))
  return `<!doctype html>${documentNode.documentElement.outerHTML}`
}

function createPlainTextHtml(text: string): string {
  return `<div>${escapeHtml(text).replace(/\r?\n/g, '<br>')}</div>`
}

function createSelectionBridge(documentNode: Document): HTMLScriptElement {
  const bridge = documentNode.createElement('script')
  bridge.textContent = `
    document.addEventListener('contextmenu', function (event) {
      var selectedText = window.getSelection().toString().trim();
      if (!selectedText) return;
      event.preventDefault();
      window.parent.postMessage({
        type: '${mailBodySelectionEvent}',
        channelId: '${bodyChannelId}',
        selectedText: selectedText,
        clientX: event.clientX,
        clientY: event.clientY
      }, '*');
    });
  `
  return bridge
}

function isUnsafeUrlAttribute(attributeName: string, attributeValue: string): boolean {
  if (attributeName !== 'href' && attributeName !== 'src') return false
  return /^\s*(javascript|data:text\/html)/i.test(attributeValue)
}

function escapeHtml(text: string): string {
  return text.replace(/[&<>"']/g, character => ({
    '&': '&amp;',
    '<': '&lt;',
    '>': '&gt;',
    '"': '&quot;',
    "'": '&#39;'
  })[character] || character)
}

function onWindowMessage(event: MessageEvent<unknown>) {
  if (event.source !== bodyFrame.value?.contentWindow || !isSelectionContextPayload(event.data)) return
  emit('quote-selection-contextmenu', event.data)
}

function isSelectionContextPayload(value: unknown): value is IFrameSelectionContextPayload {
  if (!value || typeof value !== 'object') return false
  const payload = value as Partial<IFrameSelectionContextPayload>
  return payload.type === mailBodySelectionEvent
    && payload.channelId === bodyChannelId
    && typeof payload.selectedText === 'string'
    && payload.selectedText.length > 0
    && typeof payload.clientX === 'number'
    && typeof payload.clientY === 'number'
}

watch(() => props.messageId, () => {
  void loadContent()
})

onMounted(() => {
  window.addEventListener('message', onWindowMessage)
  void loadContent()
})

onBeforeUnmount(() => window.removeEventListener('message', onWindowMessage))
</script>
