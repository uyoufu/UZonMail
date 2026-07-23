/* eslint-disable @typescript-eslint/no-explicit-any */
import logger from 'loglevel'

import type { IProxy } from 'src/api/proxy';
import { deleteProxy, updateProxy, validateProxyName } from 'src/api/proxy'
import type { IContextMenuItem } from 'src/components/contextMenu/types'
import { confirmOperation, notifySuccess } from 'src/utils/dialog'
import { getCommonProxyFields } from './headerFuncs'
import type { IPopupDialogParams } from 'src/components/popupDialog/types';
import { PopupDialogFieldType } from 'src/components/popupDialog/types'
import { showDialog } from 'src/components/popupDialog/PopupDialog'

import { useUserInfoStore } from 'src/stores/user'

export function useContextMenu (deleteRowById: (id?: number) => void) {
  const proxyContextMenuItems: IContextMenuItem[] = [
    {
      name: 'edit',
      label: '编辑',
      tooltip: '编辑代理',
      icon: 'edit',
      vif: isOwner,
      onClick: onModifyProxy
    },
    {
      name: 'delete',
      label: '删除',
      tooltip: '删除代理',
      color: 'negative',
      icon: 'delete',
      vif: isOwner,
      onClick: onDeleteProxy
    }
  ]

  const userInfo = useUserInfoStore()
  /**
   * 判断是否是拥有者
   * @param proxyInfo
   * @returns
   */
  function isOwner (proxyInfo: Record<string, any>) {
    logger.debug('[proxyManager] isOwner:', userInfo.userSqlId, proxyInfo.userId)
    return userInfo.userSqlId === proxyInfo.userId
  }

  async function onModifyProxy (proxyInfo: Record<string, any>) {
    const proxyData = proxyInfo as IProxy


    async function validateProxyInfo (data: Record<string, any>) {
      if (proxyData.name === data.name) {
        return {
          ok: true
        }
      }

      return await validateProxyName(data.name)
    }

    // 打开修改弹窗
    const fields = getCommonProxyFields()
    // 添加启用功能
    fields.push({
      name: 'isActive',
      type: PopupDialogFieldType.boolean,
      label: '是否启用',
      tooltip: '启用后代理开始生效',
      value: false
    })

    // 更新默认值
    fields.forEach(field => {
      field.value = proxyInfo[field.name]
    })

    // 打开弹窗
    // 新增发件箱
    const popupParams: IPopupDialogParams = {
      title: `修改代理: ${proxyData.name}`,
      fields,
      validate: validateProxyInfo
    }

    // 弹出对话框

    const { ok, data } = await showDialog<IProxy>(popupParams)
    if (!ok) return

    // 向服务器请求修改
    await updateProxy(Object.assign(data, { id: proxyData.id }))

    // 修改本机数据
    Object.assign(proxyData, data)
    notifySuccess('修改成功')
  }

  // 删除代理操作
  async function onDeleteProxy (proxyInfo: Record<string, any>) {
    const proxyData = proxyInfo as IProxy
    if (!proxyData.id) return

    // 进行提示
    const confirm = await confirmOperation('删除代理', `确定删除代理【${proxyData.name}】吗？`)
    if (!confirm) return

    // 向服务器请求删除
    await deleteProxy(proxyData.id)
    deleteRowById(proxyData.id)
  }
  return { proxyContextMenuItems, onModifyProxy }
}
