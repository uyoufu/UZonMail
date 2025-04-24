<template>
  <q-expansion-item header-class="text-primary card-like-borderless" popup :icon="icon" :label="label"
    :caption="caption" @before-show="onBeforeShow" group="settings1">
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

        <div v-if="!isValid" class="q-pa-xs q-mb-sm">
          <CommonBtn label="验证邮箱" @click="onValidateNotificationEmail" tooltip="通知邮箱需要验证才会生效" :loading="isValidating" />
        </div>

        <q-separator class="q-mb-sm" />
      </div>

      <div class="row justify-start items-center q-mb-sm">
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
import { updateSystemSettingJson, getSystemSetting } from 'src/api/systemSetting'
import PasswordInput from 'src/components/passwordInput/PasswordInput.vue'
import CommonBtn from 'src/components/quasarWrapper/buttons/CommonBtn.vue'

defineProps({
  label: {
    type: String,
    default: '通知设置'
  },
  caption: {
    type: String,
    default: '整个系统的发件通知相关设置(仅超管有该权限)'
  },
  icon: {
    type: String,
    default: 'notifications'
  }
})

import { useSettingsAutoSaver } from '../compisitions/useSettingsAutoSaver'
const systemSmtpNotificationKey = 'systemSmtpNotification'
interface INotificationSettings {
  email: string
  smtpHost: string
  smtpPort: number
  password: string
  isValid: boolean
}

const settings: Ref<INotificationSettings> = ref({
  email: '',
  smtpHost: '',
  smtpPort: 465,
  password: '',
  isValid: false
})

async function getSettingsAsync (): Promise<INotificationSettings> {
  const { data: fetchedSettings } = await getSystemSetting(systemSmtpNotificationKey)
  if (!fetchedSettings) {
    return settings.value
  }
  return fetchedSettings.json as unknown as INotificationSettings
}

async function updateSettingsAsync () {
  // 批量更新设置
  settings.value.isValid = false
  await updateSystemSettingJson(systemSmtpNotificationKey, settings.value)
}
const { onBeforeShow: onInitSystemNotificationSettings, updateValueSilently } = useSettingsAutoSaver(settings, getSettingsAsync, updateSettingsAsync)

// #region 管理员用户
import { usePermission } from 'src/compositions/permission'
const { isSuperAdmin } = usePermission()
// #endregion

// #region 验证按钮
import { validateNotificationEmailSettings } from 'src/api/notificationSetting'
import { notifySuccess } from 'src/utils/dialog'
const isValidating = ref(false)
const isValid = computed(() => settings.value.isValid)
async function onValidateNotificationEmail () {
  isValidating.value = true
  // 开始验证
  const { data: ok } = await validateNotificationEmailSettings()
  isValidating.value = false
  if (!ok) {
    // 错误在拦截器处理了
    return
  }
  // 暂停更新
  await updateValueSilently(() => {
    settings.value.isValid = true
  })

  notifySuccess('验证成功')
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
// #endregion

async function onBeforeShow () {
  await onInitSystemNotificationSettings()
  await onInitUserNotificationEmail()
}
</script>

<style lang="scss" scoped></style>
