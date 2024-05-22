<script>
import XLSX from 'js-xlsx'
import { newEmails } from '@/api/group'
import { notifyError, notifySuccess } from '@/components/iPrompt'

export default {
  data() {
    return {}
  },
  methods: {
    // 选择文件
    selectExcelFile() {
      const elem = document.getElementById('fileInput')
      elem.click()
      elem.value = ''
    },

    async fileSelected(e) {
      console.log('fileSelected:', e)
      // 判断是否选择了文件
      if (e.target.files.length === 0) {
        return
      }

      // 获取选择的文件
      const file = e.target.files[0]
      const excelData = await this.readExcelData(file)
      if (!excelData) {
        notifyError(this.$t('addFailed'))
        return
      }

      // 发送到服务上保存
      const res = await newEmails(this.group._id, excelData)

      // 更新本地
      this.data.push(...res.data)

      notifySuccess(thsi.$t('addSuccess'))
    },

    async readExcelData(file) {
      return new Promise((resolve, reject) => {
        const reader = new FileReader()
        reader.onload = e => {
          const data = new Uint8Array(e.target.result)
          const workbook = XLSX.read(data, { type: 'array' })
          /* DO SOMETHING WITH workbook HERE */
          // 变成json
          const jsonObj = XLSX.utils.sheet_to_json(
            workbook.Sheets[workbook.SheetNames[0]]
          )
          resolve(jsonObj)
        }
        reader.onerror = () => {
          reject(false)
        }

        reader.readAsArrayBuffer(file)
      })
    }
  }
}
</script>
