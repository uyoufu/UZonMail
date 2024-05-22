<template>
  <div class="setting-container row justify-center">
    <div class="column justify-center q-gutter-sm" style="max-width: 600px">
      <div>
        <div class="text-subtitle1 q-mb-lg">
          {{ $t('sendInterval') }}
          <q-tooltip>
            {{ $t('sendIntervalTooltip') }}
          </q-tooltip>
        </div>
        <q-range
          v-model="sendInterval"
          :min="2"
          :max="20"
          :step="0.5"
          :left-label-value="sendInterval.min + ' ' + $t('second')"
          :right-label-value="sendInterval.max + ' ' + $t('second')"
          label-always
          style="min-width: 300px" />
      </div>

      <div>
        <div class="text-subtitle1 q-mb-lg">
          {{ $t('maxEmailsPerDay') }}
          <q-tooltip> {{ $t('maxEmailsPerDayTooltip') }} </q-tooltip>
        </div>
        <q-slider
          v-model="maxEmailsPerDay"
          :min="0"
          :max="500"
          :step="10"
          label
          label-always
          :label-value="maxEmailsPerDay ? maxEmailsPerDay : $t('unlimited')"
          style="min-width: 300px" />
      </div>

      <q-checkbox
        v-model="isAutoResend"
        :label="$t('autoResend')"
        color="secondary"
        class="self-start q-ml-xs">
        <q-tooltip> {{ $t('autoResendTooltip') }} </q-tooltip>
      </q-checkbox>

      <q-checkbox
        v-model="sendWithImageAndHtml"
        :label="$t('sendWithImageAndHtml')"
        color="secondary"
        class="self-start q-ml-xs">
        <q-tooltip>
          {{ $t('sendWithImageAndHtmlTooltip') }}
        </q-tooltip>
      </q-checkbox>
    </div>
  </div>
</template>

<script>
import {
  getUserSettings,
  updateSendInterval,
  updateIsAutoResend,
  updateSendWithImageAndHtml,
  updateMaxEmailsPerDay
} from '@/api/setting'

export default {
  data() {
    return {
      sendInterval: {
        min: 3,
        max: 8
      },

      // 每日最大发件量
      maxEmailsPerDay: 0,

      isAutoResend: true,

      sendWithImageAndHtml: false
    }
  },

  watch: {
    async sendInterval(newValue) {
      await updateSendInterval(newValue.min, newValue.max)
    },

    async isAutoResend(newValue) {
      await updateIsAutoResend(newValue)
    },

    async sendWithImageAndHtml(newValue) {
      await updateSendWithImageAndHtml(newValue)
    },

    // 每日最大发件量
    async maxEmailsPerDay(newValue) {
      await updateMaxEmailsPerDay(newValue)
    }
  },

  async mounted() {
    const res = await getUserSettings()
    if (!res.data) return
    const {
      sendInterval_max,
      sendInterval_min,
      isAutoResend,
      sendWithImageAndHtml,

      maxEmailsPerDay
    } = res.data

    this.sendInterval.min = sendInterval_min || 3
    this.sendInterval.max = sendInterval_max || 8
    this.isAutoResend = isAutoResend
    this.sendWithImageAndHtml = sendWithImageAndHtml
    this.maxEmailsPerDay = maxEmailsPerDay || 0
  }
}
</script>

<style lang='scss'>
.setting-container {
  position: absolute;
  top: 0;
  bottom: 0;
  left: 0;
  right: 0;
}
</style>
