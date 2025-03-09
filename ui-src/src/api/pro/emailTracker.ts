import { httpClientPro } from 'src/api//base/httpClient'
import type { IRequestPagination } from 'src/compositions/types'

export interface IEmailAnchor {
  userId: string,
  outboxEmail: string,
  inboxEmail: string,
  visitedCount: string,
  firstVisitDate: string,
  lastVisitDate: string,
}

/**
 * 获取发件邮箱数量
 * @param groupId
 * @param filter
 */
export function getEmailAnchorsCount (filter: string | undefined) {
  return httpClientPro.get<number>('/email-tracker/filtered-count', {
    params: {
      filter
    }
  })
}

/**
 * 获取发件邮箱数据
 * @param groupId
 * @param filter
 * @param pagination
 * @returns
 */
export function getEmailAnchorsData (filter: string | undefined, pagination: IRequestPagination) {
  return httpClientPro.post<IEmailAnchor[]>('/email-tracker/filtered-data', {
    params: {
      filter
    },
    data: pagination
  })
}
