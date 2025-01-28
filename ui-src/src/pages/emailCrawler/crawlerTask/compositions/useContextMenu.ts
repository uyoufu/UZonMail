/* eslint-disable @typescript-eslint/no-explicit-any */
import { CrawlerStatus, deleteCrawlerTaskInfo, ICrawlerTaskInfo, startCrawlerTask, stopCrawlerTask, updateCrawlerTaskInfo } from 'src/api/pro/crawlerTask'
import { IContextMenuItem } from 'src/components/contextMenu/types'
import { addNewRowType, deleteRowByIdType } from 'src/compositions/qTableUtils'
import { getCrawlerTaskFields } from './useHeaderFunctions'
import { confirmOperation, notifySuccess, showDialog } from 'src/utils/dialog'
import { IPopupDialogParams } from 'src/components/popupDialog/types'
import { useRouter } from 'vue-router'

export function useContextMenu (addNewRow: addNewRowType<ICrawlerTaskInfo>, deleteRowById: deleteRowByIdType) {
  const contextMenuItems: IContextMenuItem<ICrawlerTaskInfo>[] = [
    {
      name: 'edit',
      label: '编辑',
      tooltip: '编辑当前爬虫任务',
      vif: isNotRunning,
      onClick: onUpdateCrawler
    },
    {
      name: 'delete',
      label: '删除',
      tooltip: '删除当前爬虫任务',
      color: 'negative',
      vif: value => value.status === CrawlerStatus.stopped,
      onClick: onDeleteCrawler
    },
    {
      name: 'stop',
      label: '停止',
      tooltip: '停止当前爬虫任务',
      vif: value => !isNotRunning(value),
      onClick: onStopCrawler
    },
    {
      name: 'start',
      label: '开始',
      vif: isNotRunning,
      tooltip: '启动当前爬虫任务',
      onClick: onStartCrawler
    },
    {
      name: 'viewResult',
      label: '查看',
      tooltip: '查看当前爬虫任务结果',
      onClick: onViewCrawlerResult
    }
  ]

  function isNotRunning (value: Record<string, any>) {
    return value.status !== CrawlerStatus.running
  }

  async function onUpdateCrawler (crawlerTaskInfo: Record<string, any>) {
    const fields = await getCrawlerTaskFields()

    // 添加默认值
    fields.forEach(field => {
      if (crawlerTaskInfo[field.name] !== undefined) {
        field.value = crawlerTaskInfo[field.name]
      }
    })

    // 打开弹窗
    const popupParams: IPopupDialogParams = {
      title: '修改爬虫任务',
      fields,
      oneColumn: true
    }

    // 弹出对话框
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    const { ok, data } = await showDialog<ICrawlerTaskInfo>(popupParams)
    if (!ok) return
    await updateCrawlerTaskInfo(crawlerTaskInfo.id, data)
    // 保存到 rows 中
    addNewRow(Object.assign(crawlerTaskInfo, data))

    notifySuccess('修改爬虫任务成功')
  }

  async function onDeleteCrawler (crawlerTaskInfo: ICrawlerTaskInfo) {
    const confirm = await confirmOperation('删除爬虫任务', `是否删除爬虫任务: [${crawlerTaskInfo.name}]？`)
    if (!confirm) return

    // 开始删除
    await deleteCrawlerTaskInfo(crawlerTaskInfo.id as number)

    // 删除本机数据
    deleteRowById(crawlerTaskInfo.id)

    notifySuccess('删除爬虫任务成功')
  }

  async function onStopCrawler (crawlerTaskInfo: ICrawlerTaskInfo) {
    await stopCrawlerTask(crawlerTaskInfo.id as number)
    // 更新
    crawlerTaskInfo.status = CrawlerStatus.stopped
    addNewRow(crawlerTaskInfo as any)
  }

  async function onStartCrawler (crawlerTaskInfo: ICrawlerTaskInfo) {
    await startCrawlerTask(crawlerTaskInfo.id as number)

    // 更新
    crawlerTaskInfo.status = CrawlerStatus.running
    addNewRow(crawlerTaskInfo as any)
  }

  const router = useRouter()
  async function onViewCrawlerResult (crawlerTaskInfo: ICrawlerTaskInfo) {
    // 跳转到结果页面
    router.push({
      name: 'CrawlerResult',
      params: {
        id: crawlerTaskInfo.id
      },
      query: {
        tagName: crawlerTaskInfo.name
      }
    })
  }

  return { contextMenuItems }
}
