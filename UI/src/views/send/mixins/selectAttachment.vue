<script>
import Path from 'path'
import ws from '@/utils/websocket'

export default {
  data() {
    return {
      attachments: []
    }
  },
  methods: {
    // 获取fileName
    getFileBaseName(fileFullName) {
      return Path.basename(fileFullName.split('\\').join('/'))
    },

    // 触发选择
    async sendSignToSelectAttachment() {
      const result = await ws.sendRequest({
        name: 'SelectFiles'
      })

      console.log('sendSignToSelectAttachment:', result)
      if (result.status !== 200) return

      for (const fullName of result.result) {
        if (this.attachments.findIndex(attName => attName === fullName) > -1) { continue }

        this.attachments.push(fullName)
      }
    },

    removeAttachment(fileFullName) {
      // 移除选择的附件
      this.attachments = this.attachments.filter(f => f !== fileFullName)
    }
  }
}
</script>

<style>
</style>
