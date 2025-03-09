<template>
  <div>
    {{ ellipsisContent }}
    <AsyncTooltip v-if="showEllipsis" :tooltip="tooltips" />
  </div>
</template>

<script lang="ts" setup>
import type { PropType } from 'vue'
import AsyncTooltip from '../asyncTooltip/AsyncTooltip.vue'

const props = defineProps({
  content: {
    type: String,
    required: true
  },
  maxLength: {
    type: Number,
    default: 20
  },
  direction: {
    type: String as PropType<'start' | 'middle' | 'end'>,
    default: 'end'
  }
})

const showEllipsis = computed(() => props.content && props.content.length > props.maxLength)
const ellipsisContent = computed(() => {
  if (!showEllipsis.value) return props.content

  // 按照方向截取
  if (props.direction === 'start') {
    return '...' + props.content.slice(props.content.length - props.maxLength)
  } else if (props.direction === 'middle') {
    const half = Math.floor(props.maxLength / 2)
    return props.content.slice(0, half) + '...' + props.content.slice(props.content.length - half)
  } else {
    return props.content.slice(0, props.maxLength) + '...'
  }
})
const tooltips = computed(() => {
  if (!showEllipsis) return props.content
  return props.content.split('\n')
})

</script>

<style lang="scss" scoped></style>
