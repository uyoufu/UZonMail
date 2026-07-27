import { flushPromises, mount } from '@vue/test-utils'
import { computed, onMounted, ref } from 'vue'
import { afterAll, beforeAll, beforeEach, describe, expect, it, vi } from 'vitest'
import FilesUploaderPopup from 'src/components/uploader/FilesUploaderPopup.vue'

vi.mock('loglevel', () => ({ default: { error: vi.fn() } }))

const mocks = vi.hoisted(() => ({
  fileSha256: vi.fn(),
  notifyError: vi.fn(),
  onDialogCancel: vi.fn(),
  onDialogHide: vi.fn(),
  onDialogOK: vi.fn(),
  uploadFileObject: vi.fn()
}))

vi.mock('quasar', async (importOriginal) => {
  const original = await importOriginal()
  if (!original || typeof original !== 'object') throw new Error('Quasar module mock initialization failed')
  return {
    ...original,
    throttle: <T extends (...args: never[]) => unknown>(callback: T) => callback,
    useDialogPluginComponent: Object.assign(
      () => ({
        dialogRef: ref(null),
        onDialogCancel: mocks.onDialogCancel,
        onDialogHide: mocks.onDialogHide,
        onDialogOK: mocks.onDialogOK
      }),
      { emits: ['ok', 'hide'] }
    )
  }
})

vi.mock('src/api/file', () => ({ uploadFileObject: mocks.uploadFileObject }))
vi.mock('src/utils/file', () => ({ fileSha256: mocks.fileSha256 }))
vi.mock('src/utils/dialog', () => ({ notifyError: mocks.notifyError }))
vi.mock('src/i18n/helpers', () => ({ translateComponents: (key: string) => key }))
vi.mock('src/api/base/httpClient', () => ({
  HttpClientError: class HttpClientError extends Error {}
}))

describe('FilesUploaderPopup', () => {
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
    mocks.fileSha256.mockResolvedValue('sha256')
  })

  it('returns uploaded file ids after every file succeeds', async () => {
    mocks.uploadFileObject
      .mockResolvedValueOnce({ data: { fileUsageId: 11 } })
      .mockResolvedValueOnce({ data: { fileUsageId: 12 } })

    mount(FilesUploaderPopup, {
      props: { files: [new File(['a'], 'a.txt'), new File(['b'], 'b.txt')] },
      global: { stubs: { QDialog: true, QCard: true, LinearProgress: true } }
    })
    await flushPromises()

    expect(mocks.onDialogOK).toHaveBeenCalledWith([11, 12])
    expect(mocks.onDialogCancel).not.toHaveBeenCalled()
  })

  it('closes the dialog when uploading fails', async () => {
    mocks.uploadFileObject.mockRejectedValue(new Error('upload failed'))

    mount(FilesUploaderPopup, {
      props: { files: [new File(['a'], 'a.txt')] },
      global: { stubs: { QDialog: true, QCard: true, LinearProgress: true } }
    })
    await flushPromises()

    expect(mocks.onDialogCancel).toHaveBeenCalledOnce()
    expect(mocks.onDialogOK).not.toHaveBeenCalled()
    expect(mocks.notifyError).toHaveBeenCalledWith('uploadFailed')
  })
})
