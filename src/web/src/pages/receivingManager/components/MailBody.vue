<template>
  <div class="mail-body">
    <q-spinner v-if="loading" color="primary" />
    <iframe v-else-if="documentHtml" :title="t('pages.receivingManagement.mailBody')" :srcdoc="documentHtml"
      sandbox="allow-popups allow-popups-to-escape-sandbox" />
    <div v-else class="text-body2 text-grey-7 whitespace-pre-wrap">{{ content?.textBody || t('pages.receivingManagement.noBody') }}</div>
  </div>
</template>

<script setup lang="ts">
import { getMailContent, type IMailContent } from 'src/api/mailConversation'
import { useI18n } from 'vue-i18n'

const props = defineProps<{ messageId: number }>()
const { t } = useI18n()
const loading = ref(false)
const content = ref<IMailContent>()
const documentHtml = computed(() => content.value?.htmlBody ? sanitizeDocument(content.value.htmlBody) : '')

function sanitizeDocument (html: string) {
  const documentNode = new DOMParser().parseFromString(html, 'text/html')
  documentNode.querySelectorAll('script, form, iframe, object, embed, meta[http-equiv="refresh" i]').forEach(node => node.remove())
  documentNode.querySelectorAll<HTMLElement>('*').forEach(node => {
    Array.from(node.attributes).filter(attribute => attribute.name.toLowerCase().startsWith('on')).forEach(attribute => node.removeAttribute(attribute.name))
  })
  documentNode.head.insertAdjacentHTML('beforeend', '<style>html,body{margin:0;padding:0;color:#263238;font:14px/1.5 Arial,sans-serif;overflow-wrap:anywhere}img{max-width:100%;height:auto}</style>')
  return `<!doctype html>${documentNode.documentElement.outerHTML}`
}

onMounted(async () => {
  loading.value = true
  try { content.value = (await getMailContent(props.messageId)).data } finally { loading.value = false }
})
</script>

<style scoped lang="scss">
.mail-body { min-height: 52px; }
iframe { display: block; width: 100%; min-height: 180px; border: 0; background: white; }
.whitespace-pre-wrap { white-space: pre-wrap; }
</style>
