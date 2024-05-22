<template>
  <q-card class="q-ma-md" style="width: 400px">
    <div class="q-pa-sm">
      {{ $t('sending_progress') }}{{ sendingData.receiverEmail }}, {{ sendingData.index }} /
      {{ sendingData.total }}
    </div>
    <q-linear-progress
      size="30px"
      rounded
      :value="getProgressValue()"
      color="orange"
    >
      <div class="absolute-full flex flex-center">
        <q-badge
          color="white"
          text-color="accent"
          :label="getProgressLable()"
        />
      </div>
    </q-linear-progress>
  </q-card>
</template>

<script>
import { getSendingInfo } from '@/api/send'
import { getHistoryGroupSendResult } from '@/api/history'

import { notifyError, notifySuccess } from '@/components/iPrompt'
export default {
  data() {
    return {
      sendingData: {
        index: 0,
        total: 1
      }
    }
  },

  async mounted() {
    const res = await getSendingInfo()
    this.sendingData = res.data
    if (this.sendingData.index < this.sendingData.total) {
      this.getProgressInfo()
    }
  },

  methods: {
    getProgressValue() {
      return this.sendingData.index / this.sendingData.total
    },

    getProgressLable() {
      return (
        ((this.sendingData.index * 100) / this.sendingData.total).toFixed(1) +
        ' %'
      )
    },

    getProgressInfo() {
      setTimeout(async() => {
        // 获取更新数据
        const res = await getSendingInfo()
        console.log('getProgressInfo:', res.data)
        this.sendingData = res.data
        if (this.sendingData.index < this.sendingData.total) {
          this.getProgressInfo()
        } else {
          // 获取发送结果
          const msgRes = await getHistoryGroupSendResult(
            this.sendingData.historyId
          )

          if (msgRes.data.ok) notifySuccess(msgRes.data.message)
          else notifyError(msgRes.data.message)

          this.$emit('close')
        }
      }, 2000)
    }
  }
}
</script>

<style>
</style>
