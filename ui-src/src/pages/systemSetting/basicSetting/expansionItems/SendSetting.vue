<template>
  <q-expansion-item v-model="expanded" popup :icon="icon" :label="label" :caption="caption"
    header-class="text-primary card-like-borderless" @before-show="onBeforeShow" group="settings1">
    <div class="row justify-start items-center q-pa-md">
      <q-input outlined class="col-auto-4" standout dense v-model.number="outboxSettingRef.maxSendCountPerEmailDay"
        :debounce="500" type="number" label="单个发件箱每日最大发件量" placeholder="为 0 时表示不限制">
        <AsyncTooltip :tooltip="['设置发件箱单日最大发件量', '为 0 表示不限制']" />
      </q-input>

      <q-input outlined class="col-auto-4" standout dense v-model.number="outboxSettingRef.maxSendingBatchSize"
        :debounce="500" type="number" label="合并发件最大数量" placeholder="为 0 时表示不合并">
        <AsyncTooltip :tooltip="[
          '设置多个收件人合并在一起的发件数量',
          '为 0 表示不限制',
          '该值不宜过大, 一般 20 左右, 太大会导致发送失败'
        ]" />
      </q-input>

      <q-input outlined class="col-auto-4" standout dense v-model.number="outboxSettingRef.minOutboxCooldownSecond"
        type="number" :debounce="500" label="单个发件箱最小发件间隔 (单位: 秒)" placeholder="为 0 时表示不限制">
      </q-input>

      <q-input outlined class="col-auto-4" standout dense v-model.number="outboxSettingRef.maxOutboxCooldownSecond"
        type="number" :debounce="500" label="单个发件箱最大发件间隔 (单位: 秒)" placeholder="为 0 时表示不限制">
      </q-input>

      <q-input outlined class="col-auto-4" standout dense v-model.number="outboxSettingRef.minInboxCooldownHours"
        type="number" :debounce="500" label="最短收件间隔 (单位: h)" placeholder="为 0 时表示不限制">
        <AsyncTooltip tooltip="设置同一个收件箱收件间隔，为 0 表示不限制" />
      </q-input>

      <q-input outlined class="col-auto-4" standout dense v-model="outboxSettingRef.replyToEmails" :debounce="500"
        label="回信收件人" placeholder="收件箱回信后的收信邮箱,若有多个使用逗号分隔">
        <AsyncTooltip :tooltip="['设置回信时收信人地址', '为空时表示不设置', '有多个收信地址时,使用英文逗号分隔']" />
      </q-input>

      <q-input outlined class="col-auto-4" standout dense v-model.number="outboxSettingRef.changeIpAfterEmailCount"
        :debounce="500" type="number" label="最大发件数/代理/发件箱" placeholder="为 0 时表示不限制">
        <AsyncTooltip :tooltip="['某个邮箱使用某个代理的最大发件总数', '小于等于 0 时表示不限制']" />
      </q-input>

      <q-input outlined class="col-auto-4" standout dense v-model.number="outboxSettingRef.maxCountPerIPDomainHour"
        :debounce="500" type="number" label="最大发件数/IP/域名/小时" placeholder="为 0 时表示不限制">
        <AsyncTooltip :tooltip="['每个发件域名在当前IP下的每小时最大发数', '动态IP单独计算', '小于等于 0 时表示不限制']" />
      </q-input>
    </div>
  </q-expansion-item>
</template>

<script lang="ts" setup>
import AsyncTooltip from 'src/components/asyncTooltip/AsyncTooltip.vue'
import { getSendingSetting, updateSendingSetting } from 'src/api/appSetting'
import { useUserInfoStore } from 'src/stores/user'
import { notifySuccess } from 'src/utils/dialog'

import type { ISendingSetting } from 'src/api/appSetting';
import { AppSettingType } from 'src/api/appSetting'
import type { PropType } from 'vue'
import logger from 'loglevel'

const props = defineProps({
  label: {
    type: String,
    default: '发件设置'
  },
  caption: {
    type: String,
    default: '设置发件间隔、最大发件量等'
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
  maxCountPerIPDomainHour: -1
})
// 获取设置
let updateSettingSignal = true
const expanded = ref(false)
async function onBeforeShow () {
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

    await updateSendingSetting(outboxSettingRef.value, props.settingType)

    notifySuccess('设置更改已生效')
  },
  { deep: true }
)
</script>

<style lang="scss" scoped></style>
