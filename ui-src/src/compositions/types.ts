export interface IQTablePagination {
  sortBy: string,
  descending: boolean,
  page: number,
  rowsPerPage: number,
  rowsNumber: number
}

export interface IRequestPagination {
  sortBy: string,
  descending: boolean,
  skip: number, // 跳过行数
  limit: number, // 获取数量
}

/**
 * 表格的过滤对象
 */
export type TTableFilterObject = {
  filter?: string
} & Record<string, string | object | number>

/**
 * 初始化表格的参数
 */
export interface IQTableInitParams {
  sortBy?: string,
  descending?: boolean,
  filterFactor?: (filter: string) => Promise<TTableFilterObject>,
  getRowsNumberCount: (filterObj: TTableFilterObject) => Promise<number>, // 请求数据总数

  onRequest: (filterObj: TTableFilterObject, pagination: IRequestPagination) => Promise<Array<object>>, // 请求数据
  preventRequestWhenMounted?: boolean // 在挂载时请求数据
}
