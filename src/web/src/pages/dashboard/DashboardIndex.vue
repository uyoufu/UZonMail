<template>
  <div ref="containerElementRef" class="row justify-around q-pt-sm">
    <div class="col-auto-2 q-pa-sm" style="height: 250px;">
      <div id="chart-sender-accounts" class="full-height full-width card-like"></div>
    </div>
    <div class="col-auto-2 q-pa-sm" style="height: 250px;">
      <div id="chart-recipient-contacts" class="full-height full-width card-like"></div>
    </div>
    <div id="chart-monthly" class="col-auto-2 q-pa-sm card-like q-ma-sm" style="height: 250px;"></div>
  </div>
</template>

<script lang="ts" setup>
import logger from 'loglevel'
import { translateDashboardPage } from 'src/i18n/helpers'

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
import { getRecipientContactCountStatistics, getSenderAccountCountStatistics, getMonthlySendingCountInfo } from 'src/api/statistics'
const senderAccountCounts: Ref<IEmailCount[]> = ref([])
function renderSenderAccountCountBar () {
  const barChart = initCharts.find(x => x.name === 'senderAccounts')?.chart
  logger.log('[Dashboard] barChart:', barChart, initCharts)
  // 开始渲染
  // 基于准备好的dom，初始化echarts实例
  // 绘制图表
  const options: EChartsOption = {
    title: {
      text: translateDashboardPage('senderAccountStatsTitle'),
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
      data: senderAccountCounts.value.map(domainCount => domainCount.domain)
    },
    color: '#7367f0',
    series: [
      {
        name: translateDashboardPage('emailCount'),
        type: 'bar',
        barWidth: '75%',
        data: senderAccountCounts.value.map(domainCount => domainCount.count),
        label: {
          show: true, // 开启显示
          position: 'top' // 在上方显示
        }
      }
    ]
  }
  barChart?.setOption(options)
}
watch(senderAccountCounts, () => {
  renderSenderAccountCountBar()
})

const recipientContactCounts: Ref<IEmailCount[]> = ref([])
function renderRecipientContactCountBar () {
  const barChart = initCharts.find(x => x.name === 'recipientContacts')?.chart
  // 开始渲染
  // 基于准备好的dom，初始化echarts实例
  // 绘制图表
  const options: EChartsOption = {
    title: {
      text: translateDashboardPage('recipientContactStatsTitle'),
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
      data: recipientContactCounts.value.map(domainCount => domainCount.domain)
    },
    color: '#7367f0',
    series: [
      {
        name: translateDashboardPage('emailCount'),
        type: 'bar',
        barWidth: '75%',
        data: recipientContactCounts.value.map(domainCount => domainCount.count),
        label: {
          show: true, // 开启显示
          position: 'top' // 在上方显示
        }
      }
    ]
  }
  barChart?.setOption(options)
}
watch(recipientContactCounts, () => {
  renderRecipientContactCountBar()
})

const monthlySendingInfo: Ref<IMonthlySendingInfo[]> = ref([])
function renderMonthlySendingInfoBar () {
  const barChart = initCharts.find(x => x.name === 'monthly')?.chart
  // 开始渲染
  // 基于准备好的dom，初始化echarts实例
  // 绘制图表
  const options: EChartsOption = {
    title: {
      text: translateDashboardPage('monthlySendingStatsTitle'),
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
        name: translateDashboardPage('count'),
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
  const senderAccountChart = echarts.init(document.getElementById('chart-sender-accounts'))
  initCharts.push({
    name: 'senderAccounts',
    chart: senderAccountChart
  })
  const recipientContactChart = echarts.init(document.getElementById('chart-recipient-contacts'))
  initCharts.push({
    name: 'recipientContacts',
    chart: recipientContactChart
  })
  const monthlyChart = echarts.init(document.getElementById('chart-monthly'))
  initCharts.push({
    name: 'monthly',
    chart: monthlyChart
  })

  const { data: senderAccountCountData } = await getSenderAccountCountStatistics()
  senderAccountCounts.value = senderAccountCountData

  const { data: recipientContactCountData } = await getRecipientContactCountStatistics()
  recipientContactCounts.value = recipientContactCountData

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

// 监听语言变化，重新渲染图表
import { useI18n } from 'vue-i18n'
const { locale } = useI18n()
watch(locale, () => {
  // 重新渲染图表
  renderSenderAccountCountBar()
  renderRecipientContactCountBar()
  renderMonthlySendingInfoBar()
})

// #region 版本检查
import { useVersionChecker } from './useVersionChecker'
useVersionChecker()
// #endregion
</script>

<style lang="scss" scoped></style>
