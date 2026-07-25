import type { QNotifyCreateOptions } from 'quasar'
import { Notify } from 'quasar'
import { i18n } from 'src/boot/i18n'

const { t } = i18n.global

/**
 * 显示错误通知。
 * @param options 通知配置或多语言键。
 */
export function notifyError(options: QNotifyCreateOptions | string | undefined): void {
  if (!options) return

  const inputOptions = typeof options === 'string' ? { message: t(options) } : options
  Notify.create({
    color: 'negative',
    icon: 'cancel',
    position: 'top',
    ...inputOptions
  })
}

/**
 * 显示成功通知。
 * @param options 通知配置或多语言键。
 */
export function notifySuccess(options: QNotifyCreateOptions | string | undefined): void {
  if (!options) return

  const inputOptions = typeof options === 'string' ? { message: t(options) } : options
  Notify.create({
    color: 'positive',
    icon: 'check_circle',
    position: 'top',
    ...inputOptions
  })
}

/**
 * 显示警告通知。
 * @param options 通知配置或多语言键。
 */
export function notifyWarning(options: QNotifyCreateOptions | string | undefined): void {
  if (!options) return

  const inputOptions = typeof options === 'string' ? { message: t(options) } : options
  Notify.create({
    color: 'warning',
    icon: 'circle_notifications',
    position: 'top',
    ...inputOptions
  })
}

/**
 * 根据通知类型选择预设样式。
 * @param options 通知配置或多语言键。
 */
export function notifyAny(options: QNotifyCreateOptions | string | undefined): void {
  if (!options) return

  const notifyOptions = typeof options === 'string' ? { message: t(options), type: 'success' } : options
  switch (notifyOptions.type) {
    case 'success':
      notifySuccess(notifyOptions)
      break
    case 'error':
      notifyError(notifyOptions)
      break
    default:
      Notify.create(notifyOptions)
  }
}

/**
 * 创建可更新的无限进度通知。
 * @param message 通知正文。
 * @param caption 通知副标题。
 * @returns 停止和更新通知的操作。
 */
export function useIndeterminateProgressNotify(message: string, caption: string = '') {
  const notify = Notify.create({
    group: false,
    timeout: 0,
    spinner: true,
    spinnerColor: 'negative',
    spinnerSize: 'sm',
    message,
    caption,
    color: 'primary'
  })

  const startDate = Date.now()
  const intervalId = setInterval(() => {
    notify({
      caption: generateCaption(caption)
    })
  }, 1000)

  function generateCaption(currentCaption: string) {
    return `${currentCaption} ${Math.round((Date.now() - startDate) / 1000)} s`
  }

  function stop() {
    clearInterval(intervalId)
    notify({
      timeout: 1
    })
  }

  function update(updatedCaption?: string, updatedMessage?: string) {
    const notifyOptions: Record<string, string> = {}
    if (updatedMessage !== undefined) notifyOptions.message = updatedMessage
    if (updatedCaption !== undefined) notifyOptions.caption = generateCaption(updatedCaption)
    if (Object.keys(notifyOptions).length === 0) return

    // 手动更新后停止自动计时，避免后续定时任务覆盖调用方提供的副标题。
    clearInterval(intervalId)
    notify(notifyOptions)
  }

  return { stop, update }
}

/**
 * 输出线性进度通知占位信息。
 * @param message 通知正文。
 * @param caption 通知副标题。
 */
export function useProgressNotify(message: string, caption: string = '') {
  console.log(message, caption)
}

export type NotifyUntilUpdate = (caption?: string, message?: string) => void

/**
 * 在异步任务执行期间持续显示通知。
 * @param runAsyncTask 需要执行的异步任务。
 * @param message 通知正文。
 * @param caption 通知副标题。
 * @returns 异步任务结果；传入无效任务时返回 null。
 */
export async function notifyUntil<T>(
  runAsyncTask: (update: NotifyUntilUpdate) => Promise<T>,
  message: string,
  caption: string = ''
): Promise<T | null> {
  if (typeof runAsyncTask !== 'function') return null

  const { stop, update } = useIndeterminateProgressNotify(message, caption)
  try {
    return await runAsyncTask(update)
  } finally {
    stop()
  }
}
