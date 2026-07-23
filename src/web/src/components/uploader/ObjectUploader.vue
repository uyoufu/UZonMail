<template>
  <q-uploader v-bind="$attrs" ref="uploaderRef" :factory="factoryFn" @added="onFileAdded" no-thumbnails
    @uploaded="onFileUploaded">
    <template v-slot:header="scope">
      <div class="row no-wrap items-center q-pa-sm q-gutter-xs">
        <q-btn v-if="scope.queuedFiles.length > 0" icon="clear_all" @click="scope.removeQueuedFiles" round dense flat>
          <q-tooltip>{{ translateComponents('clear') }}</q-tooltip>
        </q-btn>
        <q-btn v-if="scope.uploadedFiles.length > 0" icon="done_all" @click="scope.removeUploadedFiles" round dense
          flat>
          <q-tooltip>{{ translateComponents('removeUploadedFile') }}</q-tooltip>
        </q-btn>
        <q-spinner v-if="scope.isUploading" class="q-uploader__spinner" />
        <div class="col">
          <div class="q-uploader__title">{{ label }}</div>
          <!--<div class="q-uploader__subtitle">{{ scope.uploadSizeLabel }} / {{ scope.uploadProgressLabel }}</div>-->
        </div>
        <q-btn v-if="scope.canAddFiles" type="a" icon="add_box" @click="scope.pickFiles" round dense flat>
          <q-uploader-add-trigger />
          <q-tooltip>{{ translateComponents('selectFile') }}</q-tooltip>
        </q-btn>
        <!-- <q-btn v-if="scope.canUpload" icon="cloud_upload" @click="scope.upload" round dense flat>
          <q-tooltip>上传</q-tooltip>
        </q-btn> -->
        <q-btn v-if="canUpload" icon="cloud_upload" @click="scope.upload" round dense flat>
          <q-tooltip>{{ translateComponents('upload') }}</q-tooltip>
        </q-btn>
        <q-btn v-if="scope.isUploading" icon="clear" @click="scope.abort" round dense flat>
          <q-tooltip>{{ translateComponents('abortUpload') }}</q-tooltip>
        </q-btn>
      </div>
    </template>

    <template v-slot:list="scope">
      <q-list dense separator>
        <q-item v-for="file in scope.files" :key="file.__key">
          <q-item-section>
            <q-item-label class="full-width ellipsis">
              {{ file.name }}
            </q-item-label>

            <!-- <q-item-label caption>
              Status: {{ file.__status }}
            </q-item-label> -->

            <q-item-label caption>
              {{ file.__sizeLabel }} / {{ file.__progressLabel }}
            </q-item-label>
          </q-item-section>

          <q-item-section v-if="file.__img" thumbnail class="gt-xs">
            <img :src="file.__img.src">
          </q-item-section>

          <q-item-section top side>
            <q-btn class="gt-xs" size="12px" flat dense round icon="delete" @click="scope.removeFile(file)" />
          </q-item-section>
        </q-item>
      </q-list>
    </template>
  </q-uploader>
</template>

<script lang="ts" setup>
import logger from 'loglevel'

defineProps({
  label: {
    type: String,
    default: ''
  }
})

import { fileSha256 } from 'src/utils/file'
import type { IObsUploadedFile, IObsUploadedResult, IFileSha256Callback } from 'src/utils/file'
// 定义 v-model
const modelValue = defineModel({
  type: Array as PropType<IObsUploadedResult[]>,
  default: () => []
})
watch(modelValue, (newValue) => {
  addObsFile(newValue)
})

// 初始化显示
// 显示已经上传的文件信息
onMounted(() => {
  // 通过 modelValue 回显已经上传的文件
  // 后期有需要再实现
})

function addObsFile (files: IObsUploadedResult[]) {
  // 获取不存在的项
  const shas = uploaderRef.value.uploadedFiles.map(x => x.__sha256)
  const newFiles = files.filter(x => !shas.includes(x.__sha256))

  logger.debug('[objectUploader] modelValue changed:', files, shas, newFiles)
  uploaderRef.value.addFiles(newFiles.map(x => x as File))
}


import type { QUploaderFactoryObject } from 'quasar'
import { QUploader } from 'quasar'
const uploaderRef: Ref<QUploader> = ref(null as unknown as QUploader)
onMounted(() => {
  logger.debug('[objectUploader] uploaderRef:', uploaderRef.value)
})

