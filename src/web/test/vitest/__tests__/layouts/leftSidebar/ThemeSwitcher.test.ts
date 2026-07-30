import { mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

vi.mock('#q-app/wrappers', () => ({
  defineBoot: <T>(bootCallback: T): T => bootCallback
}))

import { i18n } from 'src/boot/i18n'
import ThemeSwitcher from 'src/layouts/components/leftSidebar/ThemeSwitcher.vue'
import { ColorThemeName, useThemeStore } from 'src/stores/theme'

const initialLocale = i18n.global.locale.value

describe('ThemeSwitcher', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    window.localStorage.clear()
    document.body.removeAttribute('style')
    i18n.global.locale.value = 'en-US'
  })

  afterEach(() => {
    i18n.global.locale.value = initialLocale
  })

  it('switches to the fairy pink theme through its accessible color swatch', async () => {
    const wrapper = mount(ThemeSwitcher)

    await wrapper.get('button[aria-label="Fairy pink theme"]').trigger('click')

    expect(useThemeStore().colorTheme).toBe(ColorThemeName.fairyPink)
    expect(document.body.style.getPropertyValue('--q-primary')).toBe('var(--uzon-fairy-pink-primary)')
    expect(wrapper.get('button[aria-label="Fairy pink theme"]').attributes('aria-pressed')).toBe('true')
  })
})
