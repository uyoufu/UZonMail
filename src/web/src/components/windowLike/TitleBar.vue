<template>
  <q-bar dense class="q-px-sm q-py-md bg-accent">
    <q-icon :name="resolveSvgFullName('favicon')" size="md" class="q-ml-sm" />
    <div class="col ellipsis text-subtitle1 text-primary text-bold">{{ title }}</div>
    <CommonBtn flat dense round icon="close" color="negative" :tooltip="translateButton('cancelCurrentOperation')"
      @click="emit('close')" />
  </q-bar>
</template>

<script lang="ts" setup>
import { translateButton } from 'src/i18n/helpers'
import { resolveSvgFullName } from 'src/utils/svgHelper'

defineProps<{
  title: string
}>()

const emit = defineEmits<{
  close: []
}>()
</script>
