/* eslint-disable @typescript-eslint/no-explicit-any */
import type { IInbox } from 'src/api/emailBox'
import { deleteInboxById, updateInbox, deleteAllDeliveredInboxesInGroup } from 'src/api/emailBox'
import { validateAllInvalidInboxes } from 'src/api/pro/emailVerify'

import type { IContextMenuItem } from 'src/components/contextMenu/types'
import type { IPopupDialogParams } from 'src/components/popupDialog/types'
import { confirmOperation, notifySuccess } from 'src/utils/dialog'
import { getInboxFields } from './headerFunctions'
import { showDialog } from 'src/components/popupDialog/PopupDialog'

import { translateInboxManager, translateGlobal } from 'src/i18n/helpers'
import type { deleteRowByIdType, refreshTableType } from 'src/compositions/qTableUtils'

export function useContextMenu (deleteRowById: deleteRowByIdType, refreshTable: refreshTableType) {
  // 更新发件箱
  async function onUpdateInbox (row: Record<string, any>) {
    const inbox = row as IInbox

    const fields = getInboxFields()
    // 修改默认值
    fields.forEach(field => {
      switch (field.name) {
        case 'email':
          field.value = inbox.email
          break
        case 'name':
          field.value = inbox.name
          break
        case 'minInboxCooldownHours':
          field.value = inbox.minInboxCooldownHours
          break
        case 'description':
          field.value = inbox.description
          break
        default:
          break
      }
    })

    // 新增发件箱
    const popupParams: IPopupDialogParams = {
      title: `${translateInboxManager('editInbox')} / ${inbox.email}`,
      fields
    }

    // 弹出对话框

    const { ok, data } = await showDialog<IInbox>(popupParams)
    if (!ok) return

    // 向服务器传递更新
    await updateInbox(inbox.id as number, data)

    // 将参数更新到 inbox 中
    Object.assign(inbox, data, { decryptedPassword: false })

    notifySuccess(translateInboxManager('updateInboxSuccess'))
  }

  const inboxContextMenuItems: ComputedRef<IContextMenuItem<IInbox>[]> = computed(() => [
    {
      name: 'edit',
      label: translateGlobal('edit'),
      tooltip: translateInboxManager('editCurrentInbox'),
      onClick: onUpdateInbox
    },
    // TODO: Not fully implemented, will enable in the future
    {
      name: 'validateSelected',
      label: translateGlobal('validate'),
      tooltip: translateInboxManager('validateCurrentOrSelectedInboxes'),
      onClick: deleteInbox,
      vif: () => false
    },
    // TODO: Not fully implemented, will enable in the future
    {
      name: 'validateAll',
      label: translateGlobal('validateMultiple'),
      tooltip: translateInboxManager('validateAllInvalidInboxesInCurrentGroup'),
      onClick: onValidateAllInvalidInboxes,
      vif: () => false
    },
    {
      name: 'delete',
      label: translateGlobal('delete'),
      tooltip: translateInboxManager('deleteCurrentInbox'),
      color: 'negative',
      onClick: deleteInbox
    },
    {
      name: 'deleteAllDelivered',
      label: translateInboxManager('ctx_deleteDelivered'),
      tooltip: translateInboxManager('ctx_deleteDeliveredInboxesInCurrentGroup'),
      color: 'negative',
      onClick: onDeleteAllDeliveredInboxes
    },
    // TODO: Not fully implemented, will enable in the future
    {
      name: 'deleteInvalid',
      label: translateInboxManager('ctx_deleteInvalid'),
      tooltip: translateInboxManager('deleteAllInvalidInboxesInCurrentGroup'),
      color: 'negative',
      onClick: deleteInbox,
      vif: () => false
    },
  ])

  // #region 验证
  async function onValidateAllInvalidInboxes (row: Record<string, any>) {
    const inbox = row as IInbox
    await validateAllInvalidInboxes(inbox.emailGroupId as number)
  }
  // #endregion


  // #region  删除
  // 删除发件箱
  async function deleteInbox (row: Record<string, any>) {
    const inbox = row as IInbox
    // 提示是否删除
    const confirm = await confirmOperation(
      translateGlobal('deleteConfirmation'),
      translateInboxManager('isDeleteEmailOf', { email: inbox.email })
    )
    if (!confirm) return

    await deleteInboxById(inbox.id as number)

    // 开始删除
    deleteRowById(inbox.id)

    notifySuccess(translateGlobal('deleteSuccess'))
  }

  // delete all delivered inboxes in current group
  async function onDeleteAllDeliveredInboxes (row: IInbox) {
    const confirm = await confirmOperation(
      translateGlobal('deleteConfirmation'),
      translateInboxManager('isDeleteAllDeliveredInboxesInCurrentGroup')
    )
    if (!confirm) return

    // start deleting
    await deleteAllDeliveredInboxesInGroup(row.emailGroupId as number)

    // 刷新当前显示
    refreshTable()

    notifySuccess(translateGlobal('deleteSuccess'))
  }
  // #endregion

  return { inboxContextMenuItems }
}
