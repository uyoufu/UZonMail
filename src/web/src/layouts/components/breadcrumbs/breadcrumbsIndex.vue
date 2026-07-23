<template>
  <q-breadcrumbs style="font-size: 15px">
    <q-breadcrumbs-el v-for="item in matchedRoutes" :key="item.path" class="animated fadeInRight"
      exact-active-class="text-secondary" :to="item.path" :icon="item.meta.icon"
      :label="translateRoutes(item.meta.label)">
    </q-breadcrumbs-el>
  </q-breadcrumbs>
</template>

<script lang="ts" setup>
import type { ExtendedRouteRecordRaw } from 'src/router/types'

const route = useRoute()
const matchedRoutes = computed(() => {
  // 获取路由的全路径
  const results = route.matched.filter(x => {
    const validChildren = x.children.filter(child => !child.meta?.noMenu)
    return validChildren.length !== 1
  })
  // console.log('results', results)
  return results as unknown as ExtendedRouteRecordRaw[]
})

import { translateRoutes } from 'src/i18n/helpers'
</script>

<style lang="scss" scoped>
::v-deep(.q-breadcrumbs__el:hover) {
  color: $negative;
}
</style>
