<template>
  <div class="login-container column justify-center bg-white full-height">
    <div class="row justify-center items-center  q-mb-xl animated fadeIn slower col" :class="mobileClass">
      <q-icon :name="resolveSvgFullName('logo-hero')" :size="iconSize" class="q-pa-md animated fadeInUp"></q-icon>

      <div
        class="longin-main q-ma-md q-pa-lg column justify-center items-center border-radius-8 animated fadeInDown hover-card card-like"
        @keyup.enter="onUserLogin">
        <div class="self-center q-mb-lg text-h5 text-secondary welcome-to-uzon-mail">{{ systemConfig.loginWelcome }}
        </div>

        <q-input outlined class="full-width q-mb-md" standout v-model="userId" :label="translateLoginPage('userName')">
          <template v-slot:prepend>
            <q-icon name="person" />
          </template>
        </q-input>

        <q-input outlined class="full-width q-mb-md" standout v-model="password" :label="translateLoginPage('password')"
          :type="isPwd ? 'password' : 'text'">
          <template v-slot:prepend>
            <q-icon name="lock" />
          </template>
          <template v-slot:append>
            <q-icon :name="isPwd ? 'visibility_off' : 'visibility'" class="cursor-pointer" @click="isPwd = !isPwd" />
          </template>
        </q-input>

        <q-btn class="full-width border-radius-8" color="primary" :label="translateLoginPage('signIn')"
          @click="onUserLogin" />
      </div>
    </div>

    <div class="row justify-center items-center q-mb-lg text-secondary">
      <div class="text-primary">{{ translateLoginPage('version') }}:&nbsp;&nbsp;</div>
      <div>{{ translateLoginPage('client') }} - {{ clientVersion }},&nbsp;&nbsp;</div>
      <div :class="serverVersionClass">{{ translateLoginPage('server') }} - {{ serverVersion }}</div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { userLogin } from 'src/api/user'

import { getCurrentLocale, translateLoginPage } from 'src/i18n/helpers'
import { useUserInfoStore } from 'src/stores/user'
import { useRoutesStore } from 'src/stores/routes'
import { notifyError } from 'src/utils/dialog'
import { resolveSvgFullName } from 'src/utils/svgHelper'
import { md5 } from 'src/utils/encrypt'
import logger from 'loglevel'

// 登录界面
const userId = ref('')
const password = ref('')
const isPwd = ref(true)

const router = useRouter()
const routeStore = useRoutesStore()

/**
 * 用户登录
 */
async function onUserLogin () {
  // 验证数据
  if (!userId.value) {
    notifyError(translateLoginPage('pleaseInputUserName'))
    return
  }

  if (!password.value) {
    notifyError(translateLoginPage('pleaseInputPassword'))
    return
  }

  const currentLocale = getCurrentLocale()
  logger.log(`[Login] 用户 ${userId.value} 使用语言 ${currentLocale.value} 登录`)

  // 登录逻辑
  // 1- 请求登录信息，返回用户信息、token、权限信息
  // 2- 保存信息、密码加密后保存，用于解析服务器的密码
  // 3- 跳转到主页或重定向的页面
  const { data: { userInfo, token, access, installedPlugins } } = await userLogin(userId.value, password.value, currentLocale.value)
  logger.debug('[Login] 用户登录信息:', userInfo, token, access)

  const userInfoStore = useUserInfoStore()
  userInfoStore.setInstalledPlugins(installedPlugins)
  userInfoStore.setUserLoginInfo(userInfo, token, access)
  userInfoStore.setSecretKey(md5(password.value))
  routeStore.resetDynamicRoutes()

  logger.log('[Login] 登录成功')
  // 跳转到主页
  await router.push({ path: '/' })
}

// #region 显示版本号
import { useConfig } from 'src/config'
import { getServerVersion } from 'src/api/system'
const config = useConfig()
const clientVersion = ref(config.version)
const serverVersion = ref('connecting...')
onMounted(async () => {
  const { data: version } = await getServerVersion()
  serverVersion.value = version
})
const serverVersionClass = computed(() => {
  return {
    'text-negative': serverVersion.value === 'connecting...'
  }
})
// #endregion

// #region 手机兼容
import { Platform } from 'quasar'
const iconSize = computed(() => {
  return Platform.is.desktop ? '260px' : '180px'
})

const mobileClass = computed(() => {
  return {
    'content-start': Platform.is.mobile,
  }
})
// #endregion

// #region 显示的系统信息
import { useSystemConfig } from 'src/layouts/components/leftSidebar/compositions/useSystemConfig'
const { systemConfig } = useSystemConfig()
// #endregion
</script>

<style lang="scss" scoped>
.login-container {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;

  .longin-main {
    min-width: min(400px, 90%);
  }

  .welcome-to-uzon-mail {
    background: -webkit-linear-gradient(315deg, #42d392 25%, #857bf0);
    background-clip: text;
    -webkit-background-clip: text;
    -webkit-text-fill-color: transparent;
  }
}
</style>
