<template>
  <q-layout class="text-accent" view="lHh lpR fFf" dark>
    <q-header class="bg-grey-12 text-primary q-pa-md">
      <q-card class="column bg-white">
        <q-toolbar class="row items-center full-width">
          <MenuOpenButton v-model="drawer" />

          <q-toolbar-title>
            <BreadcrumbsIndex />
          </q-toolbar-title>

          <GlobalSignalR />

          <I18nSetting />

          <UserInfo class="q-ml-md" />

        </q-toolbar>

        <q-separator />

        <TagsView />
      </q-card>
    </q-header>

    <q-drawer v-model="drawer" show-if-above :mini="miniState" :width="280">
      <MenuTree />
    </q-drawer>

    <q-page-container class="page-container">
      <q-page class="q-px-md q-pb-md full-height full-with">
        <q-scroll-area class="page__scroll-area full-height full-with" :thumb-style="thumbStyle"
          :content-style="contentStyle" :content-active-style="contentActiveStyle">
          <router-view v-slot="{ Component }">
            <transition appear enter-active-class="animated fadeInDown">
              <keep-alive :include="cachedViews">
                <component :is="Component" :key="getRouteId($route.fullPath, $route.query)" />
              </keep-alive>
            </transition>
          </router-view>
        </q-scroll-area>
      </q-page>
    </q-page-container>
  </q-layout>
</template>

<script lang="ts" setup>
import logger from 'loglevel'

// 导入组件
import MenuOpenButton from '../components/menuOpen/menuOpenButton.vue'
import I18nSetting from '../components/i18nSetting/I18nSetting.vue'
import UserInfo from '../components/userInfo/userInfo.vue'
import BreadcrumbsIndex from '../components/breadcrumbs/breadcrumbsIndex.vue'
import TagsView from '../components/tags/tagsView.vue'
import GlobalSignalR from '../components/signalR/GlobalSignalR.vue'
import { useScrollAreaStyle } from 'src/compositions/scrollUtils'
// import LeftSidebarIndex from '../components/leftSidebar/leftSidebarIndex.vue'
import MenuTree from '../components/leftSidebar/menuTree.vue'
import { getRouteId, useRouteHistories } from '../components/tags/routeHistories'

const drawer = ref(false)
const miniState = ref(false)
// const collapse = ref(false)

// #region 开关左侧抽屉
const $q = useQuasar()
if ($q.platform.is.desktop && $q.screen.gt.md) {
  drawer.value = true
}

// 缓存
const { routes: routeHistories } = useRouteHistories()
const cachedViews = computed(() => {
  const results = routeHistories.value.filter(x => !x.noCache).map(x => x.name)
  logger.debug('[Layout] cachedViews:', results)
  return results
})

const { contentStyle, contentActiveStyle, thumbStyle } = useScrollAreaStyle()

onMounted(() => {
  logger.debug('[Layout] onMounted')
})
</script>

<script lang="ts">
export default {
  name: 'NormalLayout'
}
</script>

<style lang="scss" scoped>
.menu-button {
  transform: scale(0.6);
}

.page-container {
  overflow: hidden;
  position: absolute;
  top: 0px;
  left: 0px;
  bottom: 0px;
  right: 0px;
  background-color: $grey-12;

  .page__scroll-area {
    :deep(.q-scrollarea__content) {
      width: 100%;
    }
  }
}
</style>
