/* eslint-disable @typescript-eslint/no-explicit-any */
import ContextMenu from 'src/components/contextMenu/ContextMenu.vue'

import type { IContextMenuItem } from 'src/components/contextMenu/types'
import { resendSendingItem } from 'src/api/emailSending'
import type { ISendingItem } from 'src/api/sendingItem'
import { SendingItemStatus, getSendingItemBody } from 'src/api/sendingItem'
import { confirmOperation, notifySuccess, showHtmlDialog2 } from 'src/utils/dialog'
import { useUserInfoStore } from 'src/stores/user'
import { useI18n } from 'vue-i18n'


export function useContextMenu () {
  const { t } = useI18n()

  const userInfoStore = useUserInfoStore()

  const sendDetailContextItems: IContextMenuItem<ISendingItem>[] = [
    {
      name: 'resent',
      label: t('sendDetail.resend'),
      tooltip: '重新发送当前邮件',
      vif: (email: Record<string, any>) => email.status === SendingItemStatus.Failed,
      onClick: resentEmail
    },
    {
      name: 'viewBody',
      label: '查看正文',
      tooltip: '查看当前发件的正文',
      vif: (email: Record<string, any>) => email.status >= SendingItemStatus.Success,
      onClick: showEmailBody
    }
  ]

  async function resentEmail (email: Record<string, any>) {
    const emailInfo = email as ISendingItem
    console.log(emailInfo)
    // 进行确认
    // eslint-disable-next-line @typescript-eslint/restrict-template-expressions
    const confirm = await confirmOperation(t('sendDetail.resend'), `即将重新发送【${emailInfo.inboxes.map(x => x.email)}】, 是否继续？`)
    if (!confirm) return

    // 重新发送
    // 向服务器请求重新发送
    await resendSendingItem(email.id, userInfoStore.smtpPasswordSecretKeys)

    // 更改本机状态为正在发送
    email.status = SendingItemStatus.Sending
    notifySuccess('正在重新发送')
  }

  async function showEmailBody (email: Record<string, any>) {
    // 当没有正文时，从服务器拉取
    if (!email.content) {
      const { data } = await getSendingItemBody(email.id)
      email.content = data
    }

    // 显示
    await showHtmlDialog2('邮件: ' + email.id, email.content)
  }

  return {
    sendDetailContextItems,
    ContextMenu
  }
}
