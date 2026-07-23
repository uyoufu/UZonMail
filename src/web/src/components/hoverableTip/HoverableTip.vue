<template>
  <q-tooltip v-bind="$attrs" class="hoverable-tip q-pa-none hover-scroll" :model-value="showing" max-height="30vh"
    @update:model-value="onUpdate" @mouseenter="onHover" @mouseleave="onLeave" :hide-delay="hideDelay">
    <slot></slot>
  </q-tooltip>
</template>

<script lang="ts" setup>
// 参考：https://github.com/quasarframework/quasar/discussions/13155
import logger from 'loglevel'

const hoverTipValue = defineModel('hoverTipValue', {
  type: Boolean,
  default: false
})
watch(hoverTipValue, (v) => {
  logger.debug('[hoverableTip] hoverTipValue', v)
  showing.value = v
})

import { Platform } from 'quasar'
const hideDelay = ref(Platform.is.desktop ? 0 : 1500)
const showing = ref(false)
const isHovered = ref(false)
// you need to debounce update:modelValue handler
// that serves as a delay
// as hovering in that empty space
// between the parent and the tooltip
// or leaving from parent will trigger its hide behavior
// leaving you no time to hover the tooltip itself
import { debounce } from 'quasar'
const onUpdate = debounce((v) => {
  logger.debug('[hoverableTip] onUpdate', v, isHovered.value)
  if (!isHovered.value) {
    showing.value = v
    // 同步 modelValue 的值
    hoverTipValue.value = v
  }
}, 150)
function onHover () {
  logger.debug('[hoverableTip] onHover')
  isHovered.value = showing.value
}
function onLeave () {
  logger.debug('[hoverableTip] onHover')
  isHovered.value = false
  showing.value = false
}
</script>

<style lang="scss">
.hoverable-tip.q-tooltip {
  pointer-events: auto !important
}
</style>
