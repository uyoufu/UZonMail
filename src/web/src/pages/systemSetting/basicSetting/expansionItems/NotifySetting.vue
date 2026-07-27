<template>
  <q-expansion-item v-model="expanded" header-class="text-primary card-like-borderless" popup :icon="icon"
    :label="label" :caption="caption" @before-show="onBeforeShow" group="settings1">
    <div class="q-pa-md column no-wrap">
      <div v-if="isSuperAdmin" class="column no-wrap">
        <div class="row justify-start items-center q-mb-sm">
          <q-input outlined class="col-auto-4" standout dense v-model="settings.email" :debounce="500" :label="t('pages.basicSettings.systemNotificationEmail')"
            :placeholder="t('pages.basicSettings.systemNotificationEmailPlaceholder')">
            <AsyncTooltip :tooltip="t('pages.basicSettings.systemNotificationEmailPlaceholder')" />
          </q-input>
          <q-input outlined class="col-auto-4" standout dense v-model="settings.smtpHost" :debounce="500"
            :label="t('pages.basicSettings.notificationSmtpHost')" :placeholder="t('pages.basicSettings.notificationSmtpHostPlaceholder')">
            <AsyncTooltip :tooltip="t('pages.basicSettings.notificationSmtpHostPlaceholder')" />
          </q-input>
          <q-input outlined class="col-auto-4" standout dense v-model="settings.smtpPort" :debounce="500" :label="t('pages.basicSettings.notificationSmtpPort')"
            :placeholder="t('pages.basicSettings.notificationSmtpPortPlaceholder')">
            <AsyncTooltip :tooltip="t('pages.basicSettings.notificationSmtpPortPlaceholder')" />
          </q-input>
          <PasswordInput outlined class="col-auto-4" standout noIcon dense v-model="settings.password" :debounce="500"
            :label="t('pages.basicSettings.notificationPassword')" :placeholder="t('pages.basicSettings.notificationPasswordPlaceholder')">
            <AsyncTooltip :tooltip="t('pages.basicSettings.notificationPasswordPlaceholder')" />
          </PasswordInput>
        </div>

        <div class="q-pa-xs q-mb-sm">
          <CommonBtn :label="t('pages.basicSettings.saveSettings')" @click="onValidateNotificationEmail" :tooltip="t('pages.basicSettings.validateEmailAndSave')" :loading="isValidating" />
        </div>
      </div>

      <q-separator v-if="showNormalUserSetting" class="q-mb-sm" />
      <div v-if="showNormalUserSetting" class="row justify-start items-center q-mb-sm">
        <q-input outlined class="col-auto-4" standout dense v-model="userNotificationClientEmail" :debounce="500"
          :label="t('pages.basicSettings.notificationRecipientEmail')" :placeholder="t('pages.basicSettings.notificationRecipientEmailPlaceholder')">
          <AsyncTooltip :tooltip="t('pages.basicSettings.notificationRecipientEmailPlaceholder')" />
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
import { t } from 'src/i18n/helpers'

const props = defineProps({
  label: {
    type: String,
    default: undefined
  },
  caption: {
    type: String,
    default: undefined
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

const label = computed(() => props.label ?? t('pages.basicSettings.notificationSettings'))
const caption = computed(() => props.caption ?? t('pages.basicSettings.notificationSettingsCaption'))

import { useSettingsAutoSaver } from '../compositions/useSettingsAutoSaver'
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
  }, t('pages.basicSettings.notificationSettingsSave'), t('pages.basicSettings.validatingEmail'))

  notifySuccess(t('pages.basicSettings.saveSuccess'))
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
