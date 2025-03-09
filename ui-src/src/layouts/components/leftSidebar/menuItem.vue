<template>
  <q-expansion-item :header-inset-level="insetLevel" v-if="!noMenu && existChildren" v-model="openExpansionItem"
    class="rounded-borders" :class="{ 'text-orange': isActive }" :icon="icon" :label="label">
    <MenuItem v-for="child in childrenRoutes" :key="child.path" :routeRaw="child" :depth="depth + 1">
    </MenuItem>
  </q-expansion-item>

  <q-item v-else-if="!noMenu" :inset-level="insetLevel" :active="isActive" class="q-pr-xs"
    :class="{ 'menu-item__active': isActive, 'menu-item__default': !isActive }" clickable v-ripple @click="goToRoute">
    <q-item-section avatar>
      <q-icon :name="icon" />
    </q-item-section>
    <q-item-section :class="{ 'slash-right': isActive }">{{ label }}</q-item-section>
  </q-item>
</template>

<script lang="ts" setup>
/**
 * 显示逻辑
 * 1- 若只有一个子菜单，则只展示子菜单
 */

import type { ExtendedRouteRecordRaw } from 'src/router/types'
import { getMenuRoute } from './helper'

// props 参数
const props = defineProps({
  // 路由的名称，是必须项
  routeRaw: {
    type: Object as () => ExtendedRouteRecordRaw,
    required: true
  },

  // 深度
  depth: {
    type: Number,
    default: 0
  },

  // 单位缩进
  unitInsetLevel: {
    type: Number,
    default: 0.25
  }
})

// 缩进
const insetLevel = computed(() => {
  return props.depth * props.unitInsetLevel
})

const { name, children, meta: { label, icon, noMenu } } = props.routeRaw
const existChildren = computed(() => children && children.length > 0)
const childrenRoutes = children?.map(x => getMenuRoute(x)) as ExtendedRouteRecordRaw[]

// 判断当前菜单是否处于激活状态
const route = useRoute()
const isActive = computed(() => {
  return name === route.name
})

// 根据路由的变化，展开或关闭当前菜单
// 若等于当前菜单的 name，则关闭
// 若是下级，则展开
const openExpansionItem = ref(false)
watch(route, () => {
  if (name === route.name) {
    openExpansionItem.value = false
    return
  }

  const matched = route.matched.find(x => x.name === name)
  openExpansionItem.value = !!matched
}, { immediate: true })

// 跳转到路由
const router = useRouter()
async function goToRoute () {
  // 若 path 为绝对路径，则直接跳转
  if (props.routeRaw.path.startsWith('http')) {
    window.open(props.routeRaw.path, '_blank')
    return
  }

  await router.push({
    name
  })
}
</script>

<script lang="ts">
export default {
  name: 'MenuItem'
}
</script>

<style lang="scss">
.menu-item__active {
  color: $secondary;
}

.menu-item__default {
  color: $accent;
}

.slash-right {
  border-right: 2px solid $primary;
}

// 悬停高亮
.menu-item__icon_hover:hover {
  color: $primary;
}

:deep(.q-expansion-item__toggle-icon,
  .q-expansion-item__toggle-icon--rotated) {
  color: $secondary
}
</style>
