import { defineBoot } from '#q-app/wrappers'
import { createI18n } from 'vue-i18n'
import { useSessionStorage } from '@vueuse/core'

/*
 * All i18n resources specified in the plugin `include` option can be loaded
 * at once using the import syntax
 */
import { messages } from 'src/i18n'
import { getDesktopLocale, resolveSupportedLocale } from 'src/i18n/desktopLocale'

export type MessageLanguages = keyof typeof messages
// Type-define 'en-US' as the master schema for the resource
export type MessageSchema = typeof messages['en-US']

// See https://vue-i18n.intlify.dev/guide/advanced/typescript.html#global-resource-schema-type-definition
/* eslint-disable @typescript-eslint/no-empty-object-type */
declare module 'vue-i18n' {
  // define the locale messages schema
  export interface DefineLocaleMessage extends MessageSchema { }

  // define the datetime format schema
  export interface DefineDateTimeFormat { }

  // define the number format schema
  export interface DefineNumberFormat { }
}
/* eslint-enable @typescript-eslint/no-empty-object-type */

export function getDefaultLocale (): string {
  const storedLocale = useSessionStorage('locale', '')
  const desktopLocale = getDesktopLocale()
  if (desktopLocale) {
    // 桌面端配置需要同时成为本次 WebView 会话的单一语言来源
    storedLocale.value = desktopLocale
    return desktopLocale
  }

  const browserLocale = resolveSupportedLocale(storedLocale.value)
    ?? (storedLocale.value
      ? Object.keys(messages).find(key => key.startsWith(storedLocale.value))
      : undefined)
  return browserLocale || 'zh-CN'
}

export const i18n = createI18n({
  locale: getDefaultLocale(),
  // 回退语言
  fallbackLocale: 'zh-CN',
  silentFallbackWarn: true, // 控制台上不打印警告
  legacy: false, // 如果要支持 compositionAPI，此项必须设置为false
  messages,
  globalInjection: true, // 注册全局 $t
})


export default defineBoot(({ app }) => {
  // Set i18n instance on app
  app.use(i18n)
})
