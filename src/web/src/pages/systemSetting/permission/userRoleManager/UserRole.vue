<template>
  <q-table class="full-height" :rows="rows" :columns="columns" row-key="id" virtual-scroll
    v-model:pagination="pagination" dense :loading="loading" :filter="filter" binary-state-sort
    @request="onTableRequest">
    <template v-slot:top-left>
      <CreateBtn @click="onCreateUserRole" />
    </template>

    <template v-slot:top-right>
      <SearchInput v-model="filter" />
    </template>

    <template v-slot:body-cell-index="props">
      <QTableIndex :props="props" />

      <ContextMenu :items="contextItems" :value="props.row"></ContextMenu>
    </template>

    <template v-slot:body-cell-userId="props">
      <q-td :props="props">
        {{ props.value }}
      </q-td>
    </template>
  </q-table>
</template>

<script lang="ts" setup>
import type { QTableColumn } from 'quasar'

import { useQTable, useQTableIndex } from 'src/compositions/qTableUtils'
import type { IRequestPagination, TTableFilterObject } from 'src/compositions/types'
import SearchInput from 'src/components/searchInput/SearchInput.vue'
import type { IPopupDialogParams} from 'src/components/lowCode/types';
import { LowCodeFieldType } from 'src/components/lowCode/types'

import type { IRole, IUserRole} from 'src/api/permission';
import { getAllRoles, getUserRolesCount, getUserRolesData, upsertUserRole, deleteUserRoles } from 'src/api/permission'
import { getAllUsers } from 'src/api/user'
import type { IContextMenuItem } from 'src/components/contextMenu/types'
import ContextMenu from 'src/components/contextMenu/ContextMenu.vue'

import type { IUserInfo } from 'src/stores/types'

import { confirmOperation, notifyError, notifySuccess, showDialog } from 'src/utils/dialog'
import { formatDate } from 'src/utils/format'
import { t } from 'src/i18n/helpers'

const { indexColumn, QTableIndex } = useQTableIndex()
const columns = computed<QTableColumn[]>(() => [
  indexColumn,
  {
    name: 'userId',
    required: true,
    label: t('pages.permissionManager.userName'),
    align: 'left',
    field: v => v.user.userName,
    sortable: true
  },
  {
    name: 'roles',
    required: true,
    label: t('pages.permissionManager.role'),
    align: 'left',
    field: 'roles',
    format: roles => roles.map((x: IRole) => x.name).join(),
    sortable: false
  },
  {
    name: 'createDate',
    required: false,
    label: t('pages.permissionManager.createdAt'),
    align: 'left',
    field: 'createDate',
    format: formatDate,
    sortable: true
  }
])

async function getRowsNumberCount (filterObj: TTableFilterObject) {
  const { data } = await getUserRolesCount(filterObj.filter)
  return data || 0
}

async function onRequest (filterObj: TTableFilterObject, pagination: IRequestPagination) {
  const { data } = await getUserRolesData(filterObj.filter, pagination)
  return data || []
}

const { pagination, rows, filter, onTableRequest, loading, addNewRow, deleteRowById } = useQTable({
  getRowsNumberCount,

  onRequest
})

// #region 新增角色
async function onCreateUserRole () {
  const dialogParams = await getPopupDialogParams()
  const result = await showDialog(dialogParams)
  if (!result.ok) return

  // 向服务器请求角色添加
  const { data } = await upsertUserRole(result.data as IUserRole)
  addNewRow(data)

  notifySuccess(t('pages.permissionManager.addSuccess'))
}

const users: Ref<IUserInfo[]> = ref([])
const roles: Ref<IRole[]> = ref([])
async function getPopupDialogParams (userRole?: IUserRole) {
  if (!users.value.length) {
    // 获取所有的用户
    const { data } = await getAllUsers()
    users.value = data
  }
  if (!roles.value.length) {
    const { data } = await getAllRoles()
    if (data.length === 0) {
      notifyError(t('pages.permissionManager.addRoleFirst'))
      throw new Error(t('pages.permissionManager.addRoleFirst'))
    }
    roles.value = data
  }

  const dialogParams: IPopupDialogParams = {
    title: userRole ? t('pages.permissionManager.editUserRole', { userId: userRole.userId }) : t('pages.permissionManager.createUserRole'),
    oneColumn: true,
    fields: [{
      name: 'userId',
      label: t('pages.permissionManager.userName'),
      type: LowCodeFieldType.selectOne,
      required: true,
      value: userRole?.userId || '',
      options: users.value,
      optionLabel: 'userId',
      optionValue: 'id',
      mapOptions: true,
      emitValue: true,
      disable: !!userRole
    }, {
      name: 'roles',
      label: t('pages.permissionManager.role'),
      type: LowCodeFieldType.selectMany,
      required: true,
      options: roles.value,
      optionLabel: 'name',
      optionValue: 'id',
      optionTooltip: 'description',
      value: userRole?.roles || []
    }]
  }
  return dialogParams
}
// #endregion

// #region 右键菜单
const contextItems = computed<IContextMenuItem<IUserRole>[]>(() => [
  {
    name: 'edit',
    label: t('pages.variableManager.edit'),
    onClick: onUserRoleClicked
  },
  {
    name: 'delete',
    label: t('pages.variableManager.delete'),
    color: 'negative',
    onClick: onDeleteUserRole
  }
])
async function onUserRoleClicked (userRole: IUserRole) {
  const dialogParams = await getPopupDialogParams(userRole)
  const result = await showDialog(dialogParams)
  if (!result.ok) return

  // 添加 id
  result.data.id = userRole.id
  await upsertUserRole(result.data as IUserRole)
  const newData = Object.assign(userRole, result.data)
  addNewRow(newData)

  notifySuccess(t('pages.permissionManager.userRoleUpdated'))
}

async function onDeleteUserRole (userRole: IUserRole) {
  const confirm = await confirmOperation(
    t('pages.permissionManager.deleteRole'),
    t('pages.permissionManager.deleteRoleAssignment', {
      userId: userRole.user.userId,
      roles: userRole.roles.map(x => x.name).join()
    })
  )
  if (!confirm) return

  await deleteUserRoles(userRole.id)

  deleteRowById(userRole.id)
  notifySuccess(t('pages.variableManager.deleteSuccess'))
}
// #endregion
</script>

<style lang="scss" scoped></style>
