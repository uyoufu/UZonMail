<script>
import ws from '@/utils/websocket'

export default {
  data() {
    return {
      isShowGlobalMessage: false,
      websocketMsg: {
        result: ''
      },

      globalProgressEventName: 'sendingInfo'
    }
  },

  async mounted() {
    // 打开 websocket 连接
    if (ws.isClosed) await ws.open()

    // 监听事件
    ws.$eventEmitter.on(
      this.globalProgressEventName,
      this.onGlobalMessage,
      this
    )
  },

  // 在此处清除注册的事件
  beforeDestroy() {
    ws.$eventEmitter.off(this.globalProgressEventName, () => {})
  },

  methods: {
    onGlobalMessage(message) {
      this.websocketMsg = message

      if (message.result.index === message.result.total) {
        this.isShowGlobalMessage = false
        return
      } else this.isShowGlobalMessage = true
    }
  }
}
</script>

<style>
</style>
