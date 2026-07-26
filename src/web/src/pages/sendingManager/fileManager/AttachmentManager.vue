<template>
  <div ref="dropZoneRef" class="full-height full-width row no-wrap">
    <FileCategoryTree class="q-mr-sm" v-show="!isCollapseCategoryTree" @change="onCategoryChange" />

    <q-table ref="fileTableRef" class="col full-height" :rows="rows" :columns="columns" row-key="id"
      selection="multiple" v-model:selected="selectedRows" v-model:pagination="pagination" virtual-scroll dense
      :loading="loading" :filter="filter" binary-state-sort @request="onTableRequest">
      <template #top-left>
        <div class="row q-gutter-sm">
          <CreateBtn :label="t('fileManager.upload')" icon="upload" :tooltip="t('fileManager.upload')"
            @click="openFileDialog" />
          <CommonBtn icon="drive_file_move" :label="t('fileManager.move')" :tooltip="t('fileManager.moveSelected')"
            :disable="selectedRows.length === 0" @click="onMoveSelected" />
          <DeleteBtn :label="t('fileManager.batchDelete')" :tooltip="t('fileManager.deleteSelected')"
            :disable="selectedRows.length === 0" @click="onDeleteSelected" />
        </div>
      </template>

      <template #top-right>
        <SearchInput v-model="filter" />
      </template>
      <template #body-cell-index="props">
        <QTableIndex :props="props" />
        <ContextMenu :items="attachmentContextMenuItems" :value="props.row" />
      </template>
    </q-table>

    <CollapseLeft v-model="isCollapseCategoryTree" :style="collapseStyleRef" />
  </div>
</template>

<script lang="ts" setup>
import { format, QTable, type QTableColumn } from 'quasar'
import { useI18n } from 'vue-i18n'
import { useDropZone, useFileDialog, useFileSystemAccess } from '@vueuse/core'
import FileCategoryTree from './FileCategoryTree.vue'
import SearchInput from 'src/components/searchInput/SearchInput.vue'
import ContextMenu from 'src/components/contextMenu/ContextMenu.vue'
import CreateBtn from 'src/components/quasarWrapper/buttons/CreateBtn.vue'
import CommonBtn from 'src/components/quasarWrapper/buttons/CommonBtn.vue'
import DeleteBtn from 'src/components/quasarWrapper/buttons/DeleteBtn.vue'
import FilesUploaderPopup from 'src/components/uploader/FilesUploaderPopup.vue'
import type { IContextMenuItem } from 'src/components/contextMenu/types'
import { useTableCollapseLeft } from 'src/components/collapseIcon/useCollapseLeft'
import { useQTable, useQTableIndex } from 'src/compositions/qTableUtils'
import type { IRequestPagination, TTableFilterObject } from 'src/compositions/types'
import { LowCodeFieldType } from 'src/components/lowCode/types'
import {
  deleteFileUsage,
  deleteFileUsages,
  getFileUsagesCount,
  getFileUsagesData,
  moveFileUsages,
  updateDisplayName,
  type IFileUsage
} from 'src/api/file'
import { getFileCategories } from 'src/api/fileCategory'
import { getFileReaderId, getFileStreamByReaderId } from 'src/api/fileReader'
import { createObjectPersistentReader } from 'src/api/pro/objectReader'
import { useConfig } from 'src/config'
import { formatDate } from 'src/utils/format'
import { saveFileSmart } from 'src/utils/file'
import { confirmOperation, notifySuccess, showComponentDialog, showDialog } from 'src/utils/dialog'

const { t } = useI18n()
const config = useConfig()
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

const { pagination, rows, filter, onTableRequest, loading, refreshTable, selectedRows } = useQTable({
  getRowsNumberCount,
  onRequest
})

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

async function onDeleteSelected() {
  const confirmed = await confirmOperation(
    t('fileManager.deleteConfirmTitle'),
    t('fileManager.batchDeleteConfirm', { count: selectedRows.value.length })
  )
  if (!confirmed) return
  await deleteFileUsages(selectedRows.value.map(row => row.id as number))
  selectedRows.value = []
  refreshTable()
  notifySuccess(t('fileManager.deleteSuccess'))
}

async function onMoveSelected() {
  const { data: categories } = await getFileCategories()
  const result = await showDialog({
    title: t('fileManager.moveSelected'),
    fields: [{
      name: 'categoryId',
      label: t('fileManager.targetCategory'),
      type: LowCodeFieldType.selectOne,
      options: categories.map(category => ({
        label: category.isDefault ? t('fileManager.defaultCategory') : category.name,
        value: category.id
      })),
      mapOptions: true,
      emitValue: true,
      required: true
    }],
    oneColumn: true
  })
  if (!result.ok) return
  await moveFileUsages(selectedRows.value.map(row => row.id as number), Number(result.data.categoryId))
  selectedRows.value = []
  refreshTable()
}

const attachmentContextMenuItems = computed<IContextMenuItem<IFileUsage>[]>(() => [
  { name: 'download', label: t('fileManager.download'), onClick: onDownloadAttachment },
  { name: 'rename', label: t('fileManager.rename'), onClick: onRenameAttachment },
  { name: 'share', label: t('fileManager.share'), onClick: onShareAttachment },
  { name: 'delete', label: t('fileManager.delete'), color: 'negative', onClick: onDeleteAttachment }
])

async function onDownloadAttachment(row: IFileUsage) {
  const { data: fileReaderId } = await getFileReaderId(row.id)
  const extension = row.displayName.split('.').pop() || ''
  const fileSystemAccess = useFileSystemAccess({
    dataType: ref<'Text' | 'ArrayBuffer' | 'Blob'>('ArrayBuffer'),
    types: [{ description: row.displayName, accept: { '*/*': extension ? [`.${extension}`] : [] } }],
    excludeAcceptAllOption: true
  })
  if (fileSystemAccess.isSupported.value) {
    await fileSystemAccess.create({ suggestedName: row.displayName })
    const { data } = await getFileStreamByReaderId(fileReaderId)
    fileSystemAccess.data.value = data
    await fileSystemAccess.save()
  } else {
    await saveFileSmart(row.displayName, `${config.baseUrl}${config.api}/file-reader/${fileReaderId}/stream`)
  }
  notifySuccess(t('fileManager.downloadSuccess'))
}

async function onRenameAttachment(row: IFileUsage) {
  const result = await showDialog({
    title: t('fileManager.rename'),
    fields: [{ name: 'displayName', label: t('fileManager.fileName'), type: LowCodeFieldType.text, required: true, value: row.displayName }],
    oneColumn: true
  })
  if (!result.ok) return
  await updateDisplayName(row.id, String(result.data.displayName))
  refreshTable()
}

async function onShareAttachment(row: IFileUsage) {
  const confirmed = await confirmOperation(t('fileManager.share'), t('fileManager.shareConfirm'))
  if (!confirmed) return
  const { data: objectReaderId } = await createObjectPersistentReader(row.id)
  await navigator.clipboard.writeText(`${config.baseUrl}/api/pro/object-reader/stream/${objectReaderId}`)
  notifySuccess(t('fileManager.shareSuccess'))
}

async function onDeleteAttachment(row: IFileUsage) {
  const confirmed = await confirmOperation(
    t('fileManager.deleteConfirmTitle'),
    t('fileManager.deleteFileConfirm', { name: row.displayName })
  )
  if (!confirmed) return
  await deleteFileUsage(row.id)
  refreshTable()
  notifySuccess(t('fileManager.deleteSuccess'))
}
</script>
