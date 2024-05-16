<script>
import { updateSendEmailSettings } from '@/api/group'
import { notifySuccess } from '@/components/iPrompt'

// 更新设置
export default {
  data() {
    return {
      isShowUpdateSettings: false,
      initSettingParams: {
        title: '设置',
        tooltip: '',
        api: updateSendEmailSettings,
        // type 可接受的值：text/password/textarea/email/search/tel/file/number/url/time/date
        fields: [
          {
            name: '_id',
            type: 'text',
            label: '发件人id',
            required: false,
            readonly: false, // 为true时会被过滤
            hidden: true
          },
          {
            name: 'userName',
            type: 'text',
            label: '姓名',
            required: true,
            readonly: true
          },
          {
            name: 'maxEmailsPerDay',
            type: 'number',
            label: '单日最大发件量',
            required: true
          }
        ]
      }
    }
  },
  methods: {
    showUpdateSettings(data) {
      // 修改初始化参数
      if (data) {
        this.$set(this.initSettingParams.fields[0], 'default', data._id || '')
        this.$set(
          this.initSettingParams.fields[1],
          'default',
          data.userName || ''
        )
        this.$set(
          this.initSettingParams.fields[2],
          'default',
          data.settings ? data.settings.maxEmailsPerDay : 0
        )
      }

      this.initSettingParams.title = '设置：' + data.email
      this.isShowUpdateSettings = true
    },

    // 更新后的邮件
    updatedSettings(updatingData, data) {
      // 替换原来的数据
      const index = this.data.findIndex(d => d._id === data._id)
      if (index > -1) this.data.splice(index, 1, data)

      this.isShowUpdateSettings = false
      notifySuccess('修改成功')
    }
  }
}
</script>
