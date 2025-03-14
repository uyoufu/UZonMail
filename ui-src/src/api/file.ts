import type { AxiosProgressEvent } from 'axios'
import { httpClient } from 'src/api/base/httpClient'
import type { IRequestPagination } from 'src/compositions/types'

/**
 * 上传文件到静态目录
 * @param subPath
 * @param fileName 文件名传入时应规范命名，不要包含特殊字符
 * @param data
 * @returns
 */
export function uploadToStaticFile (subPath: string, fileName: string, data: Blob) {
  // 去掉文件名中的空格
  fileName = fileName.replace(/\s/g, '')

  const form = new FormData()
  form.append('subPath', subPath)
  form.append('file', data, fileName)
  return httpClient.post<string>('/file/upload-static-file', {
    data: form
  })
}

/**
 * 获取文件使用ID
 * @param sha256
 * @param fileName
 * @returns
 */
export function getFileUsageId (sha256: string, fileName: string) {
  return httpClient.get<number>('/file/file-id', {
    params: {
      sha256,
      fileName
    }
  })
}

/**
 * 上传文件
 * @param sha256
 * @param fileName
 * @returns
 */
export function uploadFileObject (sha256: string, file: File, onUploadProgress?: (progressEvent: AxiosProgressEvent) => void) {
  const form = new FormData()
  form.append('sha256', sha256)
  form.append('file', file, file.name)

  return httpClient.post<number>('/file/upload-file-object', {
    data: form,
    onUploadProgress
  })
}

export interface IFileUsage {
  id: number,
  fileName: string
  createDate: string,
  displayName: string,
  fileObjectId: number,
  fileObject: {
    sha256: string,
    linkCount: number,
    size: number
  }
}

/**
 * 获取文件使用列表数量
 * @param filter
 * @returns
 */
export function getFileUsagesCount (filter?: string) {
  return httpClient.get<number>('/file/file-usages/filtered-count', {
    params: {
      filter
    }
  })
}

/**
 * 获取文件使用列表数据
 * @param filter
 * @param pagination
 * @returns
 */
export function getFileUsagesData (filter: string | undefined, pagination: IRequestPagination) {
  return httpClient.post<IFileUsage[]>('/file/file-usages/filtered-data', {
    params: {
      filter
    },
    data: pagination
  })
}

/**
 * 删除文件使用
 * @param fileUsageId
 * @returns
 */
export function deleteFileUsage (fileUsageId: number) {
  return httpClient.delete<boolean[]>(`/file/file-usages/${fileUsageId}`)
}

/**
 * 更新文件使用表中的显示名称
 * @param fileUsageId
 * @param displayName
 * @returns
 */
export function updateDisplayName (fileUsageId: number, displayName: string) {
  return httpClient.put<boolean[]>(`/file/file-usages/${fileUsageId}/display-name`, {
    params: {
      displayName
    }
  })
}
