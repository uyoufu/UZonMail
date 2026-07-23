<template>
  <q-layout class="text-accent" view="lHh lpR fFf" dark>
    <q-page-container class="page-container">
      <q-page class="q-pa-md full-height full-with">
        <q-scroll-area class="page__scroll-area" :thumb-style="thumbStyle" :content-style="contentStyle"
          :content-active-style="contentActiveStyle">
          <router-view v-slot="{ Component }">
            <transition appear enter-active-class="animated fadeInDown">
              <component :is="Component" :key="getRouteId($route.fullPath, $route.query)" />
            </transition>
          </router-view>
        </q-scroll-area>
      </q-page>
    </q-page-container>
  </q-layout>
</template>

<script lang="ts" setup>
import { useScrollAreaStyle } from 'src/compositions/scrollUtils'
const { contentStyle, contentActiveStyle, thumbStyle } = useScrollAreaStyle()
import { getRouteId } from '../components/tags/routeHistories'

import { useRoute } from 'vue-router'
const $route = useRoute()
</script>

<style lang="scss" scoped>
.page-container {
  overflow: hidden;
  position: absolute;
  top: 0px;
  left: 0px;
  bottom: 0px;
  right: 0px;
  background-color: $grey-12;

  // background-color: $grey-12;
  .page__scroll-area {
    height: 100%;

    :deep(.q-scrollarea__content) {
      width: 100%;
      height: 100%;
    }
  }
}
</style>
