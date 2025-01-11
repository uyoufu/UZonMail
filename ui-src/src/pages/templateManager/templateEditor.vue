<template>
  <q-editor ref="editorRef" class="full-height column no-wrap q-pa-xs" v-model="editorValue"
    :definitions="editorDefinitions" placeholder="在此处输入模板内容, 变量使用 {{  }} 号包裹, 例如 {{ variableName }}"
    :toolbar="editorToolbar">
    <template v-slot:templateName>
      <q-input borderless standout dense v-model="templateName" placeholder="输入模板名称">
        <template v-slot:prepend>
          <q-icon name="article" size="xs" />
        </template>
      </q-input>
    </template>

    <template v-slot:textColor>
      <q-btn-dropdown dense no-caps ref="textColorDropdownRef" no-wrap unelevated color="white" text-color="primary"
        label="颜色" size="sm">
        <div class="column justify-start q-pa-xs">
          <div class="row justify-start items-center">
            <q-icon name="format_paint" class="q-mr-sm" color="secondary" size="sm">
              <AsyncTooltip tooltip="文字颜色" />
            </q-icon>
            <q-color v-model="foreColor" no-header no-footer default-view="palette" :palette="foreColorPalette"
              @click="setColor('foreColor', foreColor)" />
          </div>

          <div class="row justify-start items-center">
            <q-icon name="highlight" class="q-mr-sm q-mt-xs" color="primary" size="sm">
              <AsyncTooltip tooltip="背景颜色" />
            </q-icon>
            <q-color v-model="highlightColor" default-view="palette" no-header no-footer
              :palette="highlightColorPalette" class="q-mt-sm" @click="setColor('backColor', highlightColor)" />
          </div>
        </div>
      </q-btn-dropdown>
    </template>
  </q-editor>
</template>

<script lang="ts" setup>
import AsyncTooltip from 'src/components/asyncTooltip/AsyncTooltip.vue'

const editorValue = ref('')
const templateId = ref(0)
const templateName = ref('')

import { getEmailTemplateById, upsertEmailTemplate } from 'src/api/emailTemplate'
import { notifyError, notifySuccess } from 'src/utils/dialog'
// 从服务器拉取内容
const route = useRoute()
onMounted(async () => {
  if (!route.query.templateId) return
  templateId.value = Number(route.query.templateId)
  // 获取数据
  const { data: templateData } = await getEmailTemplateById(templateId.value)
  templateName.value = templateData.name
  editorValue.value = templateData.content
})
import { removeHistory } from 'src/layouts/components/tags/routeHistories'
import { IRouteHistory } from 'src/layouts/components/tags/types'

// 编辑器配置
import { useWysiwygEditor } from './compositions'
const {
  editorDefinitions, editorToolbar, foreColor, highlightColor, editorRef, textColorDropdownRef,
  setColor, foreColorPalette, highlightColorPalette
} = useWysiwygEditor()
const router = useRouter()
Object.assign(editorDefinitions, {
  save: {
    tip: '保存模板',
    icon: 'save',
    label: '',
    handler: saveTemplate
  },
  back: {
    tip: '返回',
    icon: 'west',
    label: '',
    handler: () => {
      removeHistory(router, route as unknown as IRouteHistory, '/template/index')
    }
  }
})

editorToolbar.unshift(...[['back'],
  ['templateName'],
  ['save']])
// 保存模板
import domToImage from 'dom-to-image'
import { uploadToStaticFile } from 'src/api/file'

async function saveTemplate () {
  if (!templateName.value) {
    notifyError('请输入模板名称')
    return
  }

  // 生成缩略图并上传到服务器
  const blob = await new Promise((resolve, reject) => {
    const node = document.querySelector('.q-editor__content')
    if (!node) return reject('未找到编辑器内容')
    domToImage.toBlob(node, {
      bgcolor: 'white'
    })
      .then(function (blob: Blob) {
        resolve(blob)
      })
  })

  // 保存模板
  const templateData = {
    id: templateId.value,
    name: templateName.value,
    content: editorValue.value
  }
  const { data: { id } } = await upsertEmailTemplate(templateData)
  await uploadToStaticFile('template-thumbnails', `${id}.png`, blob as Blob)

  templateId.value = id as number
  notifySuccess('保存成功')
}
</script>

<style lang="scss" scoped>
:deep(.q-editor__toolbar) {
  align-items: center;
}
</style>
