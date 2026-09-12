import { beforeEach, describe, expect, it } from 'vitest'
import type { IReceivingAccount } from 'src/api/mailConversation'
import {
  RECEIVING_ACCOUNT_SELECTION_STORAGE_KEY,
  restoreReceivingAccountSelection,
  saveReceivingAccountSelection
} from 'src/pages/receivingManager/receivingAccountPreference'

const receivingAccounts: IReceivingAccount[] = [
  {
    emailAccountId: 1,
    receivingAccountId: 11,
    email: 'first@example.com',
    protocol: 1,
    status: 1,
    isSupported: true
  },
  {
    emailAccountId: 2,
    receivingAccountId: 22,
    email: 'unsupported@example.com',
    protocol: 1,
    status: 1,
    isSupported: false
  }
]

describe('receiving account preference', () => {
  beforeEach(() => window.localStorage.clear())

  it('uses the first supported account when there is no saved selection', () => {
    expect(restoreReceivingAccountSelection(receivingAccounts)).toBe(1)
  })

  it('restores the supported saved account', () => {
    saveReceivingAccountSelection(1)

    expect(restoreReceivingAccountSelection(receivingAccounts)).toBe(1)
  })

  it('persists the all-accounts choice', () => {
    saveReceivingAccountSelection(undefined)

    expect(restoreReceivingAccountSelection(receivingAccounts)).toBeUndefined()
  })

  it('removes an unavailable saved account and falls back to the first supported account', () => {
    saveReceivingAccountSelection(2)

    expect(restoreReceivingAccountSelection(receivingAccounts)).toBe(1)
    expect(window.localStorage.getItem(RECEIVING_ACCOUNT_SELECTION_STORAGE_KEY)).toBeNull()
  })
})
