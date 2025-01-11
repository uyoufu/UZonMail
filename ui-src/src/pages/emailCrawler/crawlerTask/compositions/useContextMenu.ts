/* eslint-disable @typescript-eslint/no-explicit-any */
import { CrawlerStatus, ICrawlerTaskInfo, startCrawlerTask, stopCrawlerTask } from 'src/api/pro/crawlerTask'
import { IContextMenuItem } from 'src/components/contextMenu/types'
import { addNewRowType } from 'src/compositions/qTableUtils'

export function useContextMenu (addNewRow: (newRow: addNewRowType) => void) {
  const contextMenuItems: IContextMenuItem[] = [
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
    }
  ]

  function isNotRunning (value: Record<string, any>) {
    return value.status !== CrawlerStatus.running
  }

  async function onUpdateCrawler () {

  }

  async function onDeleteCrawler () {

  }

  async function onStopCrawler (value: Record<string, any>) {
    const crawlerTaskInfo = value as ICrawlerTaskInfo
    await stopCrawlerTask(crawlerTaskInfo.id as number)
    // 更新
    crawlerTaskInfo.status = CrawlerStatus.stopped
    addNewRow(crawlerTaskInfo as any)
  }

  async function onStartCrawler (value: Record<string, any>) {
    const crawlerTaskInfo = value as ICrawlerTaskInfo
    await startCrawlerTask(crawlerTaskInfo.id as number)

    // 更新
    crawlerTaskInfo.status = CrawlerStatus.running
    addNewRow(crawlerTaskInfo as any)
  }

  return { contextMenuItems }
}
