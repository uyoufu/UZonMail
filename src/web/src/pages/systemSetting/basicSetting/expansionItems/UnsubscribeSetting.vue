<template>
  <q-expansion-item v-model="expanded" popup icon="unsubscribe" :label="t('pages.basicSettings.unsubscribeSettings')" :caption="t('pages.basicSettings.unsubscribeSettingsCaption')"
    header-class="text-primary card-like-borderless" @before-show="onBeforeShow" group="settings1">
    <div class="column justify-start q-mb-sm q-pa-md">
      <div class="row justify-start items-center">
        <span>{{ t('pages.basicSettings.enableUnsubscribe') }}</span>
        <q-checkbox class="q-ml-sm" color="secondary" keep-color v-model="unsubscribeSetting.enable"
          left-label></q-checkbox>
      </div>

      <div v-if="unsubscribeSetting.enable" class="row justify-start items-center">
        <div>{{ t('pages.basicSettings.unsubscribeCallback') }}</div>

        <div class="q-gutter-sm q-ml-sm">
          <q-radio dense v-model="unsubscribeSetting.type" :val="0" :label="t('pages.basicSettings.system')">
            <AsyncTooltip :tooltip="t('pages.basicSettings.systemUnsubscribeTooltip')" />
          </q-radio>
          <q-radio dense v-model="unsubscribeSetting.type" :val="1" :label="t('pages.basicSettings.externalLink')">
            <AsyncTooltip :tooltip="t('pages.basicSettings.externalUnsubscribeTooltip')" />
          </q-radio>
        </div>

        <q-input v-if="unsubscribeSetting.type" class="q-ml-md" outlined standout
          v-model="unsubscribeSetting.externalUrl" dense :label="t('pages.basicSettings.unsubscribeUrl')">
          <template v-slot:prepend>
            <q-icon name="http" color="secondary" />
          </template>
        </q-input>
      </div>

      <div v-if="enableUnsubscribePageSetting" class="row justify-start items-start">
        <div class="q-mt-sm">{{ t('pages.basicSettings.unsubscribePage') }}</div>
        <UnsubscribePage class="col" />
      </div>

      <OkBtn :label="t('pages.basicSettings.save')" class="self-start q-mt-xs" style="margin-left: 70px" @click="onUnsubscribeSettingSave" />
    </div>
  </q-expansion-item>
</template>

<script lang="ts" setup>
import AsyncTooltip from 'src/components/asyncTooltip/AsyncTooltip.vue'
import UnsubscribePage from './components/UnsubscribePage.vue'
import OkBtn from 'src/components/buttons/OkBtn.vue'

import { useUserInfoStore } from 'src/stores/user'
import { setTimeoutAsync } from 'src/utils/tsUtils'
import logger from 'loglevel'
import { notifySuccess } from 'src/utils/dialog'
import { t } from 'src/i18n/helpers'

import type { IUnsubscribeSettings } from 'src/api/pro/unsubscribe'
import { getUnsubscribeSettings, updateUnsubscribeSettings } from 'src/api/pro/unsubscribe'

const props = defineProps({
  // 设置类型
  settingType: {
    type: Number as PropType<AppSettingType>,
    default: AppSettingType.System
  }
})

const unsubscribeSetting: Ref<IUnsubscribeSettings> = ref({
  enable: false,
  type: 0,
  externalUrl: ''
})

// 获取设置
const userInfoStore = useUserInfoStore()
const expanded = ref(false)
async function onBeforeShow () {
  // 获取设置
  logger.debug('[setting] request unsubescribe setting', userInfoStore)
  // 从后端请求退订设置
  const { data } = await getUnsubscribeSettings(props.settingType)
  unsubscribeSetting.value = data

  await setTimeoutAsync(10)
}
watch(() => props.settingType, async () => {
  if (!expanded.value) return
  await onBeforeShow()
})

const enableUnsubscribePageSetting = computed(() => {
  if (!isOrganizationAdmin.value) return false
  // 只有组织设置才有退订页面设置
  if (props.settingType !== AppSettingType.Organization) return false

  // eslint-disable-next-line @typescript-eslint/no-unsafe-enum-comparison
  return unsubscribeSetting.value.enable && unsubscribeSetting.value.type === 0
})

// #region 保存前端 url
import { updateServerBaseApiUrl } from 'src/api/systemSetting'
import { AppSettingType } from 'src/api/appSetting'
async function onUnsubscribeSettingSave () {
  await updateUnsubscribeSettings(unsubscribeSetting.value, props.settingType)
  // 保存前端的 url
  if (unsubscribeSetting.value.enable) {
    await updateServerBaseApiUrl()
  }

  notifySuccess(t('pages.basicSettings.saveSuccess'))
}
// #endregion

// #region 用户权限
import { usePermission } from 'src/compositions/permission'
const { isOrganizationAdmin } = usePermission()
// #endregion
</script>

<style lang="scss" scoped></style>
