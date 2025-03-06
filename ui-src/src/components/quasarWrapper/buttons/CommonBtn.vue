<template>
  <q-btn class="q-py-none" :class="{ 'q-pr-sm': icon && label }" :dense="dense" :color="color" :size="size" :icon="icon"
    :label="label" v-bind="$attrs">
    <template v-for="(slot, slotName) in $slots">
      <slot :name="slotName"></slot>
    </template>
    <AsyncTooltip :tooltip="tooltip" />
  </q-btn>
</template>

<script lang="ts" setup>
import AsyncTooltip from 'src/components/asyncTooltip/AsyncTooltip.vue'

defineProps({
  color: {
    type: String,
    default: 'primary'
  },
  icon: {
    type: String,
    default: undefined
  },
  label: {
    type: String,
    default: ''
  },
  tooltip: {
    type: String,
    default: ''
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
