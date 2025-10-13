import { usePermission } from 'src/compositions/permission'
import IpWarmUpSettingDialog from '../components/IpWarmUpSettingDialog.vue'
import { showComponentDialog } from 'src/utils/dialog'

export function useIpWarmUp () {
  // #region 权限
  const { isProfession } = usePermission()
  // #endregion

  async function onIpWarmUpClick () {
    // TODO 打开预热弹窗
    await showComponentDialog(IpWarmUpSettingDialog)
  }

  return { onIpWarmUpClick, enableIpWarmUpBtn: isProfession }
}
