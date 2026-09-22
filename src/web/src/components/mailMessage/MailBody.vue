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
import {
  MailBodyInteractionType,
  createMailBodyInteractionBridge,
  isMailBodyInteractionPayload,
  type IMailBodySelectionContext
} from './mailBodyInteraction'
import { useI18n } from 'vue-i18n'

const props = defineProps<{ messageId: number }>()
const emit = defineEmits<{
  'quote-selection-contextmenu': [context: IMailBodySelectionContext]
  'body-pointerdown': []
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
  documentNode.documentElement.classList.add('mail-body-hover-scroll')
  documentNode.head.append(createHoverScrollStyle(documentNode))
  documentNode.body.append(createMailBodyInteractionBridge(documentNode, bodyChannelId))
  return `<!doctype html>${documentNode.documentElement.outerHTML}`
}

function createPlainTextHtml(text: string): string {
  return `<div>${escapeHtml(text).replace(/\r?\n/g, '<br>')}</div>`
}

/** Adds the local equivalent of the application hover-scroll style to the isolated document. */
function createHoverScrollStyle(documentNode: Document): HTMLStyleElement {
  const style = documentNode.createElement('style')
  style.textContent = `
    html.mail-body-hover-scroll { overflow: overlay; }
    html.mail-body-hover-scroll::-webkit-scrollbar { width: 0; height: 0; }
    html.mail-body-hover-scroll:hover::-webkit-scrollbar { width: 5px; height: 5px; background-color: transparent; }
    html.mail-body-hover-scroll::-webkit-scrollbar-thumb { background-color: transparent; border-radius: 2px; }
    html.mail-body-hover-scroll:hover::-webkit-scrollbar-thumb { background-color: #e0e0e0; }
    html.mail-body-hover-scroll::-webkit-scrollbar-track,
    html.mail-body-hover-scroll:hover::-webkit-scrollbar-track,
    html.mail-body-hover-scroll:hover::-webkit-scrollbar-track-piece { background-color: transparent; }
  `
  return style
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
  const frameElement = bodyFrame.value
  if (event.source !== frameElement?.contentWindow || !isMailBodyInteractionPayload(event.data, bodyChannelId)) return
  if (event.data.type === MailBodyInteractionType.pointerDown) {
    emit('body-pointerdown')
    return
  }

  const frameBounds = frameElement.getBoundingClientRect()
  emit('quote-selection-contextmenu', {
    selectedText: event.data.selectedText,
    clientX: frameBounds.left + event.data.clientX,
    clientY: frameBounds.top + event.data.clientY
  })
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
