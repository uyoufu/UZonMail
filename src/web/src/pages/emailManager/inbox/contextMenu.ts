import type { IInbox } from 'src/api/emailBox'
import { deleteInboxByIds, updateInbox } from 'src/api/emailBox'
import { validateInboxes } from 'src/api/pro/emailVerify'

import type { IActionContext, IContextMenuItem } from 'src/components/contextMenu/types'
import type { IPopupDialogParams } from 'src/components/lowCode/types'
import { confirmOperation, notifySuccess, notifyUntil } from 'src/utils/dialog'
import { getInboxFields } from './headerFunctions'
import { showDialog } from 'src/components/lowCode/PopupDialog'

import { translateInboxManager, translateGlobal } from 'src/i18n/helpers'
import type { deleteRowByIdType, refreshTableType } from 'src/compositions/qTableUtils'
import { usePermission } from 'src/compositions/permission'

/** 创建收件箱列表的右键菜单及其操作。 */
export function useContextMenu (deleteRowById: deleteRowByIdType<IInbox>, refreshTable: refreshTableType) {
  const { isProfession } = usePermission()
  // 更新发件箱
  async function onUpdateInbox (inbox: IInbox) {
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
    {
      name: 'validateSelected',
      label: translateGlobal('validate'),
      tooltip: translateInboxManager('validateCurrentOrSelectedInboxes'),
      onClick: onValidateInboxes,
      vif: () => isProfession.value
    },
    {
      name: 'delete',
      label: translateGlobal('delete'),
      tooltip: translateInboxManager('deleteCurrentInbox'),
      color: 'negative',
      onClick: onDeleteInbox
    }
  ])

  /** 验证当前收件箱或当前选择的收件箱。 */
  async function onValidateInboxes (_row: IInbox, { targetValues, clearSelection }: IActionContext<IInbox>) {
    const result = await notifyUntil(
      () => validateInboxes(targetValues.map(x => x.id as number)),
      translateInboxManager('validateCurrentOrSelectedInboxes'),
      translateGlobal('validate')
    )
    if (!result) return

    clearSelection()
    refreshTable()
    notifySuccess(translateGlobal('updateSuccess'))
  }


  /** 删除当前收件箱或已选择的全部收件箱。 */
  async function onDeleteInbox (
    _cursorInbox: IInbox,
    { targetValues, clearSelection }: IActionContext<IInbox>
  ) {
    const inboxes = targetValues.filter(isPersistedInbox)
    if (inboxes.length !== targetValues.length) return false

    const confirmationMessage = inboxes.length === 1
      ? translateInboxManager('isDeleteEmailOf', { email: inboxes[0]!.email })
      : translateInboxManager('isDeleteSelectedInboxes', { count: inboxes.length })
    const confirm = await confirmOperation(
      translateGlobal('deleteConfirmation'),
      confirmationMessage
    )
    if (!confirm) return

    await deleteInboxByIds(inboxes.map(inbox => inbox.objectId))

    inboxes.forEach(inbox => {
      deleteRowById(inbox.id)
    })
    clearSelection()

    notifySuccess(translateGlobal('deleteSuccess'))
  }

  function isPersistedInbox (inbox: IInbox): inbox is IInbox & { id: number, objectId: string } {
    return inbox.id !== undefined && inbox.objectId !== undefined
  }

  return { inboxContextMenuItems }
}
