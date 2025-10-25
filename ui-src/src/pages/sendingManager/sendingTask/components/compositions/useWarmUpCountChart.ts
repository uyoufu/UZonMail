/* eslint-disable @typescript-eslint/no-unused-vars */
/* eslint-disable @typescript-eslint/no-explicit-any */
// #region echarts
// 参考：https://echarts.apache.org/handbook/zh/basics/import/
// 引入 echarts 核心模块，核心模块提供了 echarts 使用必须要的接口。
import type { EChartsOption } from 'echarts/types/dist/shared'
import * as echarts from 'echarts/core'
import dayjs from 'dayjs'

import {
  TitleComponent,
  TooltipComponent,
  GridComponent,
  DatasetComponent,
  TransformComponent,
  DataZoomComponent,
  GraphicComponent
} from 'echarts/components'
import { LineChart } from 'echarts/charts'
import { UniversalTransition } from 'echarts/features'
import { CanvasRenderer } from 'echarts/renderers'


// 注册必须的组件
echarts.use([TitleComponent,
  TooltipComponent,
  GridComponent,
  DatasetComponent,
  TransformComponent,
  DataZoomComponent,
  GraphicComponent,
  LineChart,
  CanvasRenderer,
  UniversalTransition])
// #endregion

import logger from 'loglevel'

// 发送量预热曲线图
export function useWarmUpCountChart (
  countChartElementRef: Ref<HTMLElement | null>,
  dateRangeRef: Ref<{ from: string, to: string }>,
  totalCount: number
) {

  const chartData: Ref<[number, number][]> = ref([])

  // #region 数据生成
  function initChartData () {
    // 使用 arctan 函数生成一个平滑的预热曲线, x 区间 [-5,5]
    const xRange = 10
    const dataCount = 10
    const totalDays = dayjs(dateRangeRef.value.to).diff(dayjs(dateRangeRef.value.from), 'day') + 1

    const xStep = xRange / dataCount
    const xScale = Math.max(1, totalDays / dataCount)
    const yMin = Math.min(10, totalCount * 0.1)

    const newData: [number, number][] = []
    for (let x = -xRange / 2; x <= xRange / 2; x += xStep) {
      const y = (Math.atan(x) + Math.PI / 2) / Math.PI
      const xFactor = (x / xRange + 0.5)
      logger.debug(`[WarmUpCountChart] x: ${xFactor}, y: ${y}`)

      const xValue = Math.round(xFactor * dataCount * xScale)
      const yValue = Math.round(y * (totalCount - yMin) + yMin)
      newData.push([xValue, Math.max(1, yValue)])
    }

    // 最后一个值强制为总量
    newData[newData.length - 1]![1] = totalCount

    logger.debug('[WarmUpCountChart] Generated chart data:', newData)
    chartData.value = newData
  }
  initChartData()
  watch(dateRangeRef, () => {
    initChartData()
  })
  watch(chartData, (newVal) => {
    if (!myChart.value) return
    myChart.value.setOption({
      series: [
        {
          id: 'chartData',
          data: chartData.value
        }
      ]
    })
  })
  // #endregion

  // #region 图表更新
  // 参考：https://echarts.apache.org/handbook/zh/how-to/interaction/drag/
  const myChart: Ref<echarts.ECharts | null> = ref(null)
  const symbolSize = 10

  watch(countChartElementRef, (newVal) => {
    logger.debug('countChartElementRef changed:', newVal)
    if (!newVal) {
      return
    }

    if (myChart.value) {
      return
    }

    // 生成
    myChart.value = echarts.init(countChartElementRef.value)
    if (!myChart.value)
      return

    const options: EChartsOption = {
      title: {
        text: '发送量预热曲线',
        left: 'left',
        textStyle: {
          fontSize: 14,
          fontWeight: 'normal',
          color: '#757575'
        },
        top: '15'
      },
      tooltip: {
        triggerOn: 'none',
        formatter: function (params: any) {
          return `第 ${Math.round(params.data[0])} 天<br/>发送 ${Math.round(params.data[1])} 封`
        }
      },
      xAxis: {
        axisLabel: {
          color: '#5cc093'
        },
        axisLine: { onZero: false },
        type: 'value'
      },
      yAxis: {
        axisLabel: {
          color: '#5cc093'
        },
        axisLine: { onZero: false },
        type: 'value'
      },
      series: [
        { id: 'chartData', type: 'line', smooth: true, symbolSize: symbolSize, data: chartData.value }
      ],
      color: '#7367f0',
    }

    myChart.value.setOption(options)
    logger.debug('[WarmUpCountChart] Chart initialized', myChart)

    // 设置可拖拽点
    myChart.value.setOption({
      graphic: echarts.util.map(chartData.value, function (item, dataIndex) {
        return {
          type: 'circle',
          position: myChart.value!.convertToPixel('grid', item),
          shape: { r: symbolSize / 2 },
          invisible: true,
          draggable: true,
          ondrag: echarts.util.curry(onPointDragging, dataIndex as number),
          onmousemove: echarts.util.curry(showTooltip, dataIndex),
          onmouseout: echarts.util.curry(hideTooltip, dataIndex),
          z: 10000
        }
      })
    })
  })

  window.addEventListener('resize', function () {
    if (!myChart.value) return

    myChart.value.setOption({
      graphic: echarts.util.map(chartData.value, function (item, dataIndex: any) {
        return { position: myChart.value!.convertToPixel('grid', item) }
      })
    })
  })

  function showTooltip (dataIndex: any) {
    logger.debug('Show tooltip for point:', dataIndex)
    if (!myChart.value) return

    myChart.value.dispatchAction({
      type: 'showTip',
      seriesIndex: 0,
      dataIndex: dataIndex
    })
  }

  function hideTooltip (dataIndex: any) {
    if (!myChart.value) return

    myChart.value.dispatchAction({ type: 'hideTip' })
  }


  function onPointDragging (dataIndex: string | number, dx: number, dy: number) {
    if (!myChart.value) return

    // eslint-disable-next-line @typescript-eslint/ban-ts-comment
    // @ts-ignore
    chartData.value[dataIndex as number] = myChart.value.convertFromPixel('grid', (this as { position: [number, number] }).position)

    myChart.value.setOption({
      series: [
        {
          id: 'chartData',
          data: chartData.value
        }
      ]
    })
  }
  // #endregion


  return {
    chartData
  }
}
