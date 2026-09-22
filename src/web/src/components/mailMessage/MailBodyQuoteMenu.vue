<template>
  <span ref="menuAnchor" class="mail-body-quote-menu__anchor" aria-hidden="true">
    <q-menu ref="menu" :target="menuAnchor" no-parent-event touch-position no-focus @hide="onMenuHide">
      <q-list dense class="q-pa-xs">
        <q-item clickable @click="onInsertSelectionQuote">
          <q-item-section avatar><q-icon name="format_quote" /></q-item-section>
          <q-item-section>{{ t('components.mailBodyQuoteMenu.quoteSelection') }}</q-item-section>
        </q-item>
      </q-list>
    </q-menu>
  </span>
</template>

<script setup lang="ts">
import type { QMenu } from 'quasar'
import type { IMailMessage } from 'src/api/mailConversation'
import type { IMailBodySelectionContext } from './mailBodyInteraction'
import { useI18n } from 'vue-i18n'

interface ISelectedMailText {
  message: IMailMessage
  selectedText: string
}

const emit = defineEmits<{
  'insert-selection-quote': [message: IMailMessage, selectedText: string]
}>()
const { t } = useI18n()
const menu = ref<QMenu>()
const menuAnchor = ref<HTMLElement>()
const selectedMailText = ref<ISelectedMailText>()

/** Opens the menu only for a validated selected-text context from a specific message. */
function showForSelection(message: IMailMessage, context: IMailBodySelectionContext): void {
  const selectedText = context.selectedText.trim()
  if (!selectedText || !menu.value) return

  selectedMailText.value = { message, selectedText }
  menu.value.show(new MouseEvent('contextmenu', {
    clientX: context.clientX,
    clientY: context.clientY
  }))
}

/** Hides the menu and discards the selected-text context. */
function hide(): void {
  selectedMailText.value = undefined
  menu.value?.hide()
}

function onMenuHide(): void {
  selectedMailText.value = undefined
}

function onInsertSelectionQuote(): void {
  const selection = selectedMailText.value
  if (!selection) return

  hide()
  emit('insert-selection-quote', selection.message, selection.selectedText)
}

defineExpose({ showForSelection, hide })
</script>

<style scoped lang="scss">
.mail-body-quote-menu__anchor {
  position: fixed;
  top: 0;
  left: 0;
  width: 0;
  height: 0;
  pointer-events: none;
}
</style>
