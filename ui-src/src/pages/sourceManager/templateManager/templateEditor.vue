<template>
  <div class="full-height column no-wrap">
    <div v-if="showTitleBar" class="row items-center justify-between bg-white editor-title">
      <q-btn flat icon="west" class="q-px-xs q-ml-md" size="sm" @click="onBackToTemplateManager">
        <AsyncTooltip :tooltip="translateTemplate('backToTemplateManager')" />
      </q-btn>

      <q-input borderless dense v-model="templateName" :placeholder="translateTemplate('pleaseInputTemplateName')">
        <template v-slot:prepend>
          <q-icon color="primary" name="article" size="xs">
            <AsyncTooltip :tooltip="translateTemplate('templateName')" />
          </q-icon>
        </template>

        <template v-slot:append>
          <q-btn flat icon="save" color="secondary" class="q-px-xs q-ml-md" size="sm" @click="onSaveTemplate">
            <AsyncTooltip :tooltip="translateTemplate('saveTemplate')" />
          </q-btn>
        </template>
      </q-input>
      <div></div>
    </div>

    <TinymceEditor ref="editorRef" class="col" v-model="editorValue" tinymce-script-src="/tinymce/tinymce.min.js"
      :init="tinymceInit" />

    <div v-if="copilotRunningTip" class="q-mt-xs">
      <q-circular-progress class="q-mr-sm" rounded indeterminate size="xs" :thickness="0.3" color="primary"
        track-color="secondary" center-color="white">
      </q-circular-progress>
      <span>{{ copilotRunningTip }}</span>
    </div>
  </div>
</template>

<script lang="ts" setup>
import AsyncTooltip from 'src/components/asyncTooltip/AsyncTooltip.vue'
import logger from 'loglevel'
import { translateAI, translateTemplate } from 'src/i18n/helpers'

defineOptions({
  name: 'TemplateEditor'
})

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
    notifyError(translateTemplate('pleaseInputTemplateName'))
    return
  }

  // 生成缩略图并上传到服务器
  const node = tinymceEditor.value?.getBody()
  if (!node) {
    notifyError(translateTemplate('saveFailedCannotGenerateThumbnail'))
    return
  }

  await notifyUntil(
    async () => {
      const config = useConfig()

      let blob: Blob | null = null
      try {
        blob = await toBlob(node, {
          backgroundColor: 'white',
          skipFonts: true,
          width: 600,
          height: 600,
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
      }
      catch (err) {
        logger.error('[templateEditor] 生成缩略图失败: %O', err)
        notifyError(translateTemplate('saveFailedCurrentContentStructureInvalid'))
      }

      // 保存模板
      const templateData = {
        id: templateId.value,
        name: templateName.value,
        content: editorValue.value
      }
      const {
        data: { id }
      } = await upsertEmailTemplate(templateData)
      if (blob) {
        await uploadToStaticFile('template-thumbnails', `${id}.png`, blob)
      }

      templateId.value = id as number
    },
    translateTemplate('saveTemplate'),
    translateTemplate('savingTemplate')
  )
  notifySuccess(translateTemplate('saveTemplateSuccess'))
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

import { useI18n } from 'vue-i18n'
import { useTinyMceAICopilot } from './useTinyMceAICopilot'
const { copilotRunningTip, onGenerateContentByCopilot, onEnhanceContentByCopilot } = useTinyMceAICopilot(editorValue)

const { locale } = useI18n()

const tinymceInit = computed(() => {
  return {
    menubar: 'file edit view insert format tools table aiCopilot help',
    menu: {
      aiCopilot: { title: 'AI', items: 'aiBodyGeneration aiBodyEnhancement' },
    },
    plugins: 'advlist anchor autolink charmap code fullscreen help image insertdatetime link lists media preview searchreplace table visualblocks wordcount',
    toolbar: 'aiGenerate | undo redo | styles | bold italic underline strikethrough | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | link image | code',
    height: props.height,
    promotion: false,
    branding: false,
    language: locale.value.replace('-', '_'),
    placeholder: translateTemplate('templateEditorPlaceholder'),
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    setup: (editor: any) => {
      // 保存 editor 实例
      tinymceEditor.value = editor

      // 注册 ai 按钮
      // icons from: https://www.tiny.cloud/docs/tinymce/latest/editor-icon-identifiers/
      editor.ui.registry.addButton('aiGenerate', {
        icon: 'ai',
        tooltip: translateAI('generateBodyWithCopilot'),
        onAction: async () => {
          logger.debug('[TemplateEditor] AI button clicked')
          await onGenerateContentByCopilot()
        }
      })


      // #region AI 相关菜单
      // 参考 https://www.tiny.cloud/docs/tinymce/6/creating-custom-menu-items/
      editor.ui.registry.addMenuItem('aiBodyGeneration', {
        text: translateAI('contentGeneration'),
        onAction: async () => {
          logger.debug('[TemplateEditor] aiBodyGeneration menu item clicked')
          await onGenerateContentByCopilot()
        }
      })

      editor.ui.registry.addMenuItem('aiBodyEnhancement', {
        text: translateAI('contentEnhancement'),
        onAction: async () => {
          logger.debug('[TemplateEditor] aiBodyEnhancement menu item clicked')
          await onEnhanceContentByCopilot()
        }
      })
      // #endregion
    }

    // TODO: 此实现有 Bug, 后期再实现
    // images_upload_handler: (blobInfo: any, progress: (value: number) => void) => {
    //   logger.debug('[tinymce] 上传图片:', blobInfo, blobInfo.blobUri(), progress)

    //   // 判断是否是域名部署
    //   const config = useConfig()
    //   if (!config.baseUrl.startsWith('https') || config.baseUrl.indexOf(":") > -1) {
    //     logger.info('[tinymce] 当前配置不支持图片上传, 仅支持域名部署')
    //     return new Promise((resolve) => {
    //       resolve(blobInfo.blobUri())
    //     })
    //   }

    //   logger.debug('[tinymce] 开始上传图片到服务器')
    //   return ''
    // }
  }
})

const editorRef: Ref<{
  rerender: (options: { language: string }) => void
} | null> = ref(null)
watch(locale, (newValue) => {
  logger.debug('[TemplateEditor] 语言切换，更新编辑器语言为: ', newValue, editorRef.value)
  if (editorRef.value) {
    editorRef.value.rerender({ language: newValue.replace('-', '_') })
  }
})

// 上传图片到服务器

// 上传本地图片

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
