/* eslint-disable @typescript-eslint/no-explicit-any */
import { checkUserId, createUser, getDefaultPassword, resetUserPassword, setUserType, setUserStatus } from 'src/api/user'
import type { IContextMenuItem } from 'src/components/contextMenu/types'
import { showDialog } from 'src/components/popupDialog/PopupDialog'
import { PopupDialogFieldType } from 'src/components/popupDialog/types'
import { UserStatus, UserType } from 'src/stores/types'
import { confirmOperation, notifySuccess } from 'src/utils/dialog'
import { usePermission } from 'src/compositions/permission'

export function useContextMenu (addNewRow: (newRow: Record<string, any>) => void) {
  const { hasEnterpriseAccess } = usePermission()
  // 右键菜单
  const userManageContextItems: IContextMenuItem[] = [
    {
      name: 'addUser',
      label: '新增',
      tooltip: '新增用户',
      onClick: onNewUserClick
    },
    {
      name: 'resetPassword',
      label: '重置密码',
      tooltip: '重置用户密码',
      onClick: onResetUserPassword
    },
    {
      name: 'forbidden',
      label: '禁用',
      tooltip: '禁用后, 用户将无法登录',
      color: 'negative',
      vif: v => {
        console.log('forbidden', v, v.status, UserStatus.forbiddenLogin, v.status !== UserStatus.forbiddenLogin)
        return v.status !== UserStatus.forbiddenLogin
      },
      onClick: onForbiddenLogin
    },
    {
      name: 'cancelForbidden',
      label: '启用',
      tooltip: '取消用户的禁用状态',
      color: 'negative',
      vif: v => v.status === UserStatus.forbiddenLogin,
      onClick: onCancelForbidden
    },
    {
      name: 'setAsSubUser',
      label: '设为子账户',
      tooltip: '设为子账户后，可以统一管理子账户的设置和查看账户的一些发送数据',
      vif: v => v.type !== UserType.subUser && hasEnterpriseAccess(),
      onClick: onSetAsSubUser
    },
    {
      name: 'setAsNormalUser',
      label: '取消子账户',
      tooltip: '取消子账户，用户将变成独立账户，不受主账户管理',
      vif: v => v.type === UserType.subUser,
      onClick: onSetAsIndependentUser
    }
  ]

  async function onResetUserPassword (userInfo: Record<string, any>) {
    // 获取默认密码
    const { data: defaultPassword } = await getDefaultPassword()

    const confirm = await confirmOperation('重置密码', `密码即将重置为 ${defaultPassword}, 是否继续?`)
    if (!confirm) return false

    // 开始重置
    await resetUserPassword(userInfo.userId)

    notifySuccess('重置密码成功')
  }

  async function onForbiddenLogin (userInfo: Record<string, any>) {
    const confirm = await confirmOperation('禁用用户', `是否禁用用户 ${userInfo.userId} ? 禁用后，该用户将无法登录，但现有发件任务不会中断`)
    if (!confirm) return false

    await setUserStatus(userInfo.id, UserStatus.forbiddenLogin)

    // 更新用户的状态
    userInfo.status = UserStatus.forbiddenLogin
    notifySuccess('禁用成功')
  }

  async function onCancelForbidden (userInfo: Record<string, any>) {
    const confirm = await confirmOperation('启用用户', `是否启用用户 ${userInfo.userId} ? 启用后，该用户将可以正常登录`)
    if (!confirm) return false

    await setUserStatus(userInfo.id, UserStatus.normal)

    // 更新用户的状态
    userInfo.status = UserStatus.normal
    notifySuccess('启用成功')
  }

  async function onSetAsSubUser (userInfo: Record<string, any>) {
    const confirm = await confirmOperation('操作确认', `是否将用户 ${userInfo.userId} 设为子账户? `)
    if (!confirm) return false

    await setUserType(userInfo.id, UserType.subUser)

    // 更新用户的状态
    userInfo.type = UserType.subUser
    notifySuccess('设置成功')
  }

  async function onSetAsIndependentUser (userInfo: Record<string, any>) {
    const confirm = await confirmOperation('操作确认', `是否取消子账户 ${userInfo.userId}? `)
    if (!confirm) return false

    await setUserType(userInfo.id, UserType.independent)

    // 更新用户的状态
    userInfo.type = UserType.independent
    notifySuccess('设置成功')
  }

  // 新增用户
  async function onNewUserClick () {
    const dialogResult = await showDialog({
      title: '新增用户',
      fields: [
        {
          name: 'userId',
          label: '用户名',
          type: PopupDialogFieldType.text,
          required: true,
          placeholder: '请输入用户名,请仅用英文字母',
          // eslint-disable-next-line @typescript-eslint/require-await
          validate: async (value) => {
            return {
              ok: value && value.length >= 3,
              message: '用户名必须大小等于 3 个字符'
            }
          }
        },
        {
          name: 'password',
          label: '初始密码',
          type: PopupDialogFieldType.text,
          required: true,
          placeholder: '请输入初始密码',
          // eslint-disable-next-line @typescript-eslint/require-await
          validate: async (value) => {
            return {
              ok: value && value.length >= 6,
              message: '密码必须大小等于 6 个字符'
            }
          }
        }
      ],
      validate: async (fieldsModel) => {
        console.log('validate', fieldsModel.userId)
        // 验证用户名是否重复
        const { data } = await checkUserId(fieldsModel.userId)
        return {
          ok: data,
          message: `用户名：${fieldsModel.userId}已存在`
        }
      }
    })
    console.log('onNewUserClick', dialogResult)
    if (!dialogResult.ok) return

    // 新增用户
    const { data: modelValue } = dialogResult
    const { data: newUser } = await createUser(modelValue.userId as string, modelValue.password as string)
    notifySuccess('新增用户成功')
    addNewRow(newUser)
  }

  return { onNewUserClick, userManageContextItems }
}
