import dayjs from 'dayjs'
import type { IMailContent, IMailMessage } from 'src/api/mailConversation'
import { createManualQuoteHtml } from 'src/components/mailMessage/mailReplyContent'
import { formatMailAddressRoute } from './mailMessagePresentation'

export { createManualQuoteHtml }

export function createFullMessageQuoteHtml(message: IMailMessage, content: IMailContent): string {
  const plainText = getMailContentPlainText(content)
  const metadata = `${dayjs(message.occurredAtUtc).format('YYYY-MM-DD HH:mm')} ${formatMailAddressRoute(message)}`
  return `<blockquote><p>${toHtmlText(metadata)}</p>${toHtmlText(plainText)}</blockquote>`
}

/** 将邮件内容转换为可安全显示在引用预览中的纯文本。 */
export function getMailContentPlainText(content: IMailContent): string {
  return content.textBody || htmlToPlainText(content.htmlBody || '')
}

function htmlToPlainText(html: string): string {
  const documentNode = new DOMParser().parseFromString(html, 'text/html')
  return documentNode.body.textContent || ''
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
