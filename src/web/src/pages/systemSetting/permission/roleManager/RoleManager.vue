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
import { t } from 'src/i18n/helpers'

const { indexColumn, QTableIndex } = useQTableIndex()
const columns = computed<QTableColumn[]>(() => [
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
    label: t('global.name'),
    align: 'left',
    field: 'name',
    sortable: true
  },
  {
    name: 'description',
    required: true,
    label: t('global.description'),
    align: 'left',
    field: 'description',
    sortable: true
  },
  {
    name: 'functionsCount',
    label: t('pages.permissionManager.functionCount'),
    align: 'left',
    field: 'permissionCodeIds',
    format: v => v ? v.length : 0
  },
  {
    name: 'createDate',
    required: false,
    label: t('pages.permissionManager.createdAt'),
    align: 'left',
    field: 'createDate',
    format: (val: string) => {
      return val ? dayjs(val).format('YYYY-MM-DD HH:mm:ss') : ''
    },
    sortable: true
  }
])

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
import { showDialog } from 'src/components/lowCode/PopupDialog'
import type { IPopupDialogParams } from 'src/components/lowCode/types';
import { LowCodeFieldType } from 'src/components/lowCode/types'
import { confirmOperation, notifySuccess } from 'src/utils/dialog'
async function onCreateRole () {
  // 创建新建弹窗
  const dialogParams = await getPopupDialogParams()
  const result = await showDialog(dialogParams)
  if (!result.ok) return

  const { data } = await upsertRole(result.data as IRole)
  addNewRow(data)
  notifySuccess(t('pages.permissionManager.roleCreated'))
}

const permissionCodes: Ref<IPermissionCode[]> = ref([])
async function getPopupDialogParams (roleData?: IRole) {
  if (permissionCodes.value.length === 0) {
    // 从服务器获取
    const { data } = await getAllPermissionCodes()
    permissionCodes.value = data
  }

  const dialogParams: IPopupDialogParams = {
    title: roleData ? t('pages.permissionManager.editRoleTitle', { name: roleData.name }) : t('pages.permissionManager.createRole'),
    oneColumn: true,
    fields: [
      {
        name: 'name',
        label: t('global.name'),
        required: true,
        value: roleData ? roleData.name : ''
      },
      // {
      //   name: 'icon',
      //   label: '图标',
      //   type: LowCodeFieldType.text,
      //   required: true,
      //   value: roleData ? roleData.icon : 'supervised_user_circle',
      //   placeholder: '图标名称, 请从 https://fonts.google.com/icons 中选择'
      // },
      {
        name: 'description',
        label: t('global.description'),
        type: LowCodeFieldType.textarea,
        required: false,
        value: roleData ? roleData.description : ''
      },
      {
        name: 'permissionCodeIds',
        label: t('pages.permissionManager.permissionCodes'),
        type: LowCodeFieldType.selectMany,
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
import { ContextMenuIcon, type IContextMenuItem } from 'src/components/contextMenu/types'
const contextItems = computed<IContextMenuItem[]>(() => [
  {
    name: 'edit',
    label: t('pages.variableManager.edit'),
    tooltip: t('pages.permissionManager.editRole'),
    icon: ContextMenuIcon.edit,
    onClick: onEditRole
  },
  {
    name: 'delete',
    label: t('pages.variableManager.delete'),
    tooltip: t('pages.permissionManager.deleteRole'),
    onClick: onDeleteRole,
    color: 'negative',
    icon: ContextMenuIcon.delete
  }
])
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
  notifySuccess(t('pages.permissionManager.roleUpdated'))
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
async function onDeleteRole (data: Record<string, any>) {
  const confirm = await confirmOperation(
    t('pages.templateManager.deleteConfirmation'),
    t('pages.permissionManager.deleteRoleConfirm', { name: String(data.name) })
  )
  if (!confirm) return

  // 删除角色
  await deleteRole(data.id)
  deleteRowById(data.id)

  notifySuccess(t('pages.permissionManager.roleDeleted', { name: String(data.name) }))
}
// #endregion
</script>

<style lang="scss" scoped></style>
