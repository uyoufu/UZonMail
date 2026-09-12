import type { IReceivingAccount } from 'src/api/mailConversation'

/** 收件管理页记住最近账号所使用的本地存储键。 */
export const RECEIVING_ACCOUNT_SELECTION_STORAGE_KEY = 'receiving-manager.selected-account'

const ALL_RECEIVING_ACCOUNTS_STORAGE_VALUE = 'all'

/** 读取有效的最近收件账号；无记录或失效记录回退到首个可用账号。 */
export function restoreReceivingAccountSelection(accounts: IReceivingAccount[]): number | undefined {
  if (typeof window === 'undefined') return accounts.find(account => account.isSupported)?.emailAccountId

  const storedSelection = window.localStorage.getItem(RECEIVING_ACCOUNT_SELECTION_STORAGE_KEY)
  if (storedSelection === ALL_RECEIVING_ACCOUNTS_STORAGE_VALUE) return undefined
  if (storedSelection === null) return accounts.find(account => account.isSupported)?.emailAccountId

  const selectedAccountId = Number(storedSelection)
  if (Number.isSafeInteger(selectedAccountId) && accounts.some(account => {
    return account.emailAccountId === selectedAccountId && account.isSupported
  })) {
    return selectedAccountId
  }

  if (storedSelection !== null) window.localStorage.removeItem(RECEIVING_ACCOUNT_SELECTION_STORAGE_KEY)
  return accounts.find(account => account.isSupported)?.emailAccountId
}

/** 保存当前收件账号；undefined 表示用户选择了全部账号。 */
export function saveReceivingAccountSelection(selectedAccountId: number | undefined) {
  if (typeof window === 'undefined') return

  const selectionValue = selectedAccountId === undefined
    ? ALL_RECEIVING_ACCOUNTS_STORAGE_VALUE
    : String(selectedAccountId)
  window.localStorage.setItem(RECEIVING_ACCOUNT_SELECTION_STORAGE_KEY, selectionValue)
}
