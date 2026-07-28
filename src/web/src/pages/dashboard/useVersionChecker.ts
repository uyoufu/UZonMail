import { getServerVersion } from 'src/api/system'
import { usePermission } from 'src/compositions/permission'
import { confirmOperation, notifyError, showHtmlDialog } from 'src/utils/dialog'
import logger from 'loglevel'
import { translateDashboardPage } from 'src/i18n/helpers'

const latestPackageEndpoint = 'https://uzonmail.uzoncalc.com/updates/latest.json'
const allowVersionChecking = ref(true)

interface ILatestPackage {
  version: string
}

/** 比较桌面端使用的版本号 */
function compareVersions (left: string, right: string): number {
  const leftParts = left.split('.').map(Number)
  const rightParts = right.split('.').map(Number)
  if (
    leftParts.length === 0
    || rightParts.length === 0
    || leftParts.some(part => !Number.isInteger(part) || part < 0)
    || rightParts.some(part => !Number.isInteger(part) || part < 0)
  ) {
    throw new Error(`版本格式无效：${left} / ${right}`)
  }

  const segmentCount = Math.max(leftParts.length, rightParts.length)
  for (let index = 0; index < segmentCount; index++) {
    const difference = (leftParts[index] ?? 0) - (rightParts[index] ?? 0)
    if (difference !== 0) return difference
  }
  return 0
}

/** 尝试调用桌面端更新宿主对象 */
async function startDesktopUpdate (): Promise<boolean> {
  const updater = window.chrome?.webview?.hostObjects?.uzonMailUpdater
  if (!updater) return false
  return await updater.beginUpdate()
}

/** 在首页加载时为超级管理员检查最新发布版本 */
export function useVersionChecker () {
  const { isSuperAdmin } = usePermission()

  onMounted(async () => {
    if (!isSuperAdmin.value || !allowVersionChecking.value) return
    allowVersionChecking.value = false

    try {
      const [packageResponse, serverResponse] = await Promise.all([
        fetch(latestPackageEndpoint),
        getServerVersion()
      ])
      if (!packageResponse.ok) {
        throw new Error(`更新清单请求失败：${packageResponse.status}`)
      }

      const latestPackage = await packageResponse.json() as ILatestPackage
      const currentVersion = serverResponse.data
      logger.debug('[dashboard] version info, current:', currentVersion, 'latest:', latestPackage.version)
      if (compareVersions(currentVersion, latestPackage.version) >= 0) return

      const title = translateDashboardPage('newVersionAvailable')
      const message = translateDashboardPage('newVersionPrompt', { version: latestPackage.version })
      if (!window.chrome?.webview?.hostObjects?.uzonMailUpdater) {
        await showHtmlDialog(title, `<div class="text-accent text-subtitle1">${message}</div>`)
        return
      }

      if (!await confirmOperation(title, message)) return
      if (!await startDesktopUpdate()) {
        notifyError(translateDashboardPage('newVersionUpdateStartFailed'))
      }
    } catch (error) {
      logger.warn('[dashboard] version check failed:', error)
    }
  })
}
