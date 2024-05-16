<script>
import { newGroup } from '@/api/group'
import { notifySuccess } from '@/components/iPrompt'

export default {
  data() {
    return {
      isShowNewGroupDialog: false,

      initNewGroupParams: {
        title: '新增',
        tooltip: '',
        api: newGroup,
        // type 可接受的值：text/password/textarea/email/search/tel/file/number/url/time/date
        fields: [
          {
            name: 'groupType',
            type: 'text',
            label: '组类型',
            required: true,
            readonly: true,
            default: this.groupType
          },
          {
            name: 'parentId',
            type: 'text',
            label: '父组id',
            required: false,
            readonly: true,
            hidden: true
          },
          {
            name: 'parentName',
            type: 'text',
            label: '父组',
            required: false,
            readonly: true
          },
          {
            name: 'name',
            type: 'text',
            label: '子组名称',
            required: true
          },
          {
            name: 'description',
            type: 'textarea',
            label: '描述',
            required: false
          }
        ]
      }
    }
  },
  computed: {
    newGroupTitle() {
      if (this.groupType === 'send') return '添加发件箱'

      return '添加收件箱'
    }
  },

  methods: {
    showNewGroupDialog(data) {
      // 修改初始化参数
      if (data) {
        this.$set(this.initNewGroupParams.fields[1], 'default', data._id || '')
        this.$set(this.initNewGroupParams.fields[2], 'default', data.name || '')
      }

      this.initNewGroupParams.title = this.newGroupTitle
      this.isShowNewGroupDialog = true
    },

    addNewGroup(data) {
      this.groupsOrigin.push(data)
      this.isShowNewGroupDialog = false
      notifySuccess('添加成功')
    }
  }
}
</script>
