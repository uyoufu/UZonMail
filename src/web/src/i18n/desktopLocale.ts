import { messages } from 'src/i18n'
import logger from 'loglevel'

/** 将宿主或浏览器返回的语言代码限制为前端已加载的语言 */
export function resolveSupportedLocale(locale: unknown): string | undefined {
  if (typeof locale !== 'string') return undefined
  return Object.prototype.hasOwnProperty.call(messages, locale) ? locale : undefined
}

/** 从桌面端同步宿主对象读取当前语言 */
export function getDesktopLocale(): string | undefined {
  if (typeof window === 'undefined') return undefined

  try {
    return resolveSupportedLocale(window.chrome?.webview?.hostObjects?.sync?.uzonMailLocale?.GetCurrentLocale())
  } catch {
    return undefined
  }
}

/** 将前端选择同步到桌面端，普通浏览器中不改变现有语言行为 */
export async function persistDesktopLocale(locale: string): Promise<boolean> {
  if (typeof window === 'undefined') return true

  const desktopLocale = window.chrome?.webview?.hostObjects?.uzonMailLocale
  if (!desktopLocale) return true

  try {
    logger.info(`Persisting desktop locale to ${locale}`)
    return await desktopLocale.SetCurrentLocale(locale)
  } catch {
    return false
  }
}
