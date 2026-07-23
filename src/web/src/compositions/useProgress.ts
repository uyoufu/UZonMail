/* eslint-disable @typescript-eslint/no-explicit-any */
/**
 * 说明：像通知一样打开执行进度条
 */
import { Notify, throttle } from 'quasar'
import { setTimeoutAsync } from 'src/utils/tsUtils'

// 格式：{progressName:{notify,resolve,promise}}
interface InotifyInfo {
  notify: any,
  resolve: any,
  promise: Promise<any>
}

export interface IProgressOptions {
  doneMessage?: string,
  autoDone?: boolean,
  reusable?: boolean,
  actions?: any[],
  silent?: boolean
}

const notifyInfos: Record<string, InotifyInfo | null> = {}

/**
 * 生成文本进度条，例如：[=====>    ] 50%
 * @param {*} percent
 * @returns
 */
function getTextProgress (percent: number) {
  const length = 20
  const count = Math.floor(percent / (100 / length))
  return `<code>[ ${'#'.repeat(count)}${'-'.repeat(length - count)} ] ${percent}%</code>`
}

/**
 * 使用通知显示进度
 * @param {*} progressName
 * @param {{ doneMessage: '', autoDone: true, reusable: true }} options
 * @returns {{ done: function, update: function,updateCore:function, waitDone: function }} update 设置更新节流，若要保证自动关闭，请调用 update
 */
export function useNotifyProgress (progressName: string | number = 'default', options: IProgressOptions | null = null) {
  if (!progressName) throw new Error('progressName is required')
  const fullOptions = Object.assign({
    doneMessage: '',
    autoDone: true,
    reusable: true,
    // 功能函数，参考：https://quasar.dev/quasar-plugins/notify#with-actions，
    // 示例: actions: [{ label: 'Dismiss', color: 'white', handler: () => { /* ... */ } }]
    actions: [],
    silent: false // 成功后不显示通知
  }, options)

  function initNotify () {
    // 查找是否有对应的 notify
    if (notifyInfos[progressName]) {
      return
    }

    const newInfo = {} as InotifyInfo
    notifyInfos[progressName] = newInfo
    newInfo.promise = new Promise((resolve) => {
      newInfo.resolve = resolve
    })
  }
  initNotify()

  /**
   * 等待结束
   */
  function waitDone () {
    if (!notifyInfos[progressName]) return
    return notifyInfos[progressName].promise
  }

  let isDone = false
  let isFinished = false
  /**
   * 更新进度
   * 这个接口仅用于更新最终结果，若没有手动 done 但需要等待完成，请调用 waitDone
   * 若要等待完成，请调用 waitDone
   * done 只能被成功调用一次，若要两次调用，要先调用和 update 使进度小于 100
   * @param {string} name 不同的 name 之间的进度是独立的
   */
  async function done (silent = false) {
    if (isDone) return

    // 保证不连接调用 Done
    isDone = true
    isFinished = true
    // 在下一个事件循环里执行
    await setTimeoutAsync(10)

    const notifyInfo = notifyInfos[progressName]
    if (!notifyInfo) return

    const { notify, resolve } = notifyInfo
    if (!notify) {
      // 结束
      if (resolve) resolve()
      return
    }

    // 关闭通知
    if (silent) {
      notify({
        caption: getTextProgress(100),
        timeout: 1,
        actions: []
      })
    } else {
      notify({
        icon: 'done', // we add an icon
        color: 'positive',
        spinner: false, // we reset the spinner setting so the icon can be displayed
        message: fullOptions.doneMessage || '操作成功 !',
        actions: [],
        caption: getTextProgress(100),
        timeout: 2000 // we will timeout it in 2.5s
      })
    }

    // 结束
    if (resolve) resolve()

    // 在下一个事件循环中删除 notify
    // 否则通知可能会无法关闭
    setTimeout(() => {
      notifyInfos[progressName] = null
    }, 10)
  }

  /**
   * 更新进度
   * @param {number} percent 0-100 之间
   * @param {string} message
   */
  function updateCore (percent: number, message: string) {
    // 重置 isDone 状态
    if (percent < 100) {
      isDone = false
    }
    if (isDone) return

    // 调用结束后，不能再调用 update 了
    if (isFinished) {
      if (!fullOptions.reusable) return
      isFinished = false
    }

    initNotify()
    // 查找是否有对应的 notify
    const notifyInfo = notifyInfos[progressName]
    if (!notifyInfo) return

    if (!notifyInfo.notify) {
      console.log('progressDone -1')
      const notify = Notify.create({
        group: false, // required to be updatable
        timeout: 0, // we want to be in control when it gets dismissed
        spinner: true,
        spinnerColor: 'negative',
        message: message || '处理中...',
        caption: getTextProgress(percent),
        color: 'primary',
        html: true,
        actions: fullOptions.actions
      })
      notifyInfo.notify = notify
      // 向 notify 上挂载一个 promise
      return
    }

    // 更新进度
    notifyInfo.notify({
      message,
      caption: getTextProgress(percent)
    })

    if (percent >= 100 && fullOptions.autoDone) {
      // eslint-disable-next-line @typescript-eslint/no-floating-promises
      done(fullOptions.silent)
    }
  }

  // 对 update 进行截流封装
  const throttleUpdate = throttle(updateCore, 200)

  /**
   * 带有截流的进度更新，若 percent 为 100, 必定会更新
   * @param {*} percent
   * @param {*} message
   */
  function update (percent: number, message: string) {
    if (percent - 100 < 0.001) {
      updateCore(percent, message)
      return
    }

    throttleUpdate(percent, message)
  }

  return {
    done,
    updateCore,
    update,
    waitDone
  }
}
