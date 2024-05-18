<template>
  <div :id="id" :class="className" :style="{ height: height, width: width }" />
</template>

<script>
import 'echarts-liquidfill'
import * as echarts from 'echarts'
import resize from '@/components/Echarts/mixins/resize'
import { getInboxCountOfTyes } from '@/api/report'

export default {
  mixins: [resize],
  props: {
    className: {
      type: String,
      default: 'chart'
    },
    id: {
      type: String,
      default: 'inboxType'
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
      source: []
    }
  },
  computed: {
    receiptsStatistics(){
      return this.$t('receiptsStatistics')
    }
  },
  watch: {
    '$i18n.locale'() {
      this.initChart() // 语言切换时重新初始化图表
    }
  },

  async mounted() {
    // 获取所有收件箱的种类和数量
    const res = await getInboxCountOfTyes()
    this.source = res.data

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
      // 参数
      const option = {
        legend: {
          right: '0',
          orient: 'vertical'
        },
        tooltip: {
          trigger: 'item'
        },

        dataset: {
          source: this.source
        },
        series: [
          {
            name: `${this.receiptsStatistics}`,
            type: 'pie',
            radius: [60, 94],
            center: ['50%', '50%'],
            roseType: 'area',
            itemStyle: {
              borderRadius: 8
            },
            color: [
              '#ff915a',
              '#9fe080',
              '#7ed3f4',
              '#ff7070',
              '#ffdc60',
              '#3ba272',
              '#5c7bd9',
              '#a969c6'
            ]
          }
        ]
      }

      // 基于准备好的dom，初始化echarts实例
      var myChart = echarts.init(document.getElementById(this.id))
      // 绘制图表
      myChart.setOption(option)

      // 监听事件
    }
  }
}
</script>

<style></style>
