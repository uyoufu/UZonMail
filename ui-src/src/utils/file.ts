/* eslint-disable @typescript-eslint/prefer-promise-reject-errors */
/* eslint-disable @typescript-eslint/no-explicit-any */
import { showDialog } from 'src/components/popupDialog/PopupDialog'
import { notifyError } from 'src/utils/dialog'
import * as XLSX from 'xlsx'
import { PopupDialogFieldType } from 'src/components/popupDialog/types'
import CryptoJS from 'crypto-js'
import type { IProgressOptions } from 'src/compositions/useProgress';
import { useNotifyProgress } from 'src/compositions/useProgress'
import logger from 'loglevel'

export interface ISelectFileResult {
  ok: boolean,
  data: string | ArrayBuffer | undefined | null,
  files?: FileList
}

/**
 * 选择文件
 * @param multiple
 * @param accept
 */
export function selectFile (multiple: boolean = false, accept: string = ''): Promise<ISelectFileResult> {
  const promise = new Promise<ISelectFileResult>((resolve, reject) => {
    const inputElement = document.createElement('input')
    inputElement.type = 'file'
    inputElement.accept = accept
    inputElement.multiple = multiple

    let settled = false
    let isChanged = false

    const cleanup = () => {
      inputElement.removeEventListener('change', onChange)
      window.removeEventListener('focus', onFocus)
      inputElement.remove()
    }

    const finishResolve = (result: ISelectFileResult) => {
      if (settled) return
      settled = true
      cleanup()
      resolve(result)
    }

    const finishReject = (result: ISelectFileResult) => {
      if (settled) return
      settled = true
      cleanup()
      reject(result)
    }

    const onChange = (ev: Event) => {
      logger.debug('[file] selectFile onChange:', ev)
      isChanged = true

      // change 触发，说明用户选择了文件（或清除了选择）
      const files = (ev.target as HTMLInputElement).files
      if (!files || files.length < 1) {
        notifyError('未找到文件')
        return finishReject({ ok: false, data: null } as any)
      }

      const file = files[0] as File
      const reader = new FileReader()
      reader.onload = e => {
        const buffer = e.target?.result
        finishResolve({ ok: true, data: buffer, files })
      }
      reader.onerror = err => {
        logger.error('selectFile read error:', err)
        finishReject({ ok: false, data: null } as any)
      }
      reader.readAsArrayBuffer(file)
    }

    const onFocus = () => {
      // 等短时间，允许 change 先执行；若没有文件则视为取消
      setTimeout(() => {
        // 如果已被 resolve/reject 则不处理
        if (settled || isChanged) return
        const files = inputElement.files
        if (files && files.length > 0) {
          // 有文件，等待 change 事件处理（或直接读取）
          return
        }
        finishReject({ ok: false, data: null } as any)
      }, 500)
    }

    inputElement.addEventListener('change', onChange)
    window.addEventListener('focus', onFocus, { once: true })
    inputElement.click()
  })

  return promise
}

/**
 * 打开文件选择器
 * @param accept
 * @param multiple
 * @returns
 */
export async function openFileSelector (multiple: boolean = false, accept: string = ''): Promise<boolean | string | ArrayBuffer | undefined | null> {
  const { data } = await selectFile(multiple, accept)
  return data
}

/**
 * buffer 转 blob
 * @param buffer
 * @returns
 */
export function bufferToBlob (buffer: ArrayBuffer): Blob {
  return new Blob([buffer])
}

/**
 * buffer 转 base64 图片
 * @param buffer
 * @returns
 */
export function bufferToBase64Png (buffer: ArrayBuffer): string {
  let binary = ''
  const bytes = new Uint8Array(buffer)
  for (let len = bytes.byteLength, i = 0; i < len; i++) {
    binary += String.fromCharCode(bytes[i] as number)
  }
  const base64 = 'data:image/png;base64,' + window.btoa(binary)
  return base64
}

// #region  Excel 相关操作

// 类型定义
/**
 * Excel 列映射
 */
export interface IExcelColumnMapper {
  // 对应表格中的列标题名
  headerName: string,
  // 对应数据中的字段名
  fieldName: string,
  // 格式化函数
  format?: (value: any) => any,
  // 过滤函数
  filter?: (value: any) => boolean,
  // 是否必须
  required?: boolean
}

export interface IExcelMapperParams {
  // 映射
  mappers?: IExcelColumnMapper[],
  format?: (value: Record<string, any>) => Record<string, any>,
  filter?: (value: Record<string, any>) => boolean,
  strict?: boolean // 仅读取或保存 mappers 中对应的字段
}

