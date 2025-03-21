/* eslint-disable @typescript-eslint/no-explicit-any */
import type { IOutbox } from 'src/api/emailBox'
import { deleteOutboxByIds, OutboxStatus, updateOutbox, validateOutbox } from 'src/api/emailBox'
import { deleteAllInvalidBoxesInGroup, validateAllInvalidOutboxes } from 'src/api/emailGroup'

import type { IContextMenuItem } from 'src/components/contextMenu/types'
import type { IPopupDialogParams } from 'src/components/popupDialog/types'
import { confirmOperation, notifyError, notifySuccess, notifyUntil } from 'src/utils/dialog'
import { getOutboxFields } from './headerFunctions'
import { useUserInfoStore } from 'src/stores/user'
import { showDialog } from 'src/components/popupDialog/PopupDialog'
import { deAes } from 'src/utils/encrypt'

import type { getSelectedRowsType } from 'src/compositions/qTableUtils'

import { useI18n } from 'vue-i18n'
import logger from 'loglevel'

/**
 * 获取smtp密码
 * @param IOutbox
 * @returns
 */
function getSmtpPassword (outbox: IOutbox, smtpPasswordSecretKeys: string[]) {
  if (outbox.decryptedPassword) return outbox.password
  return deAes(smtpPasswordSecretKeys[0] as string, smtpPasswordSecretKeys[1] as string, outbox.password)
}

export function useContextMenu (deleteRowById: (id?: number) => void, getSelectedRows: getSelectedRowsType, refreshTable: () => void) {
  const { t } = useI18n()

  const outboxContextMenuItems: IContextMenuItem[] = [
    {
      name: 'edit',
      label: t('edit'),
      tooltip: t('outboxManager.editCurrentOutbox'),
      onClick: onUpdateOutbox
    },
    {
      name: 'delete',
      label: t('delete'),
      tooltip: t('outboxManager.deleteCurrentOrSelection'),
      color: 'negative',
      onClick: deleteOutbox
    },
    {
      name: 'validate',
      label: t('outboxManager.validate'),
      tooltip: t('outboxManager.sendTestToMe'),
      onClick: onValidateOutbox
    },
    {
      name: 'validateBatch',
      label: t('outboxManager.validateBatch'),
      tooltip: t('outboxManager.validateAllUnverifiedInGroup'),
      onClick: onValidateOutboxBatch
    },
    {
      name: 'deleteInvalid',
      label: t('outboxManager.deleteInvalid'),
      tooltip: t('outboxManager.deleteCurrentGroupInvalidOutboxes'),
      color: 'negative',
      onClick: onDeleteInvalidOutboxes
    }
  ]

  const userInfoStore = useUserInfoStore()

  // 删除发件箱
  async function deleteOutbox (row: Record<string, any>) {
    const { rows, selectedRows } = getSelectedRows(row)

    // 提示是否删除
    if (rows.length === 1) {
      const confirm = await confirmOperation(t('outboxManager.deleteOutbox'), t('outboxManager.isDeleteCurrentOutbox', { message: rows[0]!.email }))
      if (!confirm) return
    } else {
      const confirm = await confirmOperation(t('outboxManager.deleteOutbox'), t('outboxManager.isDeleteSelectedOutboxes', { count: rows.length }))
      if (!confirm) return
    }

    // 请求删除
    await deleteOutboxByIds(rows.map(row => row.objectId as string))

    // 开始删除
    rows.forEach(row => {
      deleteRowById(row.id)
    })
    selectedRows.value = []
    notifySuccess(t('outboxManager.deleteSuccess', { count: rows.length }))
  }

  // 更新发件箱
  async function onUpdateOutbox (row: Record<string, any>) {
    const outbox = row as IOutbox

    const fields = await getOutboxFields(userInfoStore.smtpPasswordSecretKeys)
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
          field.value = getSmtpPassword(outbox, userInfoStore.smtpPasswordSecretKeys)
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
      title: `修改发件箱 / ${outbox.email}`,
      fields
    }

    // 弹出对话框

    const { ok, data } = await showDialog<IOutbox>(popupParams)
    if (!ok) return

    // 向服务器传递更新
    await updateOutbox(outbox.id as number, data)

    // 将参数更新到 outbox 中
    Object.assign(outbox, data, { decryptedPassword: false, showPassword: false })

    notifySuccess('更新成功')
  }

  async function onValidateOutbox (row: Record<string, any>) {
    const outbox = row as IOutbox
    const result = await notifyUntil(async () => {
      return await validateOutbox(outbox.id as number, userInfoStore.smtpPasswordSecretKeys)
    }, `验证 ${row.email}`, '正在验证中...')
    if (!result) return
    const { data, message } = result

    if (data) {
      // 验证成功
      notifySuccess('验证成功')
      // 更新状态
      row.isValid = true
      row.status = OutboxStatus.Valid
      row.validFailReason = ''
      return
    }

    const fullMessage = `验证失败: ${message}`
    logger.error(fullMessage)

    // 验证失败
    notifyError(fullMessage)

    // 更新状态
    row.isValid = false
    row.validFailReason = message
  }

  async function onValidateOutboxBatch (row: Record<string, any>) {
    const confirm = await confirmOperation(t('confirmOperation'), t('outboxManager.isConfirmValidateBatch'))
    if (!confirm) return

    await notifyUntil(async () => {
      // 向后端进行批量验证，通过 websocket 更新进度信息
      return await validateAllInvalidOutboxes(row.emailGroupId as number, userInfoStore.smtpPasswordSecretKeys)
    }, t('outboxManager.validateBatch'), t('outboxManager.validating'))

    notifySuccess(t('outboxManager.validateBatchSuccess'))

    // 重新刷新
    refreshTable()
  }


  async function onDeleteInvalidOutboxes (row: Record<string, any>) {
    const confirm = await confirmOperation(t('deleteConfirm'), t('outboxManager.doDeleteAllInvalidOutboxes'))
    if (!confirm) return

    // 请求删除
    const outbox = row as IOutbox
    await deleteAllInvalidBoxesInGroup(outbox.emailGroupId as number)
    // 删除成功
    notifySuccess(t('deleteSuccess'))

    // 重新刷新
    refreshTable()
  }

  return { outboxContextMenuItems }
}
