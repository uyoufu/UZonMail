<template>
  <q-table class="full-height" :rows="rows" :columns="columns" row-key="id" virtual-scroll
    v-model:pagination="pagination" dense :loading="loading" :filter="filter" binary-state-sort
    @request="onTableRequest">
    <template v-slot:top-left>
      <CreateBtn @click="onCreateProxy" />
    </template>

    <template v-slot:top-right>
      <SearchInput v-model="filter" />
    </template>

    <template v-slot:body-cell-index="props">
      <QTableIndex :props="props" />
      <ContextMenu :items="proxyContextMenuItems" :value="props.row" />
    </template>

    <template v-slot:body-cell-name="props">
      <q-td :props="props">
        <ClickableText :text="props.value" :tooltip="t('pages.proxy.clickToEdit')" @click="onModifyProxy(props.row)" />
      </q-td>
    </template>

    <template v-slot:body-cell-url="props">
      <q-td :props="props">
        <EllipsisContent :content="props.value" :maxLength="40" direction="end" />
      </q-td>
    </template>

    <template v-slot:body-cell-isShared="props">
      <q-td :props="props">
        <q-toggle :disable="disableShareToggle(props.row)" color="secondary" dense v-model="props.row.isShared"
          @click="onToggleShareProxy(props.row)">
          <q-tooltip>
            {{ getProxyShareTooltip(props.row) }}
          </q-tooltip>
        </q-toggle>
      </q-td>
    </template>
  </q-table>
</template>

<script lang="ts" setup>
import type { QTableColumn } from 'quasar'
import { useQTable, useQTableIndex } from 'src/compositions/qTableUtils'
import type { IRequestPagination, TTableFilterObject } from 'src/compositions/types'
import SearchInput from 'src/components/searchInput/SearchInput.vue'
import ClickableText from 'src/components/clickableText/ClickableText.vue'
import EllipsisContent from 'src/components/ellipsisContent/EllipsisContent.vue'
import { t } from 'src/i18n/helpers'
import { useUserInfoStore } from 'src/stores/user'

// #region 表格定义
const { indexColumn, QTableIndex } = useQTableIndex()
const userInfoStore = useUserInfoStore()
const columns = computed<QTableColumn[]>(() => {
  const columns: QTableColumn[] = [
  indexColumn,
  {
    name: 'name',
    required: true,
    label: t('global.name'),
    align: 'left',
    field: 'name',
    sortable: true
  },
  {
    name: 'url',
    label: t('pages.sendingTask.proxy'),
    align: 'left',
    field: 'url',
    sortable: true
  },
  {
    name: 'matchRegex',
    label: t('pages.proxy.matchRule'),
    align: 'left',
    field: 'matchRegex',
    sortable: true
  },
  {
    name: 'priority',
    label: t('pages.proxy.priority'),
    align: 'left',
    field: 'priority',
    sortable: true
  },
  {
    name: 'isActive',
    label: t('pages.proxy.enabled'),
    align: 'left',
    field: 'isActive',
    sortable: true,
    format: v => v ? t('pages.proxy.yes') : t('pages.proxy.no')
  }
  ]

  if (userInfoStore.isAdmin) {
    columns.push({
    name: 'isShared',
      label: t('pages.proxy.shared'),
    align: 'left',
    field: 'isShared',
    sortable: true
    })
  }
  return columns
})

import { getProxiesCount, getProxiesData } from 'src/api/proxy'

async function getRowsNumberCount (filterObj: TTableFilterObject) {
  const { data } = await getProxiesCount(filterObj.filter)
  return data || 0
}

async function onRequest (filterObj: TTableFilterObject, pagination: IRequestPagination) {
  const { data } = await getProxiesData(filterObj.filter, pagination)
  return data || []
}

const { pagination, rows, filter, onTableRequest, loading, addNewRow, deleteRowById } = useQTable({
  getRowsNumberCount,

  onRequest
})
// #endregion

// #region 代理的新建、编辑、删除
import { useHeaderFunctions } from './headerFuncs'
const { onCreateProxy, onToggleShareProxy } = useHeaderFunctions(addNewRow)

import ContextMenu from 'src/components/contextMenu/ContextMenu.vue'
import { useContextMenu } from './contextMenu'
const { proxyContextMenuItems, onModifyProxy } = useContextMenu(deleteRowById)
// #endregion

// #region toggle 控制
import { useShareToggle } from './compositions/useShareToggle'
const { disableShareToggle, getProxyShareTooltip } = useShareToggle()
// #endregion
</script>

<style lang="scss" scoped></style>
