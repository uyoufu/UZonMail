/* eslint-disable @typescript-eslint/no-explicit-any */
import type { IProxy } from 'src/api/proxy';
import { validateProxyName, createProxy, updateProxySharedStatus } from 'src/api/proxy'
import { showDialog } from 'src/components/lowCode/PopupDialog'
import type { ILowCodeField, IPopupDialogParams } from 'src/components/lowCode/types';
import { LowCodeFieldType } from 'src/components/lowCode/types'
import { useUserInfoStore } from 'src/stores/user'
import { notifySuccess } from 'src/utils/dialog'
import type { addNewRowType } from 'src/compositions/qTableUtils'
import { t } from 'src/i18n/helpers'

// TODO: 不支持动态代理提示

/**
 * 代替 URL.canParse 方法
 * @param urlStr
 * @returns
 */
function canParseUrl (urlStr: string) {
  try {
    const url = new URL(urlStr)
    return !!url
  } catch {
    return false
  }
}
if (!URL.canParse) URL.canParse = canParseUrl

export function getCommonProxyFields (): ILowCodeField[] {
  return [
    {
      name: 'name',
      type: LowCodeFieldType.text,
      label: t('global.name'),
      placeholder: t('pages.proxy.namePlaceholder'),
      value: '',
      required: true
    },
    {
      name: 'url',
      type: LowCodeFieldType.text,
      label: t('pages.proxy.url'),
      placeholder: t('pages.proxy.urlPlaceholder'),
      tooltip: [t('pages.proxy.formatTitle'), 'schema://username:password@host', t('pages.proxy.supportedProtocols')],
      value: '',
      required: true,
      // eslint-disable-next-line @typescript-eslint/require-await
      validate: async (value: any) => {
        if (!value) {
          return {
            ok: false,
            message: t('pages.proxy.urlRequired')
          }
        }

        // 若不包含 ://, 说明没有协议，返回错误
        if (!value.includes('://')) {
          return {
            ok: false,
            message: t('pages.proxy.protocolMissing')
          }
        }

        if (!URL.canParse(value)) {
          return {
            ok: false,
            message: t('pages.proxy.urlInvalid')
          }
        }
        return {
          ok: true
        }
      }
    },
    {
      name: 'matchRegex',
      label: t('pages.proxy.matchRule'),
      type: LowCodeFieldType.text,
      placeholder: t('pages.proxy.matchPlaceholder'),
      value: '.*'
    },
    {
      name: 'priority',
      label: t('pages.proxy.priority'),
      type: LowCodeFieldType.number,
      placeholder: t('pages.proxy.priorityPlaceholder'),
      value: 0
    },
    {
      name: 'description',
      label: t('global.description')
    }
  ]
}

/**
 * 顶部功能区
 * @returns
 */
export function useHeaderFunctions (addNewRow: addNewRowType<IProxy>) {
  const userInfo = useUserInfoStore()


  async function validateProxyInfo (data: Record<string, any>) {
    return await validateProxyName(data.name)
  }

  async function onCreateProxy () {
    const fields = getCommonProxyFields()
    // 若是管理员，则添加共享字段
    if (userInfo.isAdmin) {
      fields.push({
        name: 'isShared',
        type: LowCodeFieldType.boolean,
        label: t('pages.proxy.isShared'),
        tooltip: t('pages.proxy.sharedHint'),
        value: false
      })
    }

    // 打开弹窗
    // 新增发件箱
    const popupParams: IPopupDialogParams = {
      title: t('pages.proxy.createTitle'),
      fields,
      validate: validateProxyInfo
    }

    // 弹出对话框

    const { ok, data } = await showDialog<IProxy>(popupParams)
    if (!ok) return

    const { data: newRowData } = await createProxy(data)
    addNewRow(newRowData)
    // 向服务器请求数据
    notifySuccess(t('pages.apiAccess.created'))
  }

  // 开关代理共享
  async function onToggleShareProxy (proxyInfo: IProxy) {
    if (userInfo.userSqlId !== proxyInfo.userId) {
      return
    }

    // 向服务器请求更新
    await updateProxySharedStatus(proxyInfo.id as number, !!proxyInfo.isShared)
  }

  return { onCreateProxy, onToggleShareProxy }
}
