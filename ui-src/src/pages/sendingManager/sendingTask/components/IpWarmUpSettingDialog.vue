<template>
  <q-dialog ref='dialogRef' @hide="onDialogHide" :persistent='true'>
    <q-card class='column items-center items-start q-pa-sm warm-up-settings'>
      <div class='full-width text-primary text-h6'>预热设置</div>

      <q-input outlined standout dense v-model="planName" label="计划名称" placeholder="输入计划名称" class="full-width q-mt-sm">
      </q-input>

      <q-field outlined v-model="dateRangeContent" label="日期范围" dense class="full-width q-mt-sm">
        <template v-slot:control>
          <div>{{ dateRangeContent }}</div>
        </template>

        <template v-slot:append>
          <q-icon name="event" class="cursor-pointer">
            <q-popup-proxy cover transition-show="scale" transition-hide="scale">
              <q-date class="q-pa-sm" v-model="dateRangeModelValue" range landscape :mask="dateMask" color="primary" />
            </q-popup-proxy>
          </q-icon>
        </template>
      </q-field>

      <q-field outlined v-model="dateRangeContent" label="最大发件量" dense class="full-width q-mt-sm">
        <template v-slot:control>
          <div>{{ totalCount }}</div>
        </template>
      </q-field>

      <div ref="countChartElementRef" class="full-width" style="height: 250px;"></div>

      <div class="row justify-end items-center q-mb-sm full-width">
        <CancelBtn @click="onDialogCancel" />
        <OkBtn class="q-ml-sm" @click="onOkClicked" />
      </div>
    </q-card>
  </q-dialog>
</template>

<script lang='ts' setup>
/**
 * warning: 该组件是一个弹窗的示例，不可直接使用
 * 参考：http://www.quasarchs.com/quasar-plugins/dialog#composition-api-variant
 */

import { useDialogPluginComponent } from 'quasar'
defineEmits([
  // 必需；需要指定一些事件
  // （组件将通过useDialogPluginComponent()发出）
  ...useDialogPluginComponent.emits
])
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent()

const props = defineProps({
  totalCount: {
    type: Number,
    required: true
  }
})

// #region 时期范围选择
import dayjs from 'dayjs'
const dateMask = 'YYYY-MM-DD'
const dateRangeModelValue = ref<{ from: string, to: string }>({
  from: dayjs().format(dateMask),
  to: dayjs().add(1, 'month').format(dateMask)
})
function formatDateRange () {
  const totalDays = dayjs(dateRangeModelValue.value.to).diff(dayjs(dateRangeModelValue.value.from), 'day') + 1
  return `${dateRangeModelValue.value.from} ~ ${dateRangeModelValue.value.to}, 共 ${totalDays} 天`
}
const dateRangeContent = ref(formatDateRange())
watch(dateRangeModelValue, () => {
  dateRangeContent.value = formatDateRange()
})

// #endregion

// #region 图表
const countChartElementRef = ref<HTMLElement | null>(null)
import { useWarmUpCountChart } from './compositions/useWarmUpCountChart'
const { chartData } = useWarmUpCountChart(countChartElementRef, dateRangeModelValue, props.totalCount)

// #endregion

const planName = ref('')

// #region 按钮
function onOkClicked () {
  const payloads = {
    dateRange: dateRangeModelValue.value,
    from: dateRangeModelValue.value.from,
    to: dateRangeModelValue.value.to,
    countChartPoints: chartData.value,
    name: planName.value
  }

  onDialogOK(payloads)
}
// #endregion
</script>

<style lang='scss' scoped>
.warm-up-settings {
  width: min(100vw, 400px)
}
</style>
