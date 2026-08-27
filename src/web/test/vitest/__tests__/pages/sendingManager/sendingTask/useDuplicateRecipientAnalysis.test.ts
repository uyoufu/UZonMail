import { describe, expect, it } from 'vitest'
import { getDuplicateRecipientSummaries } from 'src/pages/sendingManager/sendingTask/components/compositions/duplicateRecipient'

describe('getDuplicateRecipientSummaries', () => {
  it('groups recipient addresses after trimming and ignoring case', () => {
    const results = getDuplicateRecipientSummaries([
      { recipientEmail: ' Alice@Example.com ', recipientName: 'Alice' },
      { recipientEmail: 'alice@example.com', recipientName: '' },
      { recipientEmail: 'bob@example.com', recipientName: 'Bob' }
    ])

    expect(results).toEqual([
      {
        recipientEmail: 'Alice@Example.com',
        recipientName: 'Alice',
        sendingCount: 2
      }
    ])
  })

  it('uses the first non-empty name and excludes recipients sent once', () => {
    const results = getDuplicateRecipientSummaries([
      { recipientEmail: 'alice@example.com' },
      { recipientEmail: 'alice@example.com', recipientName: 'Alice' },
      { recipientEmail: 'invalid@example.com', recipientName: 'Ignored' },
      { recipientEmail: '   ' }
    ])

    expect(results).toEqual([
      {
        recipientEmail: 'alice@example.com',
        recipientName: 'Alice',
        sendingCount: 2
      }
    ])
  })
})
