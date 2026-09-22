/** Identifies interaction messages posted from an isolated mail-body iframe. */
export const MailBodyInteractionType = {
  pointerDown: 'uzonmail:mail-body-pointerdown',
  quoteSelectionContextMenu: 'uzonmail:mail-body-selection-contextmenu'
} as const

/** Describes the parent-document coordinates and text of a mail-body selection. */
export interface IMailBodySelectionContext {
  selectedText: string
  clientX: number
  clientY: number
}

interface IMailBodyPointerDownPayload {
  type: typeof MailBodyInteractionType.pointerDown
  channelId: string
}

interface IMailBodyQuoteSelectionPayload extends IMailBodySelectionContext {
  type: typeof MailBodyInteractionType.quoteSelectionContextMenu
  channelId: string
}

/** A validated payload emitted by the isolated mail-body document. */
export type MailBodyInteractionPayload = IMailBodyPointerDownPayload | IMailBodyQuoteSelectionPayload

/** Injects the small interaction bridge required by a sanitized mail-body document. */
export function createMailBodyInteractionBridge(documentNode: Document, channelId: string): HTMLScriptElement {
  const bridge = documentNode.createElement('script')
  bridge.textContent = `
    document.addEventListener('pointerdown', function () {
      window.parent.postMessage({
        type: '${MailBodyInteractionType.pointerDown}',
        channelId: '${channelId}'
      }, '*');
    });

    document.addEventListener('contextmenu', function (event) {
      var selection = window.getSelection();
      if (!selection || selection.isCollapsed || selection.rangeCount === 0) return;

      var target = event.target;
      var selectedRange = selection.getRangeAt(0);
      if (!(target instanceof Node) || !selectedRange.intersectsNode(target)) return;

      var selectedText = selection.toString().trim();
      if (!selectedText) return;

      event.preventDefault();
      window.parent.postMessage({
        type: '${MailBodyInteractionType.quoteSelectionContextMenu}',
        channelId: '${channelId}',
        selectedText: selectedText,
        clientX: event.clientX,
        clientY: event.clientY
      }, '*');
    });
  `
  return bridge
}

/** Checks a cross-document payload before the parent emits a mail-body interaction. */
export function isMailBodyInteractionPayload(value: unknown, channelId: string): value is MailBodyInteractionPayload {
  if (!value || typeof value !== 'object') return false

  const payload = value as {
    type?: string
    channelId?: string
    selectedText?: unknown
    clientX?: unknown
    clientY?: unknown
  }
  if (payload.channelId !== channelId) return false
  if (payload.type === MailBodyInteractionType.pointerDown) return true

  return payload.type === MailBodyInteractionType.quoteSelectionContextMenu
    && typeof payload.selectedText === 'string'
    && payload.selectedText.length > 0
    && typeof payload.clientX === 'number'
    && typeof payload.clientY === 'number'
}
