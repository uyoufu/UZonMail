<template>
  <div>
    <q-tabs v-if="showSettingTabs" v-model="settingType" align="left" dense class="text-grey q-mb-sm"
      active-color="primary" indicator-color="primary" narrow-indicator>
      <q-tab v-for="settingType in settingTypes" :key="settingType.name" :name="settingType.name"
        :label="settingType.label">
        <AsyncTooltip :tooltip="settingType.tooltip" />
      </q-tab>
    </q-tabs>

    <q-list class="basic-settings-container">
      <NotifySetting :setting-type="currentSettingType?.type" />
      <SendSetting :setting-type="currentSettingType?.type" />
      <EmailTrackingSetting v-if="isEnterpriseUser" :setting-type="currentSettingType?.type" />
      <UnsubscribeSetting v-if="isEnterpriseUser" :setting-type="currentSettingType?.type" />
    </q-list>
  </div>
</template>

<script lang="ts" setup>
import { AppSettingType } from 'src/api/appSetting'
import AsyncTooltip from 'src/components/asyncTooltip/AsyncTooltip.vue'
import NotifySetting from './expansionItems/NotifySetting.vue'
import SendSetting from './expansionItems/SendSetting.vue'

import EmailTrackingSetting from './expansionItems/EmailTrackingSetting.vue'
import UnsubscribeSetting from './expansionItems/UnsubscribeSetting.vue'

import { usePermission } from 'src/compositions/permission'
const { hasEnterpriseAccess, isSuperAdmin, isOrganizationAdmin } = usePermission()
const isEnterpriseUser = hasEnterpriseAccess()

const allSettingTypes = ref([
  {
    name: 'system',
    label: '系统',
    vif: isSuperAdmin.value,
    type: AppSettingType.System,
    tooltip: '系统设置'
  },
  {
    name: 'organization',
    label: '组织',
    vif: isOrganizationAdmin.value,
    type: AppSettingType.Organization,
    tooltip: '组织设置，该设置会覆盖系统设置'
  },
  {
    name: 'user',
    label: '用户',
    type: AppSettingType.User,
    tooltip: '用户设置，该设置会覆盖组织设置',
  }
])
// 不同的用户具有不同的权限
// 1. 系统管理员具有三者所有
// 2. 组织管理员具有组织和个人权限
// 3. 普通用户只有个人权限
const settingTypes = computed(() => {
  return allSettingTypes.value.filter(x => x.vif !== false)
})
const showSettingTabs = computed(() => {
  // 如果不是企业级用户，则只有用户设置一项
  if (!isEnterpriseUser) {
    return false
  }

  return settingTypes.value.length > 1
})

const settingType = ref('user')
const currentSettingType = computed(() => {
  return allSettingTypes.value.find(x => x.name === settingType.value)
})
</script>

<style lang="scss" scoped></style>

<style lang="scss">
// 优化：设置项的样式
.basic-settings-container {
  .q-expansion-item {
    padding-bottom: 6px;

    .q-expansion-item__container {
      border-radius: 6px;
      background-color: white;

      .q-item {
        background-color: unset;
      }
    }
  }
}
</style>
