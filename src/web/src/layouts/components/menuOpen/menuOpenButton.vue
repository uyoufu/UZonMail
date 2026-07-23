<template>
  <q-btn flat round dense :icon="iconName" class="q-mr-sm" @click="toggleModelValue" />
</template>

<script lang="ts" setup>

const modelValue = defineModel<boolean>()
const iconName = computed(() => modelValue.value ? 'menu_open' : 'menu')
function toggleModelValue () {
  modelValue.value = !modelValue.value
}
</script>

<style lang="scss" scoped></style>
