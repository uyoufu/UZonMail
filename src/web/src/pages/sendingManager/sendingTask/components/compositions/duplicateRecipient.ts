/** Excel 行中的收件人字段。 */
export interface IEmailDataRecipientRow {
  inbox?: unknown,
  inboxName?: unknown
}

/** 重复收件人的汇总信息。 */
export interface IDuplicateRecipientSummary {
  inbox: string,
  inboxName: string,
  sendingCount: number
}

/**
 * 获取 Excel 中重复收件人的汇总结果。
 * 收件邮箱会去除首尾空格并忽略大小写，避免前端展示与后端校验规则不一致。
 */
export function getDuplicateRecipientSummaries (
  emailDataRows: IEmailDataRecipientRow[]
): IDuplicateRecipientSummary[] {
  const recipients = new Map<string, IDuplicateRecipientSummary>()

  for (const emailDataRow of emailDataRows) {
    const inbox = typeof emailDataRow.inbox === 'string' ? emailDataRow.inbox.trim() : ''
    if (!inbox) continue

    const duplicateKey = inbox.toLowerCase()
    const inboxName = typeof emailDataRow.inboxName === 'string' ? emailDataRow.inboxName.trim() : ''
    const existingRecipient = recipients.get(duplicateKey)
    if (existingRecipient) {
      existingRecipient.sendingCount += 1
      if (!existingRecipient.inboxName && inboxName) {
        existingRecipient.inboxName = inboxName
      }
      continue
    }

    recipients.set(duplicateKey, {
      inbox,
      inboxName,
      sendingCount: 1
    })
  }

  return [...recipients.values()]
    .filter(recipient => recipient.sendingCount > 1)
    .map(recipient => ({
      ...recipient,
      inboxName: recipient.inboxName || recipient.inbox
    }))
}
