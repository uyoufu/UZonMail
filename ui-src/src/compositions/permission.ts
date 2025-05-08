import { useUserInfoStore } from 'src/stores/user'

/**
 * 权限控制
 */
export function usePermission () {
  const store = useUserInfoStore()

  // 是否是管理员
  const isSuperAdmin = computed(() => {
    return hasPermission('*')
  })

  const isOrganizationAdmin = computed(() => {
    return hasPermission("organizationAdmin")
  })

  const hasPermission = store.hasPermission.bind(store)
  const hasPermissionOr = store.hasPermissionOr.bind(store)

  /**
   * 是否有专业版权限
   * @returns
   */
  function hasProfessionAccess () {
    if (!store.hasProPlugin) return false
    if (hasEnterpriseAccess()) return true
    return hasPermissionOr(['professional'])
  }

  /**
   * 是否有企业版权限
   * @returns
   */
  function hasEnterpriseAccess () {
    return hasPermission('enterprise')
  }

  return {
    hasPermission,
    hasPermissionOr,
    isSuperAdmin,
    isOrganizationAdmin,
    hasProfessionAccess,
    hasEnterpriseAccess
  }
}
