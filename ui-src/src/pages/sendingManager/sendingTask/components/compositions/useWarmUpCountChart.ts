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

  const data = [
    [15, 0],
    [-50, 10],
    [-56.5, 20],
    [-46.5, 30],
    [-22.1, 40]
  ]

  function initChartData () {
    // 使用 atan 函数生成一个平滑的预热曲线
  }
  const totalDays = dayjs(dateRangeRef.value.to).diff(dayjs(dateRangeRef.value.from), 'day') + 1


  // 参考：https://echarts.apache.org/handbook/zh/how-to/interaction/drag/
  watch(countChartElementRef, (newVal) => {
    logger.debug('countChartElementRef changed:', newVal)
    if (!newVal) {
      return
    }

    const symbolSize = 20
    const myChart = echarts.init(countChartElementRef.value)
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
          return (
            'X: ' +
            params.data[0]!.toFixed(2) +
            '<br />Y: ' +
            params.data[1]!.toFixed(2)
          )
        }
      },
      xAxis: {
        axisLabel: {
          color: '#5cc093'
        },
        axisLine: { onZero: false },
        min: -100,
        max: 80,
        type: 'value'
      },
      yAxis: {
        axisLabel: {
          color: '#5cc093'
        },
        axisLine: { onZero: false },
        min: -30,
        max: 60,
        type: 'value'
      },
      series: [
        { id: 'a', type: 'line', smooth: true, symbolSize: symbolSize, data: data }
      ],
      color: '#7367f0',
    }

    myChart.setOption(options)
    logger.debug('[WarmUpCountChart] Chart initialized', myChart)

    // 设置可拖拽点
    myChart.setOption({
      graphic: echarts.util.map(data, function (item, dataIndex) {
        return {
          type: 'circle',
          position: myChart.convertToPixel('grid', item),
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

    window.addEventListener('resize', function () {
      myChart.setOption({
        graphic: echarts.util.map(data, function (item, dataIndex: any) {
          return { position: myChart.convertToPixel('grid', item) }
        })
      })
    })

    function showTooltip (dataIndex: any) {
      logger.debug('Show tooltip for point:', dataIndex)
      myChart.dispatchAction({
        type: 'showTip',
        seriesIndex: 0,
        dataIndex: dataIndex
      })
    }

    function hideTooltip (dataIndex: any) {
      myChart.dispatchAction({ type: 'hideTip' })
    }


    function onPointDragging (dataIndex: string | number, dx: number, dy: number) {
      // eslint-disable-next-line @typescript-eslint/ban-ts-comment
      // @ts-ignore
      data[dataIndex as number] = myChart.convertFromPixel('grid', (this as { position: [number, number] }).position)
      myChart.setOption({
        series: [
          {
            id: 'a',
            data
          }
        ]
      })
    }
  })

  return {
    data
  }
}
