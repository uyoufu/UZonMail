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
import UTabs from 'src/components/utabs/UTabs.vue'
import { t } from 'src/i18n/helpers'

// #region 菜单切换
const tabOptions = computed(() => [
  {
    name: 'source',
    icon: 'dataset',
    label: t('pages.variableManager.dataSource'),
    tooltip: t('pages.variableManager.dataSourceTooltip')
  },
  {
    name: 'function',
    icon: 'code',
    label: t('pages.variableManager.variableDefinition'),
    tooltip: t('pages.variableManager.variableDefinitionTooltip')
  }
])
const tab = ref('source')
// #endregion

import DataSource from './dataSource.vue'
import VariableDefinitions from './variableDefinitions.vue'
</script>

<style lang="scss" scoped></style>
