import { defineStore } from 'pinia'
import { setCssVar } from 'quasar'

/** 持久化用户主题所使用的 Local Storage 键 */
export const COLOR_THEME_STORAGE_KEY = 'color-theme'
const QUASAR_BRAND_COLOR_NAMES = ['primary', 'secondary', 'accent'] as const

type QuasarBrandColorName = typeof QUASAR_BRAND_COLOR_NAMES[number]

/** 可供用户选择的品牌色主题 */
export const ColorThemeName = {
  default: 'default',
  fairyPink: 'fairy-pink'
} as const

/** 品牌色主题的唯一标识 */
export type ColorThemeName = typeof ColorThemeName[keyof typeof ColorThemeName]

/** 每个主题覆盖的 Quasar 品牌色 */
export const COLOR_THEME_DEFINITIONS: Record<ColorThemeName, Partial<Record<QuasarBrandColorName, string>>> = {
  [ColorThemeName.default]: {},
  [ColorThemeName.fairyPink]: {
    primary: 'var(--uzon-fairy-pink-primary)',
    secondary: 'var(--uzon-fairy-pink-secondary)',
    accent: 'var(--uzon-fairy-pink-accent)'
  }
}

function isColorThemeName (value: string | null): value is ColorThemeName {
  return value === ColorThemeName.default || value === ColorThemeName.fairyPink
}

/** 从本地存储读取有效主题，无效值回退为默认主题 */
export function getStoredColorTheme (): ColorThemeName {
  if (typeof window === 'undefined') return ColorThemeName.default

  const storedColorTheme = window.localStorage.getItem(COLOR_THEME_STORAGE_KEY)
  return isColorThemeName(storedColorTheme) ? storedColorTheme : ColorThemeName.default
}

/** 将主题品牌色应用到 Quasar 的运行时 CSS 变量 */
export function applyColorTheme (colorTheme: ColorThemeName) {
  if (typeof document === 'undefined') return

  const themeColors = COLOR_THEME_DEFINITIONS[colorTheme]
  for (const colorName of QUASAR_BRAND_COLOR_NAMES) {
    const colorValue = themeColors[colorName]
    if (colorValue) {
      setCssVar(colorName, colorValue)
      continue
    }

    // 默认主题继续由 SCSS 编译的 Quasar 变量提供颜色，不能在这里重复维护色值
    document.body.style.removeProperty(`--q-${colorName}`)
  }
}

/** 恢复已保存的主题并返回实际应用的值 */
export function restoreColorTheme (): ColorThemeName {
  const colorTheme = getStoredColorTheme()
  applyColorTheme(colorTheme)
  return colorTheme
}

/** 管理用户界面显示选项与品牌色主题 */
export const useThemeStore = defineStore('theme', {
  state: () => ({
    showBreadcrumbs: true,
    showTagsView: true,
    colorTheme: getStoredColorTheme()
  }),

  actions: {
    toggleBreadcrumbs () {
      this.showBreadcrumbs = !this.showBreadcrumbs
    },

    toggleTagsView () {
      this.showTagsView = !this.showTagsView
    },

    /** 设置并持久化当前品牌色主题 */
    setColorTheme (colorTheme: ColorThemeName) {
      this.colorTheme = colorTheme
      applyColorTheme(colorTheme)

      if (typeof window !== 'undefined') {
        window.localStorage.setItem(COLOR_THEME_STORAGE_KEY, colorTheme)
      }
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
