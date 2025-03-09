import { route } from 'quasar/wrappers'
import {
  createMemoryHistory,
  createRouter,
  createWebHashHistory,
  createWebHistory
} from 'vue-router'
import type { Router } from 'vue-router'

import { constantRoutes } from './routes'
import { useRoutesStore } from 'src/stores/routes'
import { useUserInfoStore } from 'src/stores/user'
import logger from 'loglevel'

let router: Router
export function useRouter () {
  return router
}

/*
 * If not building with SSR mode, you can
 * directly export the Router instantiation;
 *
 * The function below can be async too; either use
 * async/await or return a Promise which resolves
 * with the Router instance....
 */

export default route(function (/* { store, ssrContext } */) {
  const createHistory = process.env.SERVER
    ? createMemoryHistory
    : process.env.VUE_ROUTER_MODE === 'history'
      ? createWebHistory
      : createWebHashHistory

  router = createRouter({
    scrollBehavior: () => ({ left: 0, top: 0 }),
    routes: constantRoutes,

    // Leave this as is and make changes in quasar.conf.js instead!
    // quasar.conf.js -> build -> vueRouterMode
    // quasar.conf.js -> build -> publicPath
    history: createHistory(process.env.VUE_ROUTER_BASE)
  })

  const userInfoStore = useUserInfoStore()

  logger.debug('[Router] 配置前置守卫')
  // 添加路由前置守卫
  router.beforeEach((to, from, next) => {
    if (!to.meta.anoymous && !userInfoStore.token && to.path !== '/login') {
      // 跳转到登陆界面
      next('/login')
      return
    }

    logger.debug('[Router] 路由前置守卫触发：', to, from)
    // 添加动态路由
    const routeStore = useRoutesStore()
    if (to.path !== '/login' && routeStore.addDynamicRoutes()) {
      logger.debug('[Router] 添加动态路由')
      // 重新导航
      return next({ ...to, replace: true })
    }
    next()
  })

  // 添加路由后置守卫
  // Router.afterEach((to, from) => {
  //   console.log('路由后置守卫', to, from)
  // })
  return router
})