// 上传地址配置
import { useUserInfoStore } from 'src/stores/user'
const userInfoStore = useUserInfoStore()
import { useConfig } from 'src/config'
const appConfig = useConfig()
function factoryFn (files: readonly IObsUploadedFile[]): Promise<QUploaderFactoryObject> {
  logger.debug('[objectUploader] uploader factory called:', files)
  return new Promise((resolve) => {
    // Retrieve JWT token from your store.
    const token = userInfoStore.token
    const uploadUrl = `${appConfig.baseUrl || process.env.BASE_URL}${appConfig.api}/file/upload-file-object`
    const result: QUploaderFactoryObject = {
      url: uploadUrl,
      method: 'POST',
      headers: [
        { name: 'Authorization', value: `Bearer ${token}` }
      ],
      formFields: [
        { name: 'sha256', value: files[0]!.__sha256 as string }
      ]
    }
    resolve(result)
  })
}

// 添加文件后的操作
import { getFileUsageId } from 'src/api/file'

const vm = getCurrentInstance()
function sha256Callback (params: IFileSha256Callback) {
  params.file.__progressLabel = params.progressLabel
  // 强制刷新
  vm?.proxy?.$forceUpdate()
}

async function onFileAdded (files: readonly IObsUploadedFile[]) {
  // 计算文件的 sha256 值，若已经上传过，则直接修改文件状态
  logger.debug('[objectUploader] onFileAdded files:', files, uploaderRef.value.queuedFiles)
  if (!uploaderRef.value) return

  for (const file of files) {
    if (file.__fileUsageId) {
      // 说明是原始数据，进行恢复
      // 修改文件状态
      uploaderRef.value.updateFileStatus(file, 'uploaded', file.size)
      continue
    }

    // 计算 hash 值
    const sha256 = await fileSha256(file, sha256Callback)
    // 保存 hash
    file.__sha256 = sha256

    // 向服务器请求文件是否已经上传过
    const { data: fileUsageId } = await getFileUsageId(sha256, file.name)
    if (fileUsageId < 0) continue

    // 说明文件已经上传过
    file.__fileUsageId = fileUsageId
    // 修改文件状态
    uploaderRef.value.updateFileStatus(file, 'uploaded', file.size)
    const queueIndex = uploaderRef.value.queuedFiles.findIndex(x => x.__key === file.__key)
    // 从队列中移除, 强制去修改
    // @ts-expect-error 允许直接修改
    uploaderRef.value.queuedFiles.splice(queueIndex, 1)
    // 添加到已上传文件列表
    // @ts-expect-error 允许直接修改
    uploaderRef.value.uploadedFiles.push(file)
    logger.debug('[objectUploader] queuedFiles:', uploaderRef.value.queuedFiles)
    // 保存到结果中
    updateModelValue(file)
  }
}

const canUpload = computed(() => {
  return uploaderRef.value?.queuedFiles.some(x => !x.__fileUsageId)
})

// 文件上传后的操作
import { notifyError } from 'src/utils/dialog'
import { translateComponents } from 'src/i18n/helpers'
function onFileUploaded ({ files, xhr }: { files: readonly IObsUploadedFile[], xhr: XMLHttpRequest }) {
  const file = files[0] as IObsUploadedFile
  const response = JSON.parse(xhr.responseText)
  if (!response.ok) {
    notifyError(response.message)
    return
  }

  // 向文件中记录 id
  file.__fileUsageId = response.data

  // 更新 v-model 值
  updateModelValue(file)
}
function updateModelValue (file: IObsUploadedFile) {
  if (modelValue.value.find(x => x.__sha256 === file.__sha256)) return

  const result: IObsUploadedResult = {
    __fileName: file.name,
    __sha256: file.__sha256,
    __key: file.__key,
    __sizeLabel: file.__sizeLabel,
    __progressLabel: file.__progressLabel,
    __fileUsageId: file.__fileUsageId
  }

  modelValue.value.push(result)
}

// 文件是否需要上传标记
const needUpload = defineModel('needUpload', {
  type: Boolean,
  default: false
})
watch(canUpload, (newValue) => {
  needUpload.value = newValue
})
</script>

<style lang="scss" scoped></style>
