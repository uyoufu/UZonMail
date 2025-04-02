import { getServerVersion } from 'src/api/system'
import logger from 'loglevel'
import { usePermission } from 'src/compositions/permission'
import { showHtmlDialog } from 'src/utils/dialog'

const allowVersionChecking = ref(true)


export function useVersionChecker () {
  const { isSuperAdmin } = usePermission()

  onMounted(async () => {
    if (!isSuperAdmin.value) return
    if (!allowVersionChecking.value) return

    // 检查最新版本信息
    // 从官方的仓库中下载：https://uzonmail.pages.dev/versions.html
    const res = await fetch('https://uzonmail.pages.dev/versions.html')
    const data = await res.text()
    if (!data) return

    // 获取第 1 个 h2 到第 2 个 h2 之间的内容
    const latestReleaseInfo = data.match(/(<h2.*?)<h2/)![0]
    const latestVersion = latestReleaseInfo.match(/(\d+\.\d+\.\d+\.\d+)/)![0]

    // 解析版本信息
    const { data: serviceVersion } = await getServerVersion()
    logger.debug('[dashboard] version info, current:', serviceVersion, 'latest:', latestVersion)

    allowVersionChecking.value = false
    if (serviceVersion < latestVersion) {
      await showHtmlDialog('<div class="text-negative">发现新版本<div>',
        latestReleaseInfo.replace(/h2/g, "div")
          .replace(/h3/g, "div")
          .replace(/blockquote/g, "span")
          .replace(/<p>/g, "<div>")
          .replace(/<\/p>/g, "</div>")
      )
    }
  })
}
