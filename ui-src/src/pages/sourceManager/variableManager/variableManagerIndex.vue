<template>
  <div class="column full-height">
    <UTabs v-model="tab" :tabs="tabOptions"></UTabs>

    <q-tab-panels v-model="tab" animated class="col">
      <q-tab-panel name="source" class="q-pa-none">
        <DataSource />
      </q-tab-panel>

      <q-tab-panel name="function" class="q-pa-none">
        <VariableDefinitions />
      </q-tab-panel>
    </q-tab-panels>
  </div>
</template>

<script lang="ts" setup>
import UTabs from 'src/components/quasarWrapper/UTabs.vue'

// #region 菜单切换
const tabOptions = [
  {
    name: 'source',
    icon: 'dataset',
    label: '数据源',
    tooltip: '设置数据源'
  },
  {
    name: 'function',
    icon: 'code',
    label: '变量定义',
    tooltip: '使用数据源定义函数变量'
  }
]
const tab = ref('source')
// #endregion

import DataSource from './dataSource.vue'
import VariableDefinitions from './variableDefinitions.vue'
</script>

<style lang="scss" scoped></style>
