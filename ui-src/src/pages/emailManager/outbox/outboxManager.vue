<template>
  <div class="full-height full-width row items-start">
    <EmailGroupList v-show="!isCollapseGroupList" v-model="emailGroupRef" class="q-card q-mr-sm full-height"
      style="min-width: 160px;" :contextMenuItems="groupCtxMenuItems" />

    <q-table ref="outboxTableRef" class="col full-height" :rows="rows" :columns="columns" row-key="id" virtual-scroll
      v-model:pagination="pagination" dense :loading="loading" :filter="filter" binary-state-sort selection="multiple"
      v-model:selected="selectedRows" @request="onTableRequest">
      <template v-slot:top-left>
        <div class="row justify-start q-gutter-sm">
          <CreateBtn :tooltip="translateOutboxManager('newOutbox')" @click="onNewOutboxClick"
            :disable="!isValidEmailGroup" :tooltip-when-disabled="translateOutboxManager('pleaseAddGroupFirst')" />
          <ExportBtn label="" :tooltip="translateOutboxManager('downloadOutboxTemplate')"
            @click="onExportOutboxTemplateClick" />
          <ImportBtn label="" :tooltip="translateOutboxManager('importOutbox')"
            @click="onImportOutboxFromExcelClicked()" :disable="!isValidEmailGroup"
            :tooltip-when-disabled="translateOutboxManager('pleaseAddGroupFirst')" />
          <ImportBtn label="" icon="description" :tooltip="importFromTxtTooltip" @click="onImportOutboxFromTxt()"
            :disable="!isValidEmailGroup" :tooltip-when-disabled="translateOutboxManager('pleaseAddGroupFirst')" />
        </div>
      </template>

      <template v-slot:top-right>
        <SearchInput v-model="filter" />
      </template>

      <template v-slot:body-cell-id="props">
        <q-td :props="props">
          {{ props.rowIndex + 1 }}
        </q-td>
      </template>

      <template v-slot:body-cell-email="props">
        <q-td :props="props">
          {{ props.value }}
        </q-td>
        <ContextMenu :items="outboxContextMenuItems" :value="props.row" />
      </template>

      <template v-slot:body-cell-password="props">
        <q-td class="cursor-pointer" :props="props" @click="togglePasswordViewMode(props.row)">
          {{ getPasswordValue(props.row) }}
        </q-td>
      </template>

      <template v-slot:body-cell-status="props">
        <q-td class="cursor-pointer" :props="props">
          <StatusChip :status="props.value">
            <AsyncTooltip :cache="false" :tooltip="props.row.validFailReason" />
          </StatusChip>
        </q-td>
      </template>
    </q-table>

    <CollapseLeft v-model="isCollapseGroupList" :style="collapseStyleRef" />
  </div>
</template>

<script lang="ts" setup>
import { translateGlobal, translateOutboxManager } from 'src/i18n/helpers'

import type { QTable, QTableColumn } from 'quasar'

import SearchInput from 'src/components/searchInput/SearchInput.vue'
import CreateBtn from 'src/components/quasarWrapper/buttons/CreateBtn.vue'
import ImportBtn from 'src/components/quasarWrapper/buttons/ImportBtn.vue'
import ExportBtn from 'src/components/quasarWrapper/buttons/ExportBtn.vue'
import EmailGroupList from '../components/EmailGroupList.vue'
import ContextMenu from 'components/contextMenu/ContextMenu.vue'
import StatusChip from 'src/components/statusChip/StatusChip.vue'
import AsyncTooltip from 'src/components/asyncTooltip/AsyncTooltip.vue'

import { useQTable } from 'src/compositions/qTableUtils'
import type { IRequestPagination, TTableFilterObject } from 'src/compositions/types'
import { getOutboxesCount, getOutboxesData, OutboxStatus } from 'src/api/emailBox'
import type { IOutbox } from 'src/api/emailBox'
import type { IEmailGroupListItem } from '../components/types'

// #region 左侧分组开关
import { useTableCollapseLeft } from 'src/components/collapseLeft/useCollapseLeft'
const outboxTableRef = ref<InstanceType<typeof QTable> | undefined>()
const { CollapseLeft, collapseStyleRef, isCollapseGroupList } = useTableCollapseLeft(outboxTableRef)
// #endregion

