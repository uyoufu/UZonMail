import type { IApiApiAccess } from 'src/api/pro/apiAccess'
import { upsertApiAccess, deleteApiAccessData } from 'src/api/pro/apiAccess'
import type { IPopupDialogParams } from 'src/components/popupDialog/types'
import { PopupDialogFieldType } from 'src/components/popupDialog/types'
import { confirmOperation, notifySuccess, showDialog, showHtmlDialog } from 'src/utils/dialog'
import type { addNewRowType, deleteRowByIdType } from 'src/compositions/qTableUtils'
import type { IContextMenuItem } from 'src/components/contextMenu/types'

import dayjs from 'dayjs'
import logger from 'loglevel'

export function useApiAccessContext (addNewRow: addNewRowType, deleteRowById: deleteRowByIdType) {

  async function showApiAccessDialog (apiAccess?: IApiApiAccess) {
    const popupParams: IPopupDialogParams = {
      title: apiAccess ? '修改 API 访问令牌' : '添加 API 访问令牌',
      oneColumn: true,
      fields: [
        {
          name: 'name',
          label: '名称',
          required: true,
          value: apiAccess?.name || '',
        },
        {
          name: 'description',
          label: '描述',
          required: true,
          value: apiAccess?.description || '',
        },
        {
          name: 'expireDate',
          label: '过期时间',
          type: PopupDialogFieldType.datetimeLocal,
          required: true,
          value: apiAccess?.expireDate || '',
          disable: !!apiAccess,
          validate: (value: string) => {
            const valueDate = dayjs(value)
            if (!valueDate.isValid()) {
              return {
                ok: false,
                message: '无效的日期时间格式'
              }
            }

            const nowDate = dayjs()
            if (valueDate.isBefore(nowDate)) {
              return {
                ok: false,
                message: '过期时间必须在当前时间之后'
              }
            }

            return {
              ok: true
            }
          }
        },
        {
          name: 'enable',
          label: '启用',
          type: PopupDialogFieldType.boolean,
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
    notifySuccess('创建成功')
    logger.info('API 访问令牌创建成功', data.token)

    // 进行提示
    const tokenMessage = `访问令牌已创建成功：\n\n${data.token}\n\n请妥善保存此令牌，后续将无法再次查看。`
    await showHtmlDialog("访问令牌", tokenMessage)
  }

  async function onEditApiAccess (apiAccess?: IApiApiAccess) {
    const result = await showApiAccessDialog(apiAccess)
    if (!result.ok) return

    const updateData = Object.assign({}, apiAccess, result.data)
    // 更新数据
    const { data } = await upsertApiAccess(updateData)
    addNewRow(data)
  }

  const contextMenuItems: IContextMenuItem<IApiApiAccess>[] = [
    {
      name: 'modify',
      label: '修改',
      onClick: (apiAccess) => onEditApiAccess(apiAccess),
    },
    {
      name: 'delete',
      label: '删除',
      color: 'negative',
      onClick: onDeleteApiAccess,
    }
  ]

  async function onDeleteApiAccess (apiAccess: IApiApiAccess) {
    const confirm = await confirmOperation('删除确认', `您确定要删除 API 访问令牌 "${apiAccess.name}" 吗？`)
    if (!confirm) return

    // 这里可以添加确认对话框
    await deleteApiAccessData(apiAccess.objectId)
    deleteRowById(apiAccess.id)

    notifySuccess('删除成功')
  }

  return {
    onAddApiAccess,
    contextMenuItems
  }
}
