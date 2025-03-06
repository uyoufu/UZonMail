<template>
  <q-btn class="q-pr-sm q-py-none" :dense="dense" :color="color" :size="size" :icon="icon" :label="label"
    :disable="disable" v-bind="$attrs">
    <template v-for="(slot, slotName) in $slots">
      <slot :name="slotName"></slot>
    </template>
    <AsyncTooltip :tooltip="tooltipValue" />
  </q-btn>
</template>

<script lang="ts" setup>
import AsyncTooltip from 'src/components/asyncTooltip/AsyncTooltip.vue'
import { PropType } from 'vue'

const props = defineProps({
  color: {
    type: String,
    default: 'secondary'
  },
  icon: {
    type: String,
    default: 'note_add'
  },
  label: {
    type: String,
    default: '导入'
  },
  tooltip: {
    type: [String, Array] as PropType<string | string[]>,
    default: '导入数据'
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

</script>

<style lang="scss" scoped></style>
