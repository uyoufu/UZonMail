<script>
import { newEmail } from '@/api/group'
import { notifySuccess } from '@/components/iPrompt'

const emailCommonInfo = [
  {
    name: 'groupId',
    type: 'text',
    label: '组id',
    required: true,
    readonly: false,
    hidden: true
  },
  {
    name: 'userName',
    type: 'text',
    label: '姓名',
    required: true
  },
  {
    name: 'email',
    type: 'email',
    label: '邮箱',
    required: true
  }
]

const emailSender = [
  {
    name: 'smtp',
    type: 'text',
    label: 'smtp服务器',
    required: true
  },
  {
    name: 'password',
    type: 'password',
    label: 'smtp密码',
    required: true
  }
]

export default {
  data() {
    return {
      isShowNewEmailDialog: false,
      initNewEmailParams: {
        title: '新增',
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
        return '新增发件箱'
      }
      return '新增收件箱'
    }
  },
  methods: {
    openNewEmailDialog() {
      // 添加 fields
      const fields = [...emailCommonInfo]
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
      notifySuccess('添加成功')
    }
  }
}
</script>