export interface IExcelReaderParams extends IExcelMapperParams {
  sheetIndex: number,
  // 是否要选择 sheet
  selectSheet?: boolean,
  // 数字类型保留的小数位，默认最多 5 位
  numberDecimalCount?: number
}

export interface IExcelWriterParams extends IExcelMapperParams {
  // 名字应包含后缀，比如 .xlsx
  fileName: string,
  sheetName?: string
}

/**
 * 读取 excel 文件，会同时返回文件名和文件内容
 * @param params
 * @returns
 */
export async function readExcelCore (params: IExcelReaderParams): Promise<{ data: Record<string, any>[], files?: FileList, sheetName: string }> {
  // 打开文件
  const { data: buffer, files } = await selectFile()
  console.log('readExcelCore:', buffer, files)
  const workbook = XLSX.read(buffer, { type: 'buffer' })

  // 打开选择框
  if (params.selectSheet && workbook.SheetNames.length > 1) {
    const { ok, data } = await showDialog<Record<string, any>>({
      title: '指定 Sheet',
      fields: [
        {
          name: 'sheetName',
          type: PopupDialogFieldType.selectOne,
          label: '请选择 Sheet',
          value: workbook.SheetNames[0],
          options: workbook.SheetNames
        }
      ],
      oneColumn: true
    })
    if (!ok) {
      return {
        data: [],
        files,
        sheetName: ''
      }
    }
    params.sheetIndex = workbook.SheetNames.indexOf(data.sheetName)
  }

  const sheetName = workbook.SheetNames[params.sheetIndex] as string
  const worksheet = workbook.Sheets[sheetName] as XLSX.WorkSheet
  const rowsData: Record<string, any>[] = XLSX.utils.sheet_to_json(worksheet)
  // 对数据进行处理，主要是浮点数问题
  const decimalCount = params.numberDecimalCount === undefined ? 5 : params.numberDecimalCount
  rowsData.forEach(row => {
    for (const key of Object.keys(row)) {
      const value = row[key]
      if (typeof value === 'number') {
        row[key] = Number(value.toFixed(decimalCount))
      }
    }
  })
  // 将 mappers 转换成对象
  const mapper: Record<string, IExcelColumnMapper> = {}
  if (params.mappers) {
    params.mappers.forEach(item => {
      mapper[item.headerName] = item
    })
  }

  // 对每行数据进行处理
  const formattedRows = []
  for (const row of rowsData) {
    // 对数据进行转换
    let formattedRow: Record<string, any> = {}
    for (const key of Object.keys(row)) {
      const value = row[key]
      const map = mapper[key]
      if (map) {
        // 先格式化，再过滤
        const formattedValue = map.format ? map.format(value) : value

        // 过滤
        if (map.filter && !map.filter(formattedValue)) {
          continue
        }

        // 判断是否存在
        if (map.required) {
          if (formattedValue === null || formattedValue === undefined || formattedValue === '') {
            notifyError(`字段 ${map.headerName} 不能为空`)
            throw new Error(`字段 ${map.fieldName} 不能为空`)
          }
        }
        formattedRow[map.fieldName] = formattedValue
      } else {
        formattedRow[key] = value
      }
    }

    // 对行进行格式化
    if (params.format) {
      formattedRow = params.format(formattedRow)
    }

    // 对行进行过滤
    if (params.filter && !params.filter(formattedRow)) {
      continue
    }

    formattedRows.push(formattedRow)
  }

  return {
    data: formattedRows,
    files,
    sheetName
  }
}

/**
 * 读取 excel
 * @param params
 */
export async function readExcel (params: IExcelReaderParams) {
  // 打开文件
  const { data: results } = await readExcelCore(params)
  return results
}

/**
 * 导出 Excel
 * @param rows
 * @param params
 * @param params.fileName 文件名，包含后缀
 */
