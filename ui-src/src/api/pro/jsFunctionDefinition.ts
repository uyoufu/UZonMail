import { httpClientPro } from 'src/api//base/httpClient'
import type { IRequestPagination } from 'src/compositions/types'

export interface IJsFunctionDefinition {
  id: number,
  name: string,
  functionBody: string,
  description?: string,
}

export function upsertJsFunctionDefinition (data: IJsFunctionDefinition) {
  return httpClientPro.post<IJsFunctionDefinition>('/js-function-definition', {
    data
  })
}

/**
 * 获取变量定义数量
 * @param groupId
 * @param filter
 */
export function getJsFunctionDefinitionsCount (filter: string | undefined) {
  return httpClientPro.get<number>('/js-function-definition/filtered-count', {
    params: {
      filter
    }
  })
}

/**
 * 获取变量定义数据
 * @param groupId
 * @param filter
 * @param pagination
 * @returns
 */
export function getJsFunctionDefinitionsData (filter: string | undefined, pagination: IRequestPagination) {
  return httpClientPro.post<IJsFunctionDefinition[]>('/js-function-definition/filtered-data', {
    params: {
      filter
    },
    data: pagination
  })
}

/**
 * 测试 JavaScript 函数定义
 * @param definitionId
 * @returns
 */
export function testJsFunctionDefinitionsData (definitionId: number) {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  return httpClientPro.get<any>(`/js-function-definition/test/${definitionId}`)
}

/**
 * 删除变量定义
 * @param filter
 * @param pagination
 * @returns
 */
export function deleteJsFunctionDefinitionsData (ids: number[]) {
  return httpClientPro.delete<boolean[]>('/js-function-definition/ids', {
    data: ids
  })
}



