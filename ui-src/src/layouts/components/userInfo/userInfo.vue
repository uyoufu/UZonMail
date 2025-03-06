<template>
  <UserAvatar>
    <HoverableTip class="bg-white" anchor="bottom right" self="top right" :offset="[0, 8]">
      <q-list class="rounded-borders shadow-1 user-info-menu" bordered separator
        style="min-width: 80px;overflow: hidden;" dense>
        <q-item clickable @click="onGoToProfile" class="active-item text-primary">
          <q-item-section>个人信息</q-item-section>
        </q-item>
        <q-item clickable @click="onLogout" class="active-item text-negative">
          <q-item-section>退出</q-item-section>
        </q-item>
      </q-list>
    </HoverableTip>
  </UserAvatar>
</template>

<script lang="ts" setup>
import UserAvatar from 'src/components/userAvatar/UserAvatar.vue'
import HoverableTip from 'src/components/hoverableTip/HoverableTip.vue'

import { useUserInfoStore } from 'src/stores/user'
const userInfoStore = useUserInfoStore()
function onLogout () {
  userInfoStore.logout()
}
const router = useRouter()
function onGoToProfile () {
  router.push({
    path: '/user/profile'
  })
}
</script>

<style lang="scss" scoped>
.user-info-menu {
  overflow: hidden;
  font-size: 14px;

  .active-item {
    &:hover {
      // 放大
      scale: 1.1;
      transition: all 0.3s;
    }
  }
}
</style>
