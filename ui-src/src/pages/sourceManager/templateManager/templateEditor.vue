<template>
  <div class="full-height column no-wrap">
    <div v-if="showTitleBar" class="row items-center justify-between bg-white editor-title">
      <q-btn flat icon="west" class="q-px-xs q-ml-md" size="sm" @click="onBackToTemplateManager">
        <AsyncTooltip tooltip="返回" />
      </q-btn>

      <q-input borderless dense v-model="templateName" placeholder="输入模板名称">
        <template v-slot:prepend>
          <q-icon color="primary" name="article" size="xs">
            <AsyncTooltip tooltip="模板名称" />
          </q-icon>
        </template>

        <template v-slot:append>
          <q-btn flat icon="save" color="secondary" class="q-px-xs q-ml-md" size="sm" @click="onSaveTemplate">
            <AsyncTooltip tooltip="保存" />
          </q-btn>
        </template>
      </q-input>
      <div></div>
    </div>

    <TinymceEditor class="col" v-model="editorValue" tinymce-script-src="/tinymce/tinymce.min.js" :init="tinymceInit" />
  </div>
</template>

<script lang="ts" setup>
import AsyncTooltip from 'src/components/asyncTooltip/AsyncTooltip.vue'
import logger from 'loglevel'

const editorValue = defineModel<string>({
  default: ''
})

const props = defineProps({
  showTitleBar: {
    type: Boolean,
    default: true
  },
  height: {
    type: [String, Number],
    default: '100%'
  }
})

const templateId = ref(0)
const templateName = ref('')

import { getEmailTemplateById, upsertEmailTemplate } from 'src/api/emailTemplate'
import { notifyError, notifySuccess, notifyUntil } from 'src/utils/dialog'
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
import type { IRouteHistory } from 'src/layouts/components/tags/types'

// 编辑器配置
const router = useRouter()

// #region 编辑器顶部 title 操作方法
// 保存模板
import { toBlob } from 'html-to-image'
import { uploadToStaticFile } from 'src/api/file'
import { useConfig } from 'src/config'
import { useUserInfoStore } from 'src/stores/user'

const userInfoStore = useUserInfoStore()
async function onSaveTemplate () {
  if (!templateName.value) {
    notifyError('请输入模板名称')
    return
  }

  // 生成缩略图并上传到服务器
  const node = tinymceEditor.value?.getBody()
  if (!node) {
    notifyError('保存失败: 无法生成缩略图')
    return
  }

  await notifyUntil(
    async () => {
      const config = useConfig()
      const blob = await toBlob(node, {
        backgroundColor: 'white',
        filter: (node) => {
          logger.debug("[templateEditor] node: %O", node)

          if (node.nodeName !== 'IMG') return true

          const imgNode = node as HTMLImageElement
          if (!imgNode.src.startsWith('http') || imgNode.src.startsWith(config.baseUrl)) {
            return true
          }

          // 修改图片地址为本机代理
          imgNode.src = `${config.baseUrl}${config.api}/resource-proxy/stream?uri=${encodeURIComponent(imgNode.src)}&access_token=${userInfoStore.token}`
          return true
        }
      })

      // 保存模板
      const templateData = {
        id: templateId.value,
        name: templateName.value,
        content: editorValue.value
      }
      const {
        data: { id }
      } = await upsertEmailTemplate(templateData)
      await uploadToStaticFile('template-thumbnails', `${id}.png`, blob as Blob)

      templateId.value = id as number
    },
    '保存模板',
    '保存中...'
  )
  notifySuccess('保存成功')
}

async function onBackToTemplateManager () {
  await removeHistory(router, route as unknown as IRouteHistory, '/template/index')
}
// #endregion



// #region tinymce 相关配置
// 详细配置说明参考: https://juejin.cn/post/7377335032354947126
import TinymceEditor from '@tinymce/tinymce-vue'

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const tinymceEditor = ref<any>(null)
const tinymceInit = {
  plugins: 'advlist anchor autolink charmap code fullscreen help image insertdatetime link lists media preview searchreplace table visualblocks wordcount',
  toolbar: 'undo redo | styles | bold italic underline strikethrough | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | link image | code',
  height: props.height,
  promotion: false,
  branding: false,
  language: navigator.language.replace('-', '_'),
  placeholder: '在此处输入模板内容, 变量使用 {{  }} 号包裹, 例如 {{ variableName }}',
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  setup: (editor: any) => {
    tinymceEditor.value = editor
  }
}
// #endregion
</script>

<style lang="scss">
.q-editor__toolbar {
  align-items: center;
}

.tox-tinymce {
  border-radius: 0px !important;
}
</style>

<style lang="scss" scoped>
.editor-title {
  border-left: 2px solid #eeeeee;
  border-right: 2px solid #eeeeee;
}
</style>
