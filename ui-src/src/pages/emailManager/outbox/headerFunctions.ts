/* eslint-disable @typescript-eslint/no-explicit-any */
import { showDialog } from 'src/components/popupDialog/PopupDialog'
import type { IOnSetupParams, IPopupDialogField, IPopupDialogParams } from 'src/components/popupDialog/types'
import { PopupDialogFieldType } from 'src/components/popupDialog/types'
import type { IEmailGroupListItem } from '../components/types'

import type { IOutbox } from 'src/api/emailBox'
import {
  createOutbox,
  createOutboxes,
  startOutlookDelegateAuthorization,
  ConnectionSecurity,
  OutboxType
} from 'src/api/emailBox'
import { guessSmtpInfoGet, updateSmtpInfo } from 'src/api/smtpInfo'

import { confirmOperation, notifyError, notifySuccess, notifyWarning } from 'src/utils/dialog'
import { isEmail } from 'src/utils/validator'

import type { IExcelColumnMapper } from 'src/utils/file'
import { readExcel, writeExcel } from 'src/utils/file'
import { enumEntries } from 'src/utils/enum'
import type { IProxy } from 'src/api/proxy'
import { getUsableProxies } from 'src/api/proxy'
import { debounce } from 'lodash'

import logger from 'loglevel'
import { translateGlobal, translateOutboxManager } from 'src/i18n/helpers'


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

export function isMsGraphOutbox (outbox: IOutbox): boolean {
  return outbox.type === OutboxType.MsGraph
}

/**
 * 获取发件箱字段
 * @returns
 */
