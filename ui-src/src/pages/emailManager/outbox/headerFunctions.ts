/* eslint-disable @typescript-eslint/no-explicit-any */
import { showDialog } from 'src/components/popupDialog/PopupDialog'
import type { IOnSetupParams, IPopupDialogField, IPopupDialogParams } from 'src/components/popupDialog/types'
import { PopupDialogFieldType } from 'src/components/popupDialog/types'
import type { IEmailGroupListItem } from '../components/types'

import type { IOutbox } from 'src/api/emailBox'
import { createOutbox, createOutboxes, startOutlookDelegateAuthorization, getOutboxInfo } from 'src/api/emailBox'
import { GuessSmtpInfoGet } from 'src/api/smtpInfo'

import { notifyError, notifySuccess, notifyWarning } from 'src/utils/dialog'
import { isEmail } from 'src/utils/validator'

import { useUserInfoStore } from 'src/stores/user'
import { aes, deAes } from 'src/utils/encrypt'
import type { IExcelColumnMapper } from 'src/utils/file'
import { readExcel, writeExcel } from 'src/utils/file'
import type { IProxy } from 'src/api/proxy'
import { getUsableProxies } from 'src/api/proxy'
import { debounce } from 'lodash'
import type { IUserEncryptKeys } from 'src/stores/types'

function encryptPassword (smtpPasswordSecretKeys: string[], password: string) {
  return aes(smtpPasswordSecretKeys[0] as string, smtpPasswordSecretKeys[1] as string, password)
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
    name: '无',
    isActive: true,
    url: ''
  } as IProxy)
  return [
    {
      name: 'email',
      type: PopupDialogFieldType.email,
      label: 'smtp发件邮箱',
      value: '',
      required: true
    },
    {
      name: 'name',
      type: PopupDialogFieldType.text,
      label: '发件人名称',
      value: ''
    },
    {
      name: 'smtpHost',
      label: 'smtp地址',
      type: PopupDialogFieldType.text,
      value: '',
      required: true
    },
    {
      name: 'smtpPort',
      label: 'smtp端口',
      type: PopupDialogFieldType.number,
      value: 465,
      required: true
    },
    {
      name: 'userName',
      type: PopupDialogFieldType.text,
      label: 'smtp用户名',
      placeholder: '可为空，若为空，则使用发件邮箱作用用户名',
      value: ''
    },
    {
      name: 'password',
      label: 'smtp密码',
      type: PopupDialogFieldType.password,
      parser: (value: any) => {
        const pwd = String(value)
        // 对密码进行加密
        return encryptPassword(smtpPasswordSecretKeys, pwd)
      },
      validate: (value: any, parsedValue: any, allValues: Record<string, any>) => {
        if (allValues.email && allValues.email.toLowerCase().includes("@outlook.com")) {
          // 如果是 Outlook 邮箱，则允许为空
          return {
            ok: true
          }
        }
        return {
          ok: value && value.length > 0,
          message: 'smtp密码不能为空'
        }
      },
      value: ''
    },
    {
      name: 'description',
      label: '描述'
    },
    {
      name: 'proxyId',
      label: '代理',
      type: PopupDialogFieldType.selectOne,
      value: 0,
      placeholder: '为空时使用系统设置',
      options: proxyOptions,
      optionLabel: 'name',
      optionValue: 'id',
      optionTooltip: 'description',
      mapOptions: true,
      emitValue: true
    },
    {
      name: 'replyToEmails',
      label: '回信收件人'
    },
    {
      name: 'enableSSL',
      label: '启用 SSL',
      type: PopupDialogFieldType.boolean,
      value: true,
      required: true
    }
  ]
}

