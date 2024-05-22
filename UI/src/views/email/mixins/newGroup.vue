<script>
import { newGroup } from '@/api/group'
import { notifySuccess } from '@/components/iPrompt'

export default {
  data() {
    return {
      isShowNewGroupDialog: false,

      initNewGroupParams: {
        title: this.$t('new'),
        tooltip: '',
        api: newGroup,
        // type 可接受的值：text/password/textarea/email/search/tel/file/number/url/time/date
        fields: [
          {
            name: 'groupType',
            type: 'text',
            label: this.$t('table.groupType'),
            required: true,
            readonly: true,
            default: this.groupType
          },
          {
            name: 'parentId',
            type: 'text',
            label: this.$t('table.parentId'),
            required: false,
            readonly: true,
            hidden: true
          },
          {
            name: 'parentName',
            type: 'text',
            label: this.$t('table.parentName'),
            required: false,
            readonly: true
          },
          {
            name: 'name',
            type: 'text',
            label: this.$t('table.subGroupName'),
            required: true
          },
          {
            name: 'description',
            type: 'textarea',
            label: this.$t('table.description'),
            required: false
          }
        ]
      }
    }
  },
  computed: {
    newGroupTitle() {
      if (this.groupType === 'send') return this.$t('newOutbox')

      return  this.$t('newInbox')
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
      notifySuccess(this.$t('addSuccess'))
    }
  }
}
</script>
