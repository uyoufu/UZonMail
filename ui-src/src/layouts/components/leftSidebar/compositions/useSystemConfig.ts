import type { ISystemConfig } from "src/api/pro/systemInfo"
import { getSystemConfig } from "src/api/pro/systemInfo"
import logger from 'loglevel'
import { translateGlobal } from "src/i18n/helpers"
import { useI18n } from "vue-i18n"

// TODO: 后续改成后端返回名称
export function useSystemConfig () {
  const systemConfig: Ref<ISystemConfig> = ref({
    name: translateGlobal('appName'),
    loginWelcome: 'Welcome to UZonMail',
    icon: '',
    copyright: '© 2023 UZonMail',
    icpInfo: '粤ICP备2023000000号',
  })

  onMounted(async () => {
    try {
      const { data } = await getSystemConfig()
      if (data) {
        systemConfig.value = data
        systemConfig.value.name = translateGlobal('appName')
      }
    }
    catch {
      logger.debug('[useSystemConfig] Failed to fetch system config')
    }
  })

  const { locale } = useI18n()
  watch(locale, () => {
    logger.debug('[useSystemConfig] Locale changed, update app name')
    systemConfig.value.name = translateGlobal('appName')
  })

  return { systemConfig }
}
