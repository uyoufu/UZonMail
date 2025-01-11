<template>
  <q-table ref="dropZoneRef" class="full-height" :rows="rows" :columns="columns" row-key="id" virtual-scroll
    v-model:pagination="pagination" dense :loading="loading" :filter="filter" binary-state-sort
    @request="onTableRequest">
    <template v-slot:top-left>
      <CreateBtn label="上传" icon="upload" tooltip="上传附件" @click="openFileDialog" />
    </template>

    <template v-slot:top-right>
      <SearchInput v-model="filter" />
    </template>

    <template v-slot:body-cell-index="props">
      <QTableIndex :props="props" />
      <ContextMenu :items="attachmentCtxMenuItems" :value="props.row" />
    </template>

    <template v-slot:body-cell-userId="props">
      <q-td :props="props">
        {{ props.value }}
      </q-td>
    </template>
  </q-table>
</template>

<script lang="ts" setup>
import { QTableColumn, format } from 'quasar'
import { useQTable, useQTableIndex } from 'src/compositions/qTableUtils'
import { IRequestPagination, TTableFilterObject } from 'src/compositions/types'
import SearchInput from 'src/components/searchInput/SearchInput.vue'
import { formatDate } from 'src/utils/format'

const { indexColumn, QTableIndex } = useQTableIndex()
const columns: QTableColumn[] = [
  indexColumn,
  {
    name: 'displayName',
    required: true,
    label: '文件名',
    align: 'left',
    field: v => v.displayName || v.fileName,
    sortable: true
  },
  {
    name: 'sha256',
    required: true,
    label: '哈希值',
    align: 'left',
    field: v => v.fileObject.sha256,
    format: v => {
      // 截取两端的 hash 值显示
      return v ? v.slice(0, 8) + '...' + v.slice(-8) : ''
    },
    sortable: false
  },
  {
    name: 'linkCount',
    required: true,
    label: '引用数',
    align: 'left',
    field: v => v.fileObject.linkCount,
    sortable: false
  },
  {
    name: 'size',
    required: true,
    label: '文件大小',
    align: 'left',
    field: v => v.fileObject.size,
    sortable: false,
    format: v => v ? format.humanStorageSize(v) : ''
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
import { getFileUsagesCount, getFileUsagesData, deleteFileUsage, updateDisplayName } from 'src/api/file'
// eslint-disable-next-line @typescript-eslint/no-unused-vars
async function getRowsNumberCount (filterObj: TTableFilterObject) {
  const { data } = await getFileUsagesCount(filterObj.filter)
  return data || 0
}
// eslint-disable-next-line @typescript-eslint/no-unused-vars
async function onRequest (filterObj: TTableFilterObject, pagination: IRequestPagination) {
  const { data } = await getFileUsagesData(filterObj.filter, pagination)
  return data
}

const { pagination, rows, filter, onTableRequest, loading, refreshTable, deleteRowById } = useQTable({
  getRowsNumberCount,
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  onRequest
})

import { confirmOperation, notifySuccess, showComponentDialog, showDialog } from 'src/utils/dialog'
import FilesUploaderPopup from 'src/components/uploader/FilesUploaderPopup.vue'
import { useDropZone, useFileDialog, useFileSystemAccess } from '@vueuse/core'

// #region 上传文件
const { open: openFileDialog, onChange } = useFileDialog({
  multiple: true,
  // accept: 'image/*', // Set to accept only image files
  directory: false // Select directories instead of files if set true
})

onChange(async (files) => {
  if (!files) return
  await uploadFiles(Array.from(files))
})

async function uploadFiles (files: File[]) {
  if (files.length === 0) return

  // 打开上传弹窗
  await showComponentDialog(FilesUploaderPopup, {
    files
  })

  //  更新当前界面
  refreshTable()
}
// #endregion

// #region 拖拽上传
const dropZoneRef = ref<HTMLDivElement>()
async function onDrop (files: File[] | null) {
  // called when files are dropped on zone
  console.log('onDrop:', files)
  if (!files) return

  await uploadFiles(files)
}
const { isOverDropZone } = useDropZone(dropZoneRef, {
  onDrop
  // specify the types of data to be received.
  // dataTypes: ['image/jpeg']
})
watch(isOverDropZone, () => {
  if (isOverDropZone.value) {
    notifySuccess('松开鼠标上传')
  }
})
// #endregion

// #region 右键菜单
import ContextMenu from 'src/components/contextMenu/ContextMenu.vue'
import { IContextMenuItem } from 'src/components/contextMenu/types'
const attachmentCtxMenuItems: IContextMenuItem[] = [
  {
    name: 'download',
    label: '下载',
    tooltip: '下载文件',
    // 当返回 false 时，右键菜单不会退出
    onClick: onDownloadAttachment
  },
  {
    name: 'rename',
    label: '重命名',
    tooltip: '重命名为有规律的名称，方便在发件时使用',
    // 当返回 false 时，右键菜单不会退出
    onClick: renameAttachment
  },
  {
    name: 'share',
    label: '分享',
    tooltip: ['分享文件,可通过链接直接访问', '可以将上传的图片以 img 标签的方式插入到正文中', '仅 pro 版本提供'],
    onClick: shareAttachment
  },
  {
    name: 'delete',
    label: '删除',
    color: 'negative',
    tooltip: '删除该条记录,但不会真正删除文件',
    // 当返回 false 时，右键菜单不会退出
    onClick: onRemoveAttachment
  }
]

import { useConfig } from 'src/config'
import { getFileReaderId, getFileStreamByReaderId } from 'src/api/fileReader'
import { saveFileSmart } from 'src/utils/file'
import { PopupDialogFieldType } from 'src/components/popupDialog/types'
const config = useConfig()
// eslint-disable-next-line @typescript-eslint/no-explicit-any
async function onDownloadAttachment (row: Record<string, any>) {
  const { data: fileReaderId } = await getFileReaderId(row.id)
  const extension = row.fileName.split('.').pop() as string
  const dataType = ref('ArrayBuffer') as Ref<'Text' | 'ArrayBuffer' | 'Blob'>
  const fsa = useFileSystemAccess({
    dataType,
    types: [{
      description: `${extension} 文件`,
      accept: {
        '*/*': [`.${extension}`]
      }
    }],
    excludeAcceptAllOption: true
  })
  if (fsa.isSupported.value) {
    await fsa.create({
      suggestedName: row.displayName || row.fileName
    })
    // 开始下载文件
    const { data: arrayBuffer } = await getFileStreamByReaderId(fileReaderId)
    fsa.data.value = arrayBuffer
    await fsa.save()
    notifySuccess('下载成功')
    return
  }

  // 使用旧式的下载方式
  const baseUrl = `${config.baseUrl}${config.api}` as string
  const fileUrl = `${baseUrl}/file-reader/${fileReaderId}/stream`
  saveFileSmart(row.displayName || row.fileName, fileUrl)
  notifySuccess('下载成功')
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
async function renameAttachment (row: Record<string, any>) {
  console.log(row)

  // 打开重命名弹窗
  const result = await showDialog({
    title: '重命名',
    fields: [
      {
        name: 'displayName',
        label: '原文件名',
        type: PopupDialogFieldType.text,
        required: true,
        value: row.displayName || row.fileName,
        disable: true
      },
      {
        name: 'newDisplayName',
        label: '新文件名',
        type: PopupDialogFieldType.text,
        required: true,
        value: row.displayName || row.fileName
      }
    ],
    oneColumn: true
  })

  if (!result.ok) return

  const displayName = result.data.newDisplayName
  // 修改名称
  await updateDisplayName(row.id, displayName)

  row.displayName = displayName
}

import { createObjectPersistentReader } from 'src/api/pro/objectReader'
// eslint-disable-next-line @typescript-eslint/no-explicit-any
async function shareAttachment (row: Record<string, any>) {
  console.log(row)

  const confirm = await confirmOperation('分享确认', '分享后，其他人可以在不登陆的情况下，通过链接访问该文件，是否继续？')
  if (!confirm) return

  const { data: objectReaderId } = await createObjectPersistentReader(row.id)
  // 生成全路径
  const fullUrl = `${config.baseUrl}/api/pro/object-reader/stream/${objectReaderId}` as string
  // 保存到剪切板
  await navigator.clipboard.writeText(fullUrl)

  notifySuccess('分享成功，链接已复制到剪切板')
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
async function onRemoveAttachment (row: Record<string, any>) {
  const confirm = await confirmOperation('删除确认', `即将删除文件：${row.displayName || row.fileName}，是否继续？`)
  if (!confirm) return

  // 进行删除操作
  await deleteFileUsage(row.id)

  deleteRowById(row.id)

  notifySuccess('删除成功')
}
// #endregion
</script>

<style lang="scss" scoped></style>
