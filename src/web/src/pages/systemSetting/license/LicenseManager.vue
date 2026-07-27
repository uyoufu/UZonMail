<template>
  <div class="column items-center justify-center full-height">
    <div class="column justify-start">
      <div>
        <span>{{ t('pages.license.licenseType') }}:</span>
        <span class="text-primary text-subtitle1 text-bold q-ml-sm">
          {{ formatLicenseType(activeInfo.licenseType) }}
        </span>
      </div>

      <div>
        <span>{{ t('pages.license.activationTime') }}:</span>
        <span class="q-ml-sm">{{ formatDate(activeInfo.activeDate) }}</span>
      </div>

      <div>
        <span>{{ t('pages.license.expirationTime') }}:</span>
        <span class="q-ml-sm">{{ formatDate(activeInfo.expireDate) }}</span>
      </div>
    </div>

    <q-input standout="bg-primary" label-color="white" v-model="license" dense @focus="onFocus" @blur="onBlur"
      :label="licenseLabel" @keydown.enter="onActiveLicense">
      <template v-slot:append>
        <q-icon v-if="showActiveIcon" name="motion_photos_auto" @click="onActiveLicense" class="cursor-pointer"
          :color="activeIconColor">
          <AsyncTooltip :tooltip="getActiveIconTooltip" :cache="false"></AsyncTooltip>
        </q-icon>

        <q-icon v-if="showLicenseRemoveIcon" class="q-ml-sm" name="logout" color="negative" @click="onRemoveLicense">
          <AsyncTooltip :tooltip="t('pages.license.deactivate')" :cache="false"></AsyncTooltip>
        </q-icon>
      </template>
    </q-input>
  </div>
</template>

<script lang="ts" setup>
import logger from 'loglevel'

import AsyncTooltip from 'src/components/asyncTooltip/AsyncTooltip.vue'
import { confirmOperation, notifyError, notifySuccess, notifyUntil } from 'src/utils/dialog'
import dayjs from 'dayjs'
import type { ILicenseInfo } from 'src/api/pro/license'
import { LicenseType, updateLicenseInfo, getLicenseInfo, updateExistingLicenseInfo, removeLicense } from 'src/api/pro/license'
import { t } from 'src/i18n/helpers'

const license = ref<string>('')

const showActiveIcon = ref(false)
const licenseLabel: Ref<undefined | string> = ref('')
function onFocus () {
  showActiveIcon.value = true
  licenseLabel.value = t('pages.license.enterLicenseCode')
}
function onBlur () {
  showActiveIcon.value = false
  licenseLabel.value = undefined
}

// 验证激活码是否合法
const isLicenseValid = computed(() => {
  return license.value.length > 0
})
const activeIconColor = computed(() => {
  return isLicenseValid.value ? 'positive' : 'white'
})
function getActiveIconTooltip () {
  return isLicenseValid.value ? t('pages.license.clickToActivate') : t('pages.license.invalidLicenseCodeLength')
}

// 激活激活码
import { useUserInfoStore } from 'src/stores/user'
import { useRoutesStore } from 'src/stores/routes'
import { userRelogin } from 'src/api/user'

const userInfoStore = useUserInfoStore()
const routeStore = useRoutesStore()

async function onActiveLicense () {
  // 验证授权码是否合法
  if (!isLicenseValid.value) {
    notifyError(t('pages.license.licenseCodeLengthRequirement'))
    return
  }

  const confirm = await confirmOperation(t('pages.license.upgradeConfirmation'), t('pages.license.upgradeConfirmationMessage'))
  if (!confirm) return

  await notifyUntil(async () => {
    // 调用升级接口
    const { data: licenseInfo } = await updateLicenseInfo(license.value)
    activeInfo.value = licenseInfo
  }, t('pages.license.versionLicense'), t('pages.license.upgrading'))

  // 更新用户权限
  await updateUserAccess()

  // 更新路由
  notifySuccess(t('pages.license.upgradeSuccess'))
  window.location.reload()
}

async function updateUserAccess () {
  // 重新拉取权限码
  const { data: { userInfo, token, access, installedPlugins } } = await userRelogin()
  logger.debug('[Login] 用户重新登录:', userInfo, token, access)

  const userInfoStore = useUserInfoStore()
  userInfoStore.setInstalledPlugins(installedPlugins)
  userInfoStore.setUserLoginInfo(userInfo, token, access)
  routeStore.resetDynamicRoutes()
}

// #region  激活信息
const activeInfo: Ref<ILicenseInfo> = ref({
  activeDate: dayjs().format('YYYY-MM-DD HH:mm'),
  expireDate: dayjs().add(99, 'year').format('YYYY-MM-DD HH:mm'),
  lastUpdateDate: dayjs().format('YYYY-MM-DD HH:mm'),
  activeUser: 'admin',
  licenseType: LicenseType.Community,
  licenseIcon: 'mdi-crown'
})
function formatDate (datetime: string) {
  if (!datetime) return
  return dayjs(datetime).format('YYYY-MM-DD HH:mm')
}
function formatLicenseType (licenseType: LicenseType) {
  return LicenseType[licenseType]
}

import { usePermission } from 'src/compositions/permission'
const { isSuperAdmin } = usePermission()
onMounted(async () => {
  // 判断是否包含 pro 插件
  if (!userInfoStore.hasProPlugin) {
    notifyError(t('pages.license.proPluginRequired'))
    return
  }

  // 如果是管理员，则更新授权后再拉取
  if (isSuperAdmin) {
    const { data: licenseInfo } = await updateExistingLicenseInfo()
    activeInfo.value = licenseInfo
  } else {
    // 从服务器拉取激活信息
    const { data: licenseInfo } = await getLicenseInfo()
    activeInfo.value = licenseInfo
  }
})
// #endregion

// #region 移除激活功能
const showLicenseRemoveIcon = computed(() => {
  return activeInfo.value.licenseType !== LicenseType.Community && isSuperAdmin
})
async function onRemoveLicense () {
  const confirm = await confirmOperation(t('pages.license.deactivate'), t('pages.license.deactivateConfirmation'))
  if (!confirm) return

  // 调用退出激活接口
  const { data } = await removeLicense()
  activeInfo.value = data

  // 更新用户权限
  await updateUserAccess()

  notifySuccess(t('pages.license.deactivated'))

  // 刷新页面
  window.location.reload()
}
// #endregion
</script>

<style lang="scss" scoped></style>
