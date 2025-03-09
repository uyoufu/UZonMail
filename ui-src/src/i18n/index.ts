
import enUS from './locales/en-US'
import zhCN from './locales/zh-CN'

export type Translation = {
  [key: string]: string | { [key: string]: string }
}

export interface ITranslation {
  locale: string
  label: string
  translation: Translation
}

const translations: ITranslation[] = [
  {
    locale: 'en-US',
    label: 'English',
    translation: enUS
  },
  {
    locale: 'zh-CN',
    label: '简体中文',
    translation: zhCN
  }
]

const messages = translations.reduce((acc, cur) => {
  acc[cur.locale] = cur.translation
  return acc
}, {} as Record<string, Translation>)

export {
  messages,
  translations
}
