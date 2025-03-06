<template>
  <UserAvatar>
    <HoverableTip class="bg-white">
      <q-list class="text-primary" style="min-width: 100px" dense>
        <q-item clickable @click="onGoToProfile" dense>
          <q-item-section>个人信息</q-item-section>
        </q-item>
        <q-item clickable @click="onLogout" dense>
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

<style lang="scss" scoped></style>
