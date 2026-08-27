<template>
  <q-expansion-item v-model="expanded" popup :icon="icon" :label="label" :caption="caption"
    header-class="text-primary card-like-borderless" @before-show="onBeforeShow" group="settings1">
    <div class="row justify-start items-center q-pa-md">
      <q-input outlined class="col-auto-4" standout dense v-model.number="sendingSettingRef.maxSendCountPerEmailDay"
        :debounce="500" type="number" :label="t('pages.basicSettings.maxDailySendPerSenderAccount')" :placeholder="t('pages.basicSettings.zeroMeansUnlimited')">
        <AsyncTooltip :tooltip="[t('pages.basicSettings.maxDailySendTooltip'), t('pages.basicSettings.zeroMeansUnlimited')]" />
      </q-input>

      <q-input outlined class="col-auto-4" standout dense v-model.number="sendingSettingRef.maxSendingBatchSize"
        :debounce="500" type="number" :label="t('pages.basicSettings.maxMergedRecipients')" :placeholder="t('pages.basicSettings.zeroMeansNoMerge')">
        <AsyncTooltip :tooltip="[
          t('pages.basicSettings.mergedRecipientsTooltip'),
          t('pages.basicSettings.zeroMeansUnlimited'),
          t('pages.basicSettings.mergedRecipientsRecommendation')
        ]" />
      </q-input>

      <q-input outlined class="col-auto-4" standout dense v-model.number="sendingSettingRef.minSenderAccountCooldownSecond"
        type="number" :debounce="500" :label="t('pages.basicSettings.minSenderAccountCooldown')" :placeholder="t('pages.basicSettings.zeroMeansUnlimited')">
      </q-input>

      <q-input outlined class="col-auto-4" standout dense v-model.number="sendingSettingRef.maxSenderAccountCooldownSecond"
        type="number" :debounce="500" :label="t('pages.basicSettings.maxSenderAccountCooldown')" :placeholder="t('pages.basicSettings.zeroMeansUnlimited')">
      </q-input>

      <q-input outlined class="col-auto-4" standout dense v-model.number="sendingSettingRef.minimumCooldownHours"
        type="number" :debounce="500" :label="t('pages.basicSettings.minimumRecipientCooldown')" :placeholder="t('pages.basicSettings.zeroMeansUnlimited')">
        <AsyncTooltip :tooltip="t('pages.basicSettings.minimumRecipientCooldownTooltip')" />
      </q-input>

      <q-input outlined class="col-auto-4" standout dense v-model="sendingSettingRef.replyToEmails" :debounce="500"
        :label="t('pages.basicSettings.replyRecipients')" :placeholder="t('pages.basicSettings.replyRecipientsPlaceholder')">
        <AsyncTooltip :tooltip="[t('pages.basicSettings.replyRecipientsTooltip'), t('pages.basicSettings.emptyMeansNotSet'), t('pages.basicSettings.separateEmailsWithComma')]" />
      </q-input>

      <q-input outlined class="col-auto-4" standout dense v-model.number="sendingSettingRef.changeIpAfterEmailCount"
        :debounce="500" type="number" :label="t('pages.basicSettings.maxSendPerProxySenderAccount')" :placeholder="t('pages.basicSettings.zeroMeansUnlimited')">
        <AsyncTooltip :tooltip="[t('pages.basicSettings.maxSendPerProxySenderAccountTooltip'), t('pages.basicSettings.nonPositiveMeansUnlimited')]" />
      </q-input>

      <q-input outlined class="col-auto-4" standout dense v-model.number="sendingSettingRef.maxCountPerIPDomainHour"
        :debounce="500" type="number" :label="t('pages.basicSettings.maxSendPerIpDomainHour')" :placeholder="t('pages.basicSettings.zeroMeansUnlimited')">
        <AsyncTooltip :tooltip="[t('pages.basicSettings.maxSendPerIpDomainHourTooltip'), t('pages.basicSettings.dynamicIpCalculatedSeparately'), t('pages.basicSettings.nonPositiveMeansUnlimited')]" />
      </q-input>

      <q-checkbox class="col-auto-4" dense keep-color v-model="sendingSettingRef.allowDuplicateSending" color="secondary"
        :label="translateBasicSettings('allowDuplicateSending')">
        <AsyncTooltip :tooltip="translateBasicSettings('allowDuplicateSendingTooltip')" />
      </q-checkbox>
    </div>
  </q-expansion-item>
</template>

<script lang="ts" setup>
import AsyncTooltip from 'src/components/asyncTooltip/AsyncTooltip.vue'
import { getSendingSetting, updateSendingSetting } from 'src/api/appSetting'
import { useUserInfoStore } from 'src/stores/user'
import { notifyError, notifySuccess } from 'src/utils/dialog'
import { t, translateBasicSettings } from 'src/i18n/helpers'

import type { ISendingSetting } from 'src/api/appSetting';
import { AppSettingType } from 'src/api/appSetting'
import type { PropType } from 'vue'
import logger from 'loglevel'

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
    default: 'flight_takeoff'
  },

  // 设置类型
  settingType: {
    type: Number as PropType<AppSettingType>,
    default: AppSettingType.System
  }
})

const label = computed(() => props.label ?? t('pages.basicSettings.sendSettings'))
const caption = computed(() => props.caption ?? t('pages.basicSettings.sendSettingsCaption'))

const userInfoStore = useUserInfoStore()
const sendingSettingRef: Ref<ISendingSetting> = ref({
  userId: userInfoStore.userId,
  maxSendCountPerEmailDay: 0,
  minSenderAccountCooldownSecond: 5,
  maxSenderAccountCooldownSecond: 10,
  maxSendingBatchSize: 20,
  minimumCooldownHours: 0,
  replyToEmails: '',
  changeIpAfterEmailCount: 0,
  maxCountPerIPDomainHour: -1,
  allowDuplicateSending: false
})
// 获取设置
let updateSettingSignal = true
const expanded = ref(false)
async function onBeforeShow() {
  logger.debug('[SendingSetting] when onBeforeShow expanded', expanded.value)
  // 获取设置
  const { data: setting } = await getSendingSetting(props.settingType)
  if (setting) {
    updateSettingSignal = false
    sendingSettingRef.value = setting
  }
}

watch(() => props.settingType, async () => {
  if (!expanded.value) return
  await onBeforeShow()
})
watch(
  sendingSettingRef,
  async () => {
    logger.debug('[SendingSetting] when watch expanded', expanded.value)
    // 保存设置
    if (!updateSettingSignal) {
      updateSettingSignal = true
      return
    }

    // 对参数进行验证
    if (!validateSendingSetting()) return

    await updateSendingSetting(sendingSettingRef.value, props.settingType)

    notifySuccess(t('pages.basicSettings.settingsEffective'))
  },
  { deep: true }
)

import { isEmail } from 'src/utils/validator';
function validateSendingSetting() {
  if (sendingSettingRef.value.minSenderAccountCooldownSecond > 0
    && sendingSettingRef.value.maxSenderAccountCooldownSecond < sendingSettingRef.value.minSenderAccountCooldownSecond) {
    notifyError(t('pages.basicSettings.invalidSenderAccountCooldown'))
    return false
  }

  if (sendingSettingRef.value.replyToEmails) {
    const emails = sendingSettingRef.value.replyToEmails.split(',')
    if (emails.some(email => !isEmail(email.trim()))) {
      notifyError(t('pages.basicSettings.invalidReplyRecipients'))
      return false
    }
  }

  return true
}
</script>

<style lang="scss" scoped></style>
