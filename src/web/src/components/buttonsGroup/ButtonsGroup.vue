<template>
  <div class="row items-center justify-start">
    <q-btn v-for="btn in buttons" :key="btn.name" class="q-px-sm" :dense="btn.dense" :color="btn.color" :size="btn.size"
      :icon="btn.icon" :label="btn.label" v-bind="$attrs">
      <template v-for="(slot, slotName) in $slots">
        <slot :name="slotName"></slot>
      </template>
      <AsyncTooltip :tooltip="btn.tooltip" />
    </q-btn>
  </div>
</template>

<script lang="ts" setup>
import type { IButtonParams } from './types'

const props = defineProps<{ buttons: IButtonParams[] }>()
const { buttons } = toRefs(props)
</script>

<style lang="scss" scoped></style>
