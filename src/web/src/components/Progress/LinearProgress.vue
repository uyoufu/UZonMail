<template>
  <q-linear-progress v-bind="$attrs" :value="value" class="uzon-linear-progress" color="secondary" :style="styleObject">
    <div class="absolute-full flex flex-center">
      <q-badge color="transparent" text-color="accent" :label="progressLabel" />
    </div>
  </q-linear-progress>
</template>

<script lang="ts" setup>
const props = defineProps({
  // 进度值，范围 0-1
  value: {
    type: Number,
    default: 0
  },

  // 进度条宽度，单位 px
  width: {
    type: Number,
    default: 400
  },

  borderRadius: {
    type: Number,
    default: 4
  }
})
const { value } = toRefs(props)
const progressLabel = computed(() => `${(value.value * 100).toFixed(2)}%`)
// 当屏幕宽度大于 width 时，取 width，否则取 100%
const styleObject = computed(() => ({ width: `min(${props.width}px, 100vw)`, borderRadius: `${props.borderRadius}px` }))
</script>

<style lang="scss" scoped>
.uzon-linear-progress {
  font-size: unset;
}
</style>
