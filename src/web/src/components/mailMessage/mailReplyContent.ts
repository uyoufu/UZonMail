/** Creates the escaped HTML block inserted for a manually selected mail quote. */
export function createManualQuoteHtml(selectedText: string): string {
  return `<blockquote>${toHtmlText(selectedText)}</blockquote><p><br></p>`
}

/** Creates the standard reply subject while avoiding repeated reply prefixes. */
export function createReplySubject(subject?: string): string {
  if (!subject) return 'Re: '
  return subject.startsWith('Re:') ? subject : `Re: ${subject}`
}

/** Determines whether an editor HTML value contains meaningful text content. */
export function hasEditorContent(html: string): boolean {
  return new DOMParser().parseFromString(html, 'text/html').body.textContent?.trim().length !== 0
}

function toHtmlText(text: string): string {
  return escapeHtml(text).replace(/\r?\n/g, '<br>')
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
