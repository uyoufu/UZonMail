<template>
  <q-expansion-item v-model="expanded" popup :icon="icon" :label="label" :caption="caption"
    header-class="text-primary card-like-borderless" @before-show="onBeforeShow" group="settings1">
    <div class="row justify-start items-center q-pa-md">
      <q-checkbox class="col-auto-4" dense toggle-indeterminate v-model="emailTrackingSettingRef.enableEmailTracker"
        :label="t('pages.basicSettings.enableEmailTracking')" color="secondary" keep-color>
        <AsyncTooltip :tooltip="t('pages.basicSettings.emailTrackingTooltip')"></AsyncTooltip>
      </q-checkbox>
    </div>
  </q-expansion-item>
</template>

<script lang="ts" setup>
import AsyncTooltip from 'src/components/asyncTooltip/AsyncTooltip.vue'
import { notifySuccess } from 'src/utils/dialog'
import { t } from 'src/i18n/helpers'

import { getEmailTrackingSetting, updateEmailTrackingSetting } from 'src/api/pro/emailTracker'
import type { IEmailTrackingSetting } from 'src/api/pro/emailTracker'
import { AppSettingType } from 'src/api/appSetting'
import type { PropType } from 'vue'

// import logger from 'loglevel'

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
    default: 'timeline'
  },

  // 设置类型
  settingType: {
    type: Number as PropType<AppSettingType>,
    default: AppSettingType.System
  }
})

const label = computed(() => props.label ?? t('pages.basicSettings.emailTracking'))
const caption = computed(() => props.caption ?? t('pages.basicSettings.emailTrackingCaption'))

const emailTrackingSettingRef: Ref<IEmailTrackingSetting> = ref({
  enableEmailTracker: false,
})
// 获取设置
let updateSettingSignal = true
const expanded = ref(false)
async function onBeforeShow () {
  // 获取设置
  const { data: setting } = await getEmailTrackingSetting(props.settingType)
  if (setting) {
    updateSettingSignal = false
    emailTrackingSettingRef.value = setting
  }
}

// 类型变化后，更新数据
watch(() => props.settingType, async () => {
  if (!expanded.value) return
  await onBeforeShow()
})
watch(
  emailTrackingSettingRef,
  async () => {
    // 保存设置
    if (!updateSettingSignal) {
      updateSettingSignal = true
      return
    }

    await updateEmailTrackingSetting(emailTrackingSettingRef.value, props.settingType)

    notifySuccess(t('pages.basicSettings.settingsEffective'))
  },
  { deep: true }
)

// 同时更新服务器的基本API地址
import { updateServerBaseApiUrl } from 'src/api/systemSetting'
watch(
  () => emailTrackingSettingRef.value.enableEmailTracker,
  async (newValue) => {
    if (!newValue) return
    await updateServerBaseApiUrl()
  }
)
</script>

<style lang="scss" scoped></style>
