import type { IMailMessage } from 'src/api/mailConversation'
import type { Ref } from 'vue'

export interface IMailBodyQuoteSelectionContext {
  selectedText: string
  clientX: number
  clientY: number
}

interface IReplyComposerController {
  insertManualQuote: (message: IMailMessage, selectedText: string) => Promise<void>
}

interface IQuoteMenuController {
  show: (event?: Event) => void
  hide: () => void
}

interface ISelectedMailText {
  message: IMailMessage
  selectedText: string
}

/** 管理邮件正文选区的引用菜单，并将固定的引用动作交给回复编辑器。 */
export function useMailBodyQuoteMenu(replyComposer: Ref<IReplyComposerController | undefined>) {
  const quoteMenu = ref<IQuoteMenuController>()
  const selectedMailText = ref<ISelectedMailText>()

  function onQuoteSelection(message: IMailMessage, context: IMailBodyQuoteSelectionContext) {
    const selectedText = context.selectedText.trim()
    if (!selectedText) return

    selectedMailText.value = { message, selectedText }
    quoteMenu.value?.show(new MouseEvent('contextmenu', {
      clientX: context.clientX,
      clientY: context.clientY
    }))
  }

  async function onInsertSelectionQuote() {
    const selection = selectedMailText.value
    if (!selection) return

    quoteMenu.value?.hide()
    await replyComposer.value?.insertManualQuote(selection.message, selection.selectedText)
    selectedMailText.value = undefined
  }

  return { quoteMenu, onInsertSelectionQuote, onQuoteSelection }
}
