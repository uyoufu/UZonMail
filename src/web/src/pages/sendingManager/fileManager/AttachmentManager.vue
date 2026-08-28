<template>
  <div ref="dropZoneRef" class="full-height full-width row no-wrap">
    <FileCategoryTree class="q-mr-sm" v-show="!isCollapseCategoryTree" @change="onCategoryChange" />

    <q-table ref="fileTableRef" class="col full-height" :rows="rows" :columns="columns" row-key="id"
      selection="multiple" v-model:selected="selectedRows" v-model:pagination="pagination" virtual-scroll dense
      :loading="loading" :filter="filter" binary-state-sort @request="onTableRequest">
      <template #top-left>
        <CreateBtn :label="t('fileManager.upload')" icon="upload" :tooltip="t('fileManager.upload')"
          @click="openFileDialog" />
      </template>

      <template #top-right>
        <SearchInput v-model="filter" />
      </template>
      <template #body-cell-index="props">
        <QTableIndex :props="props" />
        <ContextMenu v-model:selected-values="selectedRows" :items="attachmentContextMenuItems" :value="props.row" />
      </template>
    </q-table>

    <CollapseLeft v-model="isCollapseCategoryTree" :style="collapseStyleRef" />
  </div>
</template>

<script lang="ts" setup>
import { format, QTable, type QTableColumn } from 'quasar'
import { useI18n } from 'vue-i18n'
import { useDropZone, useFileDialog } from '@vueuse/core'
import FileCategoryTree from './FileCategoryTree.vue'
import SearchInput from 'src/components/searchInput/SearchInput.vue'
import ContextMenu from 'src/components/contextMenu/ContextMenu.vue'
import CreateBtn from 'src/components/buttons/CreateBtn.vue'
import FilesUploaderPopup from 'src/components/uploader/FilesUploaderPopup.vue'
import { useTableCollapseLeft } from 'src/components/collapseIcon/useCollapseLeft'
import { useQTable, useQTableIndex } from 'src/compositions/qTableUtils'
import type { IRequestPagination, TTableFilterObject } from 'src/compositions/types'
import {
  getFileUsagesCount,
  getFileUsagesData,
  type IFileUsage
} from 'src/api/file'
import { formatDate } from 'src/utils/format'
import { showComponentDialog } from 'src/utils/dialog'
import { useAttachmentContextMenu } from './useAttachmentContextMenu'

const { t } = useI18n()
const selectedCategoryId = ref<number>()
const fileTableRef = ref<InstanceType<typeof QTable>>()
const { CollapseLeft, collapseStyleRef, isCollapseGroupList: isCollapseCategoryTree } = useTableCollapseLeft(fileTableRef)
const { indexColumn, QTableIndex } = useQTableIndex()

const columns = computed<QTableColumn[]>(() => [
  indexColumn,
  { name: 'displayName', label: t('fileManager.fileName'), align: 'left', field: 'displayName', sortable: true },
  {
    name: 'sha256',
    label: t('fileManager.hash'),
    align: 'left',
    field: 'sha256',
    format: value => value ? `${value.slice(0, 8)}...${value.slice(-8)}` : ''
  },
  { name: 'referenceCount', label: t('fileManager.referenceCount'), align: 'left', field: 'referenceCount' },
  { name: 'size', label: t('fileManager.fileSize'), align: 'left', field: 'size', format: value => format.humanStorageSize(value) },
  { name: 'createDate', label: t('fileManager.createDate'), align: 'left', field: 'createDate', format: formatDate, sortable: true }
])

async function getRowsNumberCount(filterObject: TTableFilterObject) {
  const { data } = await getFileUsagesCount(filterObject.filter, selectedCategoryId.value)
  return data
}

async function onRequest(filterObject: TTableFilterObject, requestPagination: IRequestPagination) {
  const { data } = await getFileUsagesData(filterObject.filter, requestPagination, selectedCategoryId.value)
  return data
}

const { pagination, rows, filter, onTableRequest, loading, refreshTable, selectedRows } = useQTable<IFileUsage>({
  getRowsNumberCount,
  onRequest
})
const { attachmentContextMenuItems } = useAttachmentContextMenu(refreshTable)

function onCategoryChange(categoryId?: number) {
  selectedCategoryId.value = categoryId
  selectedRows.value = []
  refreshTable()
}

const { open: openFileDialog, onChange } = useFileDialog({ multiple: true, directory: false })
onChange(files => {
  if (files) void uploadFiles(Array.from(files))
})

async function uploadFiles(files: File[]) {
  if (files.length === 0) return
  await showComponentDialog(FilesUploaderPopup, { files, categoryId: selectedCategoryId.value })
  refreshTable()
}

const dropZoneRef = ref<HTMLElement>()
useDropZone(dropZoneRef, {
  onDrop: files => {
    if (files) void uploadFiles(files)
  }
})
</script>
