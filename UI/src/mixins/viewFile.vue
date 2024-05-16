
<script>
import { getIoFileInfo, presignedGetObject } from '@/api/ioFile'
import { isOnlyOfficeSupport } from '@/utils/onlyOffice'
import { notifyError, okCancle } from '../components/iPrompt'
import iDialog_onlyOffice from '@/components/iDialog_onlyOffice/index'
import pathJs from 'path'

export default {
  methods: {
    async viewFile_openFile(ioFileId) {
      // 获取文件类型
      const ioFileInfoRes = await getIoFileInfo(ioFileId)
      if (!ioFileInfoRes.ok) {
        return {
          ok: false,
          message: '未找到文件'
        }
      }

      // 获取查看的url
      const res = await presignedGetObject(ioFileId)
      // const url = `${gConfig.serviceIp}:${gConfig.servicePort}/api/v1/design-results/stream?designResultId=${data._id}`
      const url = res.data

      const extension = pathJs.extname(ioFileInfoRes.data.objectName)
      // 根据类型，选择打开方式
      if (extension.toLowerCase() === '.pdf') {
        window.open('/pdfjs/web/viewer.html?file=' + encodeURIComponent(url))
      } else if (isOnlyOfficeSupport(extension)) {
        // 如果是office
        await okCancle('', '', {
          component: iDialog_onlyOffice,
          parent: this, // 成为该Vue节点的子元素
          option: {
            idEdit: false,
            url,
            title: ioFileInfoRes.data.name,
            fileType: extension.toLowerCase().replace('.', '')
          }
        })
      } else {
        notifyError('无法预览该文件')
      }
    }
  }
}
</script>
