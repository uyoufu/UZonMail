
import { httpClientPro } from 'src/api/base/httpClient'
import type { IRequestPagination } from 'src/compositions/types'

export interface IUnsubscribePage {
  id: number,
  language: string,
  htmlContent: string,
  userId?: number,
  organizationId?: number,
}

/**
 * 创建退订页面
 * @param data
 * @returns
 */
export function createUnsubscribePage (data: IUnsubscribePage) {
  return httpClientPro.post<IUnsubscribePage>('/unsubscribe-page', { data })
}

/**
 * 获取退订页面
 * @param token
 * @param unsubscribePageId
 * @param language
 * @returns
 */
export function getUnsubscribePage (token: string, unsubscribePageId: number, language?: string) {
  return httpClientPro.get<IUnsubscribePage>('/unsubscribe-page', {
    params: {
      token,
      id: unsubscribePageId,
      language
    }
  })
}

/**
 * 更新退订页面
 * @param id
 * @returns
 */
export function updateUnsubscribePage (id: number, htmlContent: string) {
  return httpClientPro.put<boolean>(`/unsubscribe-page/${id}/content`, {
    data: {
      htmlContent
    }
  })
}

/**
 * 删除退订页面
 * @param id
 * @returns
 */
export function deleteUnsubscribePage (id: number) {
  return httpClientPro.put<boolean>(`/unsubscribe-page/${id}`)
}

/**
 * 获取页面数量
 * @returns
 */
export function getUnsubscribePagesCount (filter?: string) {
  return httpClientPro.get<number>('/unsubscribe-page/filtered-count', {
    params: {
      filter
    }
  })
}

/**
 * 获取页面数据
 * @param filter
 * @param pagination
 * @returns
 */
export function getUnsubscribePagesData (filter: string | undefined, pagination: IRequestPagination) {
  return httpClientPro.post<IUnsubscribePage[]>('/unsubscribe-page/filtered-data', {
    params: {
      filter
    },
    data: pagination
  })
}
