<template>
  <q-icon name="palette" size="sm" color="primary" class="cursor-pointer">
    <HoverableTip class="bg-white" anchor="bottom right" self="top right" :offset="[4, 8]">
      <q-list class="rounded-borders shadow-1 theme-switcher__menu" bordered separator dense>
        <q-item v-for="themeOption in themeOptions" :key="themeOption.name" class="theme-switcher__option text-primary"
          clickable role="button" :aria-label="themeOption.label" :aria-pressed="themeOption.name === colorTheme"
          @click="onColorThemeClick(themeOption.name)">
          <div class="row no-wrap items-center full-width">
            <span class="theme-switcher__swatch"
              :class="{ 'theme-switcher__swatch--default': themeOption.name === ColorThemeName.default }"
              :style="{ backgroundColor: themeOption.primaryColor }"></span>
            <span class="q-ml-sm">{{ themeOption.label }}</span>
            <q-space class="q-ml-sm" />
            <q-icon v-if="themeOption.name === colorTheme" name="check" color="secondary" />
          </div>
        </q-item>
      </q-list>
    </HoverableTip>
  </q-icon>
</template>

<script lang="ts" setup>
import { storeToRefs } from 'pinia'
import { computed } from 'vue'
import HoverableTip from 'src/components/hoverableTip/HoverableTip.vue'
import { translateComponents } from 'src/i18n/helpers'
import {
  COLOR_THEME_DEFINITIONS,
  ColorThemeName,
  type ColorThemeName as ColorThemeNameType,
  useThemeStore
} from 'src/stores/theme'

defineOptions({
  name: 'ThemeSwitcher'
})

const themeStore = useThemeStore()
const { colorTheme } = storeToRefs(themeStore)
const themeOptions = computed(() => [
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
])

function onColorThemeClick(colorThemeName: ColorThemeNameType) {
  themeStore.setColorTheme(colorThemeName)
}
</script>

<style lang="scss" scoped>
.theme-switcher {
  &__menu {
    overflow: hidden;
    font-size: 14px;
  }

  &__swatch {
    width: 16px;
    height: 16px;
    flex: 0 0 16px;
    border: 1px solid rgba(0, 0, 0, 0.12);
    border-radius: 50%;

    // 默认主题色必须保持编译期颜色，避免切换主题后丢失默认色预览
    &--default {
      background-color: $primary;
    }
  }
}
</style>
