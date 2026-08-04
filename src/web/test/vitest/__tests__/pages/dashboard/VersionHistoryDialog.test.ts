import { flushPromises, mount } from '@vue/test-utils'
import { computed, onMounted, ref } from 'vue'
import { afterAll, beforeAll, beforeEach, describe, expect, it, vi } from 'vitest'
import VersionHistoryDialog from 'src/pages/dashboard/VersionHistoryDialog.vue'

const mocks = vi.hoisted(() => ({
  getVersionHistory: vi.fn(),
  notifyError: vi.fn(),
  onDialogCancel: vi.fn(),
  onDialogHide: vi.fn(),
  saveFileSmart: vi.fn(),
  startDesktopUpdate: vi.fn()
}))

vi.mock('quasar', async (importOriginal) => {
  const original = await importOriginal()
  if (!original || typeof original !== 'object') throw new Error('Quasar module mock initialization failed')
  return {
    ...original,
    useDialogPluginComponent: Object.assign(
      () => ({
        dialogRef: ref(null),
        onDialogCancel: mocks.onDialogCancel,
        onDialogHide: mocks.onDialogHide
      }),
      { emits: ['hide'] }
    )
  }
})

vi.mock('src/pages/dashboard/versionHistory', () => ({
  getVersionHistory: mocks.getVersionHistory,
  VersionHistoryFragmentType: {
    Text: 'text',
    Link: 'link'
  }
}))
vi.mock('src/pages/dashboard/desktopUpdate', () => ({
  startDesktopUpdate: mocks.startDesktopUpdate
}))
vi.mock('src/utils/dialog', () => ({ notifyError: mocks.notifyError }))
vi.mock('src/utils/file', () => ({ saveFileSmart: mocks.saveFileSmart }))
vi.mock('src/i18n/helpers', () => ({ translateDashboardPage: (key: string) => key }))

const dialogStubs = {
  QCard: { template: '<div><slot /></div>' },
  QCardSection: { template: '<section><slot /></section>' },
  QDialog: { template: '<div><slot /></div>' },
  QItem: { template: '<div><slot /></div>' },
  QItemSection: { template: '<div><slot /></div>' },
  QList: { template: '<div><slot /></div>' },
  QSeparator: true,
  QSpace: true,
  QSpinnerDots: true,
  CommonBtn: {
    props: {
      disable: Boolean,
      label: String
    },
    emits: ['click'],
    template: '<button :disabled="disable" @click="$emit(\'click\', $event)">{{ label }}</button>'
  }
}

describe('VersionHistoryDialog', () => {
  beforeAll(() => {
    vi.stubGlobal('computed', computed)
    vi.stubGlobal('onMounted', onMounted)
    vi.stubGlobal('ref', ref)
  })

  afterAll(() => {
    vi.unstubAllGlobals()
  })

  beforeEach(() => {
    vi.clearAllMocks()
    mocks.getVersionHistory.mockResolvedValue([
      {
        version: '1.2.0',
        publishedAt: '2026-08-04',
        sections: [
          {
            entries: [
              {
                fragments: [
                  { type: 'text', text: 'Latest release ' },
                  {
                    type: 'link',
                    text: 'uzonmail-desktop.zip',
                    url: 'https://oss.uzoncloud.com/uzonmail-desktop.zip',
                    fileName: 'uzonmail-desktop.zip'
                  },
                  { type: 'link', text: 'docker', url: 'https://hub.docker.com/r/gmxgalens/uzon-mail/tags' }
                ]
              }
            ]
          }
        ]
      },
      { version: '1.1.0', sections: [{ entries: [{ fragments: [{ type: 'text', text: 'Previous release' }] }] }] }
    ])
    mocks.startDesktopUpdate.mockResolvedValue(true)
  })

  function mountDialog (isDesktopClient: boolean) {
    return mount(VersionHistoryDialog, {
      props: { locale: 'en-US', isDesktopClient },
      global: { stubs: dialogStubs }
    })
  }

  it('shows an update button only beside the latest desktop release', async () => {
    const wrapper = mountDialog(true)
    await flushPromises()

    expect(wrapper.findAll('button').filter(button => button.text() === 'newVersionUpdate')).toHaveLength(1)
  })

  it('does not expose the update button outside the desktop client', async () => {
    const wrapper = mountDialog(false)
    await flushPromises()

    expect(wrapper.findAll('button').filter(button => button.text() === 'newVersionUpdate')).toHaveLength(0)
  })

  it('starts the desktop updater from the latest release button', async () => {
    const wrapper = mountDialog(true)
    await flushPromises()
    const updateButton = wrapper.findAll('button').find(button => button.text() === 'newVersionUpdate')

    await updateButton?.trigger('click')

    expect(mocks.startDesktopUpdate).toHaveBeenCalledOnce()
    expect(mocks.notifyError).not.toHaveBeenCalled()
  })

  it('shows the release date and saves direct download links', async () => {
    const wrapper = mountDialog(false)
    await flushPromises()

    expect(wrapper.text()).toContain('2026-08-04')
    await wrapper.get('a[href="https://oss.uzoncloud.com/uzonmail-desktop.zip"]').trigger('click')

    expect(mocks.saveFileSmart).toHaveBeenCalledWith(
      'uzonmail-desktop.zip',
      'https://oss.uzoncloud.com/uzonmail-desktop.zip'
    )
  })

  it('opens non-file links in a separate window', async () => {
    const wrapper = mountDialog(false)
    await flushPromises()
    const dockerLink = wrapper.get('a[href="https://hub.docker.com/r/gmxgalens/uzon-mail/tags"]')

    expect(dockerLink.attributes('target')).toBe('_blank')
    expect(dockerLink.attributes('rel')).toBe('noopener noreferrer')
  })
})
