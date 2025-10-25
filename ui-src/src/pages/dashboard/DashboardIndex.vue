<template>
  <div ref="containerElementRef" class="row justify-around q-pt-sm">
    <div class="col-auto-2 q-pa-sm" style="height: 250px;">
      <div id="chart-outbox" class="full-height full-width card-like"></div>
    </div>
    <div class="col-auto-2 q-pa-sm" style="height: 250px;">
      <div id="chart-inbox" class="full-height full-width card-like"></div>
    </div>
    <div id="chart-monthly" class="col-auto-2 q-pa-sm card-like q-ma-sm" style="height: 250px;"></div>
  </div>
</template>

<script lang="ts" setup>
import logger from 'loglevel'

// #region echarts
// 参考：https://echarts.apache.org/handbook/zh/basics/import/
// 引入 echarts 核心模块，核心模块提供了 echarts 使用必须要的接口。
import type { EChartsOption, EChartsType } from 'echarts/types/dist/shared'
import * as echarts from 'echarts/core'
// 引入柱状图图表，图表后缀都为 Chart
import { BarChart, LineChart } from 'echarts/charts'
// 引入标题，提示框，直角坐标系，数据集，内置数据转换器组件，组件后缀都为 Component
import {
  TitleComponent,
  TooltipComponent,
  GridComponent,
  DatasetComponent,
  TransformComponent,
  DataZoomComponent
} from 'echarts/components'
// 标签自动布局、全局过渡动画等特性
import { LabelLayout, UniversalTransition } from 'echarts/features'
// 引入 Canvas 渲染器，注意引入 CanvasRenderer 或者 SVGRenderer 是必须的一步
import { CanvasRenderer } from 'echarts/renderers'

// 注册必须的组件
echarts.use([
  TitleComponent,
  TooltipComponent,
  GridComponent,
  DatasetComponent,
  TransformComponent,
  DataZoomComponent,
  BarChart,
  LineChart,
  LabelLayout,
  UniversalTransition,
  CanvasRenderer
])
// #endregion


const initCharts: {
  name: string,
  chart: EChartsType
}[] = []
onUnmounted(() => {
  initCharts.forEach(x => {
    x.chart.dispose()
  })
})

import type { IEmailCount, IMonthlySendingInfo } from 'src/api/statistics'
import { getInboxEmailCountStatistics, getOutboxEmailCountStatistics, getMonthlySendingCountInfo } from 'src/api/statistics'
const outboxesCount: Ref<IEmailCount[]> = ref([])
// 渲染 outbox 数量柱状图
function renderOutboxCountBar () {
  const barChart = initCharts.find(x => x.name === 'outbox')?.chart
  logger.log('[Dashboard] barChart:', barChart, initCharts)
  // 开始渲染
  // 基于准备好的dom，初始化echarts实例
  // 绘制图表
  const options: EChartsOption = {
    title: {
      text: '发件箱统计',
      left: 'center',
      textStyle: {
        fontSize: 14
      },
      top: '20'
    },
    tooltip: {},
    yAxis: {
      axisLabel: {
        color: '#5cc093'
      }
    },
    xAxis: {
      axisLabel: {
        color: '#5cc093'
      },
      data: outboxesCount.value.map(item => item.domain)
    },
    color: '#7367f0',
    series: [
      {
        name: '邮箱数量',
        type: 'bar',
        barWidth: '75%',
        data: outboxesCount.value.map(item => item.count),
        label: {
          show: true, // 开启显示
          position: 'top' // 在上方显示
        }
      }
    ]
  }
  barChart?.setOption(options)
}
watch(outboxesCount, () => {
  renderOutboxCountBar()
})

const inboxesCount: Ref<IEmailCount[]> = ref([])
function renderInboxCountBar () {
  const barChart = initCharts.find(x => x.name === 'inbox')?.chart
  // 开始渲染
  // 基于准备好的dom，初始化echarts实例
  // 绘制图表
  const options: EChartsOption = {
    title: {
      text: '收件箱统计',
      left: 'center',
      textStyle: {
        fontSize: 14
      },
      top: '20'
    },
    tooltip: {},
    yAxis: {
      axisLabel: {
        color: '#5cc093'
      }
    },
    xAxis: {
      axisLabel: {
        color: '#5cc093'
      },
      data: inboxesCount.value.map(item => item.domain)
    },
    color: '#7367f0',
    series: [
      {
        name: '邮箱数量',
        type: 'bar',
        barWidth: '75%',
        data: inboxesCount.value.map(item => item.count),
        label: {
          show: true, // 开启显示
          position: 'top' // 在上方显示
        }
      }
    ]
  }
  barChart?.setOption(options)
}
watch(inboxesCount, () => {
  renderInboxCountBar()
})

const monthlySendingInfo: Ref<IMonthlySendingInfo[]> = ref([])
function renderMonthlySendingInfoBar () {
  const barChart = initCharts.find(x => x.name === 'monthly')?.chart
  // 开始渲染
  // 基于准备好的dom，初始化echarts实例
  // 绘制图表
  const options: EChartsOption = {
    title: {
      text: '每月发件统计',
      left: 'center',
      textStyle: {
        fontSize: 14
      },
      top: '20'
    },
    tooltip: {},
    yAxis: {
      axisLabel: {
        color: '#5cc093'
      }
    },
    xAxis: {
      axisLabel: {
        color: '#5cc093'
      },
      data: monthlySendingInfo.value.map(item => `${item.year}-${(item.month).toString().padStart(2, '0')}`)
    },
    color: '#7367f0',
    series: [
      {
        name: '数量',
        type: 'line',
        data: monthlySendingInfo.value.map(x => x.count),
        label: {
          show: true, // 开启显示
          position: 'top' // 在上方显示
        }
      }
    ],
    dataZoom: {
      show: true,
      start: 100 - (100 / (monthlySendingInfo.value.length || 1)) * 12,
      end: 100
    }
  }
  barChart?.setOption(options)
}
watch(monthlySendingInfo, () => {
  renderMonthlySendingInfoBar()
})

onMounted(async () => {
  const outboxChart = echarts.init(document.getElementById('chart-outbox'))
  initCharts.push({
    name: 'outbox',
    chart: outboxChart
  })
  const inboxChart = echarts.init(document.getElementById('chart-inbox'))
  initCharts.push({
    name: 'inbox',
    chart: inboxChart
  })
  const monthlyChart = echarts.init(document.getElementById('chart-monthly'))
  initCharts.push({
    name: 'monthly',
    chart: monthlyChart
  })

  const { data: outboxesCountData } = await getOutboxEmailCountStatistics()
  outboxesCount.value = outboxesCountData

  const { data: inboxesCountData } = await getInboxEmailCountStatistics()
  inboxesCount.value = inboxesCountData

  const { data: monthlySendingInfoData } = await getMonthlySendingCountInfo()
  monthlySendingInfo.value = monthlySendingInfoData
})

import { useResizeObserver } from '@vueuse/core'
const containerElementRef = ref(null)
useResizeObserver(containerElementRef, () => {
  // 重新初始化表格
  initCharts.forEach(x => {
    x.chart?.resize()
  })
})

// #region 版本检查
import { useVersionChecker } from './useVersionChecker'
useVersionChecker()
// #endregion
</script>

<style lang="scss" scoped></style>
