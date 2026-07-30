<template>
  <q-item class="theme-switcher q-px-md q-py-sm" dense>
    <q-item-section avatar>
      <q-icon name="palette" color="primary" />
    </q-item-section>

    <q-item-section class="theme-switcher__swatches">
      <q-btn v-for="themeOption in themeOptions" :key="themeOption.name" round unelevated size="xs"
        class="theme-switcher__swatch" :class="{
          'theme-switcher__swatch--default': themeOption.name === ColorThemeName.default,
          'theme-switcher__swatch--selected': themeOption.name === colorTheme
        }" :style="{ backgroundColor: themeOption.primaryColor }" :aria-label="themeOption.label"
        :aria-pressed="themeOption.name === colorTheme" @click="onColorThemeClick(themeOption.name)">
        <q-icon v-if="themeOption.name === colorTheme" name="check" color="white" size="16px" />
        <q-tooltip>{{ themeOption.label }}</q-tooltip>
      </q-btn>
    </q-item-section>
  </q-item>
</template>

<script lang="ts" setup>
import { storeToRefs } from 'pinia'
import { COLOR_THEME_DEFINITIONS, ColorThemeName, type ColorThemeName as ColorThemeNameType, useThemeStore } from 'src/stores/theme'
import { translateComponents } from 'src/i18n/helpers'

/** 在左侧导航底部提供品牌色主题切换 */
defineOptions({
  name: 'ThemeSwitcher'
})

const themeStore = useThemeStore()
const { colorTheme } = storeToRefs(themeStore)
const themeOptions = [
  {
    name: ColorThemeName.default,
    label: translateComponents('defaultTheme'),
    primaryColor: undefined
  },
  {
    name: ColorThemeName.fairyPink,
    label: translateComponents('cherryBlossomRomanceTheme'),
    primaryColor: COLOR_THEME_DEFINITIONS[ColorThemeName.fairyPink].primary
  }
] as const

function onColorThemeClick(colorThemeName: ColorThemeNameType) {
  themeStore.setColorTheme(colorThemeName)
}
</script>

<style lang="scss" scoped>
.theme-switcher {
  &__swatches {
    display: flex;
    flex-direction: row;
    align-items: center;
    gap: 8px;
  }

  &__swatch {
    width: 20px;
    height: 20px;
    border: 2px solid transparent;

    // 默认主题色板需保持编译期颜色，以便当前主题为粉色时仍可预览默认色
    &--default {
      background-color: $primary;
    }

    &--selected {
      border-color: white;
      box-shadow: 0 0 0 1px var(--q-accent);
    }
  }
}
</style>
