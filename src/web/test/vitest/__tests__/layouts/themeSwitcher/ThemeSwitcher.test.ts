import { mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import { afterEach, beforeEach, describe, expect, it } from 'vitest'

import { i18n } from 'src/boot/i18n'
import ThemeSwitcher from 'src/layouts/components/themeSwitcher/ThemeSwitcher.vue'
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

  it('shows theme options and switches to the cherry blossom romance theme', async () => {
    const wrapper = mount(ThemeSwitcher, {
      global: {
        stubs: {
          HoverableTip: {
            template: '<div data-testid="theme-options"><slot /></div>'
          }
        }
      }
    })

    const defaultThemeOption = wrapper.get('[role="button"][aria-label="Default theme"]')
    const cherryBlossomThemeOption = wrapper.get('[role="button"][aria-label="Cherry Blossom Romance"]')

    expect(wrapper.get('[data-testid="theme-options"]').isVisible()).toBe(true)
    expect(wrapper.findAll('.theme-switcher__option')).toHaveLength(2)
    expect(defaultThemeOption.attributes('aria-pressed')).toBe('true')

    await cherryBlossomThemeOption.trigger('click')

    expect(useThemeStore().colorTheme).toBe(ColorThemeName.fairyPink)
    expect(document.body.style.getPropertyValue('--q-primary')).toBe('var(--uzon-fairy-pink-primary)')
    expect(cherryBlossomThemeOption.attributes('aria-pressed')).toBe('true')
    expect(defaultThemeOption.attributes('aria-pressed')).toBe('false')
  })
})
