<template>
  <q-btn class="q-pr-sm q-py-none" :dense="dense" :color="color" :size="size" :icon="icon" :label="labelValue"
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

const props = defineProps({
  color: {
    type: String,
    default: 'negative'
  },
  icon: {
    type: String,
    default: 'cancel'
  },
  label: {
    type: String,
  },
  tooltip: {
    type: String,
    default: translateButton('cancelCurrentOperation')
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

// #region 响应多语言切换
const labelValue = computed(() => {
  if (!props.label) return translateButton('cancel')
  return props.label
})
// #endregion
</script>

<style lang="scss" scoped></style>
