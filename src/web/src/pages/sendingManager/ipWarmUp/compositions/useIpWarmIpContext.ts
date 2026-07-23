import type { IIpWarmUpUpPlan } from "src/api/pro/ipWarmUp"
import type { IContextMenuItem } from "src/components/contextMenu/types"
import { confirmOperation, notifyError } from "src/utils/dialog"
import { useSendDetailVisitor } from '../../sendHistory/useSendDetailVisitor'

import { deleteIpWarmUpPlanByIds, getLatestSendingGroupOfSchedulePlan } from "src/api/pro/ipWarmUp"
import type { deleteRowByIdType } from "src/compositions/qTableUtils"

export function useIpWarmIpContext (deleteRowById: deleteRowByIdType) {
  const ipWarmUpContextMenuItems: IContextMenuItem<IIpWarmUpUpPlan>[] = [
    {
      name: 'onViewLatestSendingGroupOfPlan',
      label: '查看任务',
      tooltip: '查看当前预热计划的最新发送任务',
      onClick: onViewLatestSendingGroupOfPlan
    },
    {
      name: 'deleteWarmUpPlan',
      label: '删除',
      tooltip: '删除当前预热计划',
      color: 'negative',
      onClick: onDeleteWarmUpPlan
    }
  ]

  // #region 右键菜单方法
  const { visitSendDetailTable } = useSendDetailVisitor()
  async function onViewLatestSendingGroupOfPlan (data: IIpWarmUpUpPlan) {
    // 获取最新的发送任务组 id
    const { data: sendingGroupId } = await getLatestSendingGroupOfSchedulePlan(data.objectId)
    if (!sendingGroupId) {
      notifyError('当前预热计划没有关联到对应的发送任务组，请检查计划是否已开始！')
      return
    }

    // 跳转到发送任务页面
    await visitSendDetailTable(sendingGroupId, data.name)
  }

  async function onDeleteWarmUpPlan (data: IIpWarmUpUpPlan) {
    const confirm = await confirmOperation('删除预热计划', `确定要删除预热计划 "${data.name}" 吗？`)
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
