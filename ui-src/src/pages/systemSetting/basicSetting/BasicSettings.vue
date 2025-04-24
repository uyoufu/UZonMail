<template>
  <q-list class="basic-settings-container">
    <NotifySetting />
    <SendSetting />
    <ProxySetting />
    <UnsubscribeSetting v-if="isEnterpriseUser" />
  </q-list>
</template>

<script lang="ts" setup>
import NotifySetting from './expansionItems/NotifySetting.vue'
import SendSetting from './expansionItems/SendSetting.vue'
import UnsubscribeSetting from './expansionItems/UnsubscribeSetting.vue'
import ProxySetting from './expansionItems/ProxySetting.vue'

import { usePermission } from 'src/compositions/permission'
const { hasEnterpriseAccess } = usePermission()
const isEnterpriseUser = hasEnterpriseAccess()
</script>

<style lang="scss" scoped></style>

<style lang="scss">
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

