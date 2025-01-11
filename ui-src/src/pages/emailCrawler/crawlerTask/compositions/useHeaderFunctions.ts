/* eslint-disable @typescript-eslint/no-explicit-any */
import { IPopupDialogParams, PopupDialogFieldType } from 'src/components/popupDialog/types'
import dayjs from 'dayjs'

import { getUsableProxies } from 'src/api/proxy'
import { CrawlerType, createCrawlerTaskInfo, ICrawlerTaskInfo } from 'src/api/pro/crawlerTask'
import { notifySuccess, showDialog } from 'src/utils/dialog'
import { addNewRowType } from 'src/compositions/qTableUtils'

export async function getCrawlerTaskFields () {
  // 获取用户的代理
  const { data: proxies } = await getUsableProxies()

  proxies.unshift({
    id: 0,
    name: '无',
    isActive: true,
    url: '',
    isShared: true,
    userId: 0,
    organizationId: 0
  })

  return [
    {
      name: 'name',
      type: PopupDialogFieldType.text,
      label: '任务名称',
      value: '',
      required: true
    },
    {
      name: 'type',
      type: PopupDialogFieldType.selectOne,
      label: '爬虫类型',
      value: 0,
      options: [
        {
          value: CrawlerType.TikTokEmail,
          label: CrawlerType[CrawlerType.TikTokEmail]
        }
      ],
      optionLabel: 'label',
      optionValue: 'value',
      mapOptions: true,
      emitValue: true
    },
    {
      name: 'description',
      label: '描述'
    },
    {
      name: 'proxyId',
      type: PopupDialogFieldType.selectOne,
      label: '代理',
      value: 0,
      options: proxies,
      optionLabel: 'name',
      optionValue: 'id',
      optionTooltip: 'description',
      mapOptions: true,
      emitValue: true
    },
    {
      name: 'deadline',
      type: PopupDialogFieldType.date,
      label: '截止日期',
      value: dayjs().add(1, 'day').format('YYYY-MM-DD HH:mm:ss')
    }
  ]
}

/**
 * 使用头部功能
 * @returns
 */
export function useHeaderFunctions (addNewRow: (newRow: addNewRowType) => void) {
  async function onCreateCrawlerTask () {
    const fields = await getCrawlerTaskFields()

    const popupParams: IPopupDialogParams = {
      title: '新增爬虫任务',
      fields,
      oneColumn: true
    }

    // 弹出对话框
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    const { ok, data } = await showDialog<ICrawlerTaskInfo>(popupParams)
    if (!ok) return
    const { data: crawlerTaskInfo } = await createCrawlerTaskInfo(data)
    // 保存到 rows 中
    addNewRow(crawlerTaskInfo)

    notifySuccess('新增发件箱成功')
  }

  return {
    onCreateCrawlerTask
  }
}
