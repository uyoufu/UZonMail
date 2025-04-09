/* eslint-disable @typescript-eslint/no-explicit-any */
import { showDialog } from 'src/components/popupDialog/PopupDialog'
import type { IOnSetupParams, IPopupDialogField, IPopupDialogParams } from 'src/components/popupDialog/types'
import { PopupDialogFieldType } from 'src/components/popupDialog/types'
import type { IEmailGroupListItem } from '../components/types'

import type { IOutbox } from 'src/api/emailBox'
import { createOutbox, createOutboxes } from 'src/api/emailBox'
import { GuessSmtpInfoGet } from 'src/api/smtpInfo'

import { notifyError, notifySuccess } from 'src/utils/dialog'
import { isEmail } from 'src/utils/validator'

import { useUserInfoStore } from 'src/stores/user'
import { aes } from 'src/utils/encrypt'
import type { IExcelColumnMapper } from 'src/utils/file'
import { readExcel, writeExcel } from 'src/utils/file'
import type { IProxy } from 'src/api/proxy'
import { getUsableProxies } from 'src/api/proxy'
import { debounce } from 'lodash'

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
      required: true,

      parser: (value: any) => {
        const pwd = String(value)
        // 对密码进行加密
        return encryptPassword(smtpPasswordSecretKeys, pwd)
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
