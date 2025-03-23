<template>
  <q-expansion-item popup :icon="icon" :label="label" :caption="caption"
    header-class="text-primary card-like-borderless" @before-show="onBeforeShow" group="settings1">
    <div class="q-pa-md">
      <div class="row justify-start items-center q-mb-sm ">
        <q-input outlined class="col" standout dense v-model="outboxSettingRef.maxSendCountPerEmailDay" :debounce="500"
          type="number" label="单个发件箱每日最大发件量" placeholder="为 0 时表示不限制">
          <AsyncTooltip :tooltip="['设置发件箱单日最大发件量', '为 0 表示不限制']" />
        </q-input>

        <q-input outlined class="q-ml-sm col" standout dense v-model="outboxSettingRef.maxSendingBatchSize"
          :debounce="500" type="number" label="合并发件最大数量" placeholder="为 0 时表示不合并">
          <AsyncTooltip :tooltip="['设置多个收件人合并在一起的发件数量', '为 0 表示不限制', '该值不宜过大, 一般 20 左右, 太大会导致发送失败']" />
        </q-input>
      </div>

      <div class="row justify-start items-center q-mb-sm ">
        <q-input outlined class="col" standout dense v-model="outboxSettingRef.minOutboxCooldownSecond" type="number"
          :debounce="500" label="单个发件箱最小发件间隔 (单位: 秒)" placeholder="为 0 时表示不限制">
        </q-input>

        <q-input outlined class="q-ml-sm col" standout dense v-model="outboxSettingRef.maxOutboxCooldownSecond"
          type="number" :debounce="500" label="单个发件箱最大发件间隔 (单位: 秒)" placeholder="为 0 时表示不限制">
        </q-input>
      </div>

      <div class="row justify-start items-center q-mb-sm ">
        <q-input outlined class="col" standout dense v-model="outboxSettingRef.minInboxCooldownHours" type="number"
          :debounce="500" label="最短收件间隔 (单位: h)" placeholder="为 0 时表示不限制">
          <AsyncTooltip tooltip="设置同一个收件箱收件间隔，为 0 表示不限制" />
        </q-input>

        <q-input outlined class="q-ml-sm col" standout dense v-model="outboxSettingRef.replyToEmails" :debounce="500"
          label="回信收件人" placeholder="收件箱回信后的收信邮箱,若有多个使用逗号分隔">

          <AsyncTooltip :tooltip="['设置回信时收信人地址', '为空时表示不设置', '有多个收信地址时,使用英文逗号分隔']" />
        </q-input>
      </div>

      <div class="row justify-start items-center q-mb-sm">
        <q-checkbox dense v-model="outboxSettingRef.enableEmailTracker" label="启用邮件跟踪" color="secondary" class="q-ml-sm"
          keep-color>
          <AsyncTooltip tooltip="开启后，将跟踪邮件的查阅状态"></AsyncTooltip>
        </q-checkbox>
      </div>
    </div>
  </q-expansion-item>
</template>

<script lang="ts" setup>
import AsyncTooltip from 'src/components/asyncTooltip/AsyncTooltip.vue'

import type { IOrganizationSetting } from 'src/api/organizationSetting';
import { getCurrentOrganizationSetting, updateOrganizationSetting } from 'src/api/organizationSetting'
import { useUserInfoStore } from 'src/stores/user'
import { notifySuccess } from 'src/utils/dialog'
// import logger from 'loglevel'

const props = defineProps({
  label: {
    type: String,
    default: '发件设置'
  },
  caption: {
    type: String,
    default: '设置发件间隔、最大发件量等'
  },

  // 获取设置
  getSettingApi: {
    type: Function,
    default: getCurrentOrganizationSetting
  },

  // 更新设置
  updateSettingApi: {
    type: Function,
    default: updateOrganizationSetting
  },

  icon: {
    type: String,
    default: 'flight_takeoff'
  }
})

const userInfoStore = useUserInfoStore()
const outboxSettingRef: Ref<IOrganizationSetting> = ref({
  userId: userInfoStore.userId,
  maxSendCountPerEmailDay: 0,
  minOutboxCooldownSecond: 5,
  maxOutboxCooldownSecond: 10,
  maxSendingBatchSize: 20,
  minInboxCooldownHours: 0,
  replyToEmails: '',
  enableEmailTracker: false
})
// 获取设置
let updateSettingSignal = true
async function onBeforeShow () {
  // 获取设置
  const { data: setting } = await props.getSettingApi()
  if (setting) {
    updateSettingSignal = false
    outboxSettingRef.value = setting
  }
}

watch(outboxSettingRef, async () => {
  // 保存设置
  if (!updateSettingSignal) {
    updateSettingSignal = true
    return
  }

  await props.updateSettingApi(outboxSettingRef.value)

  notifySuccess('设置更改已生效')
}, { deep: true })

import { updateServerBaseApiUrl } from 'src/api/systemSetting'
watch(() => outboxSettingRef.value.enableEmailTracker, async (newValue) => {
  if (!newValue) return
  await updateServerBaseApiUrl()
})
</script>

<style lang="scss" scoped></style>
