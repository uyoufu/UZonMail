<template>
  <q-avatar color="primary" class="text-white" :size="size">
    <img v-if="userAvatar" :src="userAvatar">
    <span v-else>{{ firstLetter }}</span>
    <slot name="default"></slot>
  </q-avatar>
</template>

<script lang="ts" setup>
import { useUserInfoStore } from 'src/stores/user'
defineProps({
  size: {
    type: String,
    default: 'lg'
  }
})

const userInfoStore = useUserInfoStore()
const { userId, userAvatar } = storeToRefs(userInfoStore)
const firstLetter = computed(() => userId.value?.charAt(0).toUpperCase())
</script>

<style lang="scss" scoped></style>
