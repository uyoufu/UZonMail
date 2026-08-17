import type { ISendingItem } from 'src/api/sendingItem'
import { SendingItemStatus } from 'src/api/sendingItem'
import { resendSendingItem } from 'src/api/emailSending'
import ContextMenu from 'src/components/contextMenu/ContextMenu.vue'
import { ContextMenuIcon, type IContextMenuItem } from 'src/components/contextMenu/types'
import { confirmOperation, notifySuccess, showComponentDialog } from 'src/utils/dialog'
import { useI18n } from 'vue-i18n'
import SendingItemDetailDialog from './SendingItemDetailDialog.vue'

/** 创建发件明细的行级操作菜单。 */
export function useContextMenu() {
  const { t } = useI18n()

  const sendDetailContextItems = computed<IContextMenuItem<ISendingItem>[]>(() => [
    {
      name: 'resend',
      label: t('sendDetail.resend'),
      tooltip: t('sendDetail.resendTooltip'),
      icon: ContextMenuIcon.send,
      vif: (email) => email.status === SendingItemStatus.Failed,
      onClick: onResendEmail
    },
    {
      name: 'viewEmail',
      label: t('sendDetail.viewEmail'),
      tooltip: t('sendDetail.viewEmailTooltip'),
      icon: ContextMenuIcon.visibility,
      onClick: onViewEmail
    }
  ])

  async function onResendEmail(email: ISendingItem): Promise<void> {
    const recipients = email.inboxes.map((inbox) => inbox.email).join(', ')
    const confirmed = await confirmOperation(t('sendDetail.resend'), t('sendDetail.resendConfirm', { recipients }))
    if (!confirmed) return

    await resendSendingItem(email.id)
    email.status = SendingItemStatus.Sending
    notifySuccess(t('sendDetail.resending'))
  }

  async function onViewEmail(email: ISendingItem): Promise<void> {
    await showComponentDialog(SendingItemDetailDialog, { sendingItemId: email.id })
  }

  return { sendDetailContextItems, ContextMenu }
}
