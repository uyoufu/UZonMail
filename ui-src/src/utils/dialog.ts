
import type { QNotifyCreateOptions} from 'quasar';
import { Notify, Dialog } from 'quasar'
import { i18n } from 'src/boot/i18n'
const { t } = i18n.global
/**
 * 错误
 * @param opts
 */
export function notifyError (opts: QNotifyCreateOptions | string | undefined): void {
  if (!opts) return

  let inputOptions = opts
  if (typeof opts === 'string') { inputOptions = { message: t(opts) } }

  const appNotifyOptions: QNotifyCreateOptions = {
    color: 'negative',
    icon: 'cancel',
    position: 'top'
  }

  const fullOptions = Object.assign(appNotifyOptions, inputOptions)
  Notify.create(fullOptions)
}

/**
 * 通知成功
 * @param opts
 */
export function notifySuccess (opts: QNotifyCreateOptions | string | undefined): void {
  if (!opts) return

  let inputOptions = opts
  if (typeof opts === 'string') { inputOptions = { message: t(opts) } }

  const appNotifyOptions: QNotifyCreateOptions = {
    color: 'positive',
    icon: 'check_circle',
    position: 'top'
  }

  const fullOptions = Object.assign(appNotifyOptions, inputOptions)
  Notify.create(fullOptions)
}

/**
 * 根据传递的 type 来智能识别消息类型
 * @param opts
 * @returns
 */
export function notifyAny (opts: QNotifyCreateOptions | string | undefined): void {
  if (!opts) return
  if (typeof opts === 'string') opts = { message: t(opts), type: 'success' }

  switch (opts.type) {
    case 'success':
      notifySuccess(opts)
      break
    case 'error':
      notifyError(opts)
      break
    default:
      Notify.create(opts)
  }
}

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

// #region 对 components/popupDialog/PopupDialog.ts 进行导出，统一弹窗调用位置
export { showDialog, showComponentDialog } from 'src/components/popupDialog/PopupDialog'
// #endregion

export function useIndeterminateProgressNotify (message: string, caption: string = '') {
  const notify = Notify.create({
    group: false, // required to be updatable
    timeout: 0, // we want to be in control when it gets dismissed
    spinner: true,
    spinnerColor: 'negative',
    message,
    caption,
    color: 'primary'
  })

  const startDate = Date.now()
  // 每隔 1s 更新一次 caption
  const intervalId = setInterval(() => {
    notify({
      caption: generateCaption(caption)
    })
  }, 1000)

  function stop () {
    clearInterval(intervalId)
    notify({
      timeout: 1
    })
  }

  function generateCaption (caption: string) {
    return `${caption} ${Math.round((Date.now() - startDate) / 1000)} s`
  }

  function update (caption?: string, message?: string) {
    const notifyOptions: Record<string, string> = {
    }
    if (message !== undefined) notifyOptions.message = message
    if (caption !== undefined) {
      notifyOptions.caption = generateCaption(caption)
    }
    if (Object.keys(notifyOptions).length === 0) return

    // 如果调用了 update，说明是手动在更新，则关闭自动更新
    clearInterval(intervalId)
    notify(notifyOptions)
  }

  return { stop, update }
}

/**
 * 使用线性进度条通知
 * @param message
 * @param caption
 */
export function useProgressNotify (message: string, caption: string = '') {
  console.log(message, caption)
}

/**
 * 显示通知，直到执行完毕
 * @param runFunc 执行的方法
 * @param message 消息
 * @param caption 小标题
 * @returns
 */
export async function notifyUntil<T> (runFunc: (update: (caption?: string, message?: string,) => void) => Promise<T>
  , message: string, caption: string = ''): Promise<T | null> {
  if (typeof runFunc !== 'function') return null
  const { stop, update } = useIndeterminateProgressNotify(message, caption)
  try {
    return await runFunc(update)
  } finally {
    stop()
  }
}
