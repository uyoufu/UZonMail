/* eslint-disable @typescript-eslint/no-explicit-any */
import type { IProxy } from 'src/api/proxy';
import { validateProxyName, createProxy, updateProxySharedStatus } from 'src/api/proxy'
import { showDialog } from 'src/components/popupDialog/PopupDialog'
import type { IPopupDialogField, IPopupDialogParams } from 'src/components/popupDialog/types';
import { PopupDialogFieldType } from 'src/components/popupDialog/types'
import { useUserInfoStore } from 'src/stores/user'
import { notifySuccess } from 'src/utils/dialog'

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

export function getCommonProxyFields (): IPopupDialogField[] {
  return [
    {
      name: 'name',
      type: PopupDialogFieldType.text,
      label: '名称',
      placeholder: '代理的唯一标识，需要保证唯一',
      value: '',
      required: true
    },
    {
      name: 'url',
      type: PopupDialogFieldType.text,
      label: '代理地址',
      placeholder: '格式：schema://username:password@host',
      tooltip: ['代理格式:', 'schema://username:password@host', '支持的协议: http, https, socks5, socks4'],
      value: '',
      required: true,
      // eslint-disable-next-line @typescript-eslint/require-await
      validate: async (value: any) => {
        if (!value) {
          return {
            ok: false,
            message: '代理地址不能为空'
          }
        }

        // 若不包含 ://, 说明没有协议，返回错误
        if (!value.includes('://')) {
          return {
            ok: false,
            message: '代理地址缺失协议,格式为：schema://username:password@host 或 host'
          }
        }

        if (!URL.canParse(value)) {
          return {
            ok: false,
            message: '代理地址格式不正确,格式为：schema://username:password@host 或 schema://host'
          }
        }
        return {
          ok: true
        }
      }
    },
    {
      name: 'matchRegex',
      label: '匹配规则',
      type: PopupDialogFieldType.text,
      placeholder: '使用正则表达式进行匹配',
      value: '.*'
    },
    {
      name: 'priority',
      label: '优先级',
      type: PopupDialogFieldType.number,
      placeholder: '数字越大优先级越高',
      value: 0
    },
    {
      name: 'description',
      label: '描述'
    }
  ]
}

/**
 * 顶部功能区
 * @returns
 */
export function useHeaderFunctions (addNewRow: (newRow: Record<string, any>) => void) {
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
        type: PopupDialogFieldType.boolean,
        label: '是否共享',
        tooltip: '共享后,其它用户可以使用该代理',
        value: false
      })
    }

    // 打开弹窗
    // 新增发件箱
    const popupParams: IPopupDialogParams = {
      title: '新增代理',
      fields,
      validate: validateProxyInfo
    }

    // 弹出对话框

    const { ok, data } = await showDialog<IProxy>(popupParams)
    if (!ok) return

    const { data: newRowData } = await createProxy(data)
    addNewRow(newRowData)
    // 向服务器请求数据
    notifySuccess('创建成功')
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
