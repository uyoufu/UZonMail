<template>
  <!-- @vue-expect-error :row 没有赋予初值 -->
  <q-table class="full-height" v-bind="$attrs">
    <template #[getSlotName(slotName)]="slotProps" v-for="(slot, slotName) in $slots">
      <slot :name="slotName" v-bind="slotProps"> </slot>
    </template>
  </q-table>
</template>

<script lang="ts" setup>
import type { QTableSlots } from 'quasar'
// 解析插槽名称
function getSlotName (slotName: string | number): keyof QTableSlots {
  return slotName as keyof QTableSlots
}

</script>

<style lang="scss" scoped></style>
