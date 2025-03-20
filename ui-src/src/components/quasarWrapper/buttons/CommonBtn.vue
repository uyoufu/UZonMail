<template>
  <q-btn class="q-py-none" :class="{ 'q-pr-sm': icon && label }" :dense="dense" :color="color" :size="size" :icon="icon"
    :label="label" v-bind="$attrs">
    <template v-for="(slot, slotName) in $slots">
      <slot :name="slotName"></slot>
    </template>
    <AsyncTooltip :tooltip="tooltip" :cache="cacheTip" />
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
  // 是否缓存
  cacheTip: {
    type: Boolean,
    default: true
  },

  // 可以是字符串，字符串数组，或者是一个返回字符串数组的函数
  tooltip: {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    type: [Array, Function, String] as PropType<Array<any> | ((params?: object) => Promise<string[]>) | string>,
    default: () => []
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
