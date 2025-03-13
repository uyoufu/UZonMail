<template>
  <div class="full-height full-width row items-start">
    <EmailGroupList v-show="!isCollapseGroupList" v-model="emailGroupRef" class="q-card q-mr-sm full-height"
      style="min-width: 160px;" :contextMenuItems="groupCtxMenuItems" />

    <q-table ref="outboxTableRef" class="col full-height" :rows="rows" :columns="columns" row-key="id" virtual-scroll
      v-model:pagination="pagination" dense :loading="loading" :filter="filter" binary-state-sort selection="multiple"
      v-model:selected="selectedRows" @request="onTableRequest">
      <template v-slot:top-left>
        <div class="row justify-start q-gutter-sm">
          <CreateBtn tooltip="新增发件箱" @click="onNewOutboxClick" :disable="!isValidEmailGroup"
            tooltip-when-disabled="请先添加组" />

          <ExportBtn label="模板" tooltip="下载发件箱模板" @click="onExportOutboxTemplateClick" />

          <ImportBtn label="Excel 导入" tooltip="导入发件箱" @click="onImportOutboxClick()" :disable="!isValidEmailGroup"
            tooltip-when-disabled="请先添加组" />

          <!-- <ImportBtn :label="importFromTxtLable" :tooltip="importFromTxtTooltip" @click="onImportOutboxClick()"
            :disable="!isValidEmailGroup" tooltip-when-disabled="请先添加组" /> -->
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
import { useI18n } from 'vue-i18n'
const { t } = useI18n()

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
import { getOutboxesCount, getOutboxesData } from 'src/api/emailBox'
import type { IOutbox } from 'src/api/emailBox'
import type { IEmailGroupListItem } from '../components/types'

// #region 左侧分组开关
import { useTableCollapseLeft } from 'src/components/collapseLeft/useCollapseLeft'
const outboxTableRef = ref<InstanceType<typeof QTable> | undefined>()
const isCollapseGroupList = ref(false)
const { CollapseLeft, collapseStyleRef } = useTableCollapseLeft(outboxTableRef, isCollapseGroupList)
// #endregion

// 菜单项
const emailGroupRef: Ref<IEmailGroupListItem> = ref({
  id: 0,
  name: 'all',
  label: '',
  order: 0
})
const isValidEmailGroup = computed(() => emailGroupRef.value.id)
import type { IProxy } from 'src/api/proxy';
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
    label: '序号',
    align: 'left',
    field: 'id'
  },
  {
    name: 'email',
    required: true,
    label: '发件箱',
    align: 'left',
    field: 'email',
    sortable: true
  },
  {
    name: 'name',
    label: '名称(发件人姓名)',
    align: 'left',
    field: 'name',
    sortable: true
  },
  {
    name: 'smtpHost',
    required: true,
    label: 'smtp地址',
    align: 'left',
    field: 'smtpHost',
    sortable: true
  },
  {
    name: 'smtpPort',
    required: true,
    label: 'smtp端口',
    align: 'left',
    field: 'smtpPort',
    sortable: true
  },
  {
    name: 'userName',
    required: true,
    label: 'smtp用户名',
    align: 'left',
    field: 'userName',
    sortable: true
  },
  {
    name: 'password',
    required: true,
    label: 'smtp密码',
    align: 'left',
    field: 'password',
    sortable: true
  },
  {
    name: 'enableSSL',
    required: true,
    label: 'SSL',
    align: 'left',
    field: 'enableSSL',
    format: v => v ? '是' : '否',
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
    name: 'proxyId',
    required: true,
    label: '代理',
    align: 'left',
    field: 'proxyId',
    sortable: true,
    format: (val: number) => {
      const proxy = usableProxies.value.find(p => p.id === val)
      return proxy?.name ?? '无'
    }
  },
  {
    name: 'status',
    required: true,
    label: t('outboxManager.col_status'),
    align: 'left',
    field: 'status',
    format: (val: number) => {
      switch (val) {
        case 0:
          return t('outboxStatus.none')
        case 1:
          return t('outboxStatus.success')
        case 2:
          return t('outboxStatus.failed')
        default:
          return t('outboxStatus.unknown')
      }
    },
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
  return '******'
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
    data.password = plainPwd || '密钥变动,解密失败。请重新输入 smtp 密码'
    data.decryptedPassword = true
  }

  data.showPassword = !data.showPassword
}

// #region 表头功能
import { useHeaderFunction, getOutboxExcelDataMapper } from './headerFunctions'
const { onNewOutboxClick, onExportOutboxTemplateClick, onImportOutboxClick } = useHeaderFunction(emailGroupRef, addNewRow)
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
    name: 'import',
    label: '导入',
    tooltip: '向当前组中导入收件箱',
    onClick: value => onImportOutboxClick(value.id)
  },
  {
    name: 'export',
    label: '导出',
    tooltip: '导出当前组中的收件箱',
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
    notifyError('没有可导出项')
    return
  }
  const { data: dataRows } = await getOutboxesData(group.id, '', { sortBy: 'id', descending: false, skip: 0, limit: count })
  // 对密码进行解密
  dataRows.forEach(row => {
    const plainPwd = deAes(userInfoStore.smtpPasswordSecretKeys[0] || '', userInfoStore.smtpPasswordSecretKeys[1] || '', row.password)
    row.password = plainPwd || '密钥变动,解密失败'
  })

  await writeExcel(dataRows, {
    fileName: `${group.name}-发件箱.xlsx`,
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
// eslint-disable-next-line @typescript-eslint/no-unused-vars
const { onImportOutboxFromTxt, importFromTxtLable, importFromTxtTooltip } = useOutboxImporter()
// #endregion
</script>

<style lang="scss" scoped></style>
