import {
  moveInboxesToGroup,
  moveOutboxesToGroup,
  type IMoveEmailBoxesRequest
} from 'src/api/emailBox'
import { EmailGroupType, getEmailGroups, type IEmailGroup } from 'src/api/emailGroup'
import type { IActionContext } from 'src/components/contextMenu/types'
import { LowCodeFieldType } from 'src/components/lowCode/types'
import type { refreshTableType } from 'src/compositions/qTableUtils'
import { translateEmailGroup } from 'src/i18n/helpers'
import { notifySuccess, notifyUntil, notifyWarning, showDialog } from 'src/utils/dialog'

interface IEmailBoxGroupMoveCandidate {
  id?: number
  emailGroupId?: number
}

/** 创建将指定类型邮箱批量移动到其他分组的操作 */
export function useEmailBoxGroupMove<TEmailBox extends IEmailBoxGroupMoveCandidate> (
  emailGroupType: EmailGroupType,
  refreshTable: refreshTableType
) {
  async function onMoveEmailBoxes (
    _cursorEmailBox: TEmailBox,
    { targetValues, clearSelection }: IActionContext<TEmailBox>
  ) {
    const emailBoxIds = targetValues.map(emailBox => emailBox.id).filter(isEmailBoxId)
    if (emailBoxIds.length !== targetValues.length) return false

    const sourceGroupIds = new Set(
      targetValues.map(emailBox => emailBox.emailGroupId).filter(isEmailBoxId)
    )
    const { data: groups } = await getEmailGroups(emailGroupType)
    const targetGroups = groups.filter(group => isTargetGroup(group, sourceGroupIds))
    if (targetGroups.length === 0) {
      notifyWarning(translateEmailGroup('noAvailableTargetGroup'))
      return false
    }

    const { ok, data } = await showDialog<{ targetGroupId: number }>({
      title: translateEmailGroup('moveEmailBoxes'),
      fields: [
        {
          name: 'targetGroupId',
          label: translateEmailGroup('selectTargetGroup'),
          type: LowCodeFieldType.selectOne,
          options: targetGroups.map(group => ({ label: group.name, value: group.id })),
          mapOptions: true,
          emitValue: true,
          required: true
        }
      ],
      oneColumn: true
    })
    if (!ok) return

    const targetGroupId = Number(data.targetGroupId)
    if (!Number.isInteger(targetGroupId) || targetGroupId <= 0) return false

    const request: IMoveEmailBoxesRequest = { emailBoxIds, targetGroupId }
    await notifyUntil(
      () => moveEmailBoxesToTargetGroup(emailGroupType, request),
      translateEmailGroup('movingEmailBoxes'),
      translateEmailGroup('moveEmailBoxes')
    )
    refreshTable()
    clearSelection()
    notifySuccess(translateEmailGroup('moveEmailBoxesSuccess', { count: emailBoxIds.length }))
  }

  return { onMoveEmailBoxes }
}

function isEmailBoxId (emailBoxId: number | undefined): emailBoxId is number {
  return emailBoxId !== undefined && emailBoxId > 0
}

function isTargetGroup (group: IEmailGroup, sourceGroupIds: Set<number>): group is IEmailGroup & { id: number } {
  return group.id !== undefined && !sourceGroupIds.has(group.id)
}

function moveEmailBoxesToTargetGroup (emailGroupType: EmailGroupType, request: IMoveEmailBoxesRequest) {
  if (emailGroupType === EmailGroupType.Inbox) return moveInboxesToGroup(request)
  return moveOutboxesToGroup(request)
}
