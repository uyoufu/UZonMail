import { httpClientPro } from 'src/api//base/httpClient'
import type { IRequestPagination } from 'src/compositions/types'

// #region 变量数据源

export interface IJsVariableSource {
  name: string,
  value: object,
  description?: string,
}

export function upsertJsVariableSource (data: IJsVariableSource) {
  return httpClientPro.post<IJsVariableSource>('/js-variable-source', {
    data
  })
}

/**
 * 获取变量数据源数量
 * @param groupId
 * @param filter
 */
export function getJsVariableSourcesCount (filter: string | undefined) {
  return httpClientPro.get<number>('/js-variable-source/filtered-count', {
    params: {
      filter
    }
  })
}

/**
 * 获取变量数据源数据
 * @param groupId
 * @param filter
 * @param pagination
 * @returns
 */
export function getJsVariableSourcesData (filter: string | undefined, pagination: IRequestPagination) {
  return httpClientPro.post<IJsVariableSource[]>('/js-variable-source/filtered-data', {
    params: {
      filter
    },
    data: pagination
  })
}

/**
 * 删除变量的数据源
 * @param filter
 * @param pagination
 * @returns
 */
export function deleteJsVariableSourcesData (sourceIds: number[]) {
  return httpClientPro.delete<boolean[]>('/js-variable-source/ids', {
    data: sourceIds
  })
}
// #endregion

