<template>
  <q-expansion-item v-model="expanded" popup :icon="icon" :label="label" :caption="caption"
    header-class="text-primary card-like-borderless" @before-show="onBeforeShow" group="settings1">
    <div class="row justify-start items-center q-pa-md">
      <q-input outlined class="col-auto-4" standout dense v-model.number="outboxSettingRef.maxSendCountPerEmailDay"
        :debounce="500" type="number" :label="t('pages.basicSettings.maxDailySendPerOutbox')" :placeholder="t('pages.basicSettings.zeroMeansUnlimited')">
        <AsyncTooltip :tooltip="[t('pages.basicSettings.maxDailySendTooltip'), t('pages.basicSettings.zeroMeansUnlimited')]" />
      </q-input>

      <q-input outlined class="col-auto-4" standout dense v-model.number="outboxSettingRef.maxSendingBatchSize"
        :debounce="500" type="number" :label="t('pages.basicSettings.maxMergedRecipients')" :placeholder="t('pages.basicSettings.zeroMeansNoMerge')">
        <AsyncTooltip :tooltip="[
          t('pages.basicSettings.mergedRecipientsTooltip'),
          t('pages.basicSettings.zeroMeansUnlimited'),
          t('pages.basicSettings.mergedRecipientsRecommendation')
        ]" />
      </q-input>

      <q-input outlined class="col-auto-4" standout dense v-model.number="outboxSettingRef.minOutboxCooldownSecond"
        type="number" :debounce="500" :label="t('pages.basicSettings.minOutboxCooldown')" :placeholder="t('pages.basicSettings.zeroMeansUnlimited')">
      </q-input>

      <q-input outlined class="col-auto-4" standout dense v-model.number="outboxSettingRef.maxOutboxCooldownSecond"
        type="number" :debounce="500" :label="t('pages.basicSettings.maxOutboxCooldown')" :placeholder="t('pages.basicSettings.zeroMeansUnlimited')">
      </q-input>

      <q-input outlined class="col-auto-4" standout dense v-model.number="outboxSettingRef.minInboxCooldownHours"
        type="number" :debounce="500" :label="t('pages.basicSettings.minInboxCooldown')" :placeholder="t('pages.basicSettings.zeroMeansUnlimited')">
        <AsyncTooltip :tooltip="t('pages.basicSettings.minInboxCooldownTooltip')" />
      </q-input>

      <q-input outlined class="col-auto-4" standout dense v-model="outboxSettingRef.replyToEmails" :debounce="500"
        :label="t('pages.basicSettings.replyRecipients')" :placeholder="t('pages.basicSettings.replyRecipientsPlaceholder')">
        <AsyncTooltip :tooltip="[t('pages.basicSettings.replyRecipientsTooltip'), t('pages.basicSettings.emptyMeansNotSet'), t('pages.basicSettings.separateEmailsWithComma')]" />
      </q-input>

      <q-input outlined class="col-auto-4" standout dense v-model.number="outboxSettingRef.changeIpAfterEmailCount"
        :debounce="500" type="number" :label="t('pages.basicSettings.maxSendPerProxyOutbox')" :placeholder="t('pages.basicSettings.zeroMeansUnlimited')">
        <AsyncTooltip :tooltip="[t('pages.basicSettings.maxSendPerProxyOutboxTooltip'), t('pages.basicSettings.nonPositiveMeansUnlimited')]" />
      </q-input>

      <q-input outlined class="col-auto-4" standout dense v-model.number="outboxSettingRef.maxCountPerIPDomainHour"
        :debounce="500" type="number" :label="t('pages.basicSettings.maxSendPerIpDomainHour')" :placeholder="t('pages.basicSettings.zeroMeansUnlimited')">
        <AsyncTooltip :tooltip="[t('pages.basicSettings.maxSendPerIpDomainHourTooltip'), t('pages.basicSettings.dynamicIpCalculatedSeparately'), t('pages.basicSettings.nonPositiveMeansUnlimited')]" />
      </q-input>

      <q-checkbox class="col-auto-4" dense keep-color v-model="outboxSettingRef.allowDuplicateSending" color="secondary"
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
const outboxSettingRef: Ref<ISendingSetting> = ref({
  userId: userInfoStore.userId,
  maxSendCountPerEmailDay: 0,
  minOutboxCooldownSecond: 5,
  maxOutboxCooldownSecond: 10,
  maxSendingBatchSize: 20,
  minInboxCooldownHours: 0,
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
    outboxSettingRef.value = setting
  }
}

watch(() => props.settingType, async () => {
  if (!expanded.value) return
  await onBeforeShow()
})
watch(
  outboxSettingRef,
  async () => {
    logger.debug('[SendingSetting] when watch expanded', expanded.value)
    // 保存设置
    if (!updateSettingSignal) {
      updateSettingSignal = true
      return
    }

    // 对参数进行验证
    if (!validateOutboxSetting()) return

    await updateSendingSetting(outboxSettingRef.value, props.settingType)

    notifySuccess(t('pages.basicSettings.settingsEffective'))
  },
  { deep: true }
)

import { isEmail } from 'src/utils/validator';
function validateOutboxSetting() {
  if (outboxSettingRef.value.minOutboxCooldownSecond > 0
    && outboxSettingRef.value.maxOutboxCooldownSecond < outboxSettingRef.value.minOutboxCooldownSecond) {
    notifyError(t('pages.basicSettings.invalidOutboxCooldown'))
    return false
  }

  if (outboxSettingRef.value.replyToEmails) {
    const emails = outboxSettingRef.value.replyToEmails.split(',')
    if (emails.some(email => !isEmail(email.trim()))) {
      notifyError(t('pages.basicSettings.invalidReplyRecipients'))
      return false
    }
  }

  return true
}
</script>

<style lang="scss" scoped></style>
