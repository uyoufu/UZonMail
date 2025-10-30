/* eslint-disable @typescript-eslint/no-explicit-any */
import type { IContextMenuItem } from 'src/components/contextMenu/types'
import type { ISendingGroupHistory } from 'src/api/sendingGroup'
import { SendingGroupStatus, deleteSendingGroups } from 'src/api/sendingGroup'
import { pauseSending, restartSending, cancelSending, resendSendingGroup } from 'src/api/emailSending'
import { confirmOperation, notifyError, notifySuccess } from 'src/utils/dialog'
import { useUserInfoStore } from 'src/stores/user'
import { useSendDetailVisitor } from './useSendDetailVisitor'

import { useI18n } from 'vue-i18n'

import type { deleteRowByIdType, getSelectedRowsType } from 'src/compositions/qTableUtils'
import type { ISendingGroupInfo } from 'src/api/sendingGroup'

/**
 * 添加右键菜单
 */
export function useContextMenu (getSelectedRows: getSelectedRowsType, deleteRowById: deleteRowByIdType) {
  const router = useRouter()
  const { t } = useI18n()

  const sendingHistoryContextItems: IContextMenuItem<ISendingGroupInfo>[] = [
    {
      name: 'detail',
      label: '发件明细',
      tooltip: '查看发件明细',
      onClick: openSendDetailDialog as any
    },
    {
      name: 'pause',
      label: '暂停发件',
      tooltip: '暂停发件',
      onClick: onPauseSending as any,
      vif: canPauseSending as any
    },
    {
      name: 'start',
      label: '开始发件',
      tooltip: '开始发件',
      vif: canRestart as any,
      onClick: onRestartSending as any
    },
    {
      name: 'startForFailed',
      label: '失败重发',
      tooltip: '对失败项进行重发',
      vif: canResend as any,
      onClick: onResendSendingGroup as any
    },
    {
      name: 'cancelSchedule',
      label: '取消发件',
      tooltip: '取消当前发件任务',
      color: 'negative',
      vif: canCancel as any,
      onClick: onCancelSending as any
    },
    {
      name: 'newSendingTaskWithTemplate',
      label: '复制发件',
      tooltip: '复制该数据作为模板并新建发件',
      onClick: onNewSendingTaskWithTemplate as any
    },
    {
      name: 'delete',
      label: t('sendDetail.delete'),
      color: 'negative',
      tooltip: '删除当前或者选中的发件历史',
      onClick: onDeleteSendingGroups
    }
  ]


  // 打开发件明细
  const { visitSendDetailTable } = useSendDetailVisitor()
  async function openSendDetailDialog (data: ISendingGroupHistory) {
    // 跳转到发件明细页面
    await visitSendDetailTable(data.id, String(data.id))
  }

  // 暂停发件
  function canPauseSending (data: ISendingGroupHistory): boolean {
    return data.status === SendingGroupStatus.Sending
  }
  async function onPauseSending (data: ISendingGroupHistory) {
    // 进行确认
    const confirm = await confirmOperation('暂停确认', '确认暂停发件吗？')
    if (!confirm) return

    // 暂停发件
    // 向服务器请求暂停
    await pauseSending(data.id)

    data.status = SendingGroupStatus.Pause
    notifySuccess('暂停成功')
  }

  function canRestart (data: ISendingGroupHistory): boolean {
    return data.status === SendingGroupStatus.Pause
  }
  async function onRestartSending (data: ISendingGroupHistory) {
    // 进行确认
    const confirm = await confirmOperation('发送确认', '确认重新开始发件吗？')
    if (!confirm) return

    await restartSending(data.id, userInfoStore.smtpPasswordSecretKeys)
    data.status = SendingGroupStatus.Sending

    notifySuccess('已重新发送')
  }

  // 是否可以取消发件
  // 非完成的发件组都可以取消
  function canCancel (data: ISendingGroupHistory): boolean {
    return data.status !== SendingGroupStatus.Finish && data.status !== SendingGroupStatus.Cancel
  }
  async function onCancelSending (data: ISendingGroupHistory) {
    // 进行确认
    const confirm = await confirmOperation('取消确认', '确认取消发件吗，取消后将不可重新开始，是否继续？')
    if (!confirm) return

    await cancelSending(data.id)
    data.status = SendingGroupStatus.Cancel

    notifySuccess('取消成功')
  }

  function canResend (data: ISendingGroupHistory): boolean {
    return data.status === SendingGroupStatus.Finish && data.successCount < data.totalCount
  }

  const userInfoStore = useUserInfoStore()
  async function onResendSendingGroup (data: ISendingGroupHistory) {
    const confirm = await confirmOperation('重发确认', `即将重新发送【${data.totalCount - data.successCount}】封邮件，是否继续？`)
    if (!confirm) return

    // 开始重发
    await resendSendingGroup(data.id, userInfoStore.smtpPasswordSecretKeys)

    notifySuccess('正在重新发送...')

    // 更新进度
    data.status = SendingGroupStatus.Sending
  }


  async function onNewSendingTaskWithTemplate (data: ISendingGroupHistory) {
    await router.push({
      name: 'SendingTask',
      query: {
        sendingGroupTemplateId: data.objectId
      }
    })
  }

  async function onDeleteSendingGroups (cursorData: ISendingGroupInfo) {
    const { rows, selectedRows } = getSelectedRows(cursorData)
    // 如果在进行中，则不允许删除
    const inProgressGroups = rows.filter(x => x.status === SendingGroupStatus.Sending
      || x.status === SendingGroupStatus.Scheduled)
    if (inProgressGroups.length > 0) {
      notifyError(`选中项共有 ${inProgressGroups.length} 项正在运行，请取消后再删除`)
      return
    }

    const confirm = await confirmOperation(t('deleteConfirm'), `即将删除 ${rows.length} 项发件历史，是否继续？`)
    if (!confirm) return

    // 开始删除
    await deleteSendingGroups(rows.map(x => x.id))
    for (const row of rows) {
      deleteRowById(row.id)
    }
    selectedRows.value = []

    notifySuccess(t('deleteSuccess'))
  }

  return {
    openSendDetailDialog,
    sendingHistoryContextItems
  }
}
