import { computed, ref } from 'vue'
import { afterAll, beforeAll, beforeEach, describe, expect, it, vi } from 'vitest'
import type { IInbox } from 'src/api/emailBox'
import type { IActionContext } from 'src/components/contextMenu/types'
import { useContextMenu } from 'src/pages/emailManager/inbox/contextMenu'

const mocks = vi.hoisted(() => ({
  confirmOperation: vi.fn(),
  deleteInboxByIds: vi.fn(),
  notifySuccess: vi.fn()
}))

vi.mock('src/api/emailBox', () => ({
  deleteInboxByIds: mocks.deleteInboxByIds,
  updateInbox: vi.fn()
}))
vi.mock('src/api/pro/emailVerify', () => ({ validateInboxes: vi.fn() }))
vi.mock('src/components/lowCode/PopupDialog', () => ({ showDialog: vi.fn() }))
vi.mock('src/pages/emailManager/inbox/headerFunctions', () => ({ getInboxFields: vi.fn() }))
vi.mock('src/compositions/permission', () => ({ usePermission: () => ({ isProfession: ref(true) }) }))
vi.mock('src/utils/dialog', () => ({
  confirmOperation: mocks.confirmOperation,
  notifySuccess: mocks.notifySuccess,
  notifyUntil: vi.fn()
}))
vi.mock('src/i18n/helpers', () => ({
  translateGlobal: (key: string) => `global.${key}`,
  translateInboxManager: (key: string, params?: Record<string, unknown>) =>
    params ? `inbox.${key}:${JSON.stringify(params)}` : `inbox.${key}`
}))

function createInbox(id: number): IInbox {
  return {
    id,
    objectId: `inbox-${id}`,
    email: `inbox-${id}@example.com`
  }
}

function createActionContext(inboxes: IInbox[]): IActionContext<IInbox> {
  return {
    targetValues: inboxes,
    clearSelection: vi.fn()
  }
}

describe('useContextMenu', () => {
  beforeAll(() => {
    vi.stubGlobal('computed', computed)
    vi.stubGlobal('ref', ref)
  })

  afterAll(() => {
    vi.unstubAllGlobals()
  })

  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('only exposes supported inbox actions', () => {
    const { inboxContextMenuItems } = useContextMenu(vi.fn(), vi.fn())

    expect(inboxContextMenuItems.value.map(menuItem => menuItem.name)).toEqual([
      'edit',
      'validateSelected',
      'delete'
    ])
  })

  it('deletes every selected inbox with one batch request', async () => {
    const deleteRowById = vi.fn()
    const { inboxContextMenuItems } = useContextMenu(deleteRowById, vi.fn())
    const inboxes = [createInbox(1), createInbox(2)]
    const actionContext = createActionContext(inboxes)
    const deleteAction = inboxContextMenuItems.value.find(menuItem => menuItem.name === 'delete')!
    mocks.confirmOperation.mockResolvedValue(true)
    mocks.deleteInboxByIds.mockResolvedValue({ data: true })

    await deleteAction.onClick(inboxes[0]!, actionContext)

    expect(mocks.confirmOperation).toHaveBeenCalledWith(
      'global.deleteConfirmation',
      'inbox.isDeleteSelectedInboxes:{"count":2}'
    )
    expect(mocks.deleteInboxByIds).toHaveBeenCalledWith(['inbox-1', 'inbox-2'])
    expect(deleteRowById).toHaveBeenNthCalledWith(1, 1)
    expect(deleteRowById).toHaveBeenNthCalledWith(2, 2)
    expect(actionContext.clearSelection).toHaveBeenCalledOnce()
    expect(mocks.notifySuccess).toHaveBeenCalledWith('global.deleteSuccess')
  })

  it('uses the inbox email in the single-delete confirmation', async () => {
    const { inboxContextMenuItems } = useContextMenu(vi.fn(), vi.fn())
    const inbox = createInbox(1)
    const actionContext = createActionContext([inbox])
    const deleteAction = inboxContextMenuItems.value.find(menuItem => menuItem.name === 'delete')!
    mocks.confirmOperation.mockResolvedValue(true)
    mocks.deleteInboxByIds.mockResolvedValue({ data: true })

    await deleteAction.onClick(inbox, actionContext)

    expect(mocks.confirmOperation).toHaveBeenCalledWith(
      'global.deleteConfirmation',
      'inbox.isDeleteEmailOf:{"email":"inbox-1@example.com"}'
    )
    expect(mocks.deleteInboxByIds).toHaveBeenCalledWith(['inbox-1'])
  })

  it('does not change local state when deletion is cancelled', async () => {
    const deleteRowById = vi.fn()
    const { inboxContextMenuItems } = useContextMenu(deleteRowById, vi.fn())
    const inbox = createInbox(1)
    const actionContext = createActionContext([inbox])
    const deleteAction = inboxContextMenuItems.value.find(menuItem => menuItem.name === 'delete')!
    mocks.confirmOperation.mockResolvedValue(false)

    await deleteAction.onClick(inbox, actionContext)

    expect(mocks.deleteInboxByIds).not.toHaveBeenCalled()
    expect(deleteRowById).not.toHaveBeenCalled()
    expect(actionContext.clearSelection).not.toHaveBeenCalled()
  })

  it('keeps local rows and selection when the batch request fails', async () => {
    const deleteRowById = vi.fn()
    const { inboxContextMenuItems } = useContextMenu(deleteRowById, vi.fn())
    const inboxes = [createInbox(1), createInbox(2)]
    const actionContext = createActionContext(inboxes)
    const deleteAction = inboxContextMenuItems.value.find(menuItem => menuItem.name === 'delete')!
    mocks.confirmOperation.mockResolvedValue(true)
    mocks.deleteInboxByIds.mockRejectedValue(new Error('delete failed'))

    await expect(deleteAction.onClick(inboxes[0]!, actionContext)).rejects.toThrow('delete failed')

    expect(deleteRowById).not.toHaveBeenCalled()
    expect(actionContext.clearSelection).not.toHaveBeenCalled()
    expect(mocks.notifySuccess).not.toHaveBeenCalled()
  })
})
