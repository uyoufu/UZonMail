<template>
  <span class="clickable-text hover-underline" @click="onClick">
    <slot name="default">
      <span>
        {{ ellipsisContent }}
        <AsyncTooltip :tooltip="tooltips" :cache="cacheTip" :params="tipParams"></AsyncTooltip>
      </span>
    </slot>
  </span>
</template>

<script lang="ts" setup>
import AsyncTooltip from '../asyncTooltip/AsyncTooltip.vue'
import { useEllipsisTrimmer } from '../ellipsisContent/useEllipsisTrimmer'
const { ellipsisTrimmer } = useEllipsisTrimmer()

const props = defineProps({
  text: {
    type: String,
    default: ""
  },

  // 显示的最大长度
  // 若为 0，表示不限制
  maxLength: {
    type: Number,
    default: 20
  },

  // 省略号的方向
  ellipsis: {
    type: String as PropType<'start' | 'middle' | 'end'>,
    default: 'end'
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
    default: () => ['单击查看']
  },

  // 若提示是函数，则传入的参数
  tipParams: {
    type: Object,
    default: () => { return {} }
  },
})

const emit = defineEmits(['click'])
function onClick () {
  // 触发事件
  emit("click", { text: props.text })
}

const ellipsisContent = computed(() => {
  if (props.maxLength <= 0) return props.text
  return ellipsisTrimmer(props.text, props.ellipsis, props.maxLength)
})

const isEllipsis = computed(() => {
  if (props.maxLength <= 0) return false
  return props.text && props.text.length > props.maxLength
})

const tooltips = computed(() => {
  console.log("tooltips", props.tooltip, isEllipsis.value)

  if (!isEllipsis.value) return props.tooltip

  const results = []
  if (!Array.isArray(props.tooltip)) {
    results.push(props.tooltip)
  } else {
    results.push(...props.tooltip)
  }

  results.push(props.tooltip)
  return results
})
</script>

<style lang="scss" scoped>
.clickable-text {
  color: $primary;
}
</style>
