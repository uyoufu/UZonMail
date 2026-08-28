<template>
  <q-tabs v-bind="$attrs" inline-label align="left" dense class="text-grey q-mb-sm" active-color="primary" indicator-color="primary"
    narrow-indicator>
    <q-tab v-for="tab in tabs" :key="tab.name" :name="tab.name"  :label="tab.label" :icon="tab.icon">
      <AsyncTooltip v-if="tab.tooltip" :tooltip="tab.tooltip" />
    </q-tab>
  </q-tabs>
</template>

<script lang="ts" setup>
import type { IUTabOptions } from './types';

defineProps({
  tabs: {
    type: Array as PropType<IUTabOptions[]>,
    default: () => []
  }
})
</script>

<style lang="scss" scoped></style>
