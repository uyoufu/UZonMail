import { computed, ref } from 'vue'
import { afterAll, beforeAll, beforeEach, describe, expect, it, vi } from 'vitest'
import type { IInbox } from 'src/api/emailBox'
import { useInboxImporter } from 'src/pages/emailManager/inbox/useInboxImporter'

const mocks = vi.hoisted(() => ({
  confirmOperation: vi.fn(),
  createInboxes: vi.fn(),
  notifyError: vi.fn(),
  notifySuccess: vi.fn(),
  notifyUntil: vi.fn(),
  showDialog: vi.fn(),
  splitString: vi.fn()
}))

vi.mock('src/api/emailBox', () => ({
  InboxStatus: { Invalid: 1 },
  createInboxes: mocks.createInboxes
}))
vi.mock('src/utils/dialog', () => ({
  confirmOperation: mocks.confirmOperation,
  notifyError: mocks.notifyError,
  notifySuccess: mocks.notifySuccess,
  notifyUntil: mocks.notifyUntil,
  showDialog: mocks.showDialog
}))
vi.mock('src/utils/stringHelper', () => ({ splitString: mocks.splitString }))
vi.mock('src/i18n/helpers', () => ({
  translateGlobal: (key: string) => `global.${key}`,
  translateInboxManager: (key: string) => `inbox.${key}`
}))

describe('useInboxImporter', () => {
  beforeAll(() => {
    vi.stubGlobal('computed', computed)
  })

  afterAll(() => {
    vi.unstubAllGlobals()
  })

  beforeEach(() => {
    vi.clearAllMocks()
    mocks.notifyUntil.mockImplementation((run) => run(vi.fn()))
  })

  it('imports unique valid addresses as invalid inboxes in the active group', async () => {
    const addNewRow = vi.fn()
    const emailGroup = ref({ id: 8, name: 'inbox', label: 'Inbox', order: 0 })
    const { onImportInvalidInboxes } = useInboxImporter(emailGroup, addNewRow)
    mocks.showDialog.mockResolvedValue({ ok: true, data: { emails: 'ignored by mock' } })
    mocks.splitString.mockReturnValue(['first@example.com', 'first@example.com', 'second@example.com'])
    const importedInboxes: IInbox[] = [
      { id: 1, email: 'first@example.com', emailGroupId: 8, status: 1 },
      { id: 2, email: 'second@example.com', emailGroupId: 8, status: 1 }
    ]
    mocks.createInboxes.mockResolvedValue({ data: importedInboxes })

    await onImportInvalidInboxes()

    expect(mocks.splitString).toHaveBeenCalledWith('ignored by mock')
    expect(mocks.createInboxes).toHaveBeenCalledWith([
      { emailGroupId: 8, email: 'first@example.com', status: 1 },
      { emailGroupId: 8, email: 'second@example.com', status: 1 }
    ])
    expect(addNewRow).toHaveBeenNthCalledWith(1, importedInboxes[0], 'email')
    expect(addNewRow).toHaveBeenNthCalledWith(2, importedInboxes[1], 'email')
    expect(mocks.notifySuccess).toHaveBeenCalledWith('inbox.importInboxSuccess')
  })

  it('does not import when no parsed address is valid', async () => {
    const emailGroup = ref({ id: 8, name: 'inbox', label: 'Inbox', order: 0 })
    const { onImportInvalidInboxes } = useInboxImporter(emailGroup, vi.fn())
    mocks.showDialog.mockResolvedValue({ ok: true, data: { emails: 'invalid' } })
    mocks.splitString.mockReturnValue(['not-an-email'])

    await onImportInvalidInboxes()

    expect(mocks.createInboxes).not.toHaveBeenCalled()
    expect(mocks.notifyError).toHaveBeenCalledWith('inbox.noValidImportData')
  })
})
