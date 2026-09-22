import { describe, expect, it } from 'vitest'
import { MailBodyInteractionType, isMailBodyInteractionPayload } from 'src/components/mailMessage/mailBodyInteraction'

describe('mail-body interaction bridge', () => {
  it('accepts only the active channel and complete selected-text payloads', () => {
    const channelId = 'mail-body-test'

    expect(isMailBodyInteractionPayload({
      type: MailBodyInteractionType.pointerDown,
      channelId
    }, channelId)).toBe(true)
    expect(isMailBodyInteractionPayload({
      type: MailBodyInteractionType.quoteSelectionContextMenu,
      channelId,
      selectedText: 'Selected text',
      clientX: 20,
      clientY: 30
    }, channelId)).toBe(true)
    expect(isMailBodyInteractionPayload({
      type: MailBodyInteractionType.quoteSelectionContextMenu,
      channelId: 'another-body',
      selectedText: 'Selected text',
      clientX: 20,
      clientY: 30
    }, channelId)).toBe(false)
    expect(isMailBodyInteractionPayload({
      type: MailBodyInteractionType.quoteSelectionContextMenu,
      channelId,
      selectedText: '',
      clientX: 20,
      clientY: 30
    }, channelId)).toBe(false)
  })
})
