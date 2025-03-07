import { boot } from 'quasar/wrappers'
import { createI18n } from 'vue-i18n'
import { useSessionStorage } from '@vueuse/core'

import { messages } from 'src/i18n'

export type MessageLanguages = keyof typeof messages;
// Type-define 'en-US' as the master schema for the resource
export type MessageSchema = typeof messages['en-US'];

// See https://vue-i18n.intlify.dev/guide/advanced/typescript.html#global-resource-schema-type-definition
/* eslint-disable @typescript-eslint/no-empty-interface */
declare module 'vue-i18n' {
  // define the locale messages schema
  export interface DefineLocaleMessage extends MessageSchema { }

  // define the datetime format schema
  export interface DefineDateTimeFormat { }

  // define the number format schema
  export interface DefineNumberFormat { }
}
/* eslint-enable @typescript-eslint/no-empty-interface */

export function getDefaultLocale (): string {
  // 判断 session 中是否有 locale，若有，则使用 session 中的 locale
  const browserLang = useSessionStorage('locale', '').value
  const messagesKeys = Object.keys(messages)
  const browserLangKey = messagesKeys.find(key => key.startsWith(browserLang))
  return browserLangKey || 'zh-CN'
}

export const i18n = createI18n({
  locale: getDefaultLocale(),
  fallbackLocale: 'zh-CN',
  silentFallbackWarn: true, // 控制台上不打印警告
  legacy: false, // 如果要支持compositionAPI，此项必须设置为false
  messages,
  globalInjection: true // 注册全局 $t
})

export default boot(({ app }) => {
  // Set i18n instance on app
  app.use(i18n)
})
