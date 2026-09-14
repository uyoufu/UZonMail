<template>
  <section class="mail-reply-editor q-pa-sm">
    <div class="row items-center q-gutter-sm q-mb-sm">
      <q-input v-model="subject" dense outlined hide-bottom-space class="col min-width-0"
        :label="t('pages.receivingManagement.subject')" />
      <slot name="actions" />
    </div>

    <div class="mail-reply-editor__content relative-position">
      <q-editor ref="editorRef" v-model="htmlBody" min-height="110px" :placeholder="t('pages.receivingManagement.writeReply')"
        :toolbar="editorToolbar" />
      <CommonBtn icon="send" class="mail-reply-editor__send" :loading="isSending"
        :tooltip="t('pages.receivingManagement.send')" @click="emit('submit')" />
    </div>
  </section>
</template>

<script setup lang="ts">
import type { QEditor } from 'quasar'
import CommonBtn from 'src/components/buttons/CommonBtn.vue'
import { useI18n } from 'vue-i18n'

defineProps<{
  isSending: boolean
}>()
const emit = defineEmits<{
  submit: []
}>()
const subject = defineModel<string>('subject', { required: true })
const htmlBody = defineModel<string>('htmlBody', { required: true })
const { t } = useI18n()
const editorRef = ref<QEditor>()
const editorToolbar = [['bold', 'italic', 'underline'], ['unordered', 'ordered'], ['link'], ['undo', 'redo']]

function focus(): void {
  editorRef.value?.focus()
}

function insertHtml(html: string): void {
  editorRef.value?.runCmd('insertHTML', html)
}

defineExpose({ focus, insertHtml })
</script>

<style lang="scss" scoped>
.mail-reply-editor__content :deep(.q-editor__content) {
  padding-right: 52px;
  padding-bottom: 52px;
}

.mail-reply-editor__send {
  position: absolute;
  right: 12px;
  bottom: 12px;
  z-index: 1;
}
</style>
