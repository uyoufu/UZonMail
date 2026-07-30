import { afterEach, describe, expect, it, vi } from 'vitest'
import { getDesktopLocale, persistDesktopLocale, resolveSupportedLocale } from 'src/i18n/desktopLocale'

type WebViewHostObjects = NonNullable<NonNullable<NonNullable<Window['chrome']>['webview']>['hostObjects']>

function setDesktopHostObject(hostObjects: WebViewHostObjects) {
  Object.defineProperty(window, 'chrome', {
    configurable: true,
    value: { webview: { hostObjects } }
  })
}

describe('desktop locale bridge', () => {
  afterEach(() => {
    Reflect.deleteProperty(window, 'chrome')
  })

  it('reads a supported locale from the synchronous desktop host object', () => {
    setDesktopHostObject({
      sync: {
        uzonMailLocale: {
          GetCurrentLocale: () => 'en-US'
        }
      }
    })

    expect(getDesktopLocale()).toBe('en-US')
  })

  it('ignores unsupported locale values from the desktop host object', () => {
    setDesktopHostObject({
      sync: {
        uzonMailLocale: {
          GetCurrentLocale: () => 'de-DE'
        }
      }
    })

    expect(getDesktopLocale()).toBeUndefined()
    expect(resolveSupportedLocale('zh-CN')).toBe('zh-CN')
    expect(resolveSupportedLocale('de-DE')).toBeUndefined()
  })

  it('keeps browser behavior when the desktop host object is unavailable', async () => {
    expect(await persistDesktopLocale('zh-CN')).toBe(true)
  })

  it('persists a language choice through the asynchronous desktop host object', async () => {
    const setCurrentLocale = vi.fn().mockResolvedValue(true)
    setDesktopHostObject({
      uzonMailLocale: { SetCurrentLocale: setCurrentLocale }
    })

    await expect(persistDesktopLocale('en-US')).resolves.toBe(true)
    expect(setCurrentLocale).toHaveBeenCalledWith('en-US')
  })

  it('returns false when the desktop host object rejects the update', async () => {
    setDesktopHostObject({
      uzonMailLocale: {
        SetCurrentLocale: () => Promise.reject(new Error('write failed'))
      }
    })

    await expect(persistDesktopLocale('en-US')).resolves.toBe(false)
  })
})
