<template>
  <q-td :props="props">
    {{ props.rowIndex + 1 }}
  </q-td>
</template>

<script lang="ts" setup>
defineProps({
  props: {
    type: Object as PropType<{
      rowIndex: number
    }>,
    required: true
  }
})
</script>

<style lang="scss" scoped></style>