export function getOutboxExcelDataMapper (): IExcelColumnMapper[] {
  return [
    {
      headerName: 'smtp邮箱',
      fieldName: 'email',
      required: true
    },
    {
      headerName: '发件人名称',
      fieldName: 'name'
    },
    {
      headerName: 'smtp用户名',
      fieldName: 'userName'
    },
    {
      headerName: 'smtp密码',
      fieldName: 'password',
      required: true
    },
    {
      headerName: 'smtp地址',
      fieldName: 'smtpHost',
      required: true
    },
    {
      headerName: 'smtp端口',
      fieldName: 'smtpPort',
      required: true
    },
    {
      headerName: '描述',
      fieldName: 'description'
    },
    {
      headerName: '代理',
      fieldName: 'proxy'
    },
    {
      headerName: '回信收件人',
      fieldName: 'replyToEmails'
    },
    {
      headerName: '启用SSL',
      fieldName: 'enableSSL',
      format: (value: boolean) => {
        if (typeof value === 'boolean') {
          return value ? '是' : '否'
        }

        if (typeof value === 'string') {
          return value === '是'
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
  if (!outbox.email.toLowerCase().includes("@outlook.com")) return

  // // 判断是否采用个人委托授权
  // if (!outbox.userName || outbox.userName.includes('/')) return

  // 密钥必须小于 80 位
  // 否则可能是 refreshToken
  const plainPassword = deAes(encryptKeys.key, encryptKeys.iv, outbox.password)
  if (plainPassword.length > 80) {
    notifySuccess('该邮箱已换取 refreshToken, 无需进行委托授权')
    return
  }


  notifyWarning("检测到个人 Outlook 邮箱，需要进行委托授权，请批准")

  const { data: authorizationUrl } = await startOutlookDelegateAuthorization(outbox.id as number)
  if (!authorizationUrl) {
    notifyError("获取委托授权地址失败，请稍后重试")
    return
  }

  const win = window.open(
    authorizationUrl,
    'outlook-auth',
    'width=600,height=700,scrollbars=yes,resizable=yes'
  )

  if (!win) {
    notifyError('请允许浏览器弹出窗口')
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

  notifySuccess('委托授权结束')
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
      title: `新增发件箱 / ${emailGroup.value.label}`,
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

    notifySuccess('新增发件箱成功')

    // 进行 outlook 委托授权
    await tryOutlookDelegateAuthorization(outbox, userInfoStore.userEncryptKeys)
  }

  // 导出模板
  async function onExportOutboxTemplateClick () {
    const data: any[] = [
      {
        email: '填写发件邮箱(导入时，请删除该行数据)',
        name: '填写发件人名称(可选)',
        userName: '填写 smtp 用户名，若与邮箱一致，则设置不填写',
        password: '填写 smtp 密码',
        smtpHost: '填写 smtp 地址',
        smtpPort: 25,
        description: '描述(可选)',
        proxy: '格式为：http://username:password@domain:port(可选)',
        replyToEmails: '回信收件人(多个使用逗号分隔)',
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
      fileName: '发件箱模板.xlsx',
      sheetName: '发件箱',
      mappers: getOutboxExcelDataMapper()
    })

    notifySuccess('模板下载成功')
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
      notifyError('未找到可导入的数据')
      return
    }

    // 对密码进行加密
    for (const row of data) {
      // 验证邮箱是否正确
      // 验证 email 格式
      if (!isEmail(row.email)) {
        notifyError(`邮箱格式错误: ${row.email}`)
        return
      }

      row.password = encryptPassword(userInfoStore.smtpPasswordSecretKeys, row.password)
      row.emailGroupId = emailGroupId || emailGroup.value.id
    }

    // 向服务器请求新增
    const { data: outboxes } = await createOutboxes(data as IOutbox[])

    if (emailGroupId === emailGroup.value.id) {
      outboxes.forEach(x => {
        addNewRow(x)
      })
    }

    notifySuccess('导入成功')
  }

  return {
    onNewOutboxClick,
    onExportOutboxTemplateClick,
    onImportOutboxFromExcelClicked
  }
}
