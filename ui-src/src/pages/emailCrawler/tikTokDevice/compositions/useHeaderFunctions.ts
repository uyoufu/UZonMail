
import type { IPopupDialogField, IPopupDialogParams } from 'src/components/popupDialog/types';
import { PopupDialogFieldType } from 'src/components/popupDialog/types'

import { notifySuccess, showDialog } from 'src/utils/dialog'
import type { addNewRowType } from 'src/compositions/qTableUtils'
import type { ITikTokDevice } from 'src/api/pro/tikTokDevice';
import { createTikTokDevice } from 'src/api/pro/tikTokDevice'

export function getTikTokDeviceInfoFields (): IPopupDialogField[] {
  return [
    {
      name: 'name',
      label: '名称',
      required: true
    },
    {
      name: 'description',
      label: '描述'
    },
    {
      name: 'deviceId',
      type: PopupDialogFieldType.text,
      label: '设备ID (device_id)'
    },
    {
      name: 'odinId',
      type: PopupDialogFieldType.text,
      label: 'TikTok广告ID (odinId)'
    }
  ]
}

/**
 * 使用头部功能
 * @returns
 */
export function useHeaderFunctions (addNewRow: addNewRowType<ITikTokDevice>) {
  async function onCreateTikTokDevice () {
    const fields = getTikTokDeviceInfoFields()

    const popupParams: IPopupDialogParams = {
      title: '新增TikTok设备',
      fields,
      oneColumn: true
    }

    // 弹出对话框

    const { ok, data } = await showDialog<ITikTokDevice>(popupParams)
    if (!ok) return
    const { data: crawlerTaskInfo } = await createTikTokDevice(data)
    // 保存到 rows 中
    addNewRow(crawlerTaskInfo)
    notifySuccess('新增TikTok设备成功')
  }

  return {
    onCreateTikTokDevice
  }
}
