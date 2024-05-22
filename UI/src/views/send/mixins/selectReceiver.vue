<script>
import { okCancle } from '@/components/iPrompt'
import SelectEmail from '../components/selectEmail'

export default {
  data() {
    return {
      // 收件箱
      receivers: []
    }
  },

  methods: {
    async openSelectReceiversDialog() {
      // 打开
      const res = await okCancle(this.$t('select_receiver'), '', {
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
        value: this.receivers,

        groupType: 'receive'
      })

      if (!res) return

      this.receivers = res.data
    },

    removeReceiver(receiver) {
      const index = this.receivers.findIndex(
        re => re.type === receiver.type && re._id === receiver._id
      )
      if (index > -1) this.receivers.splice(index, 1)
    }
  }
}
</script>
