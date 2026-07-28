/* eslint-disable @typescript-eslint/no-explicit-any */
import type { ITikTokDevice } from 'src/api/pro/tikTokDevice';
import { deleteTikTokDevice, updateTikTokDevice } from 'src/api/pro/tikTokDevice'
import { ContextMenuIcon, type IContextMenuItem } from 'src/components/contextMenu/types'
import type { addNewRowType, deleteRowByIdType } from 'src/compositions/qTableUtils'
import { getTikTokDeviceInfoFields } from './useHeaderFunctions'
import { confirmOperation, notifySuccess, showDialog } from 'src/utils/dialog'
import type { IPopupDialogParams } from 'src/components/lowCode/types'
import { t } from 'src/i18n/helpers'

export function useContextMenu (
  addNewRow: addNewRowType<ITikTokDevice>,
  deleteRowById: deleteRowByIdType<ITikTokDevice>
) {
  const contextMenuItems = computed<IContextMenuItem<ITikTokDevice>[]>(() => [
    {
      name: 'edit',
      label: t('pages.variableManager.edit'),
      tooltip: t('pages.tikTokDevice.editCurrent'),
      icon: ContextMenuIcon.edit,
      onClick: onUpdateCrawler
    },
    {
      name: 'delete',
      label: t('pages.variableManager.delete'),
      tooltip: t('pages.tikTokDevice.deleteCurrent'),
      color: 'negative',
      icon: ContextMenuIcon.delete,
      onClick: onDeleteCrawler
    }
  ])

  async function onUpdateCrawler (tikTokDeviceInfo: Record<string, any>) {
    const fields = getTikTokDeviceInfoFields()

    // 添加默认值
    fields.forEach(field => {
      if (tikTokDeviceInfo[field.name] !== undefined) {
        field.value = tikTokDeviceInfo[field.name]
      }
    })

    // 打开弹窗
    const popupParams: IPopupDialogParams = {
      title: t('pages.tikTokDevice.editTitle'),
      fields,
      oneColumn: true
    }

    // 弹出对话框

    const { ok, data } = await showDialog<ITikTokDevice>(popupParams)
    if (!ok) return
    await updateTikTokDevice(tikTokDeviceInfo.id, data)
    // 保存到 rows 中
    addNewRow(Object.assign(tikTokDeviceInfo, data))

    notifySuccess(t('pages.tikTokDevice.updateSuccess'))
  }

  async function onDeleteCrawler (tikTokDeviceInfo: ITikTokDevice) {
    const confirm = await confirmOperation(
      t('pages.templateManager.deleteConfirmation'),
      t('pages.tikTokDevice.deleteConfirm', { name: tikTokDeviceInfo.name })
    )
    if (!confirm) return

    // 开始删除
    await deleteTikTokDevice(tikTokDeviceInfo.id as number)

    // 删除本机数据
    deleteRowById(tikTokDeviceInfo.id)

    notifySuccess(t('pages.tikTokDevice.deleteSuccess'))
  }

  return { contextMenuItems }
}
