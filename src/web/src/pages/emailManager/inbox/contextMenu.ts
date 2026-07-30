import type { IInbox } from 'src/api/emailBox'
import { deleteInboxByIds, InboxStatus, updateInbox, updateInboxesStatus } from 'src/api/emailBox'
import { EmailGroupType } from 'src/api/emailGroup'
import { validateInboxes } from 'src/api/pro/emailVerify'

import { ContextMenuIcon, type IActionContext, type IContextMenuItem } from 'src/components/contextMenu/types'
import type { IPopupDialogParams } from 'src/components/lowCode/types'
import { confirmOperation, notifySuccess, notifyUntil } from 'src/utils/dialog'
import { getInboxFields } from './headerFunctions'
import { showDialog } from 'src/components/lowCode/PopupDialog'

import { translateEmailGroup, translateInboxManager, translateGlobal } from 'src/i18n/helpers'
import type { deleteRowByIdType, refreshTableType } from 'src/compositions/qTableUtils'
import { usePermission } from 'src/compositions/permission'
import { useEmailBoxGroupMove } from '../components/useEmailBoxGroupMove'
import type { Ref } from 'vue'

/** 创建收件箱列表的右键菜单及其操作。 */
export function useContextMenu(
  deleteRowById: deleteRowByIdType<IInbox>,
  refreshTable: refreshTableType,
  selectedInboxes: Ref<IInbox[]>
) {
  const { isProfession } = usePermission()
  const { onMoveEmailBoxes } = useEmailBoxGroupMove<IInbox>(EmailGroupType.Inbox, refreshTable)
  // 更新发件箱
  async function onUpdateInbox(inbox: IInbox) {
    const fields = getInboxFields()
    // 修改默认值
    fields.forEach((field) => {
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
      icon: ContextMenuIcon.edit,
      onClick: onUpdateInbox
    },
    {
      name: 'moveToGroup',
      label: translateEmailGroup('moveEmailBoxes'),
      tooltip: translateEmailGroup('moveEmailBoxesToTargetGroup'),
      icon: ContextMenuIcon.driveFileMove,
      onClick: onMoveEmailBoxes
    },
    {
      name: 'validateSelected',
      label: translateGlobal('validate'),
      tooltip: translateInboxManager('validateCurrentOrSelectedInboxes'),
      icon: ContextMenuIcon.verified,
      onClick: onValidateInboxes,
      vif: () => isProfession.value
    },
    {
      name: 'markInvalid',
      label: translateInboxManager('markInvalid'),
      tooltip: translateInboxManager('markInvalid'),
      icon: ContextMenuIcon.block,
      color: 'negative',
      onClick: onMarkInboxesInvalid,
      vif: (inbox) => hasTargetInbox(inbox, (value) => value.status !== InboxStatus.Invalid)
    },
    {
      name: 'markNormal',
      label: translateInboxManager('markNormal'),
      tooltip: translateInboxManager('markNormal'),
      icon: ContextMenuIcon.verified,
      onClick: onMarkInboxesNormal,
      vif: (inbox) => hasTargetInbox(inbox, (value) => value.status !== InboxStatus.Valid)
    },
    {
      name: 'delete',
      label: translateGlobal('delete'),
      tooltip: translateInboxManager('deleteCurrentInbox'),
      color: 'negative',
      icon: ContextMenuIcon.delete,
      onClick: onDeleteInbox
    }
  ])

  /** 验证当前收件箱或当前选择的收件箱。 */
  async function onValidateInboxes(_row: IInbox, { targetValues, clearSelection }: IActionContext<IInbox>) {
    const result = await notifyUntil(
      () => validateInboxes(targetValues.map((x) => x.id as number)),
      translateInboxManager('validateCurrentOrSelectedInboxes'),
      translateGlobal('validate')
    )
    if (!result) return

    clearSelection()
    refreshTable()
    notifySuccess(translateGlobal('updateSuccess'))
  }

  /** 标记当前收件箱或已选择的全部收件箱为无效。 */
  async function onMarkInboxesInvalid(_cursorInbox: IInbox, { targetValues, clearSelection }: IActionContext<IInbox>) {
    await updateInboxStatuses(targetValues, InboxStatus.Invalid, clearSelection)
  }

  /** 标记当前收件箱或已选择的全部收件箱为正常。 */
  async function onMarkInboxesNormal(_cursorInbox: IInbox, { targetValues, clearSelection }: IActionContext<IInbox>) {
    await updateInboxStatuses(targetValues, InboxStatus.Valid, clearSelection)
  }

  async function updateInboxStatuses(inboxes: readonly IInbox[], status: InboxStatus, clearSelection: () => void) {
    const updateTargets = inboxes.filter(
      (inbox): inbox is IInbox & { id: number } => inbox.id !== undefined && inbox.status !== status
    )
    if (updateTargets.length === 0) return

    await updateInboxesStatus({
      inboxIds: updateTargets.map((inbox) => inbox.id),
      status
    })
    updateTargets.forEach((inbox) => {
      inbox.status = status
      inbox.validFailReason = undefined
    })
    clearSelection()
    notifySuccess(translateGlobal('updateSuccess'))
  }

  function hasTargetInbox(cursorInbox: IInbox, predicate: (inbox: IInbox) => boolean) {
    const targetInboxes = selectedInboxes.value.length > 0 ? selectedInboxes.value : [cursorInbox]
    return targetInboxes.some(predicate)
  }

  /** 删除当前收件箱或已选择的全部收件箱。 */
  async function onDeleteInbox(_cursorInbox: IInbox, { targetValues, clearSelection }: IActionContext<IInbox>) {
    const inboxes = targetValues.filter(isPersistedInbox)
    if (inboxes.length !== targetValues.length) return false

    const confirmationMessage =
      inboxes.length === 1
        ? translateInboxManager('isDeleteEmailOf', { email: inboxes[0]!.email })
        : translateInboxManager('isDeleteSelectedInboxes', { count: inboxes.length })
    const confirm = await confirmOperation(translateGlobal('deleteConfirmation'), confirmationMessage)
    if (!confirm) return

    await deleteInboxByIds(inboxes.map((inbox) => inbox.objectId))

    inboxes.forEach((inbox) => {
      deleteRowById(inbox.id)
    })
    clearSelection()

    notifySuccess(translateGlobal('deleteSuccess'))
  }

  function isPersistedInbox(inbox: IInbox): inbox is IInbox & { id: number; objectId: string } {
    return inbox.id !== undefined && inbox.objectId !== undefined
  }

  return { inboxContextMenuItems }
}
