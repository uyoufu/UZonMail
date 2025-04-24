<template>
  <q-expansion-item
    popup
    :icon="icon"
    :label="label"
    :caption="caption"
    header-class="text-primary card-like-borderless"
    @before-show="onBeforeShow"
    group="settings1"
  >
    <div class="q-pa-md">
      <div class="row justify-start items-center q-mb-sm">
        <q-input
          outlined
          class="col-xs-12 col-sm-6 col-md-4 col-lg-3 q-pa-xs"
          standout
          dense
          v-model="count"
          :debounce="500"
          type="number"
          label="单代理每个发件箱发件数"
          placeholder="为 0 时表示不限制"
        >
          <AsyncTooltip :tooltip="['某个邮箱使用某个代理的发件总数', '为 0 表示不限制']" />
        </q-input>
      </div>
    </div>
  </q-expansion-item>
</template>

<script lang="ts" setup>
import AsyncTooltip from 'src/components/asyncTooltip/AsyncTooltip.vue'

import { getChangeIpAfterEmailCount, updateChangeIpAfterEmailCount } from 'src/api/organizationSetting'
import { notifySuccess } from 'src/utils/dialog'
// import logger from 'loglevel'

defineProps({
  label: {
    type: String,
    default: '代理设置'
  },
  caption: {
    type: String,
    default: '动态代理相关的设置'
  },
  icon: {
    type: String,
    default: 'public'
  }
})

const count = ref(-1)
// 获取设置
let updateSettingSignal = true
async function onBeforeShow() {
  // 获取设置
  const { data } = await getChangeIpAfterEmailCount()
  updateSettingSignal = false
  count.value = data
}

watch(
  count,
  async () => {
    // 保存设置
    if (!updateSettingSignal) {
      updateSettingSignal = true
      return
    }

    await updateChangeIpAfterEmailCount(count.value)

    notifySuccess('设置更改已生效')
  },
  { deep: true }
)
</script>

<style lang="scss" scoped></style>