// 菜单项
const emailGroupRef: Ref<IEmailGroupListItem> = ref({
  id: 0,
  name: 'all',
  label: '',
  order: 0
})
const isValidEmailGroup = computed(() => emailGroupRef.value.id)
import type { IProxy } from 'src/api/proxy'
import { getUsableProxies } from 'src/api/proxy'
const usableProxies: Ref<IProxy[]> = ref([])
onMounted(async () => {
  const { data: proxies } = await getUsableProxies()
  usableProxies.value = proxies
})
const columns: QTableColumn[] = [
  {
    name: 'id',
    required: true,
    label: translateOutboxManager('col_order'),
    align: 'left',
    field: 'id'
  },
  {
    name: 'email',
    required: true,
    label: translateOutboxManager('col_email'),
    align: 'left',
    field: 'email',
    sortable: true
  },
  {
    name: 'name',
    label: translateOutboxManager('col_outboxUserName'),
    align: 'left',
    field: 'name',
    sortable: true
  },
  {
    name: 'smtpHost',
    required: true,
    label: translateOutboxManager('col_smtpHost'),
    align: 'left',
    field: 'smtpHost',
    sortable: true
  },
  {
    name: 'smtpPort',
    required: true,
    label: translateOutboxManager('col_smtpPort'),
    align: 'left',
    field: 'smtpPort',
    sortable: true
  },
  {
    name: 'userName',
    required: true,
    label: translateOutboxManager('col_smtpUserName'),
    align: 'left',
    field: 'userName',
    sortable: true
  },
  {
    name: 'password',
    required: true,
    label: translateOutboxManager('col_smtpPassword'),
    align: 'left',
    field: 'password',
    sortable: true
  },
  {
    name: 'enableSSL',
    required: true,
    label: translateOutboxManager('col_enableSSL'),
    align: 'left',
    field: 'enableSSL',
    format: v => v ? translateGlobal('yes') : translateGlobal('no'),
    sortable: true
  },
  {
    name: 'description',
    required: true,
    label: translateOutboxManager('col_description'),
    align: 'left',
    field: 'description',
    sortable: true
  },
  {
    name: 'proxyId',
    required: true,
    label: translateOutboxManager('col_proxy'),
    align: 'left',
    field: 'proxyId',
    sortable: true,
    format: (val: number) => {
      const proxy = usableProxies.value.find(p => p.id === val)
      return proxy?.name ?? translateGlobal('empty')
    }
  },
  {
    name: 'status',
    required: true,
    label: translateOutboxManager('col_status'),
    align: 'left',
    field: 'status',
    format: v => OutboxStatus[v] as string,
    sortable: true
  }
]
async function getRowsNumberCount (filterObj: TTableFilterObject) {
  const { data } = await getOutboxesCount(emailGroupRef.value.id, filterObj.filter)
  return data
}
async function onRequest (filterObj: TTableFilterObject, pagination: IRequestPagination) {
  const { data } = await getOutboxesData(emailGroupRef.value.id, filterObj.filter, pagination)
  return data
}
const { pagination, rows, filter, onTableRequest, loading,
  refreshTable, addNewRow, deleteRowById, selectedRows,
  getSelectedRows, updateExistOne } = useQTable({
    getRowsNumberCount,
    onRequest,
    preventRequestWhenMounted: true
  })
watch(emailGroupRef, () => {
  // 组切换时，触发更新
  refreshTable()
})

function getPasswordValue (data: IOutbox) {
  if (data.showPassword) return data.password
  return '*'.repeat(6)
}
import { deAes } from 'src/utils/encrypt'
import { useUserInfoStore } from 'src/stores/user'
const userInfoStore = useUserInfoStore()
function togglePasswordViewMode (data: IOutbox) {
  // console.log('togglePasswordViewMode', data)
  // 若是显示密码，但没有解密，则先解密
  if (!data.decryptedPassword) {
    // 进行解密
    const plainPwd = deAes(userInfoStore.smtpPasswordSecretKeys[0] || '', userInfoStore.smtpPasswordSecretKeys[1] || '', data.password)
    data.password = plainPwd || translateOutboxManager('decryptionFailedDueToKeyChange')
    data.decryptedPassword = true
  }

  data.showPassword = !data.showPassword
}

// #region 表头功能
import { useHeaderFunction, getOutboxExcelDataMapper } from './headerFunctions'
const { onNewOutboxClick, onExportOutboxTemplateClick, onImportOutboxFromExcelClicked } = useHeaderFunction(emailGroupRef, addNewRow)
// #endregion

// #region 数据右键菜单
import { useContextMenu } from './contextMenu'
const { outboxContextMenuItems } = useContextMenu(deleteRowById, getSelectedRows, refreshTable)
// #endregion

// #region 分组的右键菜单
import type { IContextMenuItem } from 'src/components/contextMenu/types'
import { notifyError } from 'src/utils/dialog'
const groupCtxMenuItems: Ref<IContextMenuItem[]> = ref([
  {
    name: 'importExcel',
    label: translateOutboxManager('importFromExcel'),
    tooltip: translateOutboxManager('importOutboxForCurrentGroupFromExcel'),
    onClick: value => onImportOutboxFromExcelClicked(value.id)
  } as IContextMenuItem,
  {
    name: 'importTxt',
    label: translateOutboxManager('importFromTxt'),
    tooltip: translateOutboxManager('importOutboxForCurrentGroupFromTxt'),
    onClick: value => onImportOutboxFromTxt(value.id)
  },
  {
    name: 'export',
    label: translateGlobal('export'),
    tooltip: translateOutboxManager('exportOutboxesInThisGroup'),
    onClick: exportAllInboxesInThisGroup
  }
])
// 导出当前组中的所有的收件箱
import { writeExcel } from 'src/utils/file'
// eslint-disable-next-line @typescript-eslint/no-explicit-any
async function exportAllInboxesInThisGroup (group: Record<string, any>) {
  // 获取所有的收件箱
  const { data: count } = await getOutboxesCount(group.id, '')
  if (!count) {
    notifyError(translateOutboxManager('noItemsToExport'))
    return
  }
  const { data: dataRows } = await getOutboxesData(group.id, '', { sortBy: 'id', descending: false, skip: 0, limit: count })
  // 对密码进行解密
  dataRows.forEach(row => {
    const plainPwd = deAes(userInfoStore.smtpPasswordSecretKeys[0] || '', userInfoStore.smtpPasswordSecretKeys[1] || '', row.password)
    row.password = plainPwd || translateOutboxManager('decryptionFailedDueToKeyChange')
  })

  await writeExcel(dataRows, {
    fileName: translateOutboxManager('exportFileName', { groupName: group.name }),
    sheetName: group.name,
    mappers: getOutboxExcelDataMapper(),
    strict: true
  })
}
// #endregion

// #region 注册 signalR 事件，方便进行实时更新
import { useSignalR } from './useSignalR'
useSignalR(updateExistOne)
// #endregion

// #region 邮箱导入功能
import { useOutboxImporter } from './useOutboxImporter'

const { onImportOutboxFromTxt, importFromTxtTooltip } = useOutboxImporter(emailGroupRef, addNewRow)
// #endregion
</script>

<style lang="scss" scoped></style>
