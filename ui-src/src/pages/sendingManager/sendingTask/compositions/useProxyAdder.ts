import type { IEmailCreateInfo } from "src/api/emailSending"
import { showComponentDialog } from "src/utils/dialog"
import SelectProxyDialog from "../components/SelectProxyDialog.vue"

export function useProxyAdder (emailInfo: Ref<IEmailCreateInfo>) {
  const existProxy = computed(() => {
    return emailInfo.value.proxyIds.length > 0
  })

  // 代理按钮颜色
  const proxyBtnColor = computed(() => {
    return existProxy.value ? 'primary' : 'grey'
  })

  const proxyBtnTooltip = computed(() => {
    const proxyCount = emailInfo.value.proxyIds.length
    if (proxyCount === 0) return ['单击添加代理']

    return ['单击添加代理', `已添加 ${proxyCount} 个代理`]
  })

  async function onProxyBtnClick () {
    const { ok, data } = await showComponentDialog(SelectProxyDialog, {
      proxyIds: emailInfo.value.proxyIds,
    })
    if (!ok) return

    const proxyIds = data.proxyIds as number[]
    emailInfo.value.proxyIds = proxyIds
  }

  return {
    existProxy,
    proxyBtnColor,
    proxyBtnTooltip,
    onProxyBtnClick
  }
}
