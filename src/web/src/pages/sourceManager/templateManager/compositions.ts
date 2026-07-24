import { getEmailTemplatesCount, getEmailTemplatesData } from 'src/api/emailTemplate'
import { useQTable } from 'src/compositions/qTableUtils'
import type { IRequestPagination, TTableFilterObject } from 'src/compositions/types'

// 查看缩略图
import 'viewerjs/dist/viewer.css'
import { api as viewerApi } from 'v-viewer'
import { useConfig } from 'src/config'

// 生成邮件模板数据
export function useEmailTemplateTable() {
  async function getRowsNumberCount(filterObj: TTableFilterObject) {
    const { data } = await getEmailTemplatesCount(filterObj.filter)
    return data
  }
  async function onRequest(filterObj: TTableFilterObject, pagination: IRequestPagination) {
    const { data } = await getEmailTemplatesData(filterObj.filter, pagination)
    return data
  }

  const { pagination, ...others } = useQTable({
    getRowsNumberCount,
    onRequest
  })

  const config = useConfig()
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  function getTemplateImage(row: Record<string, any>) {
    if (!row) return '/icons/undraw_mailbox_re_dvds.svg'
    const url = `${config.baseUrl}/${row.thumbnail}`
    // console.log(url)
    return url
  }

  // 查看缩略图
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  function onPreviewThumbnail(row: Record<string, any>) {
    const imageUrl = getTemplateImage(row)

    viewerApi({
      options: {
        title: false,
        transition: true,
        navbar: false,
        zIndex: 9999
      },
      images: [imageUrl]
    })
  }

  return {
    pagination,
    getTemplateImage,
    onPreviewThumbnail,
    ...others
  }
}
