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
export function uploadToStaticFile(subPath: string, fileName: string, data: Blob) {
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
export function getFileUsageId(sha256: string, fileName: string) {
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
export interface IFileUploadResult {
  fileUsageId: number
  categoryId: number
  isExisting: boolean
}

export function uploadFileObject(
  sha256: string,
  file: File,
  categoryId?: number,
  onUploadProgress?: (progressEvent: AxiosProgressEvent) => void
) {
  const form = new FormData()
  form.append('sha256', sha256)
  form.append('file', file, file.name)

  if (categoryId) form.append('categoryId', String(categoryId))

  return httpClient.post<IFileUploadResult>('/file/upload-file-object', {
    data: form,
    onUploadProgress,

  })
}

export interface IFileUsage {
  id: number
  categoryId: number
  fileName: string
  createDate: string
  displayName: string
  fileObjectId: number
  sha256: string
  referenceCount: number
  size: number
}

/**
 * 获取文件使用列表数量
 * @param filter
 * @returns
 */
export function getFileUsagesCount(filter?: string, categoryId?: number) {
  return httpClient.get<number>('/file/file-usages/filtered-count', {
    params: {
      filter,
      categoryId
    }
  })
}

/**
 * 获取文件使用列表数据
 * @param filter
 * @param pagination
 * @returns
 */
export function getFileUsagesData(filter: string | undefined, pagination: IRequestPagination, categoryId?: number) {
  return httpClient.post<IFileUsage[]>('/file/file-usages/filtered-data', {
    params: {
      filter,
      categoryId
    },
    data: pagination
  })
}

/**
 * 删除文件使用
 * @param fileUsageId
 * @returns
 */
export function deleteFileUsage(fileUsageId: number) {
  return httpClient.delete<IFileUsageDeletionResult>(`/file/file-usages/${fileUsageId}`)
}

export interface IFileUsageDeletionResult {
  deletedIds: number[]
  pendingPhysicalCleanupCount: number
}

/** 批量删除逻辑文件；任一文件被引用时后端会拒绝整批操作。 */
export function deleteFileUsages(fileUsageIds: number[]) {
  return httpClient.delete<IFileUsageDeletionResult>('/file/file-usages/ids/many', {
    data: { fileUsageIds }
  })
}

/** 将多个逻辑文件移动到指定分类。 */
export function moveFileUsages(fileUsageIds: number[], categoryId: number) {
  return httpClient.put<boolean>('/file/file-usages/category', {
    data: { fileUsageIds, categoryId }
  })
}

/**
 * 更新文件使用表中的显示名称
 * @param fileUsageId
 * @param displayName
 * @returns
 */
export function updateDisplayName(fileUsageId: number, displayName: string) {
  return httpClient.put<boolean>(`/file/file-usages/${fileUsageId}/display-name`, {
    params: {
      displayName
    }
  })
}
