<template>
  <div class="characters-container" @click="resetToInitial">
    <!-- Purple -->
    <div ref="purpleRef" class="char purple" :style="purpleStyle">
      <div class="eyes" :style="purpleEyesStyle">
        <EyeBall v-for="i in 2" :key="'p' + i" :size="18" :pupil-size="7" :max-distance="5" eye-color="white"
          pupil-color="#2D2D2D" :is-blinking="isPurpleBlinking" :force-look-x="purpleLookX"
          :force-look-y="purpleLookY" />
      </div>
    </div>
    <!-- Black -->
    <div ref="blackRef" class="char black" :style="blackStyle">
      <div class="eyes" :style="blackEyesStyle">
        <EyeBall v-for="i in 2" :key="'b' + i" :size="16" :pupil-size="6" :max-distance="4" eye-color="white"
          pupil-color="#2D2D2D" :is-blinking="isBlackBlinking" :force-look-x="blackLookX" :force-look-y="blackLookY" />
      </div>
    </div>
    <!-- Orange -->
    <div ref="orangeRef" class="char orange" :style="orangeStyle">
      <div class="eyes" :style="orangeEyesStyle">
        <EyePupil v-for="i in 2" :key="'o' + i" :size="12" :max-distance="5" pupil-color="#2D2D2D"
          :force-look-x="hiding ? -5 : undefined" :force-look-y="hiding ? -4 : undefined" />
      </div>
    </div>
    <!-- Yellow -->
    <div ref="yellowRef" class="char yellow" :style="yellowStyle">
      <div class="eyes" :style="yellowEyesStyle">
        <EyePupil v-for="i in 2" :key="'y' + i" :size="12" :max-distance="5" pupil-color="#2D2D2D"
          :force-look-x="hiding ? -5 : undefined" :force-look-y="hiding ? -4 : undefined" />
      </div>
      <div class="mouth" :style="yellowMouthStyle" />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch, onMounted, onUnmounted } from 'vue'
import EyeBall from './EyeBall.vue'
import EyePupil from './EyePupil.vue'

const props = withDefaults(defineProps<{
  /** Whether the user is currently typing in an input */
  isTyping?: boolean
  /** Whether a secret field has content */
  hasSecret?: boolean
  /** Whether the secret is currently visible (e.g. password shown) */
  secretVisible?: boolean
}>(), {
  isTyping: false,
  hasSecret: false,
  secretVisible: false,
})

const mouseX = ref(0)
const mouseY = ref(0)
const isPurpleBlinking = ref(false)
const isBlackBlinking = ref(false)
const isLookingAtEachOther = ref(false)
const isPurplePeeking = ref(false)

const purpleRef = ref<HTMLDivElement | null>(null)
const blackRef = ref<HTMLDivElement | null>(null)
const yellowRef = ref<HTMLDivElement | null>(null)
const orangeRef = ref<HTMLDivElement | null>(null)

const purplePos = reactive({ faceX: 0, faceY: 0, bodySkew: 0 })
const blackPos = reactive({ faceX: 0, faceY: 0, bodySkew: 0 })
const yellowPos = reactive({ faceX: 0, faceY: 0, bodySkew: 0 })
const orangePos = reactive({ faceX: 0, faceY: 0, bodySkew: 0 })

// Derived state
const hiding = computed(() => props.hasSecret && props.secretVisible)
// const leaning = computed(() => props.isTyping || (props.hasSecret && !props.secretVisible))
const leaning = computed(() => props.isTyping)

function resetToInitial () {
  if (props.secretVisible) return
  isLookingAtEachOther.value = false
  isPurplePeeking.value = false
}
watch(() => props.isTyping, () => {
  isPurplePeeking.value = props.isTyping && (!props.secretVisible || !props.hasSecret)
})

function calcPos (el: HTMLDivElement | null, target: { faceX: number; faceY: number; bodySkew: number }) {
  if (!el) return
  const r = el.getBoundingClientRect()
  const dx = mouseX.value - (r.left + r.width / 2)
  const dy = mouseY.value - (r.top + r.height / 3)
  target.faceX = Math.max(-15, Math.min(15, dx / 20))
  target.faceY = Math.max(-10, Math.min(10, dy / 30))
  target.bodySkew = Math.max(-6, Math.min(6, -dx / 120))
}

let rafId = 0
function tick () {
  calcPos(purpleRef.value, purplePos)
  calcPos(blackRef.value, blackPos)
  calcPos(yellowRef.value, yellowPos)
  calcPos(orangeRef.value, orangePos)
  rafId = requestAnimationFrame(tick)
}

function onMouseMove (e: MouseEvent) { mouseX.value = e.clientX; mouseY.value = e.clientY }

function setupBlink (target: { value: boolean }) {
  let t: number
  const go = () => {
    t = window.setTimeout(() => {
      target.value = true
      window.setTimeout(() => { target.value = false; go() }, 150)
    }, Math.random() * 4000 + 3000)
  }
  go()
  return () => clearTimeout(t)
}

let stopP: (() => void) | undefined
let stopB: (() => void) | undefined

