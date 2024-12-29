<template>
  <q-input dense standout="bg-secondary" debounce="300" v-model="filter" placeholder="搜索"
    :autofocus="false">
    <template v-slot:append>
      <q-icon name="search" />
    </template>
  </q-input>
</template>

<script lang="ts" setup>
const filter = defineModel<string | undefined | null>()
</script>

<style lang="scss" scoped></style>