export async function writeExcel (rows: any[], params: IExcelWriterParams) {
  if (params.strict && !params.mappers?.length) {
    logger.error('[file] 严格模式下, mappers 不能为空')
    return
  }

  // 创建一个新的工作簿
  const newWorkbook = XLSX.utils.book_new()

  // 将 mappers 转换成对象
  const mapper: Record<string, IExcelColumnMapper> = {}
  if (params.mappers) {
    params.mappers.forEach(item => {
      mapper[item.fieldName] = item
    })
  }

  const results: any[] = []
  const keys = Object.keys(mapper)

  let rowIndex = -1
  for (const row of rows) {
    rowIndex++
    // 对数据进行转换
    let formattedRow: Record<string, any> = {}
    for (const key of Object.keys(row)) {
      // 严格模式下，只读取 mappers 中的字段
      if (params.strict && !keys.includes(key)) { continue }

      const value = row[key]
      const map = mapper[key]
      if (map) {
        // 先格式化，再过滤
        const formattedValue = map.format ? map.format(value) : value

        // 过滤
        if (map.filter && !map.filter(formattedValue)) {
          continue
        }

        // 判断是否存在
        if (map.required) {
          if (formattedValue === null || formattedValue === undefined || formattedValue === '') {
            notifyError(`第 ${rowIndex} 行数据中，${map.headerName} 列不能为空`)
            throw new Error(`列 ${map.headerName} 不能为空`)
          }
        }

        formattedRow[map.headerName] = formattedValue
      } else {
        formattedRow[key] = value
      }
    }

    // 对行进行格式化
    if (params.format) {
      formattedRow = params.format(formattedRow)
    }

    // 对行进行过滤
    if (params.filter && !params.filter(formattedRow)) {
      continue
    }

    results.push(formattedRow)
  }

  logger.debug('[file] writeExcel:', results)

  // 创建一个新的工作表
  const newWorksheet = XLSX.utils.json_to_sheet(results)
  // 将工作表添加到工作簿
  XLSX.utils.book_append_sheet(newWorkbook, newWorksheet, params.sheetName || 'Sheet1')

  // 写入文件
  // XLSX.writeFile(newWorkbook, params.fileName)

  // 保存文件
  const buffer = XLSX.write(newWorkbook, { bookType: 'xlsx', type: 'buffer' })
  const blob = new Blob([buffer], {
    type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
  })
  const url = URL.createObjectURL(blob)
  const fullFileName = `${params.fileName}`
  await saveFileSmart(fullFileName, url)
}
// #endregion

// #region 文件上传相关

export interface IObsUploadedResult {
  // 该字段用于在初始化时使用
  __fileName?: string,
  __sha256?: string
  __key?: string,
  __sizeLabel?: string,
  __progressLabel?: string,
  __fileUsageId?: string | number,
}

export interface IFileObject {
  createDate: string,
  fileBucket: string,
  fileBucketId: number,
  id: number,
  isDeleted: boolean,
  isHidden: boolean,
  lastModifyDate: string,
  linkCount: number,
  objectId: string,
  path: string,
  sha256: string,
  size: number
}

/**
 * 上传的文件
 */
export interface IObsUploadedFile extends IObsUploadedResult, File {
  __img?: string,
  __src?: string,
}

/**
 * hash 计算回调
 */
export interface IFileSha256Callback {
  progressLabel: string
  process: number
  computed: number
  total: number
  file: IObsUploadedFile,
  end: boolean
}

/**
 * 计算文件的 sha256
 * 参考：https://github1s.com/emn178/online-tools/blob/master/js/main.js#L37
 * https://blog.csdn.net/weixin_39364136/article/details/132538445
 * @param file
 * @param callback 若回调中修改界面，需要强制触发刷新
 * @param progressStep 回调进度的步长，默认为 5 %
 * @returns
 */
export function fileSha256 (file: File, callback?: (params: IFileSha256Callback) => void, progressStep: number = 5): Promise<string> {
  console.log('getSha256:', file)
  return new Promise((resolve, reject) => {
    const reader = new FileReader()
    let start = 0
    const batch = 1024 * 1024 * 5
    const total = file.size
    const hashObject = CryptoJS.algo.SHA256.create()
    progressStep /= 100
    let progressDisplayLimit = 0
    function asyncUpdate () {
      if (start < total) {
        const process = start / total
        const progressLabel = '正在计算 hash 值...' + (process * 100).toFixed(2) + '%'
        const end = Math.min(start + batch, total)
        reader.readAsArrayBuffer(file.slice(start, end))
        start = end

        // 回调
        if (typeof callback === 'function' && progressDisplayLimit <= process) {
          progressDisplayLimit += progressStep
          const callbackResult = {
            progressLabel,
            process,
            computed: end,
            total,
            file,
            end: false
          }
          // console.log('fileSha256 callback:', callbackResult)
          callback(callbackResult)
        }
      } else {
        const sha256 = hashObject.finalize()
        console.log(`文件 ${file.name} sha256 值为：`, sha256.toString())

        if (typeof callback === 'function') {
          const callbackResult = {
            progressLabel: 'hash 已校验, 等待上传', // 重置为未上传的显示状态
            process: total,
            computed: total,
            total,
            file,
            end: true
          }
          callback(callbackResult)
        }
        resolve(sha256.toString())
      }
    }

    reader.onload = function () {
      // console.log('onload:', event, new Uint8Array(event.target.result))
      const arrayBuffer = reader.result as ArrayBuffer
      const wordArray = CryptoJS.lib.WordArray.create(arrayBuffer)
      // 更新哈希对象
      hashObject.update(wordArray)
      asyncUpdate()
    }

    asyncUpdate()

    reader.onerror = err => {
      reject({ ok: false, message: err })
    }
  })
}
// #endregion

