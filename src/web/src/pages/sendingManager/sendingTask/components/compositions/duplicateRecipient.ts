/** Excel 行中的收件人字段。 */
export interface IEmailDataRecipientRow {
  recipientEmail?: unknown,
  recipientName?: unknown
}

/** 重复收件人的汇总信息。 */
export interface IDuplicateRecipientSummary {
  recipientEmail: string,
  recipientName: string,
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
    const recipientEmail = typeof emailDataRow.recipientEmail === 'string' ? emailDataRow.recipientEmail.trim() : ''
    if (!recipientEmail) continue

    const duplicateKey = recipientEmail.toLowerCase()
    const recipientName = typeof emailDataRow.recipientName === 'string' ? emailDataRow.recipientName.trim() : ''
    const existingRecipient = recipients.get(duplicateKey)
    if (existingRecipient) {
      existingRecipient.sendingCount += 1
      if (!existingRecipient.recipientName && recipientName) {
        existingRecipient.recipientName = recipientName
      }
      continue
    }

    recipients.set(duplicateKey, {
      recipientEmail,
      recipientName,
      sendingCount: 1
    })
  }

  return [...recipients.values()]
    .filter(recipient => recipient.sendingCount > 1)
    .map(recipient => ({
      ...recipient,
      recipientName: recipient.recipientName || recipient.recipientEmail
    }))
}
