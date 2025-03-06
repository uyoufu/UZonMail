<template>
  <q-icon name="translate" size="sm" color="secondary">
    <HoverableTip class="bg-white" anchor="bottom right" self="top right" :offset="[0, 6]">
      <q-list class="rounded-borders shadow-1 language-menu" bordered separator style="overflow: hidden;" dense>
        <q-item v-for="translation in sortedTranslations" :key="translation.locale" clickable
          @click="onSwitchLocale(translation.locale)" class="active-item text-primary">
          <div class="row no-wrap items-center justify-between full-width">
            <div>{{ translation.label }}</div>
            <q-icon v-if="translation.locale === locale" name="check" color="secondary"></q-icon>
          </div>
        </q-item>
      </q-list>
    </HoverableTip>
  </q-icon>
</template>

<script lang="ts" setup>
import HoverableTip from 'src/components/hoverableTip/HoverableTip.vue'
import { translations } from 'src/i18n/index'
import { useUserInfoStore } from 'src/stores/user'

const store = useUserInfoStore()
const sortedTranslations = computed(() => {
  return translations.sort((a, b) => a.locale.localeCompare(b.locale))
})

import { useI18n } from 'vue-i18n'
const { locale } = useI18n()
async function onSwitchLocale (value: string) {
  store.setLocale(value)
  locale.value = value
}
</script>

<style lang="scss" scoped>
.language-menu {
  overflow: hidden;
  font-size: 14px;
  min-width: 110px;

  .active-item {
    &:hover {
      // 放大
      scale: 1.1;
      transition: all 0.3s;
    }
  }
}
</style>
