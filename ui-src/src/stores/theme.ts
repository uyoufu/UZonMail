import { defineStore } from 'pinia'

// 用户样式定义
export const useThemeStore = defineStore('theme', {
  state: () => ({
    showBreadcrumbs: true,
    showTagsView: true
  }),

  actions: {
    toggleBreadcrumbs () {
      this.showBreadcrumbs = !this.showBreadcrumbs
    },

    toggleTagsView () {
      this.showTagsView = !this.showTagsView
    }
  }
})

/**
 * 拉取用户的 UI 设置
 * @param userId
 * @returns
 */
export function pullUserUISettings (userId: string) {
  return {
    userId,
    theme: 'dark'
  }
}
