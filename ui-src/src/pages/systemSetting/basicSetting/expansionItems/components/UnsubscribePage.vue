<template>
  <q-table class="full-height" :rows="rows" :columns="columns" row-key="id" virtual-scroll
    v-model:pagination="pagination" dense :loading="loading" :filter="filter" binary-state-sort grid
    @request="onTableRequest">
    <template v-slot:top-left>
      <CreateBtn label="新增退订页" :tooltip="['按语言新增退订页面', '程序会根据用户当前语言展示对应的退订页']" @click="onNewUnsubscribePage" />
    </template>

    <template v-slot:top-right>
      <SearchInput v-model="filter" />
    </template>

    <template v-slot:item="props">
      <div class="col-xs-12 col-sm-6 col-md-4 col-lg-3 q-pa-xs">
        <q-card class="column q-pa-sm hoverable-container">
          <div class="text-primary">{{ props.row.language }}</div>
          <q-separator class="q-mb-sm" />
          <div class="unsubscribe_content_thubnail" v-html="props.row.htmlContent"></div>

          <div class="hoverable-focus">
            <div class="row full-height items-center justify-center q-gutter-sm">
              <CommonBtn icon="edit" color="secondary" tooltip="修改" @click="onModifyUnsubscribePage(props.row)" />
              <CommonBtn icon="preview" tooltip="预览" @click="onPreviewUnsubscribePage(props.row.id as number)" />
            </div>
          </div>
        </q-card>
      </div>
    </template>
  </q-table>
</template>

<script lang="ts" setup>
import type { QTableColumn } from 'quasar'
import { useQTable } from 'src/compositions/qTableUtils'
import type { IRequestPagination, TTableFilterObject } from 'src/compositions/types'
import SearchInput from 'src/components/searchInput/SearchInput.vue'
import CommonBtn from 'src/components/quasarWrapper/buttons/CommonBtn.vue'

import type { IUnsubscribePage
} from 'src/api/pro/unsubscribePage';
import {
  createUnsubscribePage, updateUnsubscribePage,
  getUnsubscribePagesCount, getUnsubscribePagesData
} from 'src/api/pro/unsubscribePage'

const columns: QTableColumn[] = [
  {
    name: 'language',
    required: true,
    label: '语言',
    align: 'left',
    field: 'language',
    sortable: true
  },
  {
    name: 'htmlContent',
    required: false,
    label: '预览',
    align: 'left',
    field: 'htmlContent',
    sortable: true
  }
]

async function getRowsNumberCount (filterObj: TTableFilterObject) {
  const { data } = await getUnsubscribePagesCount(filterObj.filter)
  return data || 0
}

async function onRequest (filterObj: TTableFilterObject, pagination: IRequestPagination) {
  const { data } = await getUnsubscribePagesData(filterObj.filter, pagination)
  return data || []
}

const { pagination, rows, filter, onTableRequest, loading, addNewRow } = useQTable({
  getRowsNumberCount,

  onRequest
})

// #region 新增退订页面
import { notifySuccess, showComponentDialog, showDialog } from 'src/utils/dialog'

import { translations } from 'src/i18n'
import type { IPopupDialogParams} from 'src/components/popupDialog/types';
import { PopupDialogFieldType } from 'src/components/popupDialog/types'
import UnsubscribePageDialog from 'src/pages/unsubscribe/UnsubscribePageDialog.vue'

function getPopupParams (data: IUnsubscribePage | null = null) {
  // 获取所有的语言
  const dialogParams: IPopupDialogParams = {
    title: data ? '编辑退定页面' : '新增退订页面',
    oneColumn: true,
    fields: [
      {
        name: 'language',
        label: '语言',
        type: PopupDialogFieldType.selectOne,
        options: translations.map(x => ({ label: x.label, value: x.locale })),
        required: true,
        emitValue: true,
        value: data?.language,
        disable: !!data
      },
      {
        name: 'htmlContent',
        label: 'HTML内容',
        type: PopupDialogFieldType.editor,
        required: true,
        value: data?.htmlContent
      }
    ],
    customBtns: [
      {
        label: '预览',
        color: 'secondary',
        onClick: previewUnsubscribePage
      }
    ]
  }
  return dialogParams
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
async function previewUnsubscribePage (pageData: Record<string, any>) {
  await showComponentDialog(UnsubscribePageDialog, {
    htmlContent: pageData.htmlContent
  })
}

async function onNewUnsubscribePage () {
  const initParams = getPopupParams()

  const result = await showDialog(initParams)
  if (!result.ok) return

  // 判断
  const { data: newPage } = await createUnsubscribePage(result.data as IUnsubscribePage)
  addNewRow(newPage)

  notifySuccess('新增成功')
}
// #endregion

// #region 修改与预览
// eslint-disable-next-line @typescript-eslint/no-explicit-any
async function onModifyUnsubscribePage (unsubscribePage: Record<string, any>) {
  const pageData = unsubscribePage as IUnsubscribePage
  const initParams = getPopupParams(pageData)

  const result = await showDialog(initParams)
  if (!result.ok) return

  // 更新
  await updateUnsubscribePage(pageData.id, result.data.htmlContent)
  pageData.htmlContent = result.data.htmlContent

  notifySuccess('更新成功')
}

function onPreviewUnsubscribePage (unsubscribePageId: number) {
  const url = `/pages/unsubscribe/pls-give-me-a-shot?unsubscribeId=${unsubscribePageId}`
  window.open(url, '_blank')
}
// #endregion
</script>

<style lang="scss">
.unsubscribe_content_thubnail {
  height: 120px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  word-break: break-all;
  word-wrap: break-word;
  display: -webkit-box;
  -webkit-box-orient: vertical;
  width: 100% !important;
}

.hover-overlay {
  // 透明
  background-color: rgba(214, 42, 42, 0.5) !important;
}
</style>
