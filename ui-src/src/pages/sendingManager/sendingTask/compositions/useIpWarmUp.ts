import { usePermission } from 'src/compositions/permission'
import IpWarmUpSettingDialog from '../components/IpWarmUpSettingDialog.vue'
import { notifySuccess, showComponentDialog } from 'src/utils/dialog'
import { useRoute } from 'vue-router'
import { formatDateToUTC } from 'src/utils/format'
import logger from 'loglevel'
import dayjs from 'dayjs'
import { createIpWarmUpPlan } from 'src/api/pro/ipWarmUp'
import { getInboxesCountInGroups } from 'src/api/emailBox'

import type { IEmailCreateInfo } from 'src/api/emailSending'

export function useIpWarmUp (validateSendingTaskParams: () => boolean, emailInfo: Ref<IEmailCreateInfo>) {
  // #region 权限
  const route = useRoute()
  const { isProfession } = usePermission()
  const enableIpWarmUpBtn = computed(() => {
    return isProfession.value && route.query.type === 'ipWarmUp'
  })
  // #endregion

  async function onIpWarmUpClick () {
    // 先要进行数据验证
    if (!validateSendingTaskParams()) {
      return
    }

    logger.debug('[IpWarmUp] 点击 IP 预热按钮', emailInfo.value)

    // 获取收件箱中的邮箱数量
    let totalInboxesCount = emailInfo.value.inboxes.length
    if (emailInfo.value.inboxGroups.length > 0) {
      const { data: inboxesCount } = await getInboxesCountInGroups(emailInfo.value.inboxGroups.map(x => x.id!))
      totalInboxesCount += inboxesCount
    }


    // TODO 打开预热弹窗
    const { ok, data } = await showComponentDialog(IpWarmUpSettingDialog, {
      totalCount: totalInboxesCount || 10
    })
    if (!ok)
      return

    logger.debug('[IpWarmUp] 预热设置结果: ', data)
    // 添加时间范围和 smtps key
    const timePart = dayjs().format('HH:mm:ss')
    const warmUpData = Object.assign({}, emailInfo.value, {
      sendStartDate: formatDateToUTC(`${data.from} ${timePart}`),
      sendEndDate: formatDateToUTC(`${data.to} ${timePart}`),
      sendCountChartPoints: data.countChartPoints,
      name: data.name
    })

    await createIpWarmUpPlan(warmUpData)

    notifySuccess('IP预热计划创建成功！')
  }

  return { onIpWarmUpClick, enableIpWarmUpBtn }
}
