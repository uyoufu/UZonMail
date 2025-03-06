<template>
  <q-table class="full-height" :rows="rows" :columns="columns" row-key="id" virtual-scroll
    v-model:pagination="pagination" dense :loading="loading" :filter="filter" binary-state-sort
    @request="onTableRequest">
    <template #top-left>
      <ImportBtn v-if="isSuperAdmin && false" label="更新" icon="fingerprint" tooltip="更新路由权限码"
        @click="onImportRoutePermissionCode"></ImportBtn>
    </template>

    <template v-slot:top-right>
      <SearchInput v-model="filter" />
    </template>

    <template v-slot:body-cell-index="props">
      <QTableIndex :props="props" />
    </template>

    <template v-slot:body-cell-userId="props">
      <q-td :props="props">
        {{ props.value }}
      </q-td>
    </template>
  </q-table>
</template>

<script lang="ts" setup>
import { QTableColumn } from 'quasar'
import { useQTable, useQTableIndex } from 'src/compositions/qTableUtils'
import { IRequestPagination, TTableFilterObject } from 'src/compositions/types'
import SearchInput from 'src/components/searchInput/SearchInput.vue'
import ImportBtn from 'src/components/quasarWrapper/buttons/ImportBtn.vue'

const { indexColumn, QTableIndex } = useQTableIndex()
const columns: QTableColumn[] = [
  indexColumn,
  {
    name: 'code',
    required: true,
    label: '功能码',
    align: 'left',
    field: 'code',
    sortable: true
  },
  {
    name: 'description',
    required: false,
    label: '描述',
    align: 'left',
    field: 'description',
    sortable: true
  }
]

import { getPermissionCodesCount, getPermissionCodesData, IPermissionCode, updateRoutePermissionCodes } from 'src/api/permission'
// eslint-disable-next-line @typescript-eslint/no-unused-vars
async function getRowsNumberCount (filterObj: TTableFilterObject) {
  const { data } = await getPermissionCodesCount(filterObj.filter)
  return data || 0
}
// eslint-disable-next-line @typescript-eslint/no-unused-vars
async function onRequest (filterObj: TTableFilterObject, pagination: IRequestPagination) {
  const { data } = await getPermissionCodesData(filterObj.filter, pagination)
  return data || []
}

const { pagination, rows, filter, onTableRequest, loading, addNewRow } = useQTable({
  getRowsNumberCount,
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  onRequest
})

// #region 导入路由权限码
import { usePermission } from 'src/compositions/permission'
const { isSuperAdmin } = usePermission()

import { dynamicRoutes } from 'src/router/routes'
import { ExtendedRouteRecordRaw } from 'src/router/types'
import { notifySuccess } from 'src/utils/dialog'

async function onImportRoutePermissionCode () {
  // 获取动态路径
  const routePermissionCodes = getRoutePermissionCode(dynamicRoutes)
  console.log('routePermissionCodes:', routePermissionCodes)

  // 向服务器发送更新请求
  const { data: newPermissionCodes } = await updateRoutePermissionCodes(routePermissionCodes)
  newPermissionCodes.forEach((item: IPermissionCode) => {
    addNewRow(item)
  })

  if (newPermissionCodes.length === 0) {
    notifySuccess('所有路由权限码已经是最新')
    return
  }

  notifySuccess('路由权限码更新成功')
}
// 遍历所有的路由，获取所有的权限码
function getRoutePermissionCode (routes: ExtendedRouteRecordRaw[], parentRoute: string = 'route') {
  const permissionCodeList: IPermissionCode[] = []
  for (const route of routes) {
    if (!route.name) { continue }
    const routePermissionCode = `${parentRoute}/${String(route.name)}`
    permissionCodeList.push({
      code: routePermissionCode,
      description: `路由/${route.meta?.label}`
    })

    if (route.children) {
      permissionCodeList.push(...getRoutePermissionCode(route.children as ExtendedRouteRecordRaw[], routePermissionCode))
    }
  }

  return permissionCodeList
}
// #endregion
</script>

<style lang="scss" scoped></style>
