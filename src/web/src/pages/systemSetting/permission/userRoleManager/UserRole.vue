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
import type { IPopupDialogParams} from 'src/components/popupDialog/types';
import { PopupDialogFieldType } from 'src/components/popupDialog/types'

import type { IRole, IUserRole} from 'src/api/permission';
import { getAllRoles, getUserRolesCount, getUserRolesData, upsertUserRole, deleteUserRoles } from 'src/api/permission'
import { getAllUsers } from 'src/api/user'
import type { IContextMenuItem } from 'src/components/contextMenu/types'
import ContextMenu from 'src/components/contextMenu/ContextMenu.vue'

import type { IUserInfo } from 'src/stores/types'

import { confirmOperation, notifyError, notifySuccess, showDialog } from 'src/utils/dialog'
import { formatDate } from 'src/utils/format'

const { indexColumn, QTableIndex } = useQTableIndex()
const columns: QTableColumn[] = [
  indexColumn,
  {
    name: 'userId',
    required: true,
    label: '用户名',
    align: 'left',
    field: v => v.user.userName,
    sortable: true
  },
  {
    name: 'roles',
    required: true,
    label: '角色',
    align: 'left',
    field: 'roles',
    format: roles => roles.map((x: IRole) => x.name).join(),
    sortable: false
  },
  {
    name: 'createDate',
    required: false,
    label: '创建日期',
    align: 'left',
    field: 'createDate',
    format: formatDate,
    sortable: true
  }
]

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

  notifySuccess('添加成功')
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
      notifyError('请先添加角色')
      throw new Error('请先添加角色')
    }
    roles.value = data
  }

  const dialogParams: IPopupDialogParams = {
    title: userRole ? `编辑用户角色: ${userRole.userId}` : '新增用户角色',
    oneColumn: true,
    fields: [{
      name: 'userId',
      label: '用户名',
      type: PopupDialogFieldType.selectOne,
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
      label: '角色',
      type: PopupDialogFieldType.selectMany,
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
const contextItems: IContextMenuItem[] = [
  {
    name: 'edit',
    label: '编辑',
    onClick: onUserRoleClicked
  },
  {
    name: 'delete',
    label: '删除',
    color: 'negative',
    onClick: onDeleteUserRole
  }
]
// eslint-disable-next-line @typescript-eslint/no-explicit-any
async function onUserRoleClicked (row: Record<string, any>) {
  const userRole = row as IUserRole

  const dialogParams = await getPopupDialogParams(userRole)
  const result = await showDialog(dialogParams)
  if (!result.ok) return

  // 添加 id
  result.data.id = userRole.id
  await upsertUserRole(result.data as IUserRole)
  const newData = Object.assign(row, result.data)
  addNewRow(newData)

  notifySuccess('用户角色更新成功')
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
async function onDeleteUserRole (row: Record<string, any>) {
  const userRole = row as IUserRole
  const confirm = await confirmOperation('删除角色', `即将删除 ${userRole.user.userId} 的 ${userRole.roles.map(x => x.name).join()} 角色，是否继续？`)
  if (!confirm) return

  await deleteUserRoles(userRole.id)

  deleteRowById(userRole.id)
  notifySuccess('删除成功')
}
// #endregion
</script>

<style lang="scss" scoped></style>
