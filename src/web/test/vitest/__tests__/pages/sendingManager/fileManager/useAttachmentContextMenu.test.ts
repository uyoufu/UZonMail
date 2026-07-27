import { computed, ref } from 'vue'
import { afterAll, beforeAll, beforeEach, describe, expect, it, vi } from 'vitest'
import type { IFileUsage } from 'src/api/file'
import type { IActionContext } from 'src/components/contextMenu/types'
import { useAttachmentContextMenu } from 'src/pages/sendingManager/fileManager/useAttachmentContextMenu'

const mocks = vi.hoisted(() => ({
  confirmOperation: vi.fn(),
  deleteFileUsages: vi.fn(),
  getFileCategories: vi.fn(),
  moveFileUsages: vi.fn(),
  notifySuccess: vi.fn(),
  showDialog: vi.fn()
}))

vi.mock('vue-i18n', () => ({
  useI18n: () => ({
    t: (key: string, params?: Record<string, unknown>) => params ? `${key}:${JSON.stringify(params)}` : key
  })
}))

vi.mock('@vueuse/core', () => ({ useFileSystemAccess: vi.fn() }))
vi.mock('src/api/file', () => ({
  deleteFileUsages: mocks.deleteFileUsages,
  moveFileUsages: mocks.moveFileUsages,
  updateDisplayName: vi.fn()
}))
vi.mock('src/api/fileCategory', () => ({ getFileCategories: mocks.getFileCategories }))
vi.mock('src/api/fileReader', () => ({ getFileReaderId: vi.fn(), getFileStreamByReaderId: vi.fn() }))
vi.mock('src/api/pro/objectReader', () => ({ createObjectPersistentReader: vi.fn() }))
vi.mock('src/config', () => ({ useConfig: () => ({ baseUrl: '', api: '/api/v1' }) }))
vi.mock('src/utils/file', () => ({ saveFileSmart: vi.fn() }))
vi.mock('src/utils/dialog', () => ({
  confirmOperation: mocks.confirmOperation,
  notifySuccess: mocks.notifySuccess,
  showDialog: mocks.showDialog
}))

function createAttachment(id: number): IFileUsage {
  return {
    id,
    categoryId: 1,
    fileName: `file-${id}.txt`,
    createDate: '2026-07-27T00:00:00Z',
    displayName: `file-${id}.txt`,
    fileObjectId: id,
    sha256: String(id),
    referenceCount: 0,
    size: 10
  }
}

function createActionContext(attachments: IFileUsage[]): IActionContext<IFileUsage> {
  return {
    targetValues: attachments,
    clearSelection: vi.fn()
  }
}

describe('useAttachmentContextMenu', () => {
  beforeAll(() => {
    vi.stubGlobal('computed', computed)
    vi.stubGlobal('ref', ref)
  })

  afterAll(() => {
    vi.unstubAllGlobals()
  })

  beforeEach(() => {
    vi.clearAllMocks()
    mocks.getFileCategories.mockResolvedValue({
      data: [{ id: 9, name: 'Archive', isDefault: false }]
    })
  })

  it('moves all selected attachments and clears selection only after success', async () => {
    mocks.showDialog.mockResolvedValue({ ok: true, data: { categoryId: 9 } })
    mocks.moveFileUsages.mockResolvedValue({ data: true })
    const refreshTable = vi.fn()
    const { attachmentContextMenuItems } = useAttachmentContextMenu(refreshTable)
    const attachments = [createAttachment(1), createAttachment(2)]
    const actionContext = createActionContext(attachments)
    const moveAction = attachmentContextMenuItems.value.find((menuItem) => menuItem.name === 'move')!

    await moveAction.onClick(attachments[0]!, actionContext)

    expect(mocks.moveFileUsages).toHaveBeenCalledWith([1, 2], 9)
    expect(refreshTable).toHaveBeenCalledOnce()
    expect(actionContext.clearSelection).toHaveBeenCalledOnce()
  })

  it('keeps selection when moving is cancelled', async () => {
    mocks.showDialog.mockResolvedValue({ ok: false, data: {} })
    const { attachmentContextMenuItems } = useAttachmentContextMenu(vi.fn())
    const attachment = createAttachment(1)
    const actionContext = createActionContext([attachment])
    const moveAction = attachmentContextMenuItems.value.find((menuItem) => menuItem.name === 'move')!

    await moveAction.onClick(attachment, actionContext)

    expect(mocks.moveFileUsages).not.toHaveBeenCalled()
    expect(actionContext.clearSelection).not.toHaveBeenCalled()
  })

  it('deletes all selected attachments and keeps selection when the request fails', async () => {
    mocks.confirmOperation.mockResolvedValue(true)
    mocks.deleteFileUsages.mockRejectedValue(new Error('delete failed'))
    const refreshTable = vi.fn()
    const { attachmentContextMenuItems } = useAttachmentContextMenu(refreshTable)
    const attachments = [createAttachment(1), createAttachment(2)]
    const actionContext = createActionContext(attachments)
    const deleteAction = attachmentContextMenuItems.value.find((menuItem) => menuItem.name === 'delete')!

    await expect(deleteAction.onClick(attachments[0]!, actionContext)).rejects.toThrow('delete failed')

    expect(mocks.deleteFileUsages).toHaveBeenCalledWith([1, 2])
    expect(refreshTable).not.toHaveBeenCalled()
    expect(actionContext.clearSelection).not.toHaveBeenCalled()
  })
})
