import type { NamedValue } from 'vue-i18n'
import type {
  LangKey,
  RoutesLangKey,
  LoginPageLangKey,
  DashboardPageLangKey,
  InboxManagerLangKey,
  ButtonLangKey,
  EmailGroupLangKey,
  GlobalLangKey,
  outboxManagerLangKey,
  SendingTaskLangKey,
  UtilsLangKey
} from './types'

import { i18n } from 'src/boot/i18n'
const i18nTranslate = i18n.global.t

/**
 * get current locale from i18n
 * @returns
 */
export function getCurrentLocale (): WritableComputedRef<string, string> {
  return i18n.global.locale
}

/**
 * 翻译为对应语言
 * @param key
 * @returns
 */
export function t (key: LangKey, named?: NamedValue): string {
  if (named === undefined) return i18nTranslate(key)
  return i18nTranslate(key, named)
}

export function translateSub<T extends string> (key: T, subKey: LangKey, named?: NamedValue): string {
  if (!key.startsWith(`${subKey}.`)) {
    key = `${subKey}.${key}` as T
  }
  return t(key as LangKey, named)
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

export function translateInboxManager (key: InboxManagerLangKey, named?: NamedValue): string {
  return translateSub<InboxManagerLangKey>(key, 'inboxManager', named)
}

export function translateOutboxManager (key: outboxManagerLangKey, named?: NamedValue): string {
  return translateSub<outboxManagerLangKey>(key, 'outboxManager', named)
}

export function translateButton (key: ButtonLangKey): string {
  return translateSub<ButtonLangKey>(key, 'buttons')
}

export function translateEmailGroup (key: EmailGroupLangKey, named?: NamedValue): string {
  return translateSub<EmailGroupLangKey>(key, 'emailGroup', named)
}

export function translateGlobal (key: GlobalLangKey, named?: NamedValue): string {
  return translateSub<GlobalLangKey>(key, 'global', named)
}

export function translateSendingTask (key: SendingTaskLangKey, named?: NamedValue): string {
  return translateSub<SendingTaskLangKey>(key, 'sendingTask', named)
}

export function translateUtils (key: UtilsLangKey, named?: NamedValue): string {
  return translateSub<UtilsLangKey>(key, 'utils', named)
}

