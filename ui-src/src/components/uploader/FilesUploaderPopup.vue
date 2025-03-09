<template>
  <q-dialog ref='dialogRef' @hide="onDialogHide" :persistent='true'>
    <q-card class='column items-center justify-center q-pa-md'>
      <div class="row justify-center items-center q-gutter-xs">
        <span v-for="(label, index) in labels" :key="index" :class="labelClass(index)">{{ label }}</span>
      </div>
      <LinearProgress class="q-mt-md" size="30px" :value="progress" />
    </q-card>
  </q-dialog>
</template>

<script lang='ts' setup>
import LinearProgress from 'src/components/Progress/LinearProgress.vue'

/**
 * warning: 该组件是一个弹窗的示例，不可直接使用
 * 参考：http://www.quasarchs.com/quasar-plugins/dialog#composition-api-variant
 */

import { useDialogPluginComponent, format, throttle } from 'quasar'
import { PropType } from 'vue'
defineEmits([
  // 必需；需要指定一些事件
  // （组件将通过useDialogPluginComponent()发出）
  ...useDialogPluginComponent.emits
])
const { dialogRef, onDialogHide, onDialogOK/* onDialogCancel */ } = useDialogPluginComponent()

const props = defineProps({
  files: {
    type: Array as PropType<File[]>,
    required: true
  }
})
import { uploadFileObject } from 'src/api/file'
import dayjs from 'dayjs'
import { fileSha256, TFileSha256Callback } from 'src/utils/file'
import { AxiosProgressEvent } from 'axios'

interface IProgressInfo {
  virtualFile: boolean
  message: string
  transferredBytes: number
  totalBytes: number
}

const { humanStorageSize } = format
const startDate = Date.now()
const progressInfos: Ref<IProgressInfo[]> = ref([
  {
    virtualFile: false, // 是否是虚拟文件,虚拟文件不计算总字节数，仅用于计算进度
    message: '', // 消息
    transferredBytes: 0, // 已上传的字节数
    totalBytes: 0 // 总字节数
  }
])
const allTransferredBytes = computed(() => progressInfos.value.reduce((sum, info) => sum + info.transferredBytes, 0))
const allBytes = computed(() => progressInfos.value.reduce((sum, info) => sum + info.totalBytes, 0))
const progress = computed(() => allBytes.value === 0 ? 0 : allTransferredBytes.value / allBytes.value)
const labels = computed(() => {
  if (progressInfos.value.length === 0) return '等待上传中...'

  let transferredBytes = 0, totalBytes = 0
  for (const info of progressInfos.value) {
    if (info.virtualFile) continue
    transferredBytes += info.transferredBytes
    totalBytes += info.totalBytes
  }
  const lastInfo = progressInfos.value[progressInfos.value.length - 1] as IProgressInfo
  const { message } = lastInfo
  const totalCount = props.files.length

  const timeSpan = Date.now() - startDate
  const totalSpeed = `${humanStorageSize(transferredBytes)}/${humanStorageSize(totalBytes)} in ${dayjs(timeSpan).format('mm:ss')}`
  const speedPerSecond = transferredBytes * 1000 / (timeSpan || 1)
  const remainingTime = (totalBytes - transferredBytes) / speedPerSecond
  const results = [
    message,
    `[${progressInfos.value.length}/${totalCount}]`,
    `${humanStorageSize(speedPerSecond)}/s`,
    `[${totalSpeed}]`,
    `剩余 ${dayjs(remainingTime).format('mm:ss')}`
  ]
  return results
})
function labelClass (index: number) {
  const isOdd = index % 2
  return {
    'text-secondary': isOdd,
    'text-primary': !isOdd
  }
}

const updateProgressInfo = throttle((index: number, message: string, transferredBytes: number, totalBytes: number, virtualFile: boolean = false) => {
  if (progressInfos.value.length <= index) {
    // 添加新的进度信息
    progressInfos.value.push({
      message,
      transferredBytes,
      totalBytes,
      virtualFile
    })
    return
  }

  const info = progressInfos.value[index] as IProgressInfo
  info.message = message
  info.transferredBytes = transferredBytes
  info.totalBytes = totalBytes
}, 300)
onMounted(async () => {
  let index = 0
  function sha256Callback (callbackData: TFileSha256Callback) {
    updateProgressInfo(index, `正在计算 ${callbackData.file.name} 哈希值`, callbackData.computed, callbackData.file.size, true)
  }

  function onUploadProgress (progressEvent: AxiosProgressEvent) {
    const file = props.files[index] as File
    updateProgressInfo(index, `正在上传 ${file.name}`, progressEvent.loaded, progressEvent.total || 1)
  }

  const fileIds: number[] = []
  for (; index < props.files.length; index++) {
    const file = props.files[index] as File
    const sha256 = await fileSha256(file, sha256Callback)
    const { data: fileId } = await uploadFileObject(sha256, file, onUploadProgress)
    fileIds.push(fileId)
  }

  // 上传完成后，返回结果
  onDialogOK(fileIds)
})
</script>

<style lang='scss' scoped></style>
