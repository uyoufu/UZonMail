/* eslint-disable @typescript-eslint/no-explicit-any */
import { showDialog } from 'src/components/popupDialog/PopupDialog'
import type { IOnSetupParams, IPopupDialogField, IPopupDialogParams } from 'src/components/popupDialog/types'
import { PopupDialogFieldType } from 'src/components/popupDialog/types'
import type { IEmailGroupListItem } from '../components/types'

import type { IOutbox } from 'src/api/emailBox'
import { createOutbox, createOutboxes, startOutlookDelegateAuthorization, getOutboxInfo } from 'src/api/emailBox'
import { GuessSmtpInfoGet } from 'src/api/smtpInfo'

import { confirmOperation, notifyError, notifySuccess, notifyWarning } from 'src/utils/dialog'
import { isEmail } from 'src/utils/validator'

import { useUserInfoStore } from 'src/stores/user'
import { aes, deAes } from 'src/utils/encrypt'
import type { IExcelColumnMapper } from 'src/utils/file'
import { readExcel, writeExcel } from 'src/utils/file'
import type { IProxy } from 'src/api/proxy'
import { getUsableProxies } from 'src/api/proxy'
import { debounce } from 'lodash'
import type { IUserEncryptKeys } from 'src/stores/types'

import logger from 'loglevel'
import { translateGlobal, translateOutboxManager } from 'src/i18n/helpers'

function encryptPassword (smtpPasswordSecretKeys: string[], password: string) {
  return aes(smtpPasswordSecretKeys[0] as string, smtpPasswordSecretKeys[1] as string, password)
}

// 判断是否是 Exchange 邮箱
export function isExchangeEmail (email: string): boolean {
  // 判断是否是 Exchange 邮箱
  const emailDomain = email.trim().split('@')[1]?.toLowerCase()
  if (!emailDomain) return false

  return isExchangeDomain(emailDomain)
}

// 判断是否是 Exchange 邮箱域名
export function isExchangeDomain (domain: string): boolean {
  if (!domain) return false

  const domains = ['outlook.com', 'hotmail.com']
  const emailDomain = domain.trim().toLowerCase()
  if (!emailDomain) return false
  return domains.some(x => emailDomain.endsWith(x))
}

// 判断是否是 Exchange 发件箱
export function isExchangeOutbox (smtpHost: string, email: string): boolean {
  if (!isExchangeEmail(email))
    return false

  // 判断 smtp 地址
  if (smtpHost && !isExchangeDomain(smtpHost))
    return false

  return true
}

/**
 * 获取发件箱字段
 * @param smtpPasswordSecretKeys
 * @returns
 */
export async function getOutboxFields (smtpPasswordSecretKeys: string[]): Promise<IPopupDialogField[]> {
  // 获取所有的代理
  const { data: proxyOptions } = await getUsableProxies()
  proxyOptions.unshift({
    id: 0,
    name: translateGlobal('empty'),
    isActive: true,
    url: ''
  } as IProxy)
  return [
    {
      name: 'email',
      type: PopupDialogFieldType.email,
      label: translateOutboxManager('col_email'),
      value: '',
      required: true
    },
    {
      name: 'name',
      type: PopupDialogFieldType.text,
      label: translateOutboxManager('col_outboxUserName'),
      value: ''
    },
    {
      name: 'smtpHost',
      label: translateOutboxManager('col_smtpHost'),
      type: PopupDialogFieldType.text,
      value: '',
      required: true
    },
    {
      name: 'smtpPort',
      label: translateOutboxManager('col_smtpPort'),
      type: PopupDialogFieldType.number,
      value: 465,
      required: true
    },
    {
      name: 'userName',
      type: PopupDialogFieldType.text,
      label: translateOutboxManager('col_smtpUserName'),
      placeholder: translateOutboxManager('ifSameAsEmailUseEmpty'),
      value: ''
    },
    {
      name: 'password',
      label: translateOutboxManager('col_smtpPassword'),
      type: PopupDialogFieldType.password,
      parser: (value: any) => {
        const pwd = String(value)
        // 对密码进行加密
        return encryptPassword(smtpPasswordSecretKeys, pwd)
      },
      validate: (value: any, parsedValue: any, allValues: Record<string, any>) => {
        if (isExchangeEmail(allValues.email)) {
          // 如果是 Outlook 邮箱，则允许为空
          return {
            ok: true
          }
        }
        return {
          ok: value && value.length > 0,
          message: translateOutboxManager('smtpPasswordIsRequired')
        }
      },
      value: ''
    },
    {
      name: 'description',
      label: translateOutboxManager('col_description'),
    },
    {
      name: 'proxyId',
      label: translateOutboxManager('col_proxy'),
      type: PopupDialogFieldType.selectOne,
      value: 0,
      placeholder: translateOutboxManager('ifEmptyProxyUseSystemSettings'),
      options: proxyOptions,
      optionLabel: 'name',
      optionValue: 'id',
      optionTooltip: 'description',
      mapOptions: true,
      emitValue: true
    },
    {
      name: 'replyToEmails',
      label: translateOutboxManager('col_replyToEmails'),
    },
    {
      name: 'enableSSL',
      label: translateOutboxManager('col_enableSSL'),
      type: PopupDialogFieldType.boolean,
      value: true,
      required: true
    }
  ]
}

