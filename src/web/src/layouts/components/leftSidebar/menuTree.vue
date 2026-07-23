<template>
  <q-list class="col text-accent hover-scroll full-height">
    <LogoItem />
    <q-separator />
    <MenuItem v-for="route in menuRoutes" :key="route.name" :routeRaw="route">
    </MenuItem>
  </q-list>
</template>

<script lang="ts" setup>
import type { ExtendedRouteRecordRaw } from 'src/router/types'

import LogoItem from './logoItem.vue'
import MenuItem from './menuItem.vue'

import { getMenuRoute } from './helper'
import { useRoutesStore } from 'src/stores/routes'
const routeStore = useRoutesStore()
const menuRoutes = routeStore.loadedRoutes.map(x => getMenuRoute(x)) as ExtendedRouteRecordRaw[]
</script>

<style lang="scss" scoped></style>
