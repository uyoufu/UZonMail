<template>
  <q-btn class="q-pr-sm q-py-none" :dense="dense" :color="color" :size="size" :icon="icon" :label="labelValue"
    :disable="disable" v-bind="$attrs">
    <template v-for="(slot, slotName) in $slots">
      <slot :name="slotName"></slot>
    </template>
    <AsyncTooltip :tooltip="tooltipValue" :cache="false" />
  </q-btn>
</template>

<script lang="ts" setup>
import AsyncTooltip from 'src/components/asyncTooltip/AsyncTooltip.vue'
import { translateButton } from 'src/i18n/helpers'

const props = defineProps({
  color: {
    type: String,
    default: 'primary'
  },
  icon: {
    type: String,
    default: 'add_circle_outline'
  },
  label: {
    type: String,
  },
  tooltip: {
    type: [String, Array] as PropType<string | string[]>,
    default: translateButton('newItem')
  },
  size: {
    type: String,
    default: 'md'
  },
  dense: {
    type: Boolean,
    default: true
  },
  disable: {
    type: Boolean,
    default: false
  },
  // 禁用时的 tooltip
  tooltipWhenDisabled: {
    type: String,
    default: ''
  }
})

const tooltipValue = computed(() => {
  if (props.disable) return props.tooltipWhenDisabled || props.tooltip
  return props.tooltip
})

const labelValue = computed(() => {
  if (!props.label) return translateButton('new')
  return props.label
})
</script>

<style lang="scss" scoped></style>
