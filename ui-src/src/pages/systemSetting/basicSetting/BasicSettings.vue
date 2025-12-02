<template>
  <div>
    <UTabs v-if="showSettingTabs" v-model="settingType" :tabs="settingTypes"></UTabs>

    <q-list class="basic-settings-container">
      <NotifySetting :setting-type="currentSettingType?.type" />
      <SendSetting :setting-type="currentSettingType?.type" />
      <EmailTrackingSetting v-if="isEnterpriseUser" :setting-type="currentSettingType?.type" />
      <UnsubscribeSetting v-if="isEnterpriseUser" :setting-type="currentSettingType?.type" />
      <AISetting :setting-type="currentSettingType?.type" />
    </q-list>
  </div>
</template>

<script lang="ts" setup>
import { AppSettingType } from 'src/api/appSetting'
import UTabs from 'src/components/quasarWrapper/UTabs.vue'
import { translateBasicSettings } from 'src/i18n/helpers'

import NotifySetting from './expansionItems/NotifySetting.vue'
import SendSetting from './expansionItems/SendSetting.vue'
import EmailTrackingSetting from './expansionItems/EmailTrackingSetting.vue'
import UnsubscribeSetting from './expansionItems/UnsubscribeSetting.vue'
import AISetting from './expansionItems/AISetting.vue'

import { usePermission } from 'src/compositions/permission'
const { hasEnterpriseAccess, isSuperAdmin, isOrganizationAdmin } = usePermission()
const isEnterpriseUser = hasEnterpriseAccess()

const allSettingTypes = ref([
  {
    name: 'system',
    label: translateBasicSettings('tabSystem'),
    vif: isSuperAdmin.value,
    type: AppSettingType.System,
    tooltip: translateBasicSettings('tabSystemTooltip')
  },
  {
    name: 'organization',
    label: translateBasicSettings('tabOrganization'),
    vif: isOrganizationAdmin.value,
    type: AppSettingType.Organization,
    tooltip: translateBasicSettings('tabOrganizationTooltip')
  },
  {
    name: 'user',
    label: translateBasicSettings('tabUser'),
    type: AppSettingType.User,
    tooltip: translateBasicSettings('tabUserTooltip'),
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
