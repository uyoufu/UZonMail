import { describe, expect, it } from 'vitest'
import { getDuplicateRecipientSummaries } from 'src/pages/sendingManager/sendingTask/components/compositions/duplicateRecipient'

describe('getDuplicateRecipientSummaries', () => {
  it('groups recipient addresses after trimming and ignoring case', () => {
    const results = getDuplicateRecipientSummaries([
      { inbox: ' Alice@Example.com ', inboxName: 'Alice' },
      { inbox: 'alice@example.com', inboxName: '' },
      { inbox: 'bob@example.com', inboxName: 'Bob' }
    ])

    expect(results).toEqual([
      {
        inbox: 'Alice@Example.com',
        inboxName: 'Alice',
        sendingCount: 2
      }
    ])
  })

  it('uses the first non-empty name and excludes recipients sent once', () => {
    const results = getDuplicateRecipientSummaries([
      { inbox: 'alice@example.com' },
      { inbox: 'alice@example.com', inboxName: 'Alice' },
      { inbox: 'invalid@example.com', inboxName: 'Ignored' },
      { inbox: '   ' }
    ])

    expect(results).toEqual([
      {
        inbox: 'alice@example.com',
        inboxName: 'Alice',
        sendingCount: 2
      }
    ])
  })
})
