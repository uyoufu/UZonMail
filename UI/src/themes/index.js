// src/themes/index.js
import { button as rawButton, table as rawTable} from './default/index'
import translateButtonConfig from './translate'
import i18n from '@/lang/index'
import Vue from 'vue'

// 初始翻译
const button = Vue.observable(translateButtonConfig(rawButton, i18n))
const table = Vue.observable(translateButtonConfig(rawTable, i18n))

// 创建一个Vue实例来监听语言变化
const vm = new Vue({
  i18n,
  data: {
    button
  },
  watch: {
    '$i18n.locale': {
      handler() {
        const translatedButton = translateButtonConfig(rawButton, i18n)
        Object.keys(translatedButton).forEach(key => {
          Vue.set(button, key, translatedButton[key])
        })
        const translatedTable = translateButtonConfig(rawTable, i18n)
        Object.keys(translatedTable).forEach(key => {
          Vue.set(table, key, translatedTable[key])
        })
      },
      immediate: true
    }
  }
})
export { button, table }
