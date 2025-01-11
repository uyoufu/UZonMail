<template>
  <q-tooltip ref="tooltipRef" :class="bgColor" :anchor="anchor" :self="self" transition-show="scale"
    transition-hide="scale" @before-show="onTooltipBeforeShow" v-model="tooltipModel" max-width="40em">
    <div v-for="tip in tooltips" :key="tip">{{ tip }}</div>
  </q-tooltip>
</template>

<script lang="ts" setup>
import { QTooltip } from 'quasar'
import { PropType } from 'vue'
import logger from 'loglevel'

/**
 * 说明
 * 该组件是一个异步组件，可以同步使用，仅在需要时，才会请求数据
 */

const props = defineProps({
  anchor: {
    type: String as PropType<'top left'
      | 'top middle'
      | 'top right'
      | 'top start'
      | 'top end'
      | 'center left'
      | 'center middle'
      | 'center right'
      | 'center start'
      | 'center end'
      | 'bottom left'
      | 'bottom middle'
      | 'bottom right'
      | 'bottom start'
      | 'bottom end'
      | undefined>,
    default: undefined
  },

  self: {
    type: String as PropType<'top left'
      | 'top middle'
      | 'top right'
      | 'top start'
      | 'top end'
      | 'center left'
      | 'center middle'
      | 'center right'
      | 'center start'
      | 'center end'
      | 'bottom left'
      | 'bottom middle'
      | 'bottom right'
      | 'bottom start'
      | 'bottom end'
      | undefined>,
    default: undefined
  },

  // 参数
  params: {
    type: Object,
    default: () => { return {} }
  },

  // 是否缓存
  cache: {
    type: Boolean,
    default: true
  },

  // 可以是字符串，字符串数组，或者是一个返回字符串数组的函数
  tooltip: {
    type: [Array, Function, String] as PropType<Array<string> | ((params?: object) => Promise<string[]>) | string>,
    default: () => []
  },

  // 背景颜色
  color: {
    type: String,
    default: 'primary'
  }
})

const bgColor = computed(() => {
  return `bg-${props.color}`
})

const tooltips: Ref<string[]> = ref([])
let cached = false
const tooltipModel = ref(false)
async function onTooltipBeforeShow () {
  const { params, cache, tooltip } = toRefs(props)
  if (cache.value && cached) {
    if (tooltips.value.length === 0) {
      tooltipModel.value = false
    }
    return
  }
  cached = true

  const tooltipResults: string[] = await generateTooltips(tooltip.value, params.value)
  // 过滤掉空值
  tooltips.value = tooltipResults.filter(item => item)
  if (!tooltips.value || tooltips.value.length === 0) {
    // 隐藏不显示
    tooltipModel.value = false
  }
}

/**
 * 生成提示信息
 * @param tooltip
 * @param params
 */
async function generateTooltips (tooltip: Array<string> | ((params?: object) => Promise<string[]>) | string, params: object) {
  const tooltipResults: string[] = []
  if (typeof tooltip === 'string') {
    tooltipResults.push(tooltip)
  } else if (Array.isArray(tooltip)) {
    tooltipResults.push(...tooltip)
  } else if (typeof tooltip === 'function') {
    const tipItems = await tooltip(params)
    const resultsTemp = await generateTooltips(tipItems, params)
    tooltipResults.push(...resultsTemp)
  }

  logger.debug('[Tooltip] tooltipsResult', tooltipResults)

  return tooltipResults
}
</script>
<style lang="scss" scoped></style>
