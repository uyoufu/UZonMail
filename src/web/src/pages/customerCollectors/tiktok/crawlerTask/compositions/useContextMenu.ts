/* eslint-disable @typescript-eslint/no-explicit-any */
import type { ICrawlerTaskInfo } from 'src/api/pro/crawlerTask'
import {
  CrawlerStatus,
  deleteCrawlerTaskInfo,
  startCrawlerTask,
  stopCrawlerTask,
  updateCrawlerTaskInfo,
  saveCrawlerResultsAsInbox
} from 'src/api/pro/crawlerTask'
import type { IContextMenuItem } from 'src/components/contextMenu/types'
import type { addNewRowType, deleteRowByIdType } from 'src/compositions/qTableUtils'
import { getCrawlerTaskFields } from './useHeaderFunctions'
import { confirmOperation, notifyError, notifySuccess, notifyUntil, showDialog } from 'src/utils/dialog'
import type { IPopupDialogParams } from 'src/components/lowCode/types'
import { useRouter } from 'vue-router'
import { t } from 'src/i18n/helpers'

export function useContextMenu(
  addNewRow: addNewRowType<ICrawlerTaskInfo>,
  deleteRowById: deleteRowByIdType<ICrawlerTaskInfo>
) {
  const contextMenuItems = computed<IContextMenuItem<ICrawlerTaskInfo>[]>(() => [
    {
      name: 'edit',
      label: t('pages.crawlerTask.edit'),
      tooltip: t('pages.crawlerTask.editCurrent'),
      vif: isNotRunning,
      onClick: onUpdateCrawler
    },
    {
      name: 'delete',
      label: t('pages.crawlerTask.delete'),
      tooltip: t('pages.crawlerTask.deleteCurrent'),
      color: 'negative',
      vif: (value) => value.status === CrawlerStatus.stopped,
      onClick: onDeleteCrawler
    },
    {
      name: 'stop',
      label: t('pages.crawlerTask.stop'),
      tooltip: t('pages.crawlerTask.stopCurrent'),
      vif: (value) => !isNotRunning(value),
      onClick: onStopCrawler
    },
    {
      name: 'start',
      label: t('pages.crawlerTask.start'),
      vif: isNotRunning,
      tooltip: t('pages.crawlerTask.startCurrent'),
      onClick: onStartCrawler
    },
    {
      name: 'viewResult',
      label: t('pages.crawlerTask.view'),
      tooltip: t('pages.crawlerTask.viewResult'),
      onClick: onViewCrawlerResult
    },
    {
      name: 'saveAsInbox',
      label: t('pages.crawlerTask.saveAsInbox'),
      tooltip: t('pages.crawlerTask.saveAsInboxTooltip'),
      onClick: onSaveAsInbox
    }
  ])

  function isNotRunning(value: Record<string, any>) {
    return value.status !== CrawlerStatus.running
  }

  async function onUpdateCrawler(crawlerTaskInfo: Record<string, any>) {
    const fields = await getCrawlerTaskFields()

    // 添加默认值
    fields.forEach((field) => {
      if (crawlerTaskInfo[field.name] !== undefined) {
        field.value = crawlerTaskInfo[field.name]
      }
    })

    // 打开弹窗
    const popupParams: IPopupDialogParams = {
      title: t('pages.crawlerTask.editTitle'),
      fields,
      oneColumn: true
    }

    // 弹出对话框

    const { ok, data } = await showDialog<ICrawlerTaskInfo>(popupParams)
    if (!ok) return
    await updateCrawlerTaskInfo(crawlerTaskInfo.id, data)
    // 保存到 rows 中
    addNewRow(Object.assign(crawlerTaskInfo, data))

    notifySuccess(t('pages.crawlerTask.updateSuccess'))
  }

  async function onDeleteCrawler(crawlerTaskInfo: ICrawlerTaskInfo) {
    const confirm = await confirmOperation(
      t('pages.crawlerTask.deleteTitle'),
      t('pages.crawlerTask.deleteConfirm', { name: crawlerTaskInfo.name })
    )
    if (!confirm) return

    // 开始删除
    await deleteCrawlerTaskInfo(crawlerTaskInfo.id as number)

    // 删除本机数据
    deleteRowById(crawlerTaskInfo.id)

    notifySuccess(t('pages.crawlerTask.deleteSuccess'))
  }

  async function onStopCrawler(crawlerTaskInfo: ICrawlerTaskInfo) {
    await stopCrawlerTask(crawlerTaskInfo.id as number)
    // 更新
    crawlerTaskInfo.status = CrawlerStatus.stopped
    addNewRow(crawlerTaskInfo)
  }

  async function onStartCrawler(crawlerTaskInfo: ICrawlerTaskInfo) {
    await startCrawlerTask(crawlerTaskInfo.id as number)

    // 更新
    crawlerTaskInfo.status = CrawlerStatus.running
    addNewRow(crawlerTaskInfo)
  }

  const router = useRouter()
  async function onViewCrawlerResult(crawlerTaskInfo: ICrawlerTaskInfo) {
    // 跳转到结果页面
    await router.push({
      name: 'CrawlerResult',
      params: {
        id: crawlerTaskInfo.id
      },
      query: {
        tagName: crawlerTaskInfo.name
      }
    })
  }

  async function onSaveAsInbox(crawlerTaskInfo: ICrawlerTaskInfo) {
    // 进行确认
    const confirm = await confirmOperation(
      t('pages.crawlerTask.saveAsInboxTooltip'),
      t('pages.crawlerTask.saveAsInboxConfirm', { name: crawlerTaskInfo.name })
    )
    if (!confirm) return

    // 开始另存为
    const inboxGroupId = await notifyUntil(async () => {
      const { data } = await saveCrawlerResultsAsInbox(crawlerTaskInfo.id as number)
      return data
    }, t('pages.crawlerTask.savingAsInbox'))

    if (!inboxGroupId) {
      notifyError(t('pages.crawlerTask.saveAsInboxFailed'))
      return
    }

    // 提示跳转
    notifySuccess(t('pages.crawlerTask.saveAsInboxSuccess'))

    const confirm2InboxDetail = await confirmOperation(
      t('pages.crawlerTask.saveAsInboxSuccess'),
      t('pages.crawlerTask.goToInboxDetail')
    )
    if (!confirm2InboxDetail) return

    // 开始跳转
    await router.push({
      name: 'inboxManager'
    })
  }

  return { contextMenuItems }
}
