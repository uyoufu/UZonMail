<template>
  <div v-show="showProgress" class="q-mr-md cursor-pointer">
    <LinearProgress :progress-value="progressValue" size="18px" :width="160">
    </LinearProgress>
    <q-menu v-if="sendingGroups.length > 0" v-model="showMore" :offset="[0, 8]">
      <q-list dense style="min-width: 160px" class="q-px-sm q-py-xs">
        <q-item class="border-radius-4" v-for="item in sendingGroups" :key="item.id" clickable v-close-popup
          @click="showSendingGroupDetail(item)">
          <q-item-section class="text-subtitle max-width-200 ellipsis">
            {{ item.subjects }}
          </q-item-section>
          <q-item-section side class="text-primary">{{ getSendingGroupProgressLabel(item) }}</q-item-section>
        </q-item>
      </q-list>
    </q-menu>
  </div>
</template>

<script lang="ts" setup>
// 进度条
import LinearProgress from 'src/components/Progress/LinearProgress.vue'

// 打开详细进度弹窗

import type { IRunningSendingGroup } from 'src/api/sendingGroup';
import { getRunningSendingGroups } from 'src/api/sendingGroup'
const sendingGroups: Ref<IRunningSendingGroup[]> = ref([])
const showMore = ref(false)
watch(sendingGroups, () => {
  if (showMore.value) {
    showMore.value = sendingGroups.value.length > 0
  }
})
async function onOpenSendingDetail () {
  const { data } = await getRunningSendingGroups()
  // 获取正在发送的邮箱组
  sendingGroups.value = data || []
}
const progressValue = computed(() => {
  // 计算总进度
  return sendingGroups.value.reduce((prev, current) => prev + current.progress, 0) / sendingGroups.value.length
})
const showProgress = computed(() => {
  return progressValue.value - 1 > -0.0001 || progressValue.value < 0.0001
})
onMounted(async () => {
  // 获取当前用户的发送进度
  await onOpenSendingDetail()
})

function getSendingGroupProgressLabel (sendingGroupInfo: IRunningSendingGroup) {
  return `${(sendingGroupInfo.progress * 100).toFixed(1)}%`
}
const router = useRouter()
async function showSendingGroupDetail (sendingGroupInfo: IRunningSendingGroup) {
  await router.push({
    name: 'SendDetailTable',
    query: {
      sendingGroupId: sendingGroupInfo.id,
      tagName: sendingGroupInfo.id
    }
  })
}

// 注册消息
import { useNotifyRegister } from './notifyFromServer'
useNotifyRegister()

// 注册进度回调
import { subscribeOne } from 'src/signalR/signalR'
import type { ISendingGroupProgressArg } from 'src/signalR/types';
import { UzonMailClientMethods, SendingGroupProgressType } from 'src/signalR/types'
import { notifySuccess } from 'src/utils/dialog'
// 注册单个发件组进度回调
function onSendingGroupProgressChanged (arg: ISendingGroupProgressArg) {
  let group = sendingGroups.value.find(item => item.id === arg.sendingGroupId)
  if (!group) {
    group = {
      id: arg.sendingGroupId,
      subjects: arg.subject,
      progress: arg.current / arg.total,
      totalCount: arg.total,
      sentCount: arg.current,
      successCount: arg.successCount
    }
    sendingGroups.value.push(group)
  }

  if (arg.progressType === SendingGroupProgressType.start) {
    // 开始发送
    return
  }

  if (arg.progressType === SendingGroupProgressType.end) {
    // 结束发送
    const index = sendingGroups.value.findIndex(item => item.id === arg.sendingGroupId)
    if (index < 0) return
    // 移除
    sendingGroups.value.splice(index, 1)

    notifySuccess(`邮件任务：${arg.sendingGroupId} 发送完成`)
    return
  }

  // 更新值
  group.totalCount = arg.total
  group.sentCount = arg.current
  group.progress = arg.current * 1.0 / arg.total
}
subscribeOne(UzonMailClientMethods.sendingGroupProgressChanged, onSendingGroupProgressChanged)
</script>
<style lang="scss" scoped></style>
