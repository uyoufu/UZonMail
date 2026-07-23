import { httpClient } from 'src/api//base/httpClient'

/**
 * 获取文件读取 Id
 * @param fileUsageId
 * @returns
 */
export function getFileReaderId (fileUsageId: number) {
  return httpClient.post<number>('/file-reader', {
    params: {
      fileUsageId
    }
  })
}

/**
 * 通过 FileReaderId 获取文件
 * @param fileReaderId
 * @returns
 */
export function getFileStreamByReaderId (fileReaderId: number) {
  return httpClient.get<ArrayBuffer>(`/file-reader/${fileReaderId}/stream`, {
    responseType: 'arraybuffer'
  })
}
