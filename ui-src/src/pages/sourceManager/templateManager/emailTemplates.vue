<template>
  <q-table class="full-height" :rows="rows" row-key="id" virtual-scroll v-model:pagination="pagination" dense
    hide-header grid :loading="loading" :filter="filter" binary-state-sort @request="onTableRequest">
    <template v-slot:top-left>
      <CreateBtn @click="onNewEmailTemplate" tooltip="新增邮件模板" />
      <ImportBtn class="q-ml-sm" @click="onImportTemplateFromHtml" :tooltip="['导入模板', '文件名为模板名']" />
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
                <AsyncTooltip tooltip="单击编辑模板" />
              </div>
              <div class="text-secondary">ID:{{ props.row.id }}</div>
            </div>
            <ContextMenu :items="contextItemsForError" :value="props.row"></ContextMenu>
          </template>

          <div class="absolute-bottom row items-center justify-center">
            <div class="text-h6 q-mr-sm hover-underline" @click.self.stop="onEditTemplateClick(props.row)">
              {{ props.row.name }}
              <AsyncTooltip tooltip="单击编辑模板" />
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

import CreateBtn from 'src/components/quasarWrapper/buttons/CreateBtn.vue'
import ImportBtn from 'src/components/quasarWrapper/buttons/ImportBtn.vue'
import AsyncTooltip from 'src/components/asyncTooltip/AsyncTooltip.vue'
import type { IEmailTemplate } from 'src/api/emailTemplate';
import { deleteEmailTemplate, upsertEmailTemplate } from 'src/api/emailTemplate'
import type { IContextMenuItem } from 'src/components/contextMenu/types'
import { confirmOperation, notifySuccess } from 'src/utils/dialog'

// 模板接口
import { useEmailTemplateTable } from './compositions'
const { pagination, rows, filter, onTableRequest, loading, deleteRowById, getTemplateImage, onPreviewThumbnail, addNewRow } = useEmailTemplateTable()

// 打开模板编辑器
const router = useRouter()
async function onNewEmailTemplate () {
  // 新增或编辑
  await router.push({
    name: 'templateEditor'
  })
}
// 导入模板
import { selectFile, saveFileSmart } from 'src/utils/file'
async function onImportTemplateFromHtml () {
  const { ok, data: buffer, files } = await selectFile()
  if (!ok) return

  notifySuccess('请保证文件格式为 utf-8 编码的文本文件')

  // 获取第一个文件
  const file = files?.item(0)
  const templateName = file?.name.substring(0, file.name.lastIndexOf('.')) || '未命名'
  const templateContent = (new TextDecoder('utf8')).decode(buffer as ArrayBuffer)

  // 向服务器请求新建模板
  const data = {
    name: templateName,
    content: templateContent,
    description: '从文件导入'
  }
  const { data: newTemplate } = await upsertEmailTemplate(data)

  // 新增数据
  addNewRow(newTemplate)

  notifySuccess('导入成功')
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
async function onDeleteEmailTemplate (templateItem: Record<string, any>) {
  const templateData = templateItem as IEmailTemplate
  // 提示
  const confirm = await confirmOperation('删除确认', `确认删除模板: ${templateData.name} 吗？`)
  if (!confirm) return

  // 向服务器请求删除模板
  await deleteEmailTemplate(templateData.id as number)

  // 更新删除
  deleteRowById(templateData.id)

  notifySuccess('删除成功')
}
// eslint-disable-next-line @typescript-eslint/no-explicit-any
async function onEditTemplateClick (value: Record<string, any>) {
  await router.push({
    name: 'templateEditor',
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
const templateContextMenuItems = ref<IContextMenuItem[]>([
  {
    name: 'preview',
    label: '预览',
    tooltip: '查看预览图',
    onClick: async () => {
      await router.push({
        name: 'templateEditor'
      })
    }
  },
  {
    name: 'edit',
    label: '编辑',
    tooltip: '编辑当前模板',
    onClick: onEditTemplateClick
  },
  {
    name: 'export',
    label: '导出',
    tooltip: '导出当前模板',
    onClick: onExportTemplateClick
  },
  {
    name: 'delete',
    label: '删除',
    tooltip: '删除当前模板',
    color: 'negative',
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
