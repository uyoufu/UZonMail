import { Notify } from 'quasar'
import { getUserSetting, updateUserSettingString } from 'src/api/userSetting'
import { getServerVersion } from 'src/api/system'
import { usePermission } from 'src/compositions/permission'
import { getCurrentLocale, translateDashboardPage } from 'src/i18n/helpers'
import { notifyError, showComponentDialog } from 'src/utils/dialog'
import logger from 'loglevel'
import VersionHistoryDialog from './VersionHistoryDialog.vue'
import { isDesktopClient, startDesktopUpdate } from './desktopUpdate'

const latestPackageEndpoint = 'https://uzonmail.uzoncloud.com/updates/latest.json'
const ignoredDesktopUpdateVersionKey = 'ignoredDesktopUpdateVersion'
const allowVersionChecking = ref(true)

interface ILatestPackage {
  version: string
}

interface IUserSettingStringValue {
  stringValue?: string | null
}

const htmlEntities: Record<string, string> = {
  '&': '&amp;',
  '<': '&lt;',
  '>': '&gt;',
  '"': '&quot;',
  "'": '&#39;'
}

/** 比较使用点号分隔的非负整数版本号。 */
export function compareVersions(left: string, right: string): number {
  const leftParts = left.split('.').map(Number)
  const rightParts = right.split('.').map(Number)
  if (
    leftParts.length === 0 ||
    rightParts.length === 0 ||
    leftParts.some((part) => !Number.isInteger(part) || part < 0) ||
    rightParts.some((part) => !Number.isInteger(part) || part < 0)
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

function isLatestPackage(value: unknown): value is ILatestPackage {
  return typeof value === 'object' && value !== null && 'version' in value && typeof value.version === 'string'
}

function escapeHtml(value: string): string {
  return value.replace(/[&<>"']/g, (character) => htmlEntities[character] ?? character)
}

function createVersionNotificationMessage(currentVersion: string, latestVersion: string): string {
  const currentVersionMessage = translateDashboardPage('newVersionCurrent', { version: escapeHtml(currentVersion) })
  const latestVersionMessage = translateDashboardPage('newVersionLatest', { version: escapeHtml(latestVersion) })
  return `<div class="text-white q-mx-lg">
    <div class="text-subtitle1">${translateDashboardPage('newVersionAvailable')}</div>
    <div class="q-mt-xs">${currentVersionMessage}</div>
    <div>${latestVersionMessage}</div>
    </div>`
}

/** 在首页加载时为超级管理员检查最新发布版本。 */
export function useVersionChecker() {
  const { isSuperAdmin } = usePermission()
  const hasDesktopUpdateCapability = isDesktopClient()

  async function onViewVersionHistoryClick(): Promise<void> {
    await showComponentDialog(VersionHistoryDialog, {
      locale: getCurrentLocale().value,
      isDesktopClient: hasDesktopUpdateCapability
    })
  }

  async function onStartDesktopUpdateClick(): Promise<void> {
    try {
      if (!(await startDesktopUpdate())) {
        notifyError(translateDashboardPage('newVersionUpdateStartFailed'))
      }
    } catch (error) {
      logger.warn('[dashboard] desktop update start failed:', error)
      notifyError(translateDashboardPage('newVersionUpdateStartFailed'))
    }
  }

  onMounted(async () => {
    if (!isSuperAdmin.value || !allowVersionChecking.value) return
    allowVersionChecking.value = false

    try {
      const [packageResponse, serverResponse, ignoredVersionResponse] = await Promise.all([
        fetch(latestPackageEndpoint),
        getServerVersion(),
        getUserSetting<IUserSettingStringValue>(ignoredDesktopUpdateVersionKey)
      ])
      if (!packageResponse.ok) {
        throw new Error(`更新清单请求失败：${packageResponse.status}`)
      }

      const latestPackageValue: unknown = await packageResponse.json()
      if (!isLatestPackage(latestPackageValue)) {
        throw new Error('更新清单缺少有效版本号')
      }

      const latestVersion = latestPackageValue.version
      const currentVersion = serverResponse.data
      logger.debug('[dashboard] version info, current:', currentVersion, 'latest:', latestVersion)
      if (
        compareVersions(currentVersion, latestVersion) >= 0 ||
        ignoredVersionResponse.data?.stringValue === latestVersion
      ) {
        return
      }

      async function onIgnoreVersionClick(): Promise<void> {
        try {
          const { data: isUpdated } = await updateUserSettingString(ignoredDesktopUpdateVersionKey, latestVersion)
          if (!isUpdated) {
            notifyError(translateDashboardPage('newVersionIgnoreFailed'))
            return
          }

          updateVersionNotification({ timeout: 1 })
        } catch (error) {
          logger.warn('[dashboard] ignore version update failed:', error)
          notifyError(translateDashboardPage('newVersionIgnoreFailed'))
        }
      }

      const notificationActions = [
        {
          label: translateDashboardPage('newVersionView'),
          color: 'accent',
          dense: true,
          handler: () => {
            void onViewVersionHistoryClick()
          }
        },
        {
          label: translateDashboardPage('newVersionIgnore'),
          color: 'negative',
          dense: true,
          handler: () => {
            void onIgnoreVersionClick()
          }
        }
      ]
      if (hasDesktopUpdateCapability) {
        notificationActions.push({
          label: translateDashboardPage('newVersionUpdate'),
          color: 'secondary',
          dense: true,
          handler: () => {
            void onStartDesktopUpdateClick()
          }
        })
      }

      const updateVersionNotification = Notify.create({
        icon: 'sentiment_satisfied_alt',
        color: 'grey-7',
        message: createVersionNotificationMessage(currentVersion, latestVersion),
        html: true,
        actions: notificationActions
      })
    } catch (error) {
      logger.warn('[dashboard] version check failed:', error)
    }
  })
}
