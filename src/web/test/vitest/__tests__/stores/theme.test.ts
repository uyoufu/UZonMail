import { createPinia, setActivePinia } from 'pinia'
import { beforeEach, describe, expect, it } from 'vitest'
import {
  applyColorTheme,
  COLOR_THEME_STORAGE_KEY,
  ColorThemeName,
  restoreColorTheme,
  useThemeStore
} from 'src/stores/theme'

const brandColorNames = ['primary', 'secondary', 'accent'] as const

function getBrandColorValue (colorName: typeof brandColorNames[number]) {
  return document.body.style.getPropertyValue(`--q-${colorName}`)
}

describe('theme store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    window.localStorage.clear()
    document.body.removeAttribute('style')
  })

  it('applies and persists the fairy pink Quasar brand colors', () => {
    const themeStore = useThemeStore()

    themeStore.setColorTheme(ColorThemeName.fairyPink)

    expect(themeStore.colorTheme).toBe(ColorThemeName.fairyPink)
    expect(window.localStorage.getItem(COLOR_THEME_STORAGE_KEY)).toBe(ColorThemeName.fairyPink)
    expect(getBrandColorValue('primary')).toBe('var(--uzon-fairy-pink-primary)')
    expect(getBrandColorValue('secondary')).toBe('var(--uzon-fairy-pink-secondary)')
    expect(getBrandColorValue('accent')).toBe('var(--uzon-fairy-pink-accent)')
  })

  it('removes runtime overrides when returning to the default theme', () => {
    applyColorTheme(ColorThemeName.fairyPink)
    const themeStore = useThemeStore()

    themeStore.setColorTheme(ColorThemeName.default)

    expect(themeStore.colorTheme).toBe(ColorThemeName.default)
    expect(window.localStorage.getItem(COLOR_THEME_STORAGE_KEY)).toBe(ColorThemeName.default)
    for (const colorName of brandColorNames) {
      expect(getBrandColorValue(colorName)).toBe('')
    }
  })

  it('falls back to the default theme when saved storage is invalid', () => {
    window.localStorage.setItem(COLOR_THEME_STORAGE_KEY, 'unsupported-theme')
    applyColorTheme(ColorThemeName.fairyPink)

    expect(restoreColorTheme()).toBe(ColorThemeName.default)
    for (const colorName of brandColorNames) {
      expect(getBrandColorValue(colorName)).toBe('')
    }
  })
})
