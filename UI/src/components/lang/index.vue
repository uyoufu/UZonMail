<template>
  <el-dropdown trigger="click" @command="changeLanguage">
    <div class="lang-icon-container">
      <svg-icon :icon-class="languageIcon" style="font-size: xx-large;" />
    </div>
    <el-dropdown-menu slot="dropdown" class="lang-dropdown">
      <el-dropdown-item command="zh" :disabled="'zh' === $i18n.locale">
        <svg-icon icon-class="zh" class="lang-dropdown-icon" />
        <span class="lang-dropdown-text">中文</span>
      </el-dropdown-item>
      <el-dropdown-item command="en" :disabled="'en' === $i18n.locale">
        <svg-icon icon-class="en" class="lang-dropdown-icon" />
        <span class="lang-dropdown-text">English</span>
      </el-dropdown-item>
      <el-dropdown-item command="it" :disabled="'it' === $i18n.locale">
        <svg-icon icon-class="it" class="lang-dropdown-icon" />
        <span class="lang-dropdown-text">Italian</span>
      </el-dropdown-item>
    </el-dropdown-menu>
  </el-dropdown>
</template>

<script>
import SvgIcon from '@/components/SvgIcon'
import langZH from 'quasar/lang/zh-hans.js'
import langEN from 'quasar/lang/en-us.js'
import langIT from 'quasar/lang/it.js'
import { Quasar } from 'quasar'

export default {
  name: 'Lang',
  components: {
    SvgIcon
  },
  methods: {
    changeLanguage(lang) {
      localStorage.setItem('lang', lang)
      switch (lang) {
        case 'en':
          Quasar.lang.set(langEN)
          break
        case 'zh-CN':
          Quasar.lang.set(langZH)
          break
        case 'it':
          Quasar.lang.set(langIT)
          break
        default:
          Quasar.lang.set(langZH)
      }

      this.$i18n.locale = lang
      this.$message.success(this.$t('langSwitchSuccess')) // 使用国际化消息
    }
  },
  computed: {
    languageIcon() {
      return this.$i18n.locale
    }
  }
}
</script>

<style scoped>
.lang-icon-container {
  display: flex;
  align-items: center;
  margin: 5px;
}

.lang-dropdown {
  padding: 0;
}

.lang-dropdown-item {
  display: flex;
  align-items: center;
}

.lang-dropdown-icon {
  margin-right: 8px;
}

.lang-dropdown-text {
  font-size: 14px;
}
</style>
