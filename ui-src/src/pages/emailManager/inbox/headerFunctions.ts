/* eslint-disable @typescript-eslint/no-explicit-any */
import { showDialog } from 'src/components/popupDialog/PopupDialog'
import type { IPopupDialogParams} from 'src/components/popupDialog/types';
import { PopupDialogFieldType } from 'src/components/popupDialog/types'
import type { IEmailGroupListItem } from '../components/types'

import type { addNewRowType } from 'src/compositions/qTableUtils'

import type { IInbox} from 'src/api/emailBox';
import { createInbox, createInboxes } from 'src/api/emailBox'
import { notifyError, notifySuccess, notifyUntil } from 'src/utils/dialog'
import type { IExcelColumnMapper} from 'src/utils/file';
import { readExcel, writeExcel } from 'src/utils/file'
import { isEmail } from 'src/utils/validator'
import logger from 'loglevel'

export function getInboxFields () {
  return [
    {
      name: 'email',
      type: PopupDialogFieldType.email,
      label: '邮箱',
      value: '',
      required: true
    },
    {
      name: 'name',
      type: PopupDialogFieldType.text,
      label: '收件人名称'
    },
    {
      name: 'minInboxCooldownHours',
      type: PopupDialogFieldType.number,
      label: '最小收件间隔(小时)',
      value: 0
    },
    {
      name: 'description',
      label: '描述'
    }
  ]
}

export function getInboxExcelDataMapper (): IExcelColumnMapper[] {
  return [
    {
      headerName: '邮箱',
      fieldName: 'email',
      required: true
    },
    {
      headerName: '收件人名称',
      fieldName: 'name'
    },
    {
      headerName: '最小收件间隔(小时)',
      fieldName: 'minInboxCooldownHours'
    },
    {
      headerName: '描述',
      fieldName: 'description'
    }
  ]
}

/**
 * 弹出新增收件箱弹窗
 * @param emailGroupLabel
 * @returns
 */
export async function showNewInboxDialog (emailGroupLabel: string) {
  // 新增发件箱
  const popupParams: IPopupDialogParams = {
    title: `新增收件箱 / ${emailGroupLabel}`,
    fields: getInboxFields()
  }

  // 弹出对话框

  return await showDialog<IInbox>(popupParams)
}


export function useHeaderFunction (emailGroup: Ref<IEmailGroupListItem>, addNewRow: addNewRowType) {
  // 新建发件箱
  async function onNewInboxClick () {
    // 新增发件箱
    // 弹出对话框

    const { ok, data } = await showNewInboxDialog(emailGroup.value.label)
    if (!ok) return
    // 添加邮箱组
    data.emailGroupId = emailGroup.value.id
    const { data: outbox } = await createInbox(data)
    // 保存到 rows 中
    addNewRow(outbox)

    notifySuccess('新增发件箱成功')
  }

  // 导出模板
  async function onExportInboxTemplateClick () {
    const data: any[] = [
      {
        name: '收件人名称',
        email: '邮箱(导入时，请删除该行数据)',
        minInboxCooldownHours: '最小收件间隔(小时)',
        description: ''
      }
    ]
    await writeExcel(data, {
      fileName: '收件箱模板.xlsx',
      sheetName: '收件箱',
      mappers: getInboxExcelDataMapper()
    })

    notifySuccess('模板下载成功')
  }

  // 导入发件箱
  async function onImportInboxClick (emailGroupId: number | null = null) {
    if (typeof emailGroupId !== 'number') emailGroupId = emailGroup.value.id as number
    logger.debug(`[Inbox] import inboxes, emailGroupId: ${emailGroupId}, currentEmailGroupId: ${emailGroup.value.id}`)

    const data = await readExcel({
      sheetIndex: 0,
      selectSheet: true,
      mappers: getInboxExcelDataMapper()
    })

    if (data.length === 0) {
      notifyError('未找到可导入的数据')
      return
    }

    // 添加组
    for (const row of data) {
      // 验证 email 格式
      if (!isEmail(row.email)) {
        notifyError(`邮箱格式错误: ${row.email}`)
        return
      }

      // 添加组 id
      row.emailGroupId = emailGroupId || emailGroup.value.id
    }

    // 向服务器请求新增
    await notifyUntil(async (update) => {
      // 对数据分批处理
      for (let i = 0; i < data.length; i += 100) {
        update(`导入中... [${i}/${data.length}]`)
        const partData = data.slice(i, i + 100)
        const { data: inboxes } = await createInboxes(partData as IInbox[])

        if (emailGroupId === emailGroup.value.id) {
          inboxes.forEach(x => {
            addNewRow(x, 'email')
          })
        }
      }
    }, '导入收件箱', '正在导入中...')

    notifySuccess('导入成功')
  }

  return {
    onNewInboxClick,
    onExportInboxTemplateClick,
    onImportInboxClick
  }
}
