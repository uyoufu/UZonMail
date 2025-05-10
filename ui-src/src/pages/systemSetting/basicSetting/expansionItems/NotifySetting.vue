<template>
  <q-expansion-item v-model="expanded" header-class="text-primary card-like-borderless" popup :icon="icon"
    :label="label" :caption="caption" @before-show="onBeforeShow" group="settings1">
    <div class="q-pa-md column no-wrap">
      <div v-if="isSuperAdmin" class="column no-wrap">
        <div class="row justify-start items-center q-mb-sm">
          <q-input outlined class="col-auto-4" standout dense v-model="settings.email" :debounce="500" label="系统通知发件邮箱"
            placeholder="当发件完成后,可使用该邮箱发送完成通知">
            <AsyncTooltip tooltip="当发件完成后,可使用该邮箱发送完成通知" />
          </q-input>
          <q-input outlined class="col-auto-4" standout dense v-model="settings.smtpHost" :debounce="500"
            label="通知邮箱Smtp服务器" placeholder="通知邮箱对应的smtp服务器地址">
            <AsyncTooltip tooltip="通知邮箱对应的smtp服务器地址" />
          </q-input>
          <q-input outlined class="col-auto-4" standout dense v-model="settings.smtpPort" :debounce="500" label="通知邮箱端口"
            placeholder="通知邮箱对应的smtp服务器端口">
            <AsyncTooltip tooltip="通知邮箱对应的smtp服务器端口" />
          </q-input>
          <PasswordInput outlined class="col-auto-4" standout noIcon dense v-model="settings.password" :debounce="500"
            label="通知邮箱密码" placeholder="通知邮箱的密码">
            <AsyncTooltip tooltip="通知邮箱的密码" />
          </PasswordInput>
        </div>

        <div class="q-pa-xs q-mb-sm">
          <CommonBtn label="保存设置" @click="onValidateNotificationEmail" tooltip="验证邮箱并保存" :loading="isValidating" />
        </div>
      </div>

      <q-separator v-if="showNormalUserSetting" class="q-mb-sm" />
      <div v-if="showNormalUserSetting" class="row justify-start items-center q-mb-sm">
        <q-input outlined class="col-auto-4" standout dense v-model="userNotificationClientEmail" :debounce="500"
          label="通知接收邮箱" placeholder="当发件完成后, 可通过该邮箱接收完成通知">
          <AsyncTooltip tooltip="当发件完成后, 可通过该邮箱接收完成通知" />
        </q-input>
      </div>
    </div>
  </q-expansion-item>
</template>

<script lang="ts" setup>
// import logger from 'loglevel'

import AsyncTooltip from 'src/components/asyncTooltip/AsyncTooltip.vue'
import PasswordInput from 'src/components/passwordInput/PasswordInput.vue'
import CommonBtn from 'src/components/quasarWrapper/buttons/CommonBtn.vue'

import { AppSettingType } from 'src/api/appSetting'

const props = defineProps({
  label: {
    type: String,
    default: '通知设置'
  },
  caption: {
    type: String,
    default: '整个系统的发件通知相关设置'
  },
  icon: {
    type: String,
    default: 'notifications'
  },
  // 设置类型
  settingType: {
    type: Number as PropType<AppSettingType>,
    default: AppSettingType.System
  }
})

import { useSettingsAutoSaver } from '../compisitions/useSettingsAutoSaver'
import type { INotificationSettings } from 'src/api/notificationSetting'
import { getSmtpNotificationSetting, updateSmtpNotificationSetting } from 'src/api/notificationSetting'

const settings: Ref<INotificationSettings> = ref({
  email: '',
  smtpHost: '',
  smtpPort: 465,
  password: '',
  isValid: false
})

async function onGetSmtpNotificationSettings () {
  const { data } = await getSmtpNotificationSetting(props.settingType)
  settings.value = data
}


// #region 管理员用户
import { usePermission } from 'src/compositions/permission'
const { isSuperAdmin } = usePermission()
// #endregion

// #region 验证按钮
import { notifySuccess, notifyUntil } from 'src/utils/dialog'
const isValidating = ref(false)
async function onValidateNotificationEmail () {
  await notifyUntil(async () => {
    isValidating.value = true

    // 调用升级接口
    // 开始验证
    const { data: ok } = await updateSmtpNotificationSetting(settings.value, props.settingType)
    isValidating.value = false
    if (!ok) {
      // 错误在拦截器处发出了
      return
    }
    settings.value.isValid = true
  }, "通知设置保存", '正在验证邮箱...')

  notifySuccess('保存成功')
}
// #endregion

// #region 普通用户的设置
import { updateUserSettingString, getUserSetting } from 'src/api/userSetting'

const userNotificationClientEmail = ref('')
const notificationRecipientEmailKey = 'notificationRecipientEmail'
async function getUserNotificationEmailSettings (): Promise<string | null | undefined> {
  const { data: fetchedSettings } = await getUserSetting(notificationRecipientEmailKey)
  if (!fetchedSettings) {
    return ""
  }
  return fetchedSettings.stringValue
}
async function updateUserNotificationEmailSettings () {
  await updateUserSettingString(notificationRecipientEmailKey, userNotificationClientEmail.value)
}
const { onBeforeShow: onInitUserNotificationEmail } = useSettingsAutoSaver(userNotificationClientEmail, getUserNotificationEmailSettings, updateUserNotificationEmailSettings)
const showNormalUserSetting = computed(() => {
  return props.settingType === AppSettingType.User
})

// #endregion

// #region 扩展列表
const expanded = ref(false)
async function onBeforeShow () {
  await onGetSmtpNotificationSettings()

  // 只有用户时，才会有用户设置
  if (props.settingType === AppSettingType.User)
    await onInitUserNotificationEmail()
}
watch(() => props.settingType, async () => {
  if (!expanded.value) return

  await onBeforeShow()
})
// #endregion
</script>

<style lang="scss" scoped></style>
