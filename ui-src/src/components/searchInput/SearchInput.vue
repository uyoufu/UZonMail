<template>
  <q-input dense standout="bg-secondary" debounce="300" v-model="filter" placeholder="搜索" :autofocus="false"
    :class="inputClass">
    <template v-slot:append>
      <q-icon name="search" />
    </template>
  </q-input>
</template>

<script lang="ts" setup>
const filter = defineModel<string | undefined | null>()

import { Platform } from 'quasar';
const inputClass = computed(() => {
  return {
    'narrow-input': !Platform.is.desktop
  }
})
</script>

<style lang="scss" scoped>
.narrow-input {
  max-width: 150px;
}
</style>
