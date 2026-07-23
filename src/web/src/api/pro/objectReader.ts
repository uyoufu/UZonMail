
import { httpClientPro } from 'src/api/base/httpClient'

/**
 * 生成一个永久的文件读取器
 * @returns
 */
export function createObjectPersistentReader (fileUsageId: number) {
  return httpClientPro.get<string>('/object-reader/persistent', {
    params: {
      fileUsageId
    }
  })
}

/**
 * 获取文件流
 * @param objectReaderId
 * @returns
 */
export function getObjectStream (objectReaderId: string) {
  return httpClientPro.get<ArrayBuffer>(`/object-reader/stream/${objectReaderId}`)
}
