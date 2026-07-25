
import { Dialog } from 'quasar'
import { i18n } from 'src/boot/i18n'
const { t } = i18n.global

/**
 * 确认操作
 * @param title
 * @param message
 */
export async function confirmOperation (title: string, message: string): Promise<boolean> {
  return new Promise((resolve) => {
    Dialog.create({
      title,
      message,
      ok: {
        color: 'primary',
        icon: 'check_circle',
        label: t('confirm'),
        tooltip: t('confirmOperation'),
        size: 'md',
        dense: true
      },
      cancel: {
        color: 'negative',
        icon: 'cancel',
        label: t('cancel'),
        tooltip: t('cancelOperation'),
        size: 'md',
        dense: true
      }
    }).onOk(() => {
      resolve(true)
    }).onCancel(() => {
      resolve(false)
    }).onDismiss(() => {
      resolve(false)
    })
  })
}

/**
 * 显示 html 内容
 * @param title
 * @param html
 * @returns
 */
export function showHtmlDialog (title: string, html: string) {
  return new Promise((resolve) => {
    Dialog.create({
      title,
      message: html,
      html: true,
      ok: {
        dense: true
      },
      persistent: true,
    }).onOk(() => {
      resolve(true)
    }).onCancel(() => {
      resolve(false)
    }).onDismiss(() => {
      resolve(false)
    })
  })
}

// #region 对 components/lowCode/PopupDialog.ts 进行导出，统一弹窗调用位置
export { showDialog, showComponentDialog, showHtmlDialog2 } from 'src/components/lowCode/PopupDialog'
// #endregion

// 保留既有导入入口，避免通知职责拆分影响业务模块。
export {
  notifyAny,
  notifyError,
  notifySuccess,
  notifyUntil,
  notifyWarning,
  useIndeterminateProgressNotify,
  useProgressNotify
} from 'src/utils/notification'
export type { NotifyUntilUpdate } from 'src/utils/notification'
