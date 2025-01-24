/* eslint-disable @typescript-eslint/no-explicit-any */
import { deleteTikTokDevice, ITikTokDevice, updateTikTokDevice } from 'src/api/pro/tikTokDevice'
import { IContextMenuItem } from 'src/components/contextMenu/types'
import { addNewRowType, deleteRowByIdType } from 'src/compositions/qTableUtils'
import { getTikTokDeviceInfoFields } from './useHeaderFunctions'
import { confirmOperation, notifySuccess, showDialog } from 'src/utils/dialog'
import { IPopupDialogParams } from 'src/components/popupDialog/types'

export function useContextMenu (addNewRow: addNewRowType<ITikTokDevice>, deleteRowById: deleteRowByIdType) {
  const contextMenuItems: IContextMenuItem<ITikTokDevice>[] = [
    {
      name: 'edit',
      label: '编辑',
      tooltip: '编辑当前TikTok设备',
      onClick: onUpdateCrawler
    },
    {
      name: 'delete',
      label: '删除',
      tooltip: '删除当前TikTok设备',
      color: 'negative',
      onClick: onDeleteCrawler
    }
  ]

  async function onUpdateCrawler (tikTokDeviceInfo: Record<string, any>) {
    const fields = await getTikTokDeviceInfoFields()

    // 添加默认值
    fields.forEach(field => {
      if (tikTokDeviceInfo[field.name] !== undefined) {
        field.value = tikTokDeviceInfo[field.name]
      }
    })

    // 打开弹窗
    const popupParams: IPopupDialogParams = {
      title: '修改TikTok设备',
      fields,
      oneColumn: true
    }

    // 弹出对话框
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    const { ok, data } = await showDialog<ITikTokDevice>(popupParams)
    if (!ok) return
    await updateTikTokDevice(tikTokDeviceInfo.id, data)
    // 保存到 rows 中
    addNewRow(Object.assign(tikTokDeviceInfo, data))

    notifySuccess('修改TikTok设备成功')
  }

  async function onDeleteCrawler (tikTokDeviceInfo: ITikTokDevice) {
    const confirm = await confirmOperation('删除确认', `是否TikTok设备: [${tikTokDeviceInfo.name}]？`)
    if (!confirm) return

    // 开始删除
    await deleteTikTokDevice(tikTokDeviceInfo.id as number)

    // 删除本机数据
    deleteRowById(tikTokDeviceInfo.id)

    notifySuccess('删除爬虫任务成功')
  }

  return { contextMenuItems }
}
