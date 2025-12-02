<template>
  <q-expansion-item v-model="expanded" popup :icon="icon" :label="label" :caption="caption"
    header-class="text-primary card-like-borderless" @before-show="onBeforeShow" group="settings1">
    <div class="row justify-start items-center q-pa-md">
      <div class="column no-wrap">
        <q-checkbox class="col-auto-4" dense toggle-indeterminate v-model="aiCopilotSettingRef.status"
          :true-value="AppSettingStatus.Enabled" :false-value="AppSettingStatus.Disabled"
          :indeterminate-value="AppSettingStatus.Ignored" :label="translateBasicSettings('enableAIFeatures')"
          color="secondary" keep-color>
          <AsyncTooltip :tooltip="translateBasicSettings('enableAIFeaturesTooltip')"></AsyncTooltip>
        </q-checkbox>

        <div class="row justify-start items-center q-mb-sm">
          <q-input outlined class="col-auto-4" standout dense v-model="aiCopilotSettingRef.providerType" :debounce="500"
            type="number" :label="translateBasicSettings('aiProviderType')">
            <AsyncTooltip :tooltip="translateBasicSettings('aiProviderTooltip')" />
          </q-input>

          <q-input outlined class="col-auto-4" standout dense v-model="aiCopilotSettingRef.endpoint" :debounce="500"
            :label="translateBasicSettings('endPoint')">
          </q-input>

          <PasswordInput outlined class="col-auto-4" standout dense v-model="aiCopilotSettingRef.apiKey" :debounce="500"
            :label="translateBasicSettings('aiKey')" :placeholder="translateBasicSettings('enterYourAiKey')">
            <AsyncTooltip :tooltip="translateBasicSettings('aiKeyTooltip')" />
          </PasswordInput>

          <q-input outlined class="col-auto-4" standout dense v-model="aiCopilotSettingRef.model" :debounce="500"
            :label="translateBasicSettings('aiModel')">
            <AsyncTooltip :tooltip="translateBasicSettings('aiModelTooltip')" />
          </q-input>

          <q-input outlined class="col-auto-4" standout dense type="number"
            v-model.number="aiCopilotSettingRef.maxTokens" :debounce="500" :label="translateBasicSettings('maxTokens')">
          </q-input>
        </div>

        <div class="q-pa-xs q-mb-sm">
          <CommonBtn :label="translateBasicSettings('btn_saveSetting')"
            :tooltip="translateBasicSettings('btn_saveSettingTooltip')" @click="onSaveAISettings" />
        </div>
      </div>
    </div>
  </q-expansion-item>
</template>

<script lang="ts" setup>
import AsyncTooltip from 'src/components/asyncTooltip/AsyncTooltip.vue'
import PasswordInput from 'src/components/passwordInput/PasswordInput.vue'

import { notifySuccess } from 'src/utils/dialog'
import { translateBasicSettings } from 'src/i18n/helpers'

import { AIProviderType, AppSettingStatus, getAICopilotSetting, updateAICopilotSetting } from 'src/api/aiCopilot'
import type { AIProviderSetting } from 'src/api/aiCopilot'

import { AppSettingType } from 'src/api/appSetting'
import type { PropType } from 'vue'

import logger from 'loglevel'

const props = defineProps({
  label: {
    type: String,
    default: translateBasicSettings('aiSettings')
  },
  caption: {
    type: String,
    default: translateBasicSettings('addAiKeyToEnableAiFeatures')
  },

  icon: {
    type: String,
    default: 'smart_toy'
  },

  // 设置类型
  settingType: {
    type: Number as PropType<AppSettingType>,
    default: AppSettingType.System
  }
})

const defaultAISetting: AIProviderSetting = {
  status: AppSettingStatus.Disabled,
  providerType: AIProviderType.OpenAI,
  endpoint: '',
  apiKey: '',
  model: '',
  maxTokens: 8 * 1024,
}
const aiCopilotSettingRef: Ref<AIProviderSetting> = ref(Object.assign({}, defaultAISetting))
// 获取设置
const expanded = ref(false)
async function onBeforeShow () {
  // 获取设置
  const { data: setting } = await getAICopilotSetting(props.settingType)
  if (setting) {
    aiCopilotSettingRef.value = setting
  } else {
    aiCopilotSettingRef.value = Object.assign({}, defaultAISetting)
  }
}

// 类型变化后，更新数据
watch(() => props.settingType, async () => {
  if (!expanded.value) return

  logger.debug('[AISetting] settingType changed:', props.settingType)
  await onBeforeShow()
})

import { notifyUntil } from 'src/utils/dialog'
async function onSaveAISettings () {
  await notifyUntil(async () => {
    // 调用升级接口
    // 开始验证
    await updateAICopilotSetting(aiCopilotSettingRef.value, props.settingType)
  }, translateBasicSettings('notify_updateAISettingsTitle'))

  notifySuccess(translateBasicSettings('notify_updateAISettingsUSuccess'))
}
</script>

<style lang="scss" scoped></style>
