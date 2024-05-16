<template>
  <q-card class="global-message-container">
    <div class="gm-shade" />
    <SendingProgress @close="closeProgress" />
    <div id="capture" v-html="htmlValue" />
  </q-card>
</template>

<script>
import ws from '@/utils/websocket'
import toImage from '@/utils/html2image'
import SendingProgress from '@/views/send/components/sendingProgress.vue'

import _ from 'lodash'

export default {
  components: { SendingProgress },
  props: {
    message: {
      type: Object,
      default() {
        return {
          result: { html: '<div>test</div>' }
        }
      }
    }
  },

  data() {
    return {}
  },

  computed: {
    htmlValue() {
      return this.message.result.html
    }
  },

  watch: {
    message(newValue) {
      if (!newValue) return

      if (newValue.command === 'html2image') {
        this.$nextTick(async() => {
          // 转成图片发送回去
          // 生成模板预览图
          // 生成图片
          const imageUrl = await toImage(document.getElementById('capture'))
          const res = _.cloneDeep(newValue)
          res.result = imageUrl

          // console.log('回发消息：', res)
          ws.sendRequest(res)
        })
      }
    }
  },

  methods: {
    closeProgress() {
      console.log('closeProgress')
      this.$emit('close')
    }
  }
}
</script>

<style lang='scss'>
.global-message-container {
  overflow: hidden !important;
  position: relative;
  width: 450px;
  height: 130px;
  padding: 10px;

  .gm-shade {
    background-color: white;
    opacity: 0.6;
    position: absolute;
    top: 0;
    left: 0;
    bottom: 0;
    right: 0;
  }
}
</style>
