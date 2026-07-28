import type { IIpWarmUpUpPlan } from "src/api/pro/ipWarmUp"
import { ContextMenuIcon, type IContextMenuItem } from "src/components/contextMenu/types"
import { confirmOperation, notifyError } from "src/utils/dialog"
import { useSendDetailVisitor } from '../../sendHistory/useSendDetailVisitor'

import { deleteIpWarmUpPlanByIds, getLatestSendingGroupOfSchedulePlan } from "src/api/pro/ipWarmUp"
import type { deleteRowByIdType } from "src/compositions/qTableUtils"
import { t } from 'src/i18n/helpers'

export function useIpWarmIpContext (deleteRowById: deleteRowByIdType<IIpWarmUpUpPlan>) {
  const ipWarmUpContextMenuItems: IContextMenuItem<IIpWarmUpUpPlan>[] = [
    {
      name: 'onViewLatestSendingGroupOfPlan',
      label: t('pages.ipWarmUp.viewTask'),
      tooltip: t('pages.ipWarmUp.viewLatestTask'),
      icon: ContextMenuIcon.visibility,
      onClick: onViewLatestSendingGroupOfPlan
    },
    {
      name: 'deleteWarmUpPlan',
      label: t('pages.ipWarmUp.delete'),
      tooltip: t('pages.ipWarmUp.deleteWarmUpPlan'),
      color: 'negative',
      icon: ContextMenuIcon.delete,
      onClick: onDeleteWarmUpPlan
    }
  ]

  // #region 右键菜单方法
  const { visitSendDetailTable } = useSendDetailVisitor()
  async function onViewLatestSendingGroupOfPlan (data: IIpWarmUpUpPlan) {
    // 获取最新的发送任务组 id
    const { data: sendingGroupId } = await getLatestSendingGroupOfSchedulePlan(data.objectId)
    if (!sendingGroupId) {
      notifyError(t('pages.ipWarmUp.noAssociatedTask'))
      return
    }

    // 跳转到发送任务页面
    await visitSendDetailTable(sendingGroupId, data.name)
  }

  async function onDeleteWarmUpPlan (data: IIpWarmUpUpPlan) {
    const confirm = await confirmOperation(t('pages.ipWarmUp.deleteWarmUpPlan'), t('pages.ipWarmUp.deleteWarmUpPlanConfirmation', { name: data.name }))
    if (!confirm)
      return

    // 调用删除接口
    await deleteIpWarmUpPlanByIds([data.id])

    // 移除列表项
    deleteRowById(data.id)
  }

  // #endregion


  return {
    ipWarmUpContextMenuItems
  }
}
