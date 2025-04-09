import type { IPopupDialogParams } from 'src/components/popupDialog/types'
import { PopupDialogFieldType } from 'src/components/popupDialog/types'
import { showDialog } from 'src/utils/dialog'
import { useI18n } from 'vue-i18n'
import type { IEmailGroupListItem } from '../components/types'

import logger from 'loglevel'

/**
 * 从 txt 文件导入邮件
 * 这种方法，需要智能计算邮件的 smtp 及端口号
 */
export function useOutboxImporter (emailGroup: Ref<IEmailGroupListItem>) {
  const { t } = useI18n()

  // #region 从文本导入
  async function onImportOutboxFromTxt (emailGroupId: number | null = null) {
    if (typeof emailGroupId !== 'number') emailGroupId = emailGroup.value.id as number

    // 新建弹窗
    const popupParams: IPopupDialogParams = {
      title: `从文本导入发件箱 / ${emailGroupId}`,
      oneColumn: true,
      fields: [
        {
          name: 'text',
          label: '发件箱文本',
          type: PopupDialogFieldType.textarea,
          placeholder: '每行一个发件箱',
          value: '',
          required: true
        }
      ]
    }

    const result = await showDialog(popupParams)
    if (!result.ok) return

    // 开始解析导入的内容
    const outboxTexts = result.data.text.split('\n')
      .map((x: string) => x.trim())
      .filter((x: string) => x.length > 0)

    // 解析其中的邮箱部分，向服务器请求补全默认值
    logger.debug('[useOutboxImporter] outboxTexts', outboxTexts)
  }

  const importFromTxtLable = t('outboxManager.importFromTxt')
  const importFromTxtTooltip = t('outboxManager.importFromTxtTooltip').split('\n')
  // #endregion

  return {
    onImportOutboxFromTxt,
    importFromTxtLable,
    importFromTxtTooltip
  }
}
