import type { LocationQuery, Router } from 'vue-router'
import type { IRouteHistory } from './types'
import logger from 'loglevel'

export function getRouteId (fullPath: string, query: LocationQuery) {
  return fullPath + JSON.stringify(query)
}

export const routes: Ref<IRouteHistory[]> = ref([])

/**
* 移除标签
* @param router
* @param route
* @param nextPath
* @returns
*/
export async function removeHistory (router: Router, route: IRouteHistory, nextPath?: string) {
  // 首页不可移除
  if (routes.value.length === 1 && routes.value[0]!.fullPath === '/') {
    return
  }

  const routeId = getRouteId(route.fullPath, route.query)
  let currentTagIndex = routes.value.findIndex((item) => item.id === routeId)
  if (currentTagIndex >= 0) {
    // 从历史中移除
    routes.value.splice(currentTagIndex, 1)
  }

  if (nextPath) {
    await router.push({
      path: nextPath
    })
    return
  }

  // 如果已经没有 tags，则跳转到首页
  if (routes.value.length === 0) {
    await router.push({
      path: '/'
    })
    return
  }

  // 向前显示
  if (currentTagIndex > 0) {
    currentTagIndex -= 1
  }

  // 显示当前
  const currentTag = routes.value[currentTagIndex] as IRouteHistory
  await router.push({
    path: currentTag.fullPath,
    query: currentTag.query
  })
}

export function useRouteHistories () {
  // 监听 route 变化
  const route = useRoute()
  watch(route, (newRoute) => {
    logger.debug('[Layout] route changed', newRoute)
    // 隐藏标签的路由，不显示
    if (newRoute.meta.noTag) return

    // 将其它路由设置为非激活状态
    routes.value.forEach((item) => {
      item.isActive = false
    })

    // 判断是否已存在
    const routeId = getRouteId(newRoute.fullPath, newRoute.query)
    const existIndex = routes.value.findIndex((item) => item.id === routeId)
    // 不存在时，添加
    if (existIndex < 0) {
      const routeTemp = {
        id: routeId,
        fullPath: newRoute.fullPath,
        label: newRoute.meta.label,
        name: newRoute.name,
        isActive: true,
        icon: newRoute.meta.icon,
        // 专门用于标签页的属性
        showCloseIcon: false,
        noCache: newRoute.meta.noCache,
        query: newRoute.query
      } as IRouteHistory
      routes.value.push(routeTemp)
    } else {
      // 替换
      const existRoute = routes.value[existIndex] as IRouteHistory
      existRoute.isActive = true
    }
  }, { immediate: true })

  return {
    routes,
    removeHistory
  }
}
