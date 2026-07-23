import { defineStore } from 'pinia'
import { useRouter } from 'vue-router'
import { useUserInfoStore } from './user'
import { constantRoutes, dynamicRoutes, exceptionRoutes } from 'src/router/routes'
import type { ExtendedRouteRecordRaw } from 'src/router/types'
import logger from 'loglevel'
import _ from 'lodash'

// 过滤动态路由
function filterDynamicRouesByAccess (routes: ExtendedRouteRecordRaw[]): ExtendedRouteRecordRaw[] {
  if (!routes || routes.length === 0) return []
  const results: ExtendedRouteRecordRaw[] = []
  const userInfoStore = useUserInfoStore()
  for (const route of routes) {
    // 判断是否被拒绝, 被拒绝的权限大于允许的权限
    if (route.meta?.denies) {
      logger.debug(`[Router] 验证路由 ${route.name as string} 拒绝权限：`, route.meta.denies)
      if (userInfoStore.hasPermission(route.meta?.denies)) {
        continue
      }
    }

    // 判断是否有权限
    if (route.meta?.access) {
      logger.debug(`[Router] 验证路由 ${route.name as string} 需要权限：`, route.meta.access)
      if (!userInfoStore.hasPermission(route.meta.access)) {
        continue
      }
    }

    // 保存
    const addedRoute = _.cloneDeep(route)
    results.push(addedRoute)

    // 判断子级权限
    const children = filterDynamicRouesByAccess(route.children as ExtendedRouteRecordRaw[])
    addedRoute.children = children
  }
  return results
}

// options 方式定义
export const useRoutesStore = defineStore('routes', {
  state: () => ({
    isAddedDynamicRoutes: false,
    // 该参数不包含异常处理路由
    loadedRoutes: [] as ExtendedRouteRecordRaw[]
  }),
  actions: {
    /**
     * 添加动态路由,若添加了，则返回 true
     * @returns
     */
    addDynamicRoutes (): boolean {
      if (this.isAddedDynamicRoutes) return false
      this.isAddedDynamicRoutes = true

      logger.debug('[Router] addDynamicRoutes. all routes:', dynamicRoutes)

      // 动态添加路由
      const accessRoutes = filterDynamicRouesByAccess(dynamicRoutes)
      logger.debug('[Router] 添加的动态路由：', accessRoutes)
      const router = useRouter()
      accessRoutes.forEach(route => router.addRoute(route))
      exceptionRoutes.forEach(route => router.addRoute(route))

      // 将静态和动态路由保存到 store 中
      this.loadedRoutes = [...constantRoutes, ...accessRoutes]
      return true
    },

    /**
     * 重置动态路由
     */
    resetDynamicRoutes () {
      this.isAddedDynamicRoutes = false
    }
  }
})
