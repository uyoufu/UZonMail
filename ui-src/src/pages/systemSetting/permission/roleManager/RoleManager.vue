<template>
  <q-table class="full-height" :rows="rows" :columns="columns" row-key="id" virtual-scroll
    v-model:pagination="pagination" dense :loading="loading" :filter="filter" binary-state-sort
    @request="onTableRequest">
    <template v-slot:top-left>
      <CreateBtn @click="onCreateRole" />
    </template>

    <template v-slot:top-right>
      <SearchInput v-model="filter" />
    </template>

    <template v-slot:body-cell-index="props">
      <QTableIndex :props="props" />
      <ContextMenu :items="contextItems" :value="props.row"></ContextMenu>
    </template>
  </q-table>
</template>

<script lang="ts" setup>
import type { QTableColumn } from 'quasar'
import { useQTable, useQTableIndex } from 'src/compositions/qTableUtils'
import type { IRequestPagination, TTableFilterObject } from 'src/compositions/types'
import SearchInput from 'src/components/searchInput/SearchInput.vue'
import dayjs from 'dayjs'

const { indexColumn, QTableIndex } = useQTableIndex()
const columns: QTableColumn[] = [
  indexColumn,
  // {
  //   name: 'icon',
  //   required: true,
  //   label: '图标',
  //   align: 'left',
  //   field: 'icon',
  //   sortable: true
  // },
  {
    name: 'name',
    required: true,
    label: '名称',
    align: 'left',
    field: 'name',
    sortable: true
  },
  {
    name: 'description',
    required: true,
    label: '描述',
    align: 'left',
    field: 'description',
    sortable: true
  },
  {
    name: 'functionsCount',
    label: '功能数',
    align: 'left',
    field: 'permissionCodeIds',
    format: v => v ? v.length : 0
  },
  {
    name: 'createDate',
    required: false,
    label: '创建日期',
    align: 'left',
    field: 'createDate',
    format: (val: string) => {
      return val ? dayjs(val).format('YYYY-MM-DD HH:mm:ss') : ''
    },
    sortable: true
  }
]

// function formatColValue (col: any, row: any) {
//   if (typeof col.format === 'function') {
//     return col.format(row[col.field])
//   }

//   return row[col.field]
// }

import type { IPermissionCode, IRole } from 'src/api/permission';
import { getRolesCount, getRolesData, upsertRole, getAllPermissionCodes, deleteRole } from 'src/api/permission'

async function getRowsNumberCount (filterObj: TTableFilterObject) {
  const { data } = await getRolesCount(filterObj.filter)
  return data || 0
}

async function onRequest (filterObj: TTableFilterObject, pagination: IRequestPagination) {
  const { data } = await getRolesData(filterObj.filter, pagination)
  return data || []
}

const { pagination, rows, filter, onTableRequest, loading, addNewRow, deleteRowById } = useQTable({
  getRowsNumberCount,

  onRequest
})

// #region 新增
import { showDialog } from 'src/components/popupDialog/PopupDialog'
import type { IPopupDialogParams } from 'src/components/popupDialog/types';
import { PopupDialogFieldType } from 'src/components/popupDialog/types'
import { confirmOperation, notifySuccess } from 'src/utils/dialog'
async function onCreateRole () {
  // 创建新建弹窗
  const dialogParams = await getPopupDialogParams()
  const result = await showDialog(dialogParams)
  if (!result.ok) return

  const { data } = await upsertRole(result.data as IRole)
  addNewRow(data)
  notifySuccess('添加角色成功')
}

const permissionCodes: Ref<IPermissionCode[]> = ref([])
async function getPopupDialogParams (roleData?: IRole) {
  if (permissionCodes.value.length === 0) {
    // 从服务器获取
    const { data } = await getAllPermissionCodes()
    permissionCodes.value = data
  }

  const dialogParams: IPopupDialogParams = {
    title: roleData ? `修改角色 ${roleData.name}` : '新增角色',
    oneColumn: true,
    fields: [
      {
        name: 'name',
        label: '名称',
        required: true,
        value: roleData ? roleData.name : ''
      },
      // {
      //   name: 'icon',
      //   label: '图标',
      //   type: PopupDialogFieldType.text,
      //   required: true,
      //   value: roleData ? roleData.icon : 'supervised_user_circle',
      //   placeholder: '图标名称, 请从 https://fonts.google.com/icons 中选择'
      // },
      {
        name: 'description',
        label: '描述',
        type: PopupDialogFieldType.textarea,
        required: false,
        value: roleData ? roleData.description : ''
      },
      {
        name: 'permissionCodeIds',
        label: '权限码',
        type: PopupDialogFieldType.selectMany,
        options: permissionCodes.value,
        optionLabel: 'code',
        optionTooltip: 'description',
        optionValue: 'id',
        emitValue: true,
        mapOptions: true,
        required: true,
        value: roleData ? roleData.permissionCodeIds : []
      }
    ]
  }

  return dialogParams
}
// #endregion

// #region 右键菜单
import ContextMenu from 'src/components/contextMenu/ContextMenu.vue'
import type { IContextMenuItem } from 'src/components/contextMenu/types'
const contextItems: IContextMenuItem[] = [
  {
    name: 'edit',
    label: '编辑',
    tooltip: '编辑角色',
    onClick: onEditRole
  },
  {
    name: 'delete',
    label: '删除',
    tooltip: '删除角色',
    onClick: onDeleteRole,
    color: 'negative'
  }
]
// eslint-disable-next-line @typescript-eslint/no-explicit-any
async function onEditRole (data: Record<string, any>) {
  const roleData = data as IRole
  const dialogParams = await getPopupDialogParams(roleData)
  const result = await showDialog(dialogParams)
  if (!result.ok) return

  // 添加 id
  result.data.id = data.id
  const { data: newRole } = await upsertRole(result.data as IRole)
  addNewRow(newRole)
  notifySuccess('修改角色成功')
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
async function onDeleteRole (data: Record<string, any>) {
  const confirm = await confirmOperation('删除确认', `确认删除角色: ${data.name} 吗？`)
  if (!confirm) return

  // 删除角色
  await deleteRole(data.id)
  deleteRowById(data.id)

  notifySuccess(`角色 ${data.name} 删除成功`)
}
// #endregion
</script>

<style lang="scss" scoped></style>
