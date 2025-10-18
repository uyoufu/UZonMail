import { usePermission } from 'src/compositions/permission'
import IpWarmUpSettingDialog from '../components/IpWarmUpSettingDialog.vue'
import { showComponentDialog } from 'src/utils/dialog'
import { useRoute } from 'vue-router'
import { useUserInfoStore } from 'src/stores/user'
import { formatDateToUTC } from 'src/utils/format'
import logger from 'loglevel'
import dayjs from 'dayjs'
import { createIpWarmUpPlan } from 'src/api/pro/ipWarmUp'

import type { IEmailCreateInfo } from 'src/api/emailSending'

export function useIpWarmUp (validateSendingTaskParams: () => boolean, emailInfo: Ref<IEmailCreateInfo>) {
  const userInfoStore = useUserInfoStore()

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

    // TODO 打开预热弹窗
    const warmUpSettingResult = await showComponentDialog(IpWarmUpSettingDialog)
    if (!warmUpSettingResult.ok)
      return

    logger.debug('[IpWarmUp] 预热设置结果: ', warmUpSettingResult.data)
    // 添加时间范围和 smtps key
    const timePart = dayjs().format('HH:mm:ss')
    const warmUpData = Object.assign({ smtpPasswordSecretKeys: userInfoStore.smtpPasswordSecretKeys }, emailInfo.value, {
      sendStartDate: formatDateToUTC(`${warmUpSettingResult.data.from} ${timePart}`),
      sendEndDate: formatDateToUTC(`${warmUpSettingResult.data.to} ${timePart}`)
    })

    await createIpWarmUpPlan(warmUpData)
  }

  return { onIpWarmUpClick, enableIpWarmUpBtn }
}
