<template>
  <q-dialog ref="dialogRef" :persistent="true">
    <q-card class="column items-center items-start">
      <div class="image-cropper__container">
        <VueCropper ref="cropperRef" :img="img" :outputSize="option.size" :outputType="option.outputType" :info="true"
          :full="option.full" :fixed="option.fixed" :fixedNumber="option.fixedNumber" :can-move="option.canMove"
          :can-move-box="option.canMoveBox" :fixed-box="option.fixedBox" :original="option.original"
          :auto-crop="option.autoCrop" :auto-crop-width="option.autoCropWidth" :auto-crop-height="option.autoCropHeight"
          :center-box="option.centerBox" :high="option.high" mode="cover">
        </VueCropper>
      </div>

      <div class="row justify-end q-pa-sm q-gutter-md">
        <CancelBtn @click="onDialogCancel" />
        <OkBtn @click="onOkClick" />
      </div>
    </q-card>
  </q-dialog>
</template>

<script lang="ts" setup>
import { useDialogPluginComponent } from 'quasar'
defineEmits([
  // 必需；需要指定一些事件
  // （组件将通过useDialogPluginComponent()发出）
  ...useDialogPluginComponent.emits
])
const { dialogRef, onDialogOK, onDialogCancel } = useDialogPluginComponent()

import 'vue-cropper/dist/index.css'
// 组件中使用
import { VueCropper } from 'vue-cropper'

import OkBtn from '../quasarWrapper/buttons/OkBtn.vue'
import CancelBtn from '../quasarWrapper/buttons/CancelBtn.vue'

defineProps({
  img: {
    type: [Blob, String],
    default: () => 'https://avatars2.githubusercontent.com/u/15681693?s=460&v=4'
  }
})

const option = ref({
  size: 1,
  full: false,
  outputType: 'png',
  canMove: false,
  fixedBox: false,
  original: false,
  canMoveBox: true,
  autoCrop: true,
  // 只有自动截图开启 宽度高度才生效
  autoCropWidth: 200,
  autoCropHeight: 200,
  centerBox: true,
  high: true,
  fixed: true,
  fixedNumber: [1, 1]
})

const cropperRef = ref<InstanceType<typeof VueCropper> | null>(null)
function getBlobDataFromCropper (): Promise<Blob> {
  const promise = new Promise<Blob>(resolve => {
    cropperRef.value.getCropBlob((data: Blob) => {
      resolve(data)
    })
  })
  return promise
}
async function onOkClick () {
  const blob = await getBlobDataFromCropper()
  onDialogOK(blob)
}
</script>

<style lang="scss" scoped>
.image-cropper__container {
  width: 440px;
  height: 400px;
  overflow: hidden;
}
</style>
