<template>
  <q-table class="full-height" :rows="rows" row-key="id" virtual-scroll v-model:pagination="pagination" dense
    hide-header grid :loading="loading" :filter="filter" binary-state-sort @request="onTableRequest">
    <template v-slot:top-left>
      <CreateBtn @click="onNewEmailTemplate" :tooltip="t('pages.templateManager.newTemplate')" />
      <ImportBtn class="q-ml-sm" @click="onImportTemplateFromHtml" :tooltip="[t('pages.templateManager.importTemplate'), t('pages.templateManager.importTemplateHint')]" />
    </template>

    <template v-slot:top-right>
      <SearchInput v-model="filter" />
    </template>

    <template v-slot:item="props">
      <div class="q-card rounded-borders q-ma-sm" style="height: 150px; width: 250px">
        <q-img :src="getTemplateImage(props.row)" error-src="/icons/undraw_mailbox_re_dvds.svg" spinner-color="white"
          class="full-width full-height cursor-pointer" fit="cover" position="left top"
          @click="onPreviewThumbnail(props.row)">

          <template v-slot:loading>
            <q-spinner-gears color="white" />
          </template>

          <template v-slot:error>
            <div class="absolute-bottom row items-center justify-center">
              <div class="text-h6 q-mr-sm hover-underline" @click.self.stop="onEditTemplateClick(props.row)">
                {{ props.row.name }}
                <AsyncTooltip :tooltip="t('pages.templateManager.clickToEdit')" />
              </div>
              <div class="text-secondary">ID:{{ props.row.id }}</div>
            </div>
            <ContextMenu :items="contextItemsForError" :value="props.row"></ContextMenu>
          </template>

          <div class="absolute-bottom row items-center justify-center">
            <div class="text-h6 q-mr-sm hover-underline" @click.self.stop="onEditTemplateClick(props.row)">
              {{ props.row.name }}
              <AsyncTooltip :tooltip="t('pages.templateManager.clickToEdit')" />
            </div>
            <div class="text-secondary">ID:{{ props.row.id }}</div>
          </div>

          <ContextMenu :items="templateContextMenuItems" :value="props.row"></ContextMenu>
        </q-img>
      </div>
    </template>
  </q-table>
</template>

<script lang="ts" setup>
import SearchInput from 'src/components/searchInput/SearchInput.vue'
import ContextMenu from 'src/components/contextMenu/ContextMenu.vue'

import CreateBtn from 'src/components/buttons/CreateBtn.vue'
import ImportBtn from 'src/components/buttons/ImportBtn.vue'
import AsyncTooltip from 'src/components/asyncTooltip/AsyncTooltip.vue'
import type { IEmailTemplate } from 'src/api/emailTemplate'
import { deleteEmailTemplate, upsertEmailTemplate } from 'src/api/emailTemplate'
import { ContextMenuIcon, type IContextMenuItem } from 'src/components/contextMenu/types'
import { confirmOperation, notifySuccess } from 'src/utils/dialog'
import { t } from 'src/i18n/helpers'

// 模板接口
import { useEmailTemplateTable } from './compositions'
const { pagination, rows, filter, onTableRequest, loading, deleteRowById, getTemplateImage, onPreviewThumbnail, addNewRow } = useEmailTemplateTable()

// 打开模板编辑器
const router = useRouter()
async function onNewEmailTemplate () {
  // 新增或编辑
  await router.push({
    name: 'TemplateEditor'
  })
}
// 导入模板
import { selectFile, saveFileSmart } from 'src/utils/file'
async function onImportTemplateFromHtml () {
  const { ok, data: buffer, files } = await selectFile()
  if (!ok) return

  notifySuccess(t('pages.templateManager.importEncodingHint'))

  // 获取第一个文件
  const file = files?.item(0)
  const templateName = file?.name.substring(0, file.name.lastIndexOf('.')) || t('pages.templateManager.untitled')
  const templateContent = (new TextDecoder('utf8')).decode(buffer as ArrayBuffer)

  // 向服务器请求新建模板
  const data = {
    name: templateName,
    content: templateContent,
    description: t('pages.templateManager.importedFromFile')
  }
  const { data: newTemplate } = await upsertEmailTemplate(data)

  // 新增数据
  addNewRow(newTemplate)

  notifySuccess(t('pages.templateManager.importSuccess'))
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
async function onDeleteEmailTemplate (templateItem: Record<string, any>) {
  const templateData = templateItem as IEmailTemplate
  // 提示
  const confirm = await confirmOperation(
    t('pages.templateManager.deleteConfirmation'),
    t('pages.templateManager.deleteTemplateConfirm', { name: templateData.name })
  )
  if (!confirm) return

  // 向服务器请求删除模板
  await deleteEmailTemplate(templateData.id as number)

  // 更新删除
  deleteRowById(templateData.id)

  notifySuccess(t('pages.variableManager.deleteSuccess'))
}
// eslint-disable-next-line @typescript-eslint/no-explicit-any
async function onEditTemplateClick (value: Record<string, any>) {
  await router.push({
    name: 'TemplateEditor',
    query: {
      templateId: value.id,
      tagName: value.id
    }
  })
}
// 导出模板
// eslint-disable-next-line @typescript-eslint/no-explicit-any
async function onExportTemplateClick (value: Record<string, any>) {
  await saveFileSmart(value.name + '.html', value.content)
}
// 右键菜单
const templateContextMenuItems = computed<IContextMenuItem[]>(() => [
  {
    name: 'preview',
    label: t('pages.templateManager.preview'),
    tooltip: t('pages.templateManager.viewPreview'),
    icon: ContextMenuIcon.visibility,
    onClick: async () => {
      await router.push({
        name: 'TemplateEditor'
      })
    }
  },
  {
    name: 'edit',
    label: t('pages.variableManager.edit'),
    tooltip: t('pages.templateManager.editCurrentTemplate'),
    icon: ContextMenuIcon.edit,
    onClick: onEditTemplateClick
  },
  {
    name: 'export',
    label: t('pages.templateManager.export'),
    tooltip: t('pages.templateManager.exportCurrentTemplate'),
    icon: ContextMenuIcon.download,
    onClick: onExportTemplateClick
  },
  {
    name: 'delete',
    label: t('pages.variableManager.delete'),
    tooltip: t('pages.templateManager.deleteCurrentTemplate'),
    color: 'negative',
    icon: ContextMenuIcon.delete,
    onClick: onDeleteEmailTemplate
  }
])
const contextItemsForError = computed(() => {
  return templateContextMenuItems.value.filter(item => item.name !== 'preview')
})
</script>

<style lang="scss" scoped>
:deep(.q-table__grid-content) {
  align-content: start;
  justify-content: space-around;
  overflow-y: auto;
}
</style>