export function getOutboxExcelDataMapper (): IExcelColumnMapper[] {
  return [
    {
      headerName: translateOutboxManager('col_email'),
      fieldName: 'email',
      required: true
    },
    {
      headerName: translateOutboxManager('col_outboxUserName'),
      fieldName: 'name'
    },
    {
      headerName: translateOutboxManager('col_smtpUserName'),
      fieldName: 'userName'
    },
    {
      headerName: translateOutboxManager('col_smtpPassword'),
      fieldName: 'password',
      required: true
    },
    {
      headerName: translateOutboxManager('col_smtpHost'),
      fieldName: 'smtpHost',
      required: true
    },
    {
      headerName: translateOutboxManager('col_smtpPort'),
      fieldName: 'smtpPort',
      required: true
    },
    {
      headerName: translateOutboxManager('col_description'),
      fieldName: 'description'
    },
    {
      headerName: translateOutboxManager('col_proxy'),
      fieldName: 'proxy'
    },
    {
      headerName: translateOutboxManager('col_replyToEmails'),
      fieldName: 'replyToEmails'
    },
    {
      headerName: translateOutboxManager('col_enableSSL'),
      fieldName: 'enableSSL',
      format: (value: boolean) => {
        if (typeof value === 'boolean') {
          return value ? translateGlobal('yes') : translateGlobal('no')
        }

        if (typeof value === 'string') {
          return value === translateGlobal('yes')
        }
        return !!value
      }
    }
  ]
}

/**
 * 进行 Outlook 委托授权
 * 当满足条件时，才会执行
 * @param outbox
 * @param encryptKeys
 * @returns
 */
export async function tryOutlookDelegateAuthorization (outbox: IOutbox, encryptKeys: IUserEncryptKeys) {
  if (!isExchangeEmail(outbox.email)) return
  // 存在但非 Exchange 地址
  if (outbox.smtpHost && !isExchangeDomain(outbox.smtpHost)) {
    notifyWarning(translateOutboxManager('outlookDelegateAuthorizationSkippedNonExchangeSmtp'))
    return
  }

  // // 判断是否采用个人委托授权
  // if (!outbox.userName || outbox.userName.includes('/')) return

  // 密钥必须小于 80 位
  // 否则可能是 refreshToken
  const plainPassword = deAes(encryptKeys.key, encryptKeys.iv, outbox.password)
  if (plainPassword.length > 80) {
    notifySuccess(translateOutboxManager('existingRefreshTokenNoNeedDelegateAuthorization'))
    return
  }


  notifyWarning(translateOutboxManager('detectedExchangeEmailStartingOutlookDelegateAuthorization'))

  const { data: authorizationUrl } = await startOutlookDelegateAuthorization(outbox.id as number)
  if (!authorizationUrl) {
    notifyError(translateOutboxManager('failedToGetAuthorizationUrl'))
    return
  }

  const win = window.open(
    authorizationUrl,
    'outlook-auth',
    'width=600,height=700,scrollbars=yes,resizable=yes'
  )

  if (!win) {
    notifyError(translateOutboxManager('allowPopupWindowsForAuthorization'))
    return
  }

  const waitingPromise = new Promise<void>((resolve) => {
    const interval = setInterval(() => {
      if (win.closed) {
        clearInterval(interval)
        resolve()
      }
    }, 200)
  })

  await waitingPromise

  // 更新发件箱的数据
  const { data: newOutbox } = await getOutboxInfo(outbox.id as number)
  outbox.password = newOutbox.password

  notifySuccess(translateOutboxManager('delegateCopleted'))
}

/**
 * 使用发件箱头部功能
 * @param emailGroup
 * @param addNewRow
 * @returns
 */
