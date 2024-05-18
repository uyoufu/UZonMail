<template>
  <div :id="id" :class="className" :style="{ height: height, width: width }" />
</template>

<script>
import 'echarts-liquidfill'
import * as echarts from 'echarts'
import resize from '@/components/Echarts/mixins/resize'
import _ from 'lodash'
import { getSuccessRate } from '@/api/report'

export default {
  mixins: [resize],
  props: {
    className: {
      type: String,
      default: 'chart'
    },
    id: {
      type: String,
      default: 'successfullRate'
    },
    width: {
      type: String,
      default: '400px'
    },
    height: {
      type: String,
      default: '200px'
    }
  },
  data() {
    return {
      chart: null,
      progress: 1
    }
  },

  computed: {
    progressValue() {
      return _.clamp(this.progress, 0.1, 0.9)
    },
    arrivalRate(){
      return this.$t('arrivalRate')
    }
  },
  watch: {
    '$i18n.locale'() {
      this.initChart() // 语言切换时重新初始化图表
    }
  },
  async mounted() {
    // 获取成功率
    const res = await getSuccessRate()
    this.progress = res.data || 0.99

    this.initChart()
  },
  beforeDestroy() {
    if (!this.chart) {
      return
    }
    this.chart.dispose()
    this.chart = null
  },
  methods: {
    initChart() {
      const value = this.progress || 1
      const mainColor = '#67b279'
      // 参数
      const option = {
        // 背景色
        backgroundColor: 'white',
        series: [
          {
            // 水位图
            type: 'liquidFill',
            // 显示比例
            radius: '80%',
            // 中心点
            center: ['50%', '50%'],
            // 水波振幅
            amplitude: 20,
            // data个数代表波浪数
            data: [this.progressValue, this.progressValue, this.progressValue],
            // 波浪颜色
            color: [mainColor],
            backgroundStyle: {
              // 外边框
              borderWidth: 6,
              // 边框颜色
              borderColor: mainColor,
              // 边框内部填充部分颜色
              color: '#e0e0e0'
            },
            label: {
              // 标签设置
              normal: {
                position: ['50%', '30%'],
                // 显示文本
                formatter: `${this.arrivalRate}: ${(value * 100).toFixed(0)}%`,
                textStyle: {
                  // 文本字号
                  fontSize: '28px',
                  color: mainColor
                }
              }
            },
            shape: 'container',
            outline: {
              // 最外层边框显示控制
              show: false
            }
          }
        ]
      }

      // 基于准备好的dom，初始化echarts实例
      var myChart = echarts.init(document.getElementById(this.id))
      // 绘制图表
      myChart.setOption(option)
    }
  }
}
</script>

<style>
</style>
