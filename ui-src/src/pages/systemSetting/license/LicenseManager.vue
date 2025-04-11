<template>
  <div class="column items-center justify-center full-height">
    <div class="column justify-start">
      <div>
        <span>授权类型:</span>
        <span class="text-primary text-subtitle1 text-bold q-ml-sm">
          {{ formatLicenseType(activeInfo.licenseType) }}
        </span>
      </div>

      <div>
        <span>激活时间:</span>
        <span class="q-ml-sm">{{ formatDate(activeInfo.activeDate) }}</span>
      </div>

      <div>
        <span>到期时间:</span>
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
          <AsyncTooltip tooltip="退出激活状态" :cache="false"></AsyncTooltip>
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
import type { ILicenseInfo } from 'src/api/pro/license';
import { LicenseType, updateLicenseInfo, getLicenseInfo, updateExistingLicenseInfo, removeLicense } from 'src/api/pro/license'

const license = ref<string>('')

const showActiveIcon = ref(false)
const licenseLabel: Ref<undefined | string> = ref('')
function onFocus () {
  showActiveIcon.value = true
  licenseLabel.value = '请输入授权码'
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
  return isLicenseValid.value ? '单击激活' : '激活码长度不满足要求'
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
    notifyError('激活码长度应为 24 位')
    return
  }

  const confirm = await confirmOperation('升级确认', '即将进行升级, 是否继续?')
  if (!confirm) return

  await notifyUntil(async () => {
    // 调用升级接口
    const { data: licenseInfo } = await updateLicenseInfo(license.value)
    activeInfo.value = licenseInfo
  }, "企业版升级", '升级中...')

  // 重新拉取权限码
  const { data: { userInfo, token, access, installedPlugins } } = await userRelogin()
  logger.debug('[Login] 用户重新登录:', userInfo, token, access)

  const userInfoStore = useUserInfoStore()
  userInfoStore.setInstalledPlugins(installedPlugins)
  userInfoStore.setUserLoginInfo(userInfo, token, access)
  routeStore.resetDynamicRoutes()

  // 更新路由
  notifySuccess('升级成功!')
  window.location.reload()
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
    notifyError('当前为免费版本, 请先安装 pro 版本插件')
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
  const confirm = await confirmOperation('退出激活状态', '退出激活后，该系统下所有用户的高级功能将不可用，是否继续?')
  if (!confirm) return

  // 调用退出激活接口
  const { data } = await removeLicense()
  activeInfo.value = data
  notifySuccess('已退出激活状态')

  // 刷新页面
  window.location.reload()
}
// #endregion
</script>

<style lang="scss" scoped></style>
