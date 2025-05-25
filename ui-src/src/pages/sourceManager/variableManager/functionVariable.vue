<template>
  <div>
    <UTabs v-model="tab" :tabs="tabOptions"></UTabs>

    <q-tab-panels v-model="tab" animated>
      <q-tab-panel name="source">
        <DataSource />
      </q-tab-panel>
      <q-tab-panel name="function">
        <div class="text-h6 q-mt-md">变量定义</div>
        <div class="text-grey-7 q-mt-sm">使用数据源定义函数变量</div>
      </q-tab-panel>
    </q-tab-panels>
  </div>
</template>

<script lang="ts" setup>

// #region 菜单切换
const tabOptions = [
  {
    name: 'source',
    label: '数据源',
    tooltip: '设置数据源'
  },
  {
    name: 'function',
    label: '变量定义',
    tooltip: '使用数据源定义函数变量'
  }
]
const tab = ref('source')
// #endregion

import DataSource from './dataSource.vue'
</script>

<style lang="scss" scoped></style>
