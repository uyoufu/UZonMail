
import type { ILowCodeField, IPopupDialogParams } from 'src/components/lowCode/types';
import { LowCodeFieldType } from 'src/components/lowCode/types'

import { notifySuccess, showDialog } from 'src/utils/dialog'
import type { addNewRowType } from 'src/compositions/qTableUtils'
import type { ITikTokDevice } from 'src/api/pro/tikTokDevice';
import { createTikTokDevice } from 'src/api/pro/tikTokDevice'
import { t } from 'src/i18n/helpers'

export function getTikTokDeviceInfoFields (): ILowCodeField[] {
  return [
    {
      name: 'name',
      label: t('global.name'),
      required: true
    },
    {
      name: 'description',
      label: t('global.description')
    },
    {
      name: 'deviceId',
      type: LowCodeFieldType.text,
      label: t('pages.tikTokDevice.deviceId')
    },
    {
      name: 'odinId',
      type: LowCodeFieldType.text,
      label: t('pages.tikTokDevice.adId')
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
      title: t('pages.tikTokDevice.createTitle'),
      fields,
      oneColumn: true
    }

    // 弹出对话框

    const { ok, data } = await showDialog<ITikTokDevice>(popupParams)
    if (!ok) return
    const { data: crawlerTaskInfo } = await createTikTokDevice(data)
    // 保存到 rows 中
    addNewRow(crawlerTaskInfo)
    notifySuccess(t('pages.tikTokDevice.createSuccess'))
  }

  return {
    onCreateTikTokDevice
  }
}
