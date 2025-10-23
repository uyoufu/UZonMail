<template>
  <q-dialog ref='dialogRef' @hide="onDialogHide" :persistent='true'>
    <q-card class='column items-center items-start q-pa-sm warm-up-settings'>
      <div class='full-width text-primary text-h6'>预热设置</div>

      <q-field v-model="dateRangeContent" label="日期范围" dense class="full-width">
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

      <div ref="countChartElementRef" class="full-width" style="height: 250px;"></div>

      <div class="row justify-end items-center q-mb-sm full-width">
        <CancelBtn @click="onDialogCancel" />
        <OkBtn class="q-ml-sm" @click="onDialogOK(dateRangeModelValue)" />
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
  return `${dateRangeModelValue.value.from} ~ ${dateRangeModelValue.value.to}`
}
const dateRangeContent = ref(formatDateRange())
watch(dateRangeModelValue, () => {
  dateRangeContent.value = formatDateRange()
})

// #endregion
const countChartElementRef = ref<HTMLElement | null>(null)
import { useWarmUpCountChart } from './compositions/useWarmUpCountChart'
useWarmUpCountChart(countChartElementRef, dateRangeModelValue, props.totalCount)
</script>

<style lang='scss' scoped>
.warm-up-settings {
  width: min(100vw, 400px)
}
</style>
