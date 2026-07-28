/* eslint-disable @typescript-eslint/no-explicit-any */
import { checkUserId, createUser, getDefaultPassword, resetUserPassword, setUserType, setUserStatus } from 'src/api/user'
import { ContextMenuIcon, type IContextMenuItem } from 'src/components/contextMenu/types'
import { showDialog } from 'src/components/lowCode/PopupDialog'
import { LowCodeFieldType } from 'src/components/lowCode/types'
import { UserStatus, UserType } from 'src/stores/types'
import { confirmOperation, notifySuccess } from 'src/utils/dialog'
import { usePermission } from 'src/compositions/permission'
import type { addNewRowType } from 'src/compositions/qTableUtils'
import type { IUserInfo } from 'src/stores/types'
import { t } from 'src/i18n/helpers'

export function useContextMenu (addNewRow: addNewRowType<IUserInfo>) {
  const { hasEnterpriseAccess } = usePermission()
  // 右键菜单
  const userManageContextItems = computed<IContextMenuItem<IUserInfo>[]>(() => [
    {
      name: 'addUser',
      label: t('pages.userManager.newUser'),
      tooltip: t('pages.userManager.createUserTooltip'),
      icon: ContextMenuIcon.personAdd,
      onClick: onNewUserClick
    },
    {
      name: 'resetPassword',
      label: t('pages.userManager.resetPassword'),
      tooltip: t('pages.userManager.resetUserPassword'),
      icon: ContextMenuIcon.lockReset,
      onClick: onResetUserPassword
    },
    {
      name: 'forbidden',
      label: t('pages.userManager.disable'),
      tooltip: t('pages.userManager.disableHint'),
      color: 'negative',
      icon: ContextMenuIcon.block,
      vif: v => {
        console.log('forbidden', v, v.status, UserStatus.forbiddenLogin, v.status !== UserStatus.forbiddenLogin)
        return v.status !== UserStatus.forbiddenLogin
      },
      onClick: onForbiddenLogin
    },
    {
      name: 'cancelForbidden',
      label: t('pages.userManager.enable'),
      tooltip: t('pages.userManager.enableHint'),
      color: 'negative',
      icon: ContextMenuIcon.checkCircle,
      vif: v => v.status === UserStatus.forbiddenLogin,
      onClick: onCancelForbidden
    },
    {
      name: 'setAsSubUser',
      label: '设为子账户',
      tooltip: '设为子账户后，可以统一管理子账户的设置和查看账户的一些发送数据',
      icon: ContextMenuIcon.groupAdd,
      vif: v => v.type !== UserType.subUser && hasEnterpriseAccess(),
      onClick: onSetAsSubUser
    },
    {
      name: 'setAsNormalUser',
      label: '取消子账户',
      tooltip: '取消子账户，用户将变成独立账户，不受主账户管理',
      icon: ContextMenuIcon.groupRemove,
      vif: v => v.type === UserType.subUser,
      onClick: onSetAsIndependentUser
    }
  ])

  async function onResetUserPassword (userInfo: Record<string, any>) {
    // 获取默认密码
    const { data: defaultPassword } = await getDefaultPassword()

    const confirm = await confirmOperation(
      t('pages.userManager.resetPassword'),
      t('pages.userManager.resetConfirmation', { password: defaultPassword })
    )
    if (!confirm) return false

    // 开始重置
    await resetUserPassword(userInfo.userId)

    notifySuccess(t('pages.userManager.resetSuccess'))
  }

  async function onForbiddenLogin (userInfo: Record<string, any>) {
    const confirm = await confirmOperation(
      t('pages.userManager.disableUser'),
      t('pages.userManager.disableConfirmation', { userId: userInfo.userId })
    )
    if (!confirm) return false

    await setUserStatus(userInfo.id, UserStatus.forbiddenLogin)

    // 更新用户的状态
    userInfo.status = UserStatus.forbiddenLogin
    notifySuccess(t('pages.userManager.disableSuccess'))
  }

  async function onCancelForbidden (userInfo: Record<string, any>) {
    const confirm = await confirmOperation(
      t('pages.userManager.enableUser'),
      t('pages.userManager.enableConfirmation', { userId: userInfo.userId })
    )
    if (!confirm) return false

    await setUserStatus(userInfo.id, UserStatus.normal)

    // 更新用户的状态
    userInfo.status = UserStatus.normal
    notifySuccess(t('pages.userManager.enableSuccess'))
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
      title: t('pages.userManager.newUser'),
      fields: [
        {
          name: 'userId',
          label: t('pages.userManager.userName'),
          type: LowCodeFieldType.text,
          required: true,
          placeholder: t('pages.userManager.enterUserName'),
          // eslint-disable-next-line @typescript-eslint/require-await
          validate: async (value) => {
            return {
              ok: value && value.length >= 3,
              message: t('pages.userManager.userNameLength')
            }
          }
        },
        {
          name: 'password',
          label: t('pages.userManager.initialPassword'),
          type: LowCodeFieldType.text,
          required: true,
          placeholder: t('pages.userManager.enterInitialPassword'),
          // eslint-disable-next-line @typescript-eslint/require-await
          validate: async (value) => {
            return {
              ok: value && value.length >= 6,
              message: t('pages.userManager.passwordLength')
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
          message: t('pages.userManager.userNameExists', { userId: String(fieldsModel.userId) })
        }
      }
    })
    console.log('onNewUserClick', dialogResult)
    if (!dialogResult.ok) return

    // 新增用户
    const { data: modelValue } = dialogResult
    const { data: newUser } = await createUser(modelValue.userId as string, modelValue.password as string)
    notifySuccess(t('pages.userManager.createSuccess'))
    addNewRow(newUser)
  }

  return { onNewUserClick, userManageContextItems }
}
