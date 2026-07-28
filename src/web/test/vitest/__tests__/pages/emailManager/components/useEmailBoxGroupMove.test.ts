import { beforeEach, describe, expect, it, vi } from 'vitest'
import type { IActionContext } from 'src/components/contextMenu/types'
import { useEmailBoxGroupMove } from 'src/pages/emailManager/components/useEmailBoxGroupMove'

const mocks = vi.hoisted(() => ({
  getEmailGroups: vi.fn(),
  moveInboxesToGroup: vi.fn(),
  moveOutboxesToGroup: vi.fn(),
  notifySuccess: vi.fn(),
  notifyUntil: vi.fn(async (operation: () => Promise<unknown>) => await operation()),
  notifyWarning: vi.fn(),
  showDialog: vi.fn()
}))

vi.mock('src/api/emailBox', () => ({
  moveInboxesToGroup: mocks.moveInboxesToGroup,
  moveOutboxesToGroup: mocks.moveOutboxesToGroup
}))
vi.mock('src/api/emailGroup', () => ({
  EmailGroupType: { Outbox: 1, Inbox: 2 },
  getEmailGroups: mocks.getEmailGroups
}))
vi.mock('src/components/lowCode/types', () => ({ LowCodeFieldType: { selectOne: 'selectOne' } }))
vi.mock('src/i18n/helpers', () => ({
  translateEmailGroup: (key: string, params?: Record<string, unknown>) =>
    params ? `emailGroup.${key}:${JSON.stringify(params)}` : `emailGroup.${key}`
}))
vi.mock('src/utils/dialog', () => ({
  notifySuccess: mocks.notifySuccess,
  notifyUntil: mocks.notifyUntil,
  notifyWarning: mocks.notifyWarning,
  showDialog: mocks.showDialog
}))

interface ITestEmailBox {
  id: number
  emailGroupId: number
}

function createActionContext(emailBoxes: ITestEmailBox[]): IActionContext<ITestEmailBox> {
  return {
    targetValues: emailBoxes,
    clearSelection: vi.fn()
  }
}

describe('useEmailBoxGroupMove', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('moves all selected inboxes to the selected non-source group', async () => {
    const refreshTable = vi.fn()
    const actionContext = createActionContext([
      { id: 1, emailGroupId: 10 },
      { id: 2, emailGroupId: 10 }
    ])
    mocks.getEmailGroups.mockResolvedValue({
      data: [
        { id: 10, name: 'Source', order: 1 },
        { id: 20, name: 'Target', order: 2 }
      ]
    })
    mocks.showDialog.mockResolvedValue({ ok: true, data: { targetGroupId: 20 } })
    mocks.moveInboxesToGroup.mockResolvedValue({ data: true })

    const { onMoveEmailBoxes } = useEmailBoxGroupMove<ITestEmailBox>(2, refreshTable)
    await onMoveEmailBoxes(actionContext.targetValues[0]!, actionContext)

    expect(mocks.getEmailGroups).toHaveBeenCalledWith(2)
    expect(mocks.showDialog).toHaveBeenCalledWith(expect.objectContaining({
      fields: [expect.objectContaining({
        options: [{ label: 'Target', value: 20 }]
      })]
    }))
    expect(mocks.moveInboxesToGroup).toHaveBeenCalledWith({ emailBoxIds: [1, 2], targetGroupId: 20 })
    expect(refreshTable).toHaveBeenCalledOnce()
    expect(actionContext.clearSelection).toHaveBeenCalledOnce()
    expect(mocks.notifySuccess).toHaveBeenCalledWith('emailGroup.moveEmailBoxesSuccess:{"count":2}')
  })

  it('uses the outbox endpoint for outbox groups', async () => {
    const actionContext = createActionContext([{ id: 1, emailGroupId: 10 }])
    mocks.getEmailGroups.mockResolvedValue({ data: [{ id: 20, name: 'Target', order: 1 }] })
    mocks.showDialog.mockResolvedValue({ ok: true, data: { targetGroupId: 20 } })
    mocks.moveOutboxesToGroup.mockResolvedValue({ data: true })

    const { onMoveEmailBoxes } = useEmailBoxGroupMove<ITestEmailBox>(1, vi.fn())
    await onMoveEmailBoxes(actionContext.targetValues[0]!, actionContext)

    expect(mocks.moveOutboxesToGroup).toHaveBeenCalledWith({ emailBoxIds: [1], targetGroupId: 20 })
  })

  it('keeps the selection when the dialog is cancelled', async () => {
    const actionContext = createActionContext([{ id: 1, emailGroupId: 10 }])
    mocks.getEmailGroups.mockResolvedValue({ data: [{ id: 20, name: 'Target', order: 1 }] })
    mocks.showDialog.mockResolvedValue({ ok: false, data: {} })

    const { onMoveEmailBoxes } = useEmailBoxGroupMove<ITestEmailBox>(2, vi.fn())
    await onMoveEmailBoxes(actionContext.targetValues[0]!, actionContext)

    expect(mocks.moveInboxesToGroup).not.toHaveBeenCalled()
    expect(actionContext.clearSelection).not.toHaveBeenCalled()
  })

  it('does not open the dialog when there is no other target group', async () => {
    const actionContext = createActionContext([{ id: 1, emailGroupId: 10 }])
    mocks.getEmailGroups.mockResolvedValue({ data: [{ id: 10, name: 'Source', order: 1 }] })

    const { onMoveEmailBoxes } = useEmailBoxGroupMove<ITestEmailBox>(2, vi.fn())
    await onMoveEmailBoxes(actionContext.targetValues[0]!, actionContext)

    expect(mocks.showDialog).not.toHaveBeenCalled()
    expect(mocks.notifyWarning).toHaveBeenCalledWith('emailGroup.noAvailableTargetGroup')
    expect(actionContext.clearSelection).not.toHaveBeenCalled()
  })

  it('keeps the table and selection when the move request fails', async () => {
    const refreshTable = vi.fn()
    const actionContext = createActionContext([{ id: 1, emailGroupId: 10 }])
    mocks.getEmailGroups.mockResolvedValue({ data: [{ id: 20, name: 'Target', order: 1 }] })
    mocks.showDialog.mockResolvedValue({ ok: true, data: { targetGroupId: 20 } })
    mocks.moveInboxesToGroup.mockRejectedValue(new Error('move failed'))

    const { onMoveEmailBoxes } = useEmailBoxGroupMove<ITestEmailBox>(2, refreshTable)

    await expect(onMoveEmailBoxes(actionContext.targetValues[0]!, actionContext)).rejects.toThrow('move failed')

    expect(refreshTable).not.toHaveBeenCalled()
    expect(actionContext.clearSelection).not.toHaveBeenCalled()
    expect(mocks.notifySuccess).not.toHaveBeenCalled()
  })
})
