import { useI18n } from 'vue-i18n'

/**
 * 从 txt 文件导入邮件
 * 这种方法，需要智能计算邮件的 smtp 及端口号
 */
export function useOutboxImporter () {
  const { t } = useI18n()

  // #region 从文本导入
  async function onImportOutboxFromTxt () {

  }

  const importFromTxtLable = t('outboxManager.importFromTxt')
  const importFromTxtTooltip = t('outboxManager.importFromTxtTooltip').split('\n')
  // #endregion

  return {
    onImportOutboxFromTxt,
    importFromTxtLable,
    importFromTxtTooltip
  }
}
