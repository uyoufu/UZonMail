import type { IApiApiAccess } from 'src/api/pro/apiAccess'
import { upsertApiAccess, deleteApiAccessData } from 'src/api/pro/apiAccess'
import type { IPopupDialogParams } from 'src/components/lowCode/types'
import { LowCodeFieldType } from 'src/components/lowCode/types'
import { confirmOperation, notifySuccess, showDialog, showHtmlDialog } from 'src/utils/dialog'
import type { addNewRowType, deleteRowByIdType } from 'src/compositions/qTableUtils'
import type { IContextMenuItem } from 'src/components/contextMenu/types'

import dayjs from 'dayjs'
import logger from 'loglevel'
import { t } from 'src/i18n/helpers'

export function useApiAccessContext (
  addNewRow: addNewRowType<IApiApiAccess>,
  deleteRowById: deleteRowByIdType<IApiApiAccess>
) {

  async function showApiAccessDialog (apiAccess?: IApiApiAccess) {
    const popupParams: IPopupDialogParams = {
      title: apiAccess ? t('pages.apiAccess.editTitle') : t('pages.apiAccess.createTitle'),
      oneColumn: true,
      fields: [
        {
          name: 'name',
          label: t('global.name'),
          required: true,
          value: apiAccess?.name || '',
        },
        {
          name: 'description',
          label: t('global.description'),
          required: true,
          value: apiAccess?.description || '',
        },
        {
          name: 'expireDate',
          label: t('pages.apiAccess.expiry'),
          type: LowCodeFieldType.datetimeLocal,
          required: true,
          value: apiAccess?.expireDate || '',
          disable: !!apiAccess,
          validate: (value: string) => {
            const valueDate = dayjs(value)
            if (!valueDate.isValid()) {
              return {
                ok: false,
                message: t('pages.apiAccess.invalidDateTime')
              }
            }

            const nowDate = dayjs()
            if (valueDate.isBefore(nowDate)) {
              return {
                ok: false,
                message: t('pages.apiAccess.futureExpiryRequired')
              }
            }

            return {
              ok: true
            }
          }
        },
        {
          name: 'enable',
          label: t('pages.apiAccess.enabled'),
          type: LowCodeFieldType.boolean,
          value: apiAccess?.enable || true,
        }
      ]
    }

    const result = await showDialog<IApiApiAccess>(popupParams)
    return result
  }

  async function onAddApiAccess () {
    const result = await showApiAccessDialog()
    if (!result.ok) return

    // 更新数据
    const { data } = await upsertApiAccess(result.data)
    addNewRow(data)

    // 提示复制 token 结果
    notifySuccess(t('pages.apiAccess.created'))
    logger.info('API 访问令牌创建成功', data.token)

    // 进行提示
    const tokenMessage = `${t('pages.apiAccess.tokenCreatedPrefix')}\n\n${data.token}\n\n${t('pages.apiAccess.tokenCreatedSuffix')}`
    await showHtmlDialog(t('pages.apiAccess.tokenTitle'), tokenMessage)
  }

  async function onEditApiAccess (apiAccess?: IApiApiAccess) {
    const result = await showApiAccessDialog(apiAccess)
    if (!result.ok) return

    const updateData = Object.assign({}, apiAccess, result.data)
    // 更新数据
    const { data } = await upsertApiAccess(updateData)
    addNewRow(data)
  }

  const contextMenuItems = computed<IContextMenuItem<IApiApiAccess>[]>(() => [
    {
      name: 'modify',
      label: t('pages.apiAccess.modify'),
      onClick: (apiAccess) => onEditApiAccess(apiAccess),
    },
    {
      name: 'delete',
      label: t('pages.variableManager.delete'),
      color: 'negative',
      onClick: onDeleteApiAccess,
    }
  ])

  async function onDeleteApiAccess (apiAccess: IApiApiAccess) {
    const confirm = await confirmOperation(
      t('pages.templateManager.deleteConfirmation'),
      t('pages.apiAccess.deleteConfirm', { name: apiAccess.name })
    )
    if (!confirm) return

    // 这里可以添加确认对话框
    await deleteApiAccessData(apiAccess.objectId)
    deleteRowById(apiAccess.id)

    notifySuccess(t('pages.variableManager.deleteSuccess'))
  }

  return {
    onAddApiAccess,
    contextMenuItems
  }
}
