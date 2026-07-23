<template>
  <q-btn class="q-pr-sm q-py-none" :dense="dense" :color="color" :size="size" :icon="icon" :label="btnLabel"
    v-bind="$attrs">
    <template v-for="(slot, slotName) in $slots">
      <slot :name="slotName"></slot>
    </template>
    <AsyncTooltip :tooltip="btnTooltip" />
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
    default: 'check_circle'
  },
  label: {
    type: String
  },
  tooltip: {
    type: String
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

const btnLabel = computed(() => {
  if (props.label) return props.label
  return translateButton('confirm')
})

const btnTooltip = computed(() => {
  if (props.tooltip) return props.tooltip
  return translateButton('confirmCurrentOperation')
})

</script>

<style lang="scss" scoped></style>
