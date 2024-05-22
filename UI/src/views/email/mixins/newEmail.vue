<script>
import { newEmail } from '@/api/group'
import { notifySuccess } from '@/components/iPrompt'


export default {
  data() {
    return {
      isShowNewEmailDialog: false,
      initNewEmailParams: {
        title: this.$t('new'),
        tooltip: '',
        api: newEmail,
        // type 可接受的值：text/password/textarea/email/search/tel/file/number/url/time/date
        fields: []
      }
    }
  },
  computed: {
    newEmailTitle() {
      if (this.group.groupType === 'send') {
        return this.$t('newOutbox')
      }
      return this.$t('newInbox')
    }
  },
  methods: {
    openNewEmailDialog() {
      // 添加 fields
      const fields = [{
        name: 'groupId',
        type: 'text',
        label: this.$t('table.groupId'),
        required: true,
        readonly: false,
        hidden: true
      },
      {
        name: 'userName',
        type: 'text',
        label: this.$t('table.userName'),
        required: true
      },
      {
        name: 'email',
        type: 'email',
        label: this.$t('table.email'),
        required: true
      }]

      const emailSender = [
        {
          name: 'smtp',
          type: 'text',
          label: this.$t('table.smtp'),
          required: true
        },
        {
          name: 'password',
          type: 'password',
          label: this.$t('table.password'),
          required: true
        }]
      if (this.group.groupType === 'send') {
        fields.push(...emailSender)
      }
      fields[0].default = this.group._id
      this.initNewEmailParams.fields = fields
      this.initNewEmailParams.title = this.newEmailTitle

      this.isShowNewEmailDialog = true
    },

    addedNewEmail(data) {
      // 查看是否存在，如果存在，替换
      const index = this.data.findIndex(d => d._id === data._id)
      if (index > -1) this.data.splice(index, 1, data)
      else this.data.push(data)

      this.isShowNewEmailDialog = false
      notifySuccess(this.$t('addSuccess'))
    }
  }
}
</script>
