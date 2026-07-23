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
import { useEllipsisTrimmer } from './useEllipsisTrimmer'
const { ellipsisTrimmer } = useEllipsisTrimmer()

const ellipsisContent = computed(() => {
  if (!showEllipsis.value) return props.content
  return ellipsisTrimmer(props.content, props.direction, props.maxLength)
})
const tooltips = computed(() => {
  if (!showEllipsis) return props.content
  return props.content.split('\n')
})

</script>

<style lang="scss" scoped></style>
