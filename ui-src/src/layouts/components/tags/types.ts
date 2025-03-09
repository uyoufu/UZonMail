import type { LocationQuery } from 'vue-router'

/**
 * 路由历史
 */
export interface IRouteHistory {
  id: string, // 全路径 + 查询参数
  fullPath: string,
  query: LocationQuery,
  name: string,
  label: string,
  isActive: boolean,
  icon: string,
  noCache: boolean,
  showCloseIcon: boolean,
}
