<template>
  <div class="collapse-bar" :class="collapseBarClass" @click="onCollapse">
    <q-tooltip anchor="center left" self="center right">
      {{ tooltipText }}
    </q-tooltip>
  </div>
</template>
<script lang="ts" setup>
import { t } from 'src/i18n/helpers'

// 向右折叠
const modelValue = defineModel({ default: false })
function onCollapse() {
  modelValue.value = !modelValue.value
}
const collapseBarClass = computed(() => {
  return {
    'collapse-bar__normal': !modelValue.value,
    'collapse-bar__arrow-left': modelValue.value
  }
})
const tooltipText = computed(() => {
  return modelValue.value ? t('collapseRight.expand') : t('collapseRight.collapse')
})
</script>

<style lang="scss" scoped>
.collapse-bar {
  position: absolute;
  right: 5px;
  top: 50%;
  transform: translateY(-50%);
  z-index: 2000;
}

.collapse-bar__normal {
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
    border-top-left-radius: 2px;
    border-top-right-radius: 2px;
    transform: rotate(0deg);
    right: 0;
  }

  &::after {
    border-bottom-left-radius: 2px;
    border-bottom-right-radius: 2px;
    transform: translateY(100%) rotate(0deg);
    right: 0;
  }

  &:hover::before,
  &:hover::after {
    background-color: $primary;
  }

  &:hover::before {
    transform: translateY(10%) rotate(-25deg);
  }

  &:hover::after {
    transform: translateY(90%) rotate(25deg);
  }
}

// 箭头向左
.collapse-bar__arrow-left {
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
    right: 0;
  }

  &::before {
    border-top-left-radius: 2px;
    border-top-right-radius: 2px;
    transform: translateY(10%) rotate(25deg);
  }

  &::after {
    border-bottom-left-radius: 2px;
    border-bottom-right-radius: 2px;
    transform: translateY(90%) rotate(-25deg);
  }

  &:hover::before,
  &:hover::after {
    background-color: $primary;
  }
}
</style>
