import { showComponentDialog } from 'src/utils/dialog'
import DuplicateRecipientDialog from '../DuplicateRecipientDialog.vue'
import {
  getDuplicateRecipientSummaries,
  type IEmailDataRecipientRow
} from './duplicateRecipient'

export type { IDuplicateRecipientSummary } from './duplicateRecipient'

/** 创建 Excel 重复收件人详情的响应式状态与弹窗操作。 */
export function useDuplicateRecipientAnalysis (emailDataRows: Ref<IEmailDataRecipientRow[]>) {
  const duplicateRecipients = computed(() => getDuplicateRecipientSummaries(emailDataRows.value))

  /** 显示当前 Excel 的重复收件人明细。 */
  async function onShowDuplicateRecipientsClick (): Promise<void> {
    await showComponentDialog(DuplicateRecipientDialog, {
      duplicateRecipients: duplicateRecipients.value
    })
  }

  return {
    duplicateRecipients,
    onShowDuplicateRecipientsClick
  }
}
