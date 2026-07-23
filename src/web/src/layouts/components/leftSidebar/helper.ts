import type { RouteRecordRaw } from 'vue-router'

// 获取可显示的路由数量
function getDisplayableRoutesCount (routes: RouteRecordRaw[]): number {
  return routes.filter(route => !route.meta?.noMenu).length
}

/**
 * 获取菜单路由
 * 若路由仅有一个子路由，则返回子路由
 */
export function getMenuRoute (route: RouteRecordRaw) {
  if (route.children && getDisplayableRoutesCount(route.children) === 1) {
    // 根的 noMenu 要向下传递
    const child = route.children[0] as RouteRecordRaw
    if (child.meta && route.meta?.noMenu) {
      child.meta.noMenu = true
    }

    return getMenuRoute(child)
  }

  return route
}
