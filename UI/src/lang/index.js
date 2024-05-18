import VueI18n from 'vue-i18n'
import customZH from './zh'
import customEN from './en'
import customIT from './it'

import elementEN from 'element-ui/lib/locale/lang/en'
import elementZH from 'element-ui/lib/locale/lang/zh-CN'
import elementIT from 'element-ui/lib/locale/lang/it'

import Vue from 'vue'
Vue.use(VueI18n)

// 定义语言包
export const i18n = new VueI18n({
  locale: localStorage.getItem('lang') || 'it',
  fallbackLocale: 'it',
  messages: {
    en: {
      ...elementEN,
      ...customEN
    },
    zh: {
      ...elementZH,
      ...customZH
    },
    it: {
      ...elementIT,
      ...customIT
    }
  }
})

export default i18n
