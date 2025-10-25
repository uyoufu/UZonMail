import type {
  LangKey,
  RoutesLangKey,
  LoginPageLangKey,
  DashboardPageLangKey,
  InboxManagerLangKey,
  ButtonLangKey,
  EmailGroupLangKey,
  GlobalLangKey
} from './types'

import { i18n } from 'src/boot/i18n'
const t = i18n.global.t


/**
 * 翻译为对应语言
 * @param key
 * @returns
 */
export function translate (key: LangKey): string {
  return t(key)
}

export function translateSub<T extends string> (key: T, subKey: LangKey): string {
  if (!key.startsWith(`${subKey}.`)) {
    key = `${subKey}.${key}` as T
  }
  return translate(key as LangKey)
}

/**
 * 获取路由的国际化名称
 * @param key
 * @param default
 * @returns
 */
export function translateRoutes (key: RoutesLangKey): string {
  return translateSub<RoutesLangKey>(key, 'routes')
}

/**
 * 登录页面国际化
 * @param key
 * @returns
 */
export function translateLoginPage (key: LoginPageLangKey): string {
  return translateSub<LoginPageLangKey>(key, 'loginPage')
}

export function translateDashboardPage (key: DashboardPageLangKey): string {
  return translateSub<DashboardPageLangKey>(key, 'dashboardPage')
}

export function translateInboxManager (key: InboxManagerLangKey): string {
  return translateSub<InboxManagerLangKey>(key, 'inboxManager')
}

export function translateButton (key: ButtonLangKey): string {
  return translateSub<ButtonLangKey>(key, 'buttons')
}

export function translateEmailGroup (key: EmailGroupLangKey): string {
  return translateSub<EmailGroupLangKey>(key, 'emailGroup')
}

export function translateGlobal (key: GlobalLangKey): string {
  return translateSub<GlobalLangKey>(key, 'global')
}

