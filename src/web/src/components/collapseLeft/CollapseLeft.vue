<template>
  <div class="collapse-bar" :class="collapseBarClass" @click="onCollapse">
    <q-tooltip anchor="center right" self="center left">
      {{ tooltipText }}
    </q-tooltip>
  </div>
</template>
<script lang="ts" setup>
// 向左折叠
const modelValue = defineModel({ default: false })
function onCollapse () {
  modelValue.value = !modelValue.value
}
const collapseBarClass = computed(() => {
  return {
    'collapse-bar__normal': !modelValue.value,
    'collapse-bar__arrow-right': modelValue.value
  }
})
const tooltipText = computed(() => {
  return modelValue.value ? '展开' : '折叠'
})
</script>

<style lang="scss" scoped>
.collapse-bar {
  z-index: 2000;
}

.collapse-bar__normal {
  position: relative;
  width: 10px;
  height: 20px;
  background-color: transparent;
  cursor: pointer;

  &::before,
  &::after {
    content: "";
    position: absolute;
    width: 50%;
    height: 50%;
    background-color: #e0e0e0;
    transition: all 0.3s linear;
  }

  &::before {
    border-top-right-radius: 2px;
    border-top-left-radius: 2px;
    transform: rotate(0deg);
  }

  &::after {
    border-bottom-right-radius: 2px;
    border-bottom-left-radius: 2px;
    transform: translateY(100%) rotate(0deg);
  }

  &:hover::before,
  &:hover::after {
    background-color: $primary;
  }

  &:hover::before {
    transform: translateY(10%) rotate(25deg);
  }

  &:hover::after {
    transform: translateY(90%) rotate(-25deg);
  }
}

// 箭头向右
.collapse-bar__arrow-right {
  position: relative;
  width: 10px;
  height: 20px;
  background-color: transparent;
  cursor: pointer;

  &::before,
  &::after {
    content: "";
    position: absolute;
    width: 50%;
    height: 50%;
    background-color: #e0e0e0;
    transition: all 0.3s linear;
  }

  &::before {
    border-top-right-radius: 2px;
    border-top-left-radius: 2px;
    transform: translateY(10%) rotate(-25deg);
  }

  &::after {
    border-bottom-right-radius: 2px;
    border-bottom-left-radius: 2px;
    transform: translateY(90%) rotate(25deg);
  }

  &:hover::before,
  &:hover::after {
    background-color: $primary;
  }
}
</style>