// #region 文件保存相关操作
export async function saveFileSmart (fileName: string, contentOrString: string) {
  // 若不是 url，则转换成 objectUrl
  if (!validUrl(contentOrString)) {
    const blob = new Blob([contentOrString], { type: 'text/plain' })
    const url = URL.createObjectURL(blob)
    contentOrString = url
  }

  if (window.showSaveFilePicker) {
    return saveByFileSystemAccess(fileName, contentOrString)
  } else {
    return saveByUrl(fileName, contentOrString)
  }
}

/**
 * 保存 url 到文件
 * @param fileName
 * @param fileUrl
 */
export function saveByUrl (fileName: string, fileUrl: string) {
  const aLink = document.createElement('a')
  aLink.href = fileUrl
  aLink.download = fileName
  aLink.click()
  // 删除 alink
  aLink.remove()
}

function validUrl (url: string) {
  if (url && typeof (url) === 'string' && url.indexOf('http') >= 0) return true
  return false
}

declare global {
  interface Window {
    showSaveFilePicker?: (filePickerOptions: {
      suggestedName: string,
      types: {
        description: string,
        accept: Record<string, string[]>
      }[]
    }) => Promise<any>;
  }
}

/**
 * 通过 FileSystemAccess API 保存文件
 * @param downloadUrl
 * @param fileName
 * @param options
 * @returns
 */
export async function saveByFileSystemAccess (fileName: string, downloadUrl: string, options: IProgressOptions = {}) {
  if (!validUrl(downloadUrl)) {
    throw new Error('下载地址不合法,应以 http 开头')
  }

  const _options = Object.assign({
    token: '',
    initMessage: '正在解析文件...',
    doneMessage: '下载完成',
    cancellable: false
  }, options)
  if (!window.showSaveFilePicker) return false

  // 解析文件后缀
  const ext = fileName.split('.').pop() || '.*'
  let progressDoneFn: null | ((silent: boolean) => Promise<void>) = null
  try {
    logger.debug(`[file] saveByFileSystemAccess: ${fileName}, ${downloadUrl}`)
    const fileHandle = await window.showSaveFilePicker({
      suggestedName: fileName,
      types: [
        {
          description: `${ext} 文件`,
          accept: {
            '*/*': [`.${ext.toLowerCase()}`]
          }
        }
      ]
    })
    if (!fileHandle) return false

    // 创建 AbortController 实例
    const controller = new AbortController()
    const signal = controller.signal
    if (_options.cancellable) {
      _options.actions = [
        {
          label: '取消',
          dense: true,
          color: 'white',
          padding: 'none',
          class: 'q-px-xs',
          flat: true,
          handler: () => {
            controller.abort()
            // 不需要手动取消，因为会自动取消
          }
        }
      ]
    }
    const { update, done: progressDone } = useNotifyProgress(Date.now(), _options)
    progressDoneFn = progressDone

    // 选择文件后，显示初始消息
    update(0, _options.initMessage)

    const writableStream = await fileHandle.createWritable()
    const response = await fetch(downloadUrl, { signal })
    if (!response || !response.body) { throw new Error('下载失败') }
    const reader = response.body.getReader()
    const writer = writableStream.getWriter()

    const contentLength = Number(response.headers.get('Content-Length'))
    const totalLength = isNaN(contentLength) ? 1 : contentLength
    let receivedLength = 0
    function updateProgress (bytes: Uint8Array) {
      receivedLength += bytes.length
      const percent = Math.floor(receivedLength / totalLength * 100)
      if (percent > 100) return 90

      update(percent, `${fileHandle.name} 下载中...`)
    }

    while (true) {
      const { done, value } = await reader.read()
      if (done) {
        writer.close()
        await progressDone()
        break
      }

      updateProgress(value)
      writer.write(value)
    }
    return true
  } catch (e) {
    if (e instanceof DOMException && e.name === 'AbortError') {
      return false
    }

    // 关闭进度条
    if (progressDoneFn) await progressDoneFn(true)
    console.error(e)
    return false
  }
}
// #endregion
