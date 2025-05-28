import type { ISystemConfig } from "src/api/pro/systemInfo";
import { getSystemConfig } from "src/api/pro/systemInfo"
import logger from 'loglevel'

export function useSystemConfig () {
  const systemConfig: Ref<ISystemConfig> = ref({
    name: '宇正群邮',
    loginWelcome: 'Welcome to UZonMail',
    icon: '',
    copyright: '© 2023 UZonMail',
    icpInfo: '粤ICP备2023000000号',
  })

  onMounted(async () => {
    try {
      const { data } = await getSystemConfig()
      if (data) systemConfig.value = data
    }
    catch {
      logger.debug('[useSystemConfig] Failed to fetch system config')
    }
  })

  return { systemConfig }
}
