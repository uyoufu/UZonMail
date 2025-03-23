<template>
  <q-dialog ref='dialogRef' @hide="onDialogHide" :persistent='true'>
    <q-card class='column justify-start q-pa-sm'>
      <div class="text-subtitle1 q-mb-md text-primary row items-center">
        <div>
          {{ title }}
        </div>
        <q-circular-progress v-if="existSendingGroupId" class="q-ml-sm" rounded indeterminate size="16px"
          :thickness="0.3" color="primary" track-color="secondary" center-color="white">
        </q-circular-progress>
      </div>

      <LinearProgress v-if="existSendingGroupId" size="32px" :value="progressValue" :width="400" />

      <div v-else class="width-400 column items-center q-mb-md">
        <q-circular-progress rounded indeterminate size="40px" :thickness="0.3" color="primary" track-color="secondary"
          center-color="white" class="q-mb-md">
        </q-circular-progress>
        <div>正在处理发件数据, 请稍候...</div>
      </div>

      <div v-if="existSendingGroupId" class="row justify-end q-mt-md q-gutter-sm">
        <CancelBtn @click="OnCancelSending" tooltip="取消发件" />
        <CommonBtn @click="onToggleTaskSending" :label="toggleLabel" :tooltip="toggleTooltip" />
        <OkBtn @click="onSendBackGround" label="后台" tooltip="在后台发件" />
      </div>

    </q-card>
  </q-dialog>
</template>

<script lang='ts' setup>
/**
 * warning: 该组件是一个弹窗的示例，不可直接使用
 * 参考：http://www.quasarchs.com/quasar-plugins/dialog#composition-api-variant
 */

import { useDialogPluginComponent } from 'quasar'
defineEmits([
  // 必需；需要指定一些事件
  // （组件将通过useDialogPluginComponent()发出）
  ...useDialogPluginComponent.emits
])
const { dialogRef, onDialogOK, onDialogCancel, onDialogHide } = useDialogPluginComponent()

import LinearProgress from 'src/components/Progress/LinearProgress.vue'
import CommonBtn from 'src/components/quasarWrapper/buttons/CommonBtn.vue'
import OkBtn from 'src/components/quasarWrapper/buttons/OkBtn.vue'
import CancelBtn from 'src/components/quasarWrapper/buttons/CancelBtn.vue'

import { confirmOperation, notifyError, notifySuccess } from 'src/utils/dialog'

import logger from 'loglevel'

const props = defineProps({
  // 发件组 id
  // 若有，说明是查看
  sendingGroupId: {
    type: Number,
    required: false
  },

  // 发件接口
  sendingApi: {
    type: Function,
    required: false
  },

  title: {
    type: String,
    default: () => '发送进度'
  }
})

const sendingGroupIdRef = ref(props.sendingGroupId)
onMounted(async () => {
  // 当有 api 时，说明是新增，先调用 api
  if (typeof props.sendingApi === 'function') {
    try {
      const { data: groupDoc } = await props.sendingApi()
      sendingGroupIdRef.value = groupDoc.id
    } catch (error) {
      logger.error(error)
      notifyError((error as Error).message)
      // 关闭进度
      onDialogCancel()
      return
    }
  }

  if (!sendingGroupIdRef.value) return

  // 有 id 时，获取 id 信息
  logger.debug(sendingGroupIdRef.value)
})
const existSendingGroupId = computed(() => {
  return sendingGroupIdRef.value
})

// 显示一个初始值
const progressValue = ref(0.01)

// 发送进度
import { subscribeOne } from 'src/signalR/signalR'
import { UzonMailClientMethods, ISendingGroupProgressArg, SendingGroupProgressType } from 'src/signalR/types'
async function onEmailGroupSendingProgressChanged (progress: ISendingGroupProgressArg) {
  console.log('onEmailGroupSendingProgressChanged', progress)
  if (!progress) return
  if (sendingGroupIdRef.value && progress.sendingGroupId !== sendingGroupIdRef.value) return
  if (progress.progressType === SendingGroupProgressType.end) {
    onDialogCancel()
    // 不提示成功，因为全局进度条也在监听事件，由全局发出通知即可

    // 跳转到明细中
    const sendingGroupId = sendingGroupIdRef.value || progress.sendingGroupId
    router.push({
      name: 'SendDetailTable',
      query: {
        sendingGroupId,
        tagName: sendingGroupId
      }
    })

    return
  }

  // 更新进度
  progressValue.value = progress.current / progress.total
}
subscribeOne(UzonMailClientMethods.sendingGroupProgressChanged, onEmailGroupSendingProgressChanged)

// 取消发件
import { cancelSending, pauseSending, restartSending } from 'src/api/emailSending'
async function OnCancelSending () {
  const confirm = await confirmOperation('取消发件', '确定取消发件吗？')
  if (!confirm) return
  // 开始取消
  await cancelSending(sendingGroupIdRef.value as number)
  notifySuccess('取消成功')
  // 关闭窗体
  onDialogCancel()
}

const isSendingPause = ref(false)
const toggleLabel = computed(() => {
  return isSendingPause.value ? '继续' : '暂停'
})
const toggleTooltip = computed(() => {
  return isSendingPause.value ? '继续发件' : '暂停发件'
})
import { useUserInfoStore } from 'src/stores/user'
const userInfoStore = useUserInfoStore()
async function onToggleTaskSending () {
  // 向服务器发送暂停/继续请求
  if (isSendingPause.value) {
    await restartSending(sendingGroupIdRef.value as number, userInfoStore.smtpPasswordSecretKeys)
  } else {
    await pauseSending(sendingGroupIdRef.value as number)
  }

  isSendingPause.value = !isSendingPause.value
}
const router = useRouter()
async function onSendBackGround () {
  // 关闭当前操作
  onDialogOK()

  console.log('发送到后台', sendingGroupIdRef.value)
  // 切换到明细中
  router.push({
    name: 'SendDetailTable',
    query: {
      sendingGroupId: sendingGroupIdRef.value,
      tagName: sendingGroupIdRef.value
    }
  })
}
</script>

<style lang='scss' scoped></style>
