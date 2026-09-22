/** Stores mail-targeted drafts and the last active target for each containing scope. */
export interface IMailMessageDraftCache<TDraft> {
  getDraft(messageId: number): TDraft | undefined
  saveDraft(messageId: number, draft: TDraft): void
  removeDraft(messageId: number): void
  getLastMessageId(scopeId: number): number | undefined
  saveLastMessageId(scopeId: number, messageId: number): void
  removeLastMessageId(scopeId: number, messageId?: number): void
}

/**
 * Creates an in-memory draft cache whose content is keyed strictly by mail ID.
 * Scope IDs only retain the last active mail ID used to restore a view.
 */
export function useMailMessageDraftCache<TDraft>(copyDraft: (draft: TDraft) => TDraft): IMailMessageDraftCache<TDraft> {
  const draftsByMessageId = new Map<number, TDraft>()
  const lastMessageIdByScopeId = new Map<number, number>()

  function getDraft(messageId: number): TDraft | undefined {
    const draft = draftsByMessageId.get(messageId)
    return draft ? copyDraft(draft) : undefined
  }

  function saveDraft(messageId: number, draft: TDraft): void {
    draftsByMessageId.set(messageId, copyDraft(draft))
  }

  function removeDraft(messageId: number): void {
    draftsByMessageId.delete(messageId)
  }

  function getLastMessageId(scopeId: number): number | undefined {
    return lastMessageIdByScopeId.get(scopeId)
  }

  function saveLastMessageId(scopeId: number, messageId: number): void {
    lastMessageIdByScopeId.set(scopeId, messageId)
  }

  function removeLastMessageId(scopeId: number, messageId?: number): void {
    if (messageId === undefined || lastMessageIdByScopeId.get(scopeId) === messageId) {
      lastMessageIdByScopeId.delete(scopeId)
    }
  }

  return {
    getDraft,
    saveDraft,
    removeDraft,
    getLastMessageId,
    saveLastMessageId,
    removeLastMessageId
  }
}
