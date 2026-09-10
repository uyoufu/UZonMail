import { describe, expect, it } from 'vitest'
import { MailMessageDirection, type IMailMessage } from 'src/api/mailConversation'
import { getMessagePreview, formatMailAddressRoute, formatMailEmailRoute } from 'src/pages/receivingManager/components/mailMessagePresentation'
import { createFullMessageQuoteHtml, createManualQuoteHtml } from 'src/pages/receivingManager/components/mailQuote'

const incomingMessage: IMailMessage = {
  id: 1,
  direction: MailMessageDirection.Incoming,
  subject: 'Project update',
  previewText: 'The release is ready.',
  occurredAtUtc: '2026-09-10T08:30:00Z',
  isRead: false,
  from: [{ email: 'sender@example.com', displayName: 'Sender' }],
  to: [{ email: 'recipient@example.com', displayName: 'Recipient' }],
  cc: [],
  attachments: []
}

describe('receiving manager presentation', () => {
  it('renders the complete sender to recipient route and sync preview', () => {
    expect(formatMailAddressRoute(incomingMessage)).toBe('Sender -> Recipient')
    expect(formatMailEmailRoute(incomingMessage)).toBe('sender@example.com -> recipient@example.com')
    expect(getMessagePreview(incomingMessage, 'No preview')).toBe('The release is ready.')
  })

  it('escapes manually selected text before inserting it as a quote', () => {
    expect(createManualQuoteHtml('<script>alert(1)</script>\nSecond line'))
      .toBe('<blockquote>&lt;script&gt;alert(1)&lt;/script&gt;<br>Second line</blockquote><p><br></p>')
  })

  it('turns full message HTML into plain escaped quoted content', () => {
    const quote = createFullMessageQuoteHtml(incomingMessage, {
      conversationMessageId: incomingMessage.id,
      htmlBody: '<p>Hello <strong>team</strong></p>',
      attachments: []
    })

    expect(quote).toContain('Sender -&gt; Recipient')
    expect(quote).toContain('Hello team')
    expect(quote).not.toContain('<strong>')
  })
})
