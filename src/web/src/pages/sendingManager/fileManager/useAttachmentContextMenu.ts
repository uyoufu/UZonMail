import { useI18n } from 'vue-i18n'
import { createObjectPersistentReader } from 'src/api/pro/objectReader'
import { deleteFileUsages, moveFileUsages, updateDisplayName, type IFileUsage } from 'src/api/file'
import { getFileCategories } from 'src/api/fileCategory'
import {
  ContextMenuIcon,
  ContextMenuWhen,
  type IActionContext,
  type IContextMenuItem
} from 'src/components/contextMenu/types'
import { LowCodeFieldType } from 'src/components/lowCode/types'
import { useFileUsageDownload } from 'src/compositions/useFileUsageDownload'
import type { refreshTableType } from 'src/compositions/qTableUtils'
import { useConfig } from 'src/config'
import { confirmOperation, notifySuccess, showDialog } from 'src/utils/dialog'

/** 创建附件表格右键菜单，并内聚单项与批量附件操作。 */
export function useAttachmentContextMenu(refreshTable: refreshTableType) {
  const { t } = useI18n()
  const config = useConfig()
  const { downloadFileUsage } = useFileUsageDownload()

  const attachmentContextMenuItems = computed<IContextMenuItem<IFileUsage>[]>(() => [
    {
      name: 'download',
      label: t('fileManager.download'),
      icon: ContextMenuIcon.download,
      when: ContextMenuWhen.onlySingle,
      onClick: downloadFileUsage
    },
    {
      name: 'rename',
      label: t('fileManager.rename'),
      icon: ContextMenuIcon.edit,
      when: ContextMenuWhen.onlySingle,
      onClick: onRenameAttachment
    },
    {
      name: 'share',
      label: t('fileManager.share'),
      icon: ContextMenuIcon.link,
      when: ContextMenuWhen.onlySingle,
      onClick: onShareAttachment
    },
    {
      name: 'move',
      label: t('fileManager.move'),
      icon: ContextMenuIcon.driveFileMove,
      onClick: onMoveAttachments
    },
    {
      name: 'delete',
      label: t('fileManager.delete'),
      icon: ContextMenuIcon.delete,
      color: 'negative',
      onClick: onDeleteAttachments
    }
  ])

  async function onRenameAttachment(attachment: IFileUsage) {
    const result = await showDialog({
      title: t('fileManager.rename'),
      fields: [
        {
          name: 'displayName',
          label: t('fileManager.fileName'),
          type: LowCodeFieldType.text,
          required: true,
          value: attachment.displayName
        }
      ],
      oneColumn: true
    })
    if (!result.ok) return

    await updateDisplayName(attachment.id, String(result.data.displayName))
    refreshTable()
  }

  async function onShareAttachment(attachment: IFileUsage) {
    const confirmed = await confirmOperation(t('fileManager.share'), t('fileManager.shareConfirm'))
    if (!confirmed) return

    const { data: objectReaderId } = await createObjectPersistentReader(attachment.id)
    await navigator.clipboard.writeText(`${config.baseUrl}/api/pro/object-reader/stream/${objectReaderId}`)
    notifySuccess(t('fileManager.shareSuccess'))
  }

  async function onMoveAttachments(_cursorAttachment: IFileUsage, actionContext: IActionContext<IFileUsage>) {
    const { data: categories } = await getFileCategories()
    const result = await showDialog({
      title: t('fileManager.moveSelected'),
      fields: [
        {
          name: 'categoryId',
          label: t('fileManager.targetCategory'),
          type: LowCodeFieldType.selectOne,
          options: categories.map((category) => ({
            label: category.isDefault ? t('fileManager.defaultCategory') : category.name,
            value: category.id
          })),
          mapOptions: true,
          emitValue: true,
          required: true
        }
      ],
      oneColumn: true
    })
    if (!result.ok) return

    await moveFileUsages(
      actionContext.targetValues.map((attachment) => attachment.id),
      Number(result.data.categoryId)
    )
    refreshTable()
    actionContext.clearSelection()
  }

  async function onDeleteAttachments(_cursorAttachment: IFileUsage, actionContext: IActionContext<IFileUsage>) {
    const attachments = actionContext.targetValues
    const confirmationMessage =
      attachments.length === 1
        ? t('fileManager.deleteFileConfirm', { name: attachments[0]!.displayName })
        : t('fileManager.batchDeleteConfirm', { count: attachments.length })
    const confirmed = await confirmOperation(t('fileManager.deleteConfirmTitle'), confirmationMessage)
    if (!confirmed) return

    await deleteFileUsages(attachments.map((attachment) => attachment.id))
    refreshTable()
    actionContext.clearSelection()
    notifySuccess(t('fileManager.deleteSuccess'))
  }

  return { attachmentContextMenuItems }
}
