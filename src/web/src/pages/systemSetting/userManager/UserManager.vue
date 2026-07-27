<template>
  <q-table class="full-height" :rows="rows" :columns="columns" row-key="id" virtual-scroll
    v-model:pagination="pagination" dense :loading="loading" :filter="filter" binary-state-sort
    @request="onTableRequest">
    <template v-slot:top-left>
      <CreateBtn @click="onNewUserClick" />
    </template>

    <template v-slot:top-right>
      <SearchInput v-model="filter" />
    </template>

    <template v-slot:body-cell-userId="props">
      <q-td :props="props">
        {{ props.value }}
      </q-td>
      <ContextMenu :items="userManageContextItems" :value="props.row" />
    </template>

    <template v-slot:body-cell-status="props">
      <q-td :props="props">
        <StatusChip :status="props.value">
        </StatusChip>
      </q-td>
    </template>

    <template v-slot:body-cell-type="props">
      <q-td :props="props">
        <StatusChip :status="props.value">
        </StatusChip>
      </q-td>
    </template>
  </q-table>
</template>

<script lang="ts" setup>
import type { QTableColumn } from 'quasar'
import { useQTable } from 'src/compositions/qTableUtils'
import type { IRequestPagination, TTableFilterObject } from 'src/compositions/types'
import { usePermission } from 'src/compositions/permission'

import SearchInput from 'src/components/searchInput/SearchInput.vue'
import StatusChip from 'src/components/statusChip/StatusChip.vue'
import CreateBtn from 'src/components/quasarWrapper/buttons/CreateBtn.vue'

import { getFilteredUsersCount, getFilteredUsersData } from 'src/api/user'

import { UserStatus, UserType } from 'src/stores/types'
import { formatDate } from 'src/utils/format'
import { t } from 'src/i18n/helpers'

const { hasEnterpriseAccess } = usePermission()
const columns = computed<QTableColumn[]>(() => {
  const columns: QTableColumn[] = [
  {
    name: 'userId',
    required: true,
    label: t('pages.userManager.userName'),
    align: 'left',
    field: 'userId',
    sortable: true
  },
  {
    name: 'createDate',
    required: false,
    label: t('pages.userManager.registrationDate'),
    align: 'left',
    field: 'createDate',
    format: formatDate,
    sortable: true
  },
  {
    name: 'status',
    required: true,
    label: t('global.status'),
    align: 'left',
    field: 'status',
    format: v => UserStatus[v] as string,
    sortable: true
  }
  ]
  if (hasEnterpriseAccess()) {
    columns.push({
    name: 'type',
    required: true,
      label: t('pages.userManager.userType'),
    align: 'left',
    field: 'type',
    format: v => UserType[v] as string,
    sortable: true
    })
  }
  return columns
})

async function getRowsNumberCount (filterObj: TTableFilterObject) {
  const { data } = await getFilteredUsersCount(filterObj.filter)
  return data
}
async function onRequest (filterObj: TTableFilterObject, pagination: IRequestPagination) {
  const { data } = await getFilteredUsersData(filterObj.filter as string, pagination)
  return data
}

const { pagination, rows, filter, onTableRequest, loading, addNewRow } = useQTable({
  getRowsNumberCount,

  onRequest
})

// #region 右键菜单
import ContextMenu from 'src/components/contextMenu/ContextMenu.vue'
import { useContextMenu } from './useContextMenu'
const { onNewUserClick, userManageContextItems } = useContextMenu(addNewRow)
// #endregion
</script>

<style lang="scss" scoped></style>
