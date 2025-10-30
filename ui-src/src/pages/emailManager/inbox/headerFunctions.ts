/* eslint-disable @typescript-eslint/no-explicit-any */
import { showDialog } from 'src/components/popupDialog/PopupDialog'
import type { IPopupDialogParams } from 'src/components/popupDialog/types'
import { PopupDialogFieldType } from 'src/components/popupDialog/types'
import type { IEmailGroupListItem } from '../components/types'

import type { addNewRowType } from 'src/compositions/qTableUtils'

import type { IInbox } from 'src/api/emailBox'
import { createInbox, createInboxes } from 'src/api/emailBox'
import { confirmOperation, notifyError, notifySuccess, notifyUntil } from 'src/utils/dialog'
import type { IExcelColumnMapper } from 'src/utils/file'
import { readExcel, writeExcel } from 'src/utils/file'
import { isEmail } from 'src/utils/validator'
import logger from 'loglevel'

import { translateInboxManager, translateGlobal } from 'src/i18n/helpers'

export function getInboxFields () {
  return [
    {
      name: 'email',
      type: PopupDialogFieldType.email,
      label: translateInboxManager('col_email'),
      value: '',
      required: true
    },
    {
      name: 'name',
      type: PopupDialogFieldType.text,
      label: translateInboxManager('col_name'),
    },
    {
      name: 'minInboxCooldownHours',
      type: PopupDialogFieldType.number,
      label: translateInboxManager('col_minInboxCooldownHours'),
      value: 0
    },
    {
      name: 'description',
      label: translateInboxManager('col_description'),
    }
  ]
}

export function getInboxExcelDataMapper (): IExcelColumnMapper[] {
  return [
    {
      headerName: translateInboxManager('col_email'),
      fieldName: 'email',
      required: true
    },
    {
      headerName: translateInboxManager('col_name'),
      fieldName: 'name'
    },
    {
      headerName: translateInboxManager('col_minInboxCooldownHours'),
      fieldName: 'minInboxCooldownHours'
    },
    {
      headerName: translateInboxManager('col_description'),
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
    title: `${translateInboxManager('newInbox')} / ${emailGroupLabel}`,
    fields: getInboxFields()
  }

  // 弹出对话框
  return await showDialog<IInbox>(popupParams)
}


export function useHeaderFunction (emailGroup: Ref<IEmailGroupListItem>, addNewRow: addNewRowType) {
  // 新建收件箱
  async function onNewInboxClick () {
    // 新增收件箱
    // 弹出对话框

    const { ok, data } = await showNewInboxDialog(emailGroup.value.label)
    if (!ok) return
    // 添加邮箱组
    data.emailGroupId = emailGroup.value.id
    const { data: inbox } = await createInbox(data)
    // 保存到 rows 中
    addNewRow(inbox)

    notifySuccess(translateInboxManager('newInboxSuccess'))
  }

  // 导出模板
  async function onExportInboxTemplateClick () {
    const data: any[] = [
      {
        name: translateInboxManager('col_name'),
        email: translateInboxManager('export_EmailColumnTooltip'),
        minInboxCooldownHours: translateInboxManager('col_minInboxCooldownHours'),
        description: translateInboxManager('col_description')
      }
    ]
    await writeExcel(data, {
      fileName: `${translateInboxManager('inboxTemplate')}.xlsx`,
      sheetName: translateInboxManager('inbox'),
      mappers: getInboxExcelDataMapper()
    })

    notifySuccess(translateInboxManager('templateDonwloadSuccess'))
  }

  // 导入收件箱
  async function onImportInboxClick (emailGroupId: number | null = null) {
    if (typeof emailGroupId !== 'number') emailGroupId = emailGroup.value.id as number
    logger.debug(`[Inbox] import inboxes, emailGroupId: ${emailGroupId}, currentEmailGroupId: ${emailGroup.value.id}`)

    const data = await readExcel({
      sheetIndex: 0,
      selectSheet: true,
      mappers: getInboxExcelDataMapper()
    })

    if (data.length === 0) {
      notifyError(translateInboxManager('availableImportDataNotFound'))
      return
    }

    const validRows: IInbox[] = []
    // 添加组id
    for (const [index, row] of data.entries()) {
      if (!row.email) {
        logger.info(translateInboxManager('emptyDataAtRow', {
          row: index + 1
        }))
        continue
      }

      // 验证 email 格式
      if (!isEmail(row.email)) {
        notifyError(translateInboxManager('emailFormatInvalid', { email: row.email }))
        continue
      }

      // 添加组 id
      row.emailGroupId = emailGroupId || emailGroup.value.id
      validRows.push(row as IInbox)
    }

    if (validRows.length === 0) {
      notifyError(translateInboxManager('noValidImportData'))
      return
    }

    // 判断是否数据相等
    if (validRows.length < data.length) {
      const continueImport = await confirmOperation(
        translateGlobal('warning'),
        translateInboxManager('confirmImportWithErrors'))
      if (!continueImport) {
        notifyError(translateInboxManager('importCancelled'))
        return
      }
    }

    // 向服务器请求新增
    await notifyUntil(async (update) => {
      // 对数据分批处理
      for (let i = 0; i < validRows.length; i += 100) {
        update(`${translateGlobal('importing')}... [${i}/${validRows.length}]`)
        const partData = validRows.slice(i, i + 100)
        const { data: inboxes } = await createInboxes(partData)

        if (emailGroupId === emailGroup.value.id) {
          inboxes.forEach(x => {
            addNewRow(x, 'email')
          })
        }
      }
    }, translateInboxManager('importInbox'), translateGlobal('importing'))

    notifySuccess(translateInboxManager('importInboxSuccess'))
  }

  return {
    onNewInboxClick,
    onExportInboxTemplateClick,
    onImportInboxClick
  }
}