onMounted(() => {
  window.addEventListener('mousemove', onMouseMove)
  stopP = setupBlink(isPurpleBlinking)
  stopB = setupBlink(isBlackBlinking)
  rafId = requestAnimationFrame(tick)
})
onUnmounted(() => {
  window.removeEventListener('mousemove', onMouseMove)
  cancelAnimationFrame(rafId)
  stopP?.(); stopB?.()
})

// Look at each other when typing starts
watch(() => props.isTyping, (v) => {
  if (v) {
    isLookingAtEachOther.value = true
    setTimeout(() => { isLookingAtEachOther.value = false }, 800)
  } else {
    isLookingAtEachOther.value = false
  }
})

// Purple peeks when secret is visible
let peekT: number | undefined
watch([() => props.hasSecret, () => props.secretVisible, isPurplePeeking], () => {
  clearTimeout(peekT)
  if (props.hasSecret && props.secretVisible) {
    peekT = window.setTimeout(() => {
      isPurplePeeking.value = true
      setTimeout(() => { isPurplePeeking.value = false }, 800)
    }, Math.random() * 3000 + 2000)
  } else {
    isPurplePeeking.value = false
  }
})

// Computed styles
const purpleStyle = computed(() => ({
  height: leaning.value ? '440px' : '400px',
  transition: 'all 0.4s ease-in-out',
  transform: hiding.value ? 'skewX(0deg)'
    : leaning.value ? `skewX(${purplePos.bodySkew - 12}deg) translateX(40px)`
      : `skewX(${purplePos.bodySkew}deg)`,
}))
const purpleEyesStyle = computed(() => ({
  left: hiding.value ? '20px' : isLookingAtEachOther.value ? '55px' : `${45 + purplePos.faceX}px`,
  top: hiding.value ? '35px' : isLookingAtEachOther.value ? '65px' : `${40 + purplePos.faceY}px`,
  gap: '32px',
}))
const purpleLookX = computed(() => hiding.value ? (isPurplePeeking.value ? 4 : -4) : isLookingAtEachOther.value ? 3 : undefined)
const purpleLookY = computed(() => hiding.value ? (isPurplePeeking.value ? 5 : -4) : isLookingAtEachOther.value ? 4 : undefined)

const blackStyle = computed(() => ({
  transform: hiding.value ? 'skewX(0deg)'
    : isLookingAtEachOther.value ? `skewX(${blackPos.bodySkew * 1.5 + 10}deg) translateX(20px)`
      : leaning.value ? `skewX(${blackPos.bodySkew * 1.5}deg)`
        : `skewX(${blackPos.bodySkew}deg)`,
}))
const blackEyesStyle = computed(() => ({
  left: hiding.value ? '10px' : isLookingAtEachOther.value ? '32px' : `${26 + blackPos.faceX}px`,
  top: hiding.value ? '28px' : isLookingAtEachOther.value ? '12px' : `${32 + blackPos.faceY}px`,
  gap: '24px',
}))
const blackLookX = computed(() => hiding.value ? -4 : isLookingAtEachOther.value ? 0 : undefined)
const blackLookY = computed(() => hiding.value ? -4 : isLookingAtEachOther.value ? -4 : undefined)

const orangeStyle = computed(() => ({
  transform: hiding.value ? 'skewX(0deg)' : `skewX(${orangePos.bodySkew}deg)`,
}))
const orangeEyesStyle = computed(() => ({
  left: hiding.value ? '50px' : `${82 + orangePos.faceX}px`,
  top: hiding.value ? '85px' : `${90 + orangePos.faceY}px`,
  gap: '32px',
}))

const yellowStyle = computed(() => ({
  transform: hiding.value ? 'skewX(0deg)' : `skewX(${yellowPos.bodySkew}deg)`,
}))
const yellowEyesStyle = computed(() => ({
  left: hiding.value ? '20px' : `${52 + yellowPos.faceX}px`,
  top: hiding.value ? '35px' : `${40 + yellowPos.faceY}px`,
  gap: '24px',
}))
const yellowMouthStyle = computed(() => ({
  left: hiding.value ? '10px' : `${40 + yellowPos.faceX}px`,
  top: hiding.value ? '88px' : `${88 + yellowPos.faceY}px`,
}))
</script>

<style scoped>
.characters-container {
  position: relative;
  width: 550px;
  height: 400px;
}

.char {
  position: absolute;
  bottom: 0;
  transition: all 0.7s ease-in-out;
  transform-origin: bottom center;
}

.purple {
  left: 70px;
  width: 180px;
  background: #6C3FF5;
  border-radius: 10px 10px 0 0;
  z-index: 1;
}

.black {
  left: 240px;
  width: 120px;
  height: 310px;
  background: #2D2D2D;
  border-radius: 8px 8px 0 0;
  z-index: 2;
}

.orange {
  left: 0;
  width: 240px;
  height: 200px;
  background: #FF9B6B;
  border-radius: 120px 120px 0 0;
  z-index: 3;
}

.yellow {
  left: 310px;
  width: 140px;
  height: 230px;
  background: #E8D754;
  border-radius: 70px 70px 0 0;
  z-index: 4;
}

.eyes {
  position: absolute;
  display: flex;
  transition: all 0.7s ease-in-out;
}

.mouth {
  position: absolute;
  width: 60px;
  height: 4px;
  background: #2D2D2D;
  border-radius: 4px;
  transition: all 0.2s ease-out;
}
</style>
