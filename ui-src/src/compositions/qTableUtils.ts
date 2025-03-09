/* eslint-disable @typescript-eslint/no-explicit-any */
import type { QTableColumn } from 'quasar'
import type { IQTableInitParams, TTableFilterObject, IQTablePagination, IRequestPagination } from './types'
import QTableIndex from 'src/components/tableComponents/TableIndex.vue'

export type addNewRowType<T = Record<string, any>> = (newRow: T, idField?: string) => void

export type deleteRowByIdType = (id?: number, idField?: string) => void

export type getSelectedRowsType = (cursorData: Record<string, any>) => { rows: Record<string, any>[]; selectedRows: Ref<Record<string, any>[]> }

export interface ITableRequestProp {
  /**
   * Pagination object
   */
  pagination: {
    /**
     * Column name (from column definition)
     */
    sortBy: string;
    /**
     * Is sorting in descending order?
     */
    descending: boolean;
    /**
     * Page number (1-based)
     */
    page: number;
    /**
     * How many rows per page? 0 means Infinite
     */
    rowsPerPage: number;
    /**
     * For server-side fetching only. How many total database rows are there to be added to the table.
     */
    rowsNumber?: number;
  }

  /**
   * String/Object to filter table with (the 'filter' prop)
   */
  // eslint-disable-next-line @typescript-eslint/no-redundant-type-constituents
  filter?: string | any

  /**
   * Function to get a cell value
   * @param col Column name from column definitions
   * @param row The row object
   * @returns Parsed/Processed cell value
   */
  getCellValue?: (col: any, row: any) => any
}

/**
 * 返回一个QTable的配置对象
 * @param initParams
 * @returns
 */
export function useQTable (initParams: IQTableInitParams) {
  // 分页
  const pagination: Ref<IQTablePagination> = ref({
    sortBy: initParams.sortBy || 'id',
    descending: initParams.descending !== undefined ? initParams.descending : true,
    page: 1,
    rowsPerPage: 10,
    rowsNumber: 10
  })

  // 过滤
  const filter = ref('')
  const filterCache = ref('')
  async function getFilterObject (filter: string): Promise<TTableFilterObject> {
    let filterObj: TTableFilterObject = { filter, refreshCounter: refreshCounter.value }
    if (initParams.filterFactor) {
      filterObj = await initParams.filterFactor(filter)
    }
    return filterObj
  }

  // 通过缓存实现的数据总数请求
  async function getRowsNumberCount (filter: string): Promise<number> {
    if (!initParams.getRowsNumberCount) return 0

    const filterObj = await getFilterObject(filter)
    // 对 filter 进行序列化,若变动，才向后端请求总数
    const filterJson = JSON.stringify(filterObj)
    if (filterJson === filterCache.value) {
      return pagination.value.rowsNumber
    } else {
      filterCache.value = filterJson
    }

    // 请求数据总数
    const count = await initParams.getRowsNumberCount(filterObj)
    return count
  }

  // 表格数据请求
  const loading = ref(false)
  const rows: Ref<Record<string, any>[]> = ref([])
  async function onTableRequest (qTableProps: ITableRequestProp) {
    if (refreshCounter.value < 0) return
    if (!initParams.onRequest) return

    const { page, rowsPerPage, sortBy, descending } = (qTableProps.pagination || pagination.value) as IQTablePagination
    const filter = qTableProps.filter

    try {
      loading.value = true
      const totalCount = await getRowsNumberCount(filter)
      let data: object[] = []
      if (totalCount > 0) {
        // get all rows if "All" (0) is selected
        const fetchCount = rowsPerPage === 0 ? totalCount : rowsPerPage
        // calculate starting row of data
        const startRow = (page - 1) * (rowsPerPage)
        const filterObj = await getFilterObject(filter)
        data = await initParams.onRequest(filterObj, {
          sortBy,
          descending,
          skip: startRow,
          limit: fetchCount
        } as IRequestPagination)
      }

      // 更新数据
      rows.value = data

      // don't forget to update local pagination object
      pagination.value.rowsNumber = totalCount
      pagination.value.page = page
      pagination.value.rowsPerPage = rowsPerPage
      pagination.value.sortBy = sortBy
      pagination.value.descending = descending
    } finally {
      loading.value = false
    }
  }

  // 增减行数
  function increaseRowsNumber (count: number) {
    pagination.value.rowsNumber += count
  }

  // 刷新表格
  const refreshCounter = ref(0)
  // 通过增加refreshCounter的值来触发表格刷新
  function refreshTable () {
    refreshCounter.value++
  }
  watch(refreshCounter, async () => {
    // 更新数据
    await onTableRequest({
      pagination: pagination.value,
      filter: filter.value
    })
  })

  // 加载时，请求数据
  onMounted(() => {
    // 初始化表格数据
    if (initParams.preventRequestWhenMounted) return
    refreshTable()
  })

  /**
   * 增加新数据或者更新既有数据
  // 新增数据时，可以使用这个方法，增加一行数据
   * @param newRow
   * @param idField
   * @returns
   */
  function addNewRow (newRow: Record<string, any>, idField: string = 'id') {
    // 查找是否存在
    const found = rows.value.find(x => x[idField] === newRow[idField])
    if (found) {
      // 更新
      Object.assign(found, newRow)
      return
    }

    rows.value.push(newRow)
    increaseRowsNumber(1)
  }

  // 删除行
  function deleteRowById (id?: number, idField: string = 'id') {
    if (!id) return

    rows.value = rows.value.filter(x => x[idField] !== id)
    increaseRowsNumber(-1)
  }

  const selectedRows = ref<Record<string, any>[]>([])
  /**
   * 获取选中的行
   * @param cursorData
   * @returns { rows, selectedRows }, rows 是当前选中的行，selectedRows 是选中的行的容器
   */
  function getSelectedRows (cursorData: Record<string, any>): { rows: Record<string, any>[]; selectedRows: Ref<Record<string, any>[]> } {
    let rows = [cursorData]
    if (selectedRows.value.length > 0) {
      rows = selectedRows.value
    }

    return { rows, selectedRows }
  }

  return {
    rows,
    pagination,
    filter,
    loading,
    selectedRows,
    onTableRequest,
    increaseRowsNumber,
    refreshTable,
    addNewRow,
    deleteRowById,
    getSelectedRows
  }
}

export function useQTableIndex () {
  // 序号
  const indexColumn: QTableColumn = {
    name: 'index',
    label: '序号',
    align: 'left',
    field: v => v
  }

  return {
    indexColumn,
    QTableIndex
  }
}