export function useHeaderFunction (emailGroup: Ref<IEmailGroupListItem>,
  addNewRow: (newRow: Record<string, any>) => void) {
  const userInfoStore = useUserInfoStore()

  // 新建发件箱
  async function onNewOutboxClick () {
    const GuessSmtpInfoGetDebounce = debounce(async (email: string, params: IOnSetupParams) => {
      // 从服务器请求数据
      const guessResult = await GuessSmtpInfoGet(email)

      params.fieldsModel.value.smtpHost = guessResult.data.host
      if (!params.fieldsModel.value.smtpPort)
        params.fieldsModel.value.smtpPort = guessResult.data.port
    }, 1000, {
      trailing: true
    })

    // 新增发件箱
    const popupParams: IPopupDialogParams = {
      title: translateOutboxManager('newOutboxTitle', { groupName: emailGroup.value.label }),
      fields: await getOutboxFields(userInfoStore.smtpPasswordSecretKeys),
      onSetup: (params) => {
        watch(() => params.fieldsModel.value.email, async newValue => {
          if (!newValue) return

          const host = params.fieldsModel.value.smtpHost as string
          if (host) return

          await GuessSmtpInfoGetDebounce(newValue, params)
        })
      }
    }

    // 弹出对话框
    const { ok, data } = await showDialog<IOutbox>(popupParams)
    if (!ok) return
    // 新建请求
    // 添加邮箱组
    data.emailGroupId = emailGroup.value.id
    const { data: outbox } = await createOutbox(data)
    // 保存到 rows 中
    addNewRow(outbox)

    notifySuccess(translateOutboxManager('newOutboxSuccess'))

    // 进行 outlook 委托授权
    await tryOutlookDelegateAuthorization(outbox, userInfoStore.userEncryptKeys)
  }

  // 导出模板
  async function onExportOutboxTemplateClick () {
    const data: any[] = [
      {
        email: translateOutboxManager('exportColumn_email'),
        name: translateOutboxManager('exportColumn_name'),
        userName: translateOutboxManager('exportColumn_userName'),
        password: translateOutboxManager('exportColumn_password'),
        smtpHost: translateOutboxManager('exportColumn_smtpHost'),
        smtpPort: 25,
        description: translateOutboxManager('exportColumn_description'),
        proxy: translateOutboxManager('exportColumn_proxy'),
        replyToEmails: translateOutboxManager('exportColumn_replyToEmails'),
        enableSSL: true
      }, {
        email: 'test@163.com',
        name: '',
        userName: '',
        password: 'ThisIsYour163SmtpPassword',
        smtpHost: 'smtp.163.com',
        smtpPort: 465,
        description: '',
        proxy: '',
        replyToEmails: '',
        enableSSL: true
      }
    ]
    await writeExcel(data, {
      fileName: translateOutboxManager('outboxTemplateFileName'),
      sheetName: translateOutboxManager('col_email'),
      mappers: getOutboxExcelDataMapper()
    })

    notifySuccess(translateOutboxManager('outboxTemplateDownloadSuccess'))
  }

  // 从 excel 导入
  async function onImportOutboxFromExcelClicked (emailGroupId: number | null = null) {
    if (typeof emailGroupId !== 'number') emailGroupId = emailGroup.value.id as number

    const data = await readExcel({
      sheetIndex: 0,
      selectSheet: true,
      mappers: getOutboxExcelDataMapper()
    })

    if (data.length === 0) {
      notifyError(translateOutboxManager('noItemsToImport'))
      return
    }

    const validRows: IOutbox[] = []

    // 对密码进行加密
    for (const [index, row] of data.entries()) {
      if (!row.email) {
        logger.info(translateOutboxManager('emaiEmptyAtRow', { row: index + 1 }))
        continue
      }

      // 验证邮箱是否正确
      // 验证 email 格式
      if (!isEmail(row.email)) {
        const message = translateOutboxManager('emailFormatInvalid', { email: row.email })
        logger.info(message, row)
        notifyError(message)
        continue
      }

      row.password = encryptPassword(userInfoStore.smtpPasswordSecretKeys, row.password)
      row.emailGroupId = emailGroupId || emailGroup.value.id
      validRows.push(row as IOutbox)
    }

    if (validRows.length === 0) {
      notifyError(translateOutboxManager('noValidImportData'))
      return
    }

    // 判断是否数据相等
    if (validRows.length < data.length) {
      const continueImport = await confirmOperation(
        translateGlobal('confirmOperation'),
        translateOutboxManager('confirmImportWithErrors'))
      if (!continueImport) {
        notifyError(translateOutboxManager('importCancelled'))
        return
      }
    }

    // 向服务器请求新增
    const { data: outboxes } = await createOutboxes(validRows)

    if (emailGroupId === emailGroup.value.id) {
      outboxes.forEach(x => {
        addNewRow(x)
      })
    }

    notifySuccess(translateOutboxManager('importOutboxSuccess', { count: outboxes.length }))
  }

  return {
    onNewOutboxClick,
    onExportOutboxTemplateClick,
    onImportOutboxFromExcelClicked
  }
}
