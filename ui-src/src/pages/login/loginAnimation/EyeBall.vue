<template>
  <div ref="eyeRef" class="eyeball" :style="{
    width: size + 'px', height: isBlinking ? '2px' : size + 'px',
    backgroundColor: eyeColor,
  }">
    <div v-if="!isBlinking" class="pupil-inner" :style="{
      width: pupilSize + 'px', height: pupilSize + 'px',
      backgroundColor: pupilColor,
      transform: `translate(${pos.x}px, ${pos.y}px)`,
    }" />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'

const props = withDefaults(defineProps<{
  size?: number; pupilSize?: number; maxDistance?: number
  eyeColor?: string; pupilColor?: string; isBlinking?: boolean
  forceLookX?: number; forceLookY?: number
}>(), { size: 48, pupilSize: 16, maxDistance: 10, eyeColor: 'white', pupilColor: 'black', isBlinking: false })

const mx = ref(0), my = ref(0)
const eyeRef = ref<HTMLDivElement>()
const onMove = (e: MouseEvent) => { mx.value = e.clientX; my.value = e.clientY }
onMounted(() => window.addEventListener('mousemove', onMove))
onUnmounted(() => window.removeEventListener('mousemove', onMove))

const pos = computed(() => {
  if (!eyeRef.value) return { x: 0, y: 0 }
  if (props.forceLookX !== undefined && props.forceLookY !== undefined)
    return { x: props.forceLookX, y: props.forceLookY }
  const r = eyeRef.value.getBoundingClientRect()
  const dx = mx.value - (r.left + r.width / 2)
  const dy = my.value - (r.top + r.height / 2)
  const d = Math.min(Math.sqrt(dx ** 2 + dy ** 2), props.maxDistance)
  const a = Math.atan2(dy, dx)
  return { x: Math.cos(a) * d, y: Math.sin(a) * d }
})
</script>

<style scoped>
.eyeball {
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.15s;
  overflow: hidden;
}

.pupil-inner {
  border-radius: 50%;
  transition: transform 0.1s ease-out;
}
</style>
