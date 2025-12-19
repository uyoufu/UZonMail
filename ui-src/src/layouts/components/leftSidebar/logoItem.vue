<template>
  <q-item style="height: 100px;" class="animated slideInDown">
    <q-item-section avatar class="q-pr-none">
      <q-icon :name="resolveSvgFullName('favicon-128x128')" size="70px" class="q-ma-lg"></q-icon>
    </q-item-section>

    <q-item-section>
      <h6>{{ systemConfig.name }}</h6>
    </q-item-section>
  </q-item>
</template>

<script lang="ts" setup>
import { resolveSvgFullName } from 'src/utils/svgHelper'

import { useSystemConfig } from './compositions/useSystemConfig'
const { systemConfig } = useSystemConfig()
</script>

<style lang="scss" scoped></style>
