import type { IPopupDialogParams } from 'src/components/popupDialog/types'
import { PopupDialogFieldType } from 'src/components/popupDialog/types'
import { notifyError, notifySuccess, showDialog } from 'src/utils/dialog'
import { useI18n } from 'vue-i18n'
import type { IEmailGroupListItem } from '../components/types'

import logger from 'loglevel'
import { splitString } from 'src/utils/stringHelper'
import type { IInbox } from 'src/api/emailBox';
import { createInboxes } from 'src/api/emailBox'

/**
 * 从 txt 文件导入邮件
 * 这种方法，需要智能计算邮件的 smtp 及端口号
 */
// eslint-disable-next-line @typescript-eslint/no-explicit-any
export function useInboxImporter (emailGroup: Ref<IEmailGroupListItem>, addNewRow: (newRow: Record<string, any>) => void) {
  const { t } = useI18n()

  // #region 从文本导入
  async function onImportInboxFromTxt (emailGroupId: number | null = null) {
    if (typeof emailGroupId !== 'number') emailGroupId = emailGroup.value.id as number

    // 新建弹窗
    const popupParams: IPopupDialogParams = {
      title: `从文本导入收件箱 / ${emailGroupId}`,
      oneColumn: true,
      fields: [
        {
          name: 'text',
          label: '收件箱文本',
          type: PopupDialogFieldType.textarea,
          placeholder: '每行一个收件箱',
          value: '',
          required: true,
          disableAutogrow: true
        }
      ]
    }

    const result = await showDialog(popupParams)
    if (!result.ok) return

    // 开始解析导入的内容
    const inboxTexts: string[][] = result.data.text.split('\n')
      .map((x: string) => x.trim())
      .filter((x: string) => x.length > 0)
      .map((x: string) => {
        return splitString(x)
      })
      .filter((x: string[]) => x.length > 0)

    if (inboxTexts.length === 0) {
      notifyError('未找到可导入的数据')
      return
    }

    // 解析其中的邮箱部分，向服务器请求补全默认值
    logger.debug('[useInboxImporter] inboxTexts', inboxTexts)

    const newData: IInbox[] = []
    for (const outboxText of inboxTexts) {
      const outbox = __getNewInboxData(emailGroupId, outboxText)
      if (outbox) newData.push(outbox)
    }

    // 向服务器请求新增
    const { data: outboxes } = await createInboxes(newData)

    if (emailGroupId === emailGroup.value.id) {
      outboxes.forEach(x => {
        addNewRow(x)
      })
    }

    notifySuccess('导入成功')
  }

  function __getNewInboxData (emailGroupId: number, outboxTexts: string[]): IInbox | null {
    const inbox: IInbox = {
      emailGroupId,
      email: '',
      name: '',
      minInboxCooldownHours: 0
    }

    // 获取邮箱
    const email = outboxTexts.find(x => x.includes('@'))
    if (!email) return null
    inbox.email = email

    // 解析最小冷却时间
    const minInboxCooldownHours = outboxTexts.find(x => !isNaN(Number(x)))
    inbox.minInboxCooldownHours = Number(minInboxCooldownHours) || 0

    // 获取名称
    const name = outboxTexts.find(x => !x.includes('@') && isNaN(Number(x)))
    inbox.name = name || ''

    return inbox
  }

  const importFromTxtLable = t('inboxManager.importFromTxt')
  const importFromTxtTooltip = t('inboxManager.importFromTxtTooltip').split('\n')
  // #endregion

  return {
    onImportInboxFromTxt,
    importFromTxtLable,
    importFromTxtTooltip
  }
}
