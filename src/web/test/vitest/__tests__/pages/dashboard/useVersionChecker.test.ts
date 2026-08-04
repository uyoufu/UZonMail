import { afterAll, beforeAll, beforeEach, describe, expect, it, vi } from 'vitest'

const mocks = vi.hoisted(() => ({
  getServerVersion: vi.fn(),
  getUserSetting: vi.fn(),
  isDesktopClient: vi.fn(),
  isSuperAdmin: { value: true },
  notifyCreate: vi.fn(),
  notifyError: vi.fn(),
  notificationUpdate: vi.fn(),
  showComponentDialog: vi.fn(),
  startDesktopUpdate: vi.fn(),
  updateUserSettingString: vi.fn()
}))

vi.mock('quasar', async (importOriginal) => {
  const original = await importOriginal()
  if (!original || typeof original !== 'object') throw new Error('Quasar module mock initialization failed')
  return {
    ...original,
    Notify: {
      create: mocks.notifyCreate
    }
  }
})

vi.mock('src/api/system', () => ({ getServerVersion: mocks.getServerVersion }))
vi.mock('src/api/userSetting', () => ({
  getUserSetting: mocks.getUserSetting,
  updateUserSettingString: mocks.updateUserSettingString
}))
vi.mock('src/compositions/permission', () => ({
  usePermission: () => ({ isSuperAdmin: mocks.isSuperAdmin })
}))
vi.mock('src/i18n/helpers', () => ({
  getCurrentLocale: () => ({ value: 'zh-CN' }),
  translateDashboardPage: (key: string, parameters?: { version?: string }) => {
    return parameters?.version ? `${key}:${parameters.version}` : key
  }
}))
vi.mock('src/utils/dialog', () => ({
  notifyError: mocks.notifyError,
  showComponentDialog: mocks.showComponentDialog
}))
vi.mock('src/pages/dashboard/desktopUpdate', () => ({
  isDesktopClient: mocks.isDesktopClient,
  startDesktopUpdate: mocks.startDesktopUpdate
}))
vi.mock('src/pages/dashboard/VersionHistoryDialog.vue', () => ({ default: {} }))
vi.mock('loglevel', () => ({ default: { debug: vi.fn(), warn: vi.fn() } }))

let mountedCallback: (() => void | Promise<void>) | undefined

describe('useVersionChecker', () => {
  beforeAll(() => {
    vi.stubGlobal('onMounted', (callback: () => void | Promise<void>) => {
      mountedCallback = callback
    })
    vi.stubGlobal('ref', <T>(value: T) => ({ value }))
  })

  afterAll(() => {
    vi.unstubAllGlobals()
  })

  beforeEach(() => {
    vi.resetModules()
    vi.clearAllMocks()
    mountedCallback = undefined
    mocks.isSuperAdmin.value = true
    mocks.isDesktopClient.mockReturnValue(true)
    mocks.getServerVersion.mockResolvedValue({ data: '1.0.0' })
    mocks.getUserSetting.mockResolvedValue({ data: { stringValue: '' } })
    mocks.updateUserSettingString.mockResolvedValue({ data: true })
    mocks.startDesktopUpdate.mockResolvedValue(true)
    mocks.notifyCreate.mockReturnValue(mocks.notificationUpdate)
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({
      ok: true,
      json: vi.fn().mockResolvedValue({ version: '1.1.0' })
    }))
  })

  async function runVersionChecker (): Promise<void> {
    const { useVersionChecker } = await import('src/pages/dashboard/useVersionChecker')
    useVersionChecker()
    await mountedCallback?.()
  }

  it('checks for updates outside the desktop client without exposing an update action', async () => {
    mocks.isDesktopClient.mockReturnValue(false)

    await runVersionChecker()

    expect(fetch).toHaveBeenCalledOnce()
    expect(mocks.getServerVersion).toHaveBeenCalledOnce()
    expect(mocks.notifyCreate).toHaveBeenCalledOnce()
    const notificationOptions = mocks.notifyCreate.mock.calls[0]?.[0] as {
      actions: Array<{ label: string }>
    }
    expect(notificationOptions.actions.map((action) => action.label)).toEqual(['newVersionView', 'newVersionIgnore'])
  })

  it('creates an HTML notification with the default timeout for an available update', async () => {
    await runVersionChecker()

    expect(mocks.notifyCreate).toHaveBeenCalledOnce()
    const notificationOptions = mocks.notifyCreate.mock.calls[0]?.[0] as {
      html: boolean
      message: string
      timeout?: number
      actions: Array<{ label: string }>
    }
    expect(notificationOptions.html).toBe(true)
    expect(notificationOptions.message).toContain('newVersionCurrent:1.0.0')
    expect(notificationOptions.message).toContain('newVersionLatest:1.1.0')
    expect(notificationOptions.timeout).toBeUndefined()
    expect(notificationOptions.actions.map(action => action.label)).toEqual([
      'newVersionView',
      'newVersionIgnore',
      'newVersionUpdate'
    ])
  })

  it('does not notify when the latest version has already been ignored', async () => {
    mocks.getUserSetting.mockResolvedValue({ data: { stringValue: '1.1.0' } })

    await runVersionChecker()

    expect(mocks.notifyCreate).not.toHaveBeenCalled()
  })

  it('persists the ignored latest version and dismisses the notification', async () => {
    await runVersionChecker()
    const notificationOptions = mocks.notifyCreate.mock.calls[0]?.[0] as {
      actions: Array<{ label: string, handler: () => void }>
    }
    const ignoreAction = notificationOptions.actions.find(action => action.label === 'newVersionIgnore')

    ignoreAction?.handler()

    await vi.waitFor(() => {
      expect(mocks.updateUserSettingString).toHaveBeenCalledWith('ignoredDesktopUpdateVersion', '1.1.0')
    })

    expect(mocks.notificationUpdate).toHaveBeenCalledWith({ timeout: 1 })
  })

  it('starts the desktop updater from the desktop notification action', async () => {
    await runVersionChecker()
    const notificationOptions = mocks.notifyCreate.mock.calls[0]?.[0] as {
      actions: Array<{ label: string, handler: () => void }>
    }
    const updateAction = notificationOptions.actions.find((action) => action.label === 'newVersionUpdate')

    updateAction?.handler()

    await vi.waitFor(() => {
      expect(mocks.startDesktopUpdate).toHaveBeenCalledOnce()
    })
  })

  it.each([true, false])('passes the actual desktop capability to version history: %s', async (isDesktopClient) => {
    mocks.isDesktopClient.mockReturnValue(isDesktopClient)

    await runVersionChecker()
    const notificationOptions = mocks.notifyCreate.mock.calls[0]?.[0] as {
      actions: Array<{ label: string, handler: () => void }>
    }
    const viewAction = notificationOptions.actions.find((action) => action.label === 'newVersionView')

    viewAction?.handler()

    await vi.waitFor(() => {
      expect(mocks.showComponentDialog).toHaveBeenCalledWith(expect.anything(), {
        locale: 'zh-CN',
        isDesktopClient
      })
    })
  })
})
