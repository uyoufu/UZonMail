/* eslint-disable @typescript-eslint/no-explicit-any */
import type { IOutbox } from 'src/api/emailBox'
import { deleteOutboxByIds, OutboxStatus, updateOutbox, validateOutbox } from 'src/api/emailBox'
import { deleteAllInvalidOutboxesInGroup, validateAllInvalidOutboxes } from 'src/api/emailGroup'

import type { IContextMenuItem } from 'src/components/contextMenu/types'
import type { IPopupDialogParams } from 'src/components/popupDialog/types'
import { confirmOperation, notifyError, notifySuccess, notifyUntil } from 'src/utils/dialog'
import { getOutboxFields } from './headerFunctions'

import { showDialog } from 'src/components/popupDialog/PopupDialog'

import type { getSelectedRowsType } from 'src/compositions/qTableUtils'

import { translateGlobal, translateOutboxManager } from 'src/i18n/helpers'

import logger from 'loglevel'

import { tryOutlookDelegateAuthorization, isExchangeOutbox } from './headerFunctions'


export function useContextMenu (deleteRowById: (id?: number) => void, getSelectedRows: getSelectedRowsType, refreshTable: () => void) {
  const outboxContextMenuItems: Ref<IContextMenuItem<IOutbox>[]> = computed(() =>
    [
      {
        name: 'edit',
        label: translateGlobal('edit'),
        tooltip: translateOutboxManager('editCurrentOutbox'),
        onClick: onUpdateOutbox
      },
      {
        name: 'delete',
        label: translateGlobal('delete'),
        tooltip: translateOutboxManager('deleteCurrentOrSelection'),
        color: 'negative',
        onClick: onDeleteOutbox
      },
      {
        name: 'validate',
        label: translateGlobal('validate'),
        tooltip: translateOutboxManager('sendTestToMe'),
        onClick: onValidateOutbox
      },
      {
        name: 'validateBatch',
        label: translateOutboxManager('validateBatch'),
        tooltip: translateOutboxManager('validateAllUnverifiedInGroup'),
        onClick: onValidateOutboxBatch
      },
      {
        name: 'deleteInvalid',
        label: translateOutboxManager('deleteInvalid'),
        tooltip: translateOutboxManager('deleteCurrentGroupInvalidOutboxes'),
        color: 'negative',
        onClick: onDeleteInvalidOutboxes
      },
      {
        name: 'outlookDelegateAuthorization',
        label: translateOutboxManager('outlookDelegateAuthorization'),
        tooltip: translateOutboxManager('outlookDelegateAuthorization'),
        onClick: onRequestOutlookDelegateAuthorization,
        vif: row => isExchangeOutbox(row.smtpHost, row.email)
      },
    ])

  // 删除发件箱
  async function onDeleteOutbox (row: Record<string, any>) {
    const { rows, selectedRows } = getSelectedRows(row)

    // 提示是否删除
    if (rows.length === 1) {
      const confirm = await confirmOperation(
        translateOutboxManager('deleteOutbox'),
        translateOutboxManager('isDeleteCurrentOutbox',
          { message: rows[0]!.email }))
      if (!confirm) return
    } else {
      const confirm = await confirmOperation(
        translateOutboxManager('deleteOutbox'),
        translateOutboxManager('isDeleteSelectedOutboxes',
          { count: rows.length }))
      if (!confirm) return
    }

    // 请求删除
    await deleteOutboxByIds(rows.map(row => row.objectId as string))

    // 开始删除
    rows.forEach(row => {
      deleteRowById(row.id)
    })
    selectedRows.value = []
    notifySuccess(translateOutboxManager('deleteSuccess', { count: rows.length }))
  }

  /**
   * 进行 Outlook 委托授权
   * @param outbox
   */
  async function onRequestOutlookDelegateAuthorization (outbox: IOutbox) {
    // 清除密码，重新进行授权
    if (!outbox.userName)
      outbox.password = ""

    await tryOutlookDelegateAuthorization(outbox)
  }

  // 更新发件箱
  async function onUpdateOutbox (row: Record<string, any>) {
    const outbox = row as IOutbox

    const fields = await getOutboxFields()
    // 修改默认值
    fields.forEach(field => {
      switch (field.name) {
        case 'email':
          field.value = outbox.email
          break
        case 'name':
          field.value = outbox.name
          break
        case 'userName':
          field.value = outbox.userName
          break
        case 'password':
          field.value = outbox.password
          break
        case 'smtpHost':
          field.value = outbox.smtpHost
          break
        case 'smtpPort':
          field.value = outbox.smtpPort
          break
        case 'description':
          field.value = outbox.description
          break
        case 'proxyId':
          field.value = outbox.proxyId
          break
        case 'replyToEmails':
          field.value = outbox.replyToEmails
          break
        case 'enableSSL':
          field.value = outbox.enableSSL
          break
        default:
          break
      }
    })

    // 新增发件箱
    const popupParams: IPopupDialogParams = {
      title: translateOutboxManager('modifyOutboxTitle', { email: outbox.email }),
      fields
    }

    // 弹出对话框

    const { ok, data } = await showDialog<IOutbox>(popupParams)
    if (!ok) return

    // 向服务器传递更新
    await updateOutbox(outbox.id as number, data)

    // 将参数更新到 outbox 中
    Object.assign(outbox, data, { decryptedPassword: false, showPassword: false })

    notifySuccess(translateGlobal('updateSuccess'))

    // 尝试进行 outlook 委托授权
    await tryOutlookDelegateAuthorization(outbox)
  }

  async function onValidateOutbox (row: Record<string, any>) {
    const outbox = row as IOutbox
    const result = await notifyUntil(async () => {
      return await validateOutbox(outbox.id as number)
    }, translateOutboxManager('validatingEmail', { email: row.email }), translateOutboxManager('validating'))
    if (!result) return
    const { data, message } = result

    if (data) {
      // 验证成功
      notifySuccess(translateOutboxManager('validationSuccessful'))
      // 更新状态
      row.isValid = true
      row.status = OutboxStatus.Valid
      row.validFailReason = ''
      return
    }

    const fullMessage = translateOutboxManager('validateFailed', { email: outbox.email, reason: message })
    logger.error(fullMessage)

    // 验证失败
    notifyError(fullMessage)

    // 更新状态
    row.isValid = false
    row.validFailReason = message
  }

  async function onValidateOutboxBatch (row: Record<string, any>) {
    const confirm = await confirmOperation(translateGlobal('confirmOperation'), translateOutboxManager('isConfirmValidateBatch'))
    if (!confirm) return

    await notifyUntil(async () => {
      // 向后端进行批量验证，通过 websocket 更新进度信息
      return await validateAllInvalidOutboxes(row.emailGroupId as number)
    }, translateOutboxManager('validateBatch'), translateOutboxManager('validating'))

    notifySuccess(translateOutboxManager('validateBatchSuccess'))

    // 重新刷新
    refreshTable()
  }


  async function onDeleteInvalidOutboxes (row: Record<string, any>) {
    const confirm = await confirmOperation(translateGlobal('deleteConfirmation'), translateOutboxManager('doDeleteAllInvalidOutboxes'))
    if (!confirm) return

    // 请求删除
    const outbox = row as IOutbox
    await deleteAllInvalidOutboxesInGroup(outbox.emailGroupId as number)
    // 删除成功
    notifySuccess(translateGlobal('deleteSuccess'))

    // 重新刷新
    refreshTable()
  }

  return { outboxContextMenuItems }
}
