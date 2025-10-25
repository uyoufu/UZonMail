<template>
  <q-btn class="q-pr-sm q-py-none" :dense="dense" :color="color" :size="size" :icon="icon" :label="label"
    v-bind="$attrs">
    <template v-for="(slot, slotName) in $slots">
      <slot :name="slotName"></slot>
    </template>
    <AsyncTooltip :tooltip="tooltip" />
  </q-btn>
</template>

<script lang="ts" setup>
import AsyncTooltip from 'src/components/asyncTooltip/AsyncTooltip.vue'
import { translateButton } from 'src/i18n/helpers'

defineProps({
  color: {
    type: String,
    default: 'negative'
  },
  icon: {
    type: String,
    default: 'delete'
  },
  label: {
    type: String,
    default: translateButton('delete')
  },
  tooltip: {
    type: String,
    default: translateButton('deletetItem')
  },
  size: {
    type: String,
    default: 'md'
  },
  dense: {
    type: Boolean,
    default: true
  }
})

</script>

<style lang="scss" scoped></style>
