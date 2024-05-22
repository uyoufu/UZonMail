<script>
import { okCancle } from '@/components/iPrompt'
import SelectEmail from '../components/selectEmail'

export default {
  data() {
    return {
      // 发件箱
      senders: []
    }
  },

  methods: {
    // 打开发件人选择框
    async openSelectSendersDialog() {
      // 打开
      const res = await okCancle(this.$t('select_Sender'), '', {
        component: SelectEmail,

        // 如果要访问自定义组件中的
        // 路由管理器、Vuex存储等,
        // 则为可选：
        parent: this, // 成为该Vue节点的子元素
        // （“this”指向您的Vue组件）
        // （此属性在<1.1.0中称为“root”
        //  仍然可以使用，但建议切换到
        //  更合适的“parent”名称）

        // 传递给组件的属性
        // （上述“component”和“parent”属性除外）：
        value: this.senders,

        groupType: 'send'
      })

      console.log('openSelectSendersDialog:', res)

      if (!res) return

      this.senders = res.data
    },

    // 移除发件人
    removeSender(sender) {
      const index = this.senders.findIndex(
        re => re.type === sender.type && re._id === sender._id
      )
      if (index > -1) this.senders.splice(index, 1)
    }
  }
}
</script>
