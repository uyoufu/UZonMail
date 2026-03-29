<template>
  <div ref="pupilRef" class="pupil" :style="{
    width: size + 'px', height: size + 'px',
    backgroundColor: pupilColor,
    transform: `translate(${pos.x}px, ${pos.y}px)`,
  }" />
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'

const props = withDefaults(defineProps<{
  size?: number; maxDistance?: number; pupilColor?: string
  forceLookX?: number; forceLookY?: number
}>(), { size: 12, maxDistance: 5, pupilColor: 'black' })

const mx = ref(0), my = ref(0)
const pupilRef = ref<HTMLDivElement>()
const onMove = (e: MouseEvent) => { mx.value = e.clientX; my.value = e.clientY }
onMounted(() => window.addEventListener('mousemove', onMove))
onUnmounted(() => window.removeEventListener('mousemove', onMove))

const pos = computed(() => {
  if (!pupilRef.value) return { x: 0, y: 0 }
  if (props.forceLookX !== undefined && props.forceLookY !== undefined)
    return { x: props.forceLookX, y: props.forceLookY }
  const r = pupilRef.value.getBoundingClientRect()
  const dx = mx.value - (r.left + r.width / 2)
  const dy = my.value - (r.top + r.height / 2)
  const d = Math.min(Math.sqrt(dx ** 2 + dy ** 2), props.maxDistance)
  const a = Math.atan2(dy, dx)
  return { x: Math.cos(a) * d, y: Math.sin(a) * d }
})
</script>

<style scoped>
.pupil {
  border-radius: 50%;
  transition: transform 0.1s ease-out;
}
</style>