export async function getOutboxFields (): Promise<IPopupDialogField[]> {
  // 获取所有的代理
  const { data: proxyOptions } = await getUsableProxies()
  proxyOptions.unshift({
    id: 0,
    name: translateGlobal('empty'),
    isActive: true,
    url: ''
  } as IProxy)

  // 获取连接安全协议选项
  const secureSocketOptions = enumEntries(ConnectionSecurity).map(([key, value]) => ({
    label: key,
    value: value
  }))

  logger.debug('[outbox] secureSocketOptions', secureSocketOptions)

  function isSmtp (value: Record<string, any>) {
    return value.type === OutboxType.SMTP
  }

  function isMsGraph (value: Record<string, any>) {
    return value.type === OutboxType.MsGraph
  }

  return [
    {
      name: 'type',
      type: PopupDialogFieldType.selectOne,
      options: enumEntries(OutboxType).map(([key, value]) => ({
        label: key,
        value: value
      })),
      mapOptions: true,
      emitValue: true,
      label: translateOutboxManager('col_type'),
      value: OutboxType.SMTP,
      required: true
    },
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
      label: translateOutboxManager('col_outboxName'),
      value: ''
    },
    {
      name: 'smtpHost',
      label: translateOutboxManager('col_smtpHost'),
      type: PopupDialogFieldType.text,
      value: '',
      required: true,
      visible: isSmtp
    },
    {
      name: 'smtpPort',
      label: translateOutboxManager('col_smtpPort'),
      type: PopupDialogFieldType.number,
      value: 465,
      required: true,
      visible: isSmtp
    },
    {
      name: 'userName',
      type: PopupDialogFieldType.text,
      label: translateOutboxManager('smtpUserName'),
      placeholder: translateOutboxManager('ifSameAsEmailUseEmpty'),
      value: '',
      visible: isSmtp
    },
    {
      name: 'userName',
      type: PopupDialogFieldType.text,
      label: translateOutboxManager('clientId'),
      placeholder: translateOutboxManager('clientIdPlaceholder'),
      value: '',
      visible: isMsGraph
    },
    {
      name: 'password',
      label: translateOutboxManager('smtpPassword'),
      type: PopupDialogFieldType.password,
      // validate: (value: any, parsedValue: any, allValues: Record<string, any>) => {
      //   if (isMsGraphOutbox(allValues as IOutbox)) {
      //     // 如果是 Outlook 邮箱，则允许为空
      //     return {
      //       ok: true
      //     }
      //   }
      //   return {
      //     ok: value && value.length > 0,
      //     message: translateOutboxManager('smtpPasswordIsRequired')
      //   }
      // },
      value: '',
      required: true,
      visible: isSmtp
    },
    {
      name: 'password',
      label: translateOutboxManager('refreshToken'),
      placeholder: translateOutboxManager('refreshTokenPlaceholder'),
      type: PopupDialogFieldType.password,
      value: '',
      visible: isMsGraph
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
      name: 'connectionSecurity',
      label: translateOutboxManager('col_connectionSecurity'),
      type: PopupDialogFieldType.selectOne,
      options: secureSocketOptions,
      emitValue: true,
      value: ConnectionSecurity.SSL,
      mapOptions: true,
      required: true,
      visible: isSmtp
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
      headerName: translateOutboxManager('col_outboxName'),
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
      headerName: translateOutboxManager('col_connectionSecurity'),
      fieldName: 'connectionSecurity'
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
export async function tryOutlookDelegateAuthorization (outbox: IOutbox) {
  if (!isMsGraphOutbox(outbox)) {
    notifyWarning(translateOutboxManager('outlookDelegateAuthorizationSkippedNonMsGraphType'))
    return
  }

  // 若密码是加密后的状态，则进行提示
  if (outbox.password && outbox.password.startsWith('*')) {
    notifyError(translateOutboxManager('outlookDelegateAuthorizationSkippedEncryptedPassword'))
    return
  }

  // 判断是否使用 clientId + refreshToken 进行授权
  // 该方式直接在后端检测，直接返回
  if (outbox.userName && outbox.password) {
    // 后端在使用时，或者前端验证时，会检测授权
    // 此处不处理
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

  await new Promise<void>((resolve) => {
    const interval = setInterval(() => {
      if (win.closed) {
        clearInterval(interval)
        resolve()
      }
    }, 200)
  })

  notifySuccess(translateOutboxManager('delegateCompleted'))
}

/**
 * 使用发件箱头部功能
 * @param emailGroup
 * @param addNewRow
 * @returns
 */
export function useHeaderFunction (emailGroup: Ref<IEmailGroupListItem>,
  addNewRow: (newRow: Record<string, any>) => void) {
  // 新建发件箱
  async function onNewOutboxClick () {
    const guessSmtpInfoGetDebounce = debounce(async (email: string, params: IOnSetupParams) => {
      // 从服务器请求数据
      const guessResult = await guessSmtpInfoGet(email)

      params.fieldsModel.value.smtpHost = guessResult.data.host
      if (!params.fieldsModel.value.smtpPort)
        params.fieldsModel.value.smtpPort = guessResult.data.port
    }, 1000, {
      trailing: true
    })

    // 新增发件箱
    const outboxFields = await getOutboxFields()
    const popupParams: IPopupDialogParams = {
      title: translateOutboxManager('newOutboxTitle', { groupName: emailGroup.value.label }),
      fields: outboxFields,
      onSetup: (params) => {
        // email 变化时，猜测 smtp 信息
        watch(() => params.fieldsModel.value.email, async newValue => {
          if (!newValue) return

          const host = params.fieldsModel.value.smtpHost as string
          if (host) return

          await guessSmtpInfoGetDebounce(newValue, params)
        })

        // 加密方式变化时，更改端口号
        watch(() => params.fieldsModel.value.connectionSecurity, (newValue) => {
          let defaultPort = 465
          switch (newValue) {
            case ConnectionSecurity.None:
              defaultPort = 25
              break
            case ConnectionSecurity.SSL:
              defaultPort = 465
              break
            case ConnectionSecurity.TLS:
              defaultPort = 465
              break
            case ConnectionSecurity.StartTLS:
              defaultPort = 587
              break
            default:
              defaultPort = 465
          }
          params.fieldsModel.value.smtpPort = defaultPort
        })
      },
      // 整体验证
      validate (values: Record<string, any>) {
        if (values.type === OutboxType.MsGraph) {
          // userName + password 必须同时存在或者同时为空
          const hasUserName = !!values.userName
          const hasPassword = !!values.password
          if (hasUserName !== hasPassword) {
            return {
              ok: false,
              message: translateOutboxManager('clientIdAndRefreshTokenBothRequired')
            }
          }
        }

        return { ok: true }
      }
    }

    // 弹出对话框
    const { ok, data } = await showDialog<IOutbox>(popupParams)
    if (!ok) return
    // 新建请求
    // 添加邮箱组
    data.emailGroupId = emailGroup.value.id
    const { data: outbox } = await createOutbox(data)
    // 将密码设置为本机密码
    outbox.password = data.password

    // 保存到 rows 中
    addNewRow(outbox)

    notifySuccess(translateOutboxManager('newOutboxSuccess'))

    // 如果是 SMTP 类型，保存到 SMTP 信息库
    if (outbox.type === OutboxType.SMTP) {
      await updateSmtpInfo({
        domain: outbox.email.split('@')[1]!,
        host: outbox.smtpHost,
        port: outbox.smtpPort!,
        connectionSecurity: outbox.connectionSecurity
      })
    }

    // 进行 outlook 委托授权
    await tryOutlookDelegateAuthorization(outbox)
  }

  // 导出模板
  async function onExportOutboxTemplateClick () {
    const data: any[] = [
      {
        email: translateOutboxManager('exportColumn_email'),
        name: translateOutboxManager('exportColumn_name'),
        type: translateOutboxManager('exportColumn_type'),
        userName: translateOutboxManager('exportColumn_userName'),
        password: translateOutboxManager('exportColumn_password'),
        smtpHost: translateOutboxManager('exportColumn_smtpHost'),
        smtpPort: 25,
        description: translateOutboxManager('exportColumn_description'),
        proxy: translateOutboxManager('exportColumn_proxy'),
        replyToEmails: translateOutboxManager('exportColumn_replyToEmails'),
        connectionSecurity: translateOutboxManager('exportColumn_connectionSecurity')
      }, {
        email: 'test@163.com',
        name: '',
        type: OutboxType[OutboxType.SMTP],
        userName: '',
        password: 'ThisIsYour163SmtpPassword',
        smtpHost: 'smtp.163.com',
        smtpPort: 465,
        description: '',
        proxy: '',
        replyToEmails: '',
        connectionSecurity: ConnectionSecurity[ConnectionSecurity.SSL]
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
        logger.info(translateOutboxManager('emailEmptyAtRow', { row: index + 1 }))
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

      // 添加邮箱组 ID
      row.emailGroupId = emailGroupId || emailGroup.value.id
      // 将安全设置转换成枚举
      row.connectionSecurity = ConnectionSecurity[row.connectionSecurity as keyof typeof ConnectionSecurity]
        || ConnectionSecurity.None
      // 将类型转换成枚举
      row.type = OutboxType[row.type as keyof typeof OutboxType] || OutboxType.SMTP

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
        // 将密码设置为本机密码
        x.password = validRows.find(y => y.email === x.email)?.password || '******'
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
