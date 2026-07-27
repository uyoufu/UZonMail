import { mount } from '@vue/test-utils'
import { defineComponent, h, onMounted, ref, watch, type Ref } from 'vue'
import { afterAll, beforeAll, describe, expect, expectTypeOf, it, vi } from 'vitest'
import { useQTable } from 'src/compositions/qTableUtils'
import type {
  addNewRowType,
  deleteRowByIdType,
  updateExistOneType
} from 'src/compositions/qTableUtils'

interface TestTableRow {
  id: number
  code: string
  name: string
}

interface TestTableApi {
  rows: Ref<TestTableRow[]>
  selectedRows: Ref<TestTableRow[]>
  addNewRow: addNewRowType<TestTableRow>
  updateExistOne: updateExistOneType<TestTableRow>
  deleteRowById: deleteRowByIdType<TestTableRow>
}

function mountTestTable() {
  let tableApi: TestTableApi | undefined
  const TestTableHost = defineComponent({
    setup() {
      const currentTableApi = useQTable<TestTableRow>({ preventRequestWhenMounted: true })
      expectTypeOf(currentTableApi.rows).toEqualTypeOf<Ref<TestTableRow[]>>()
      expectTypeOf(currentTableApi.selectedRows).toEqualTypeOf<Ref<TestTableRow[]>>()
      tableApi = currentTableApi
      return () => h('div')
    }
  })

  const wrapper = mount(TestTableHost)
  return { wrapper, tableApi: tableApi! }
}

describe('useQTable', () => {
  beforeAll(() => {
    vi.stubGlobal('onMounted', onMounted)
    vi.stubGlobal('ref', ref)
    vi.stubGlobal('watch', watch)
  })

  afterAll(() => {
    vi.unstubAllGlobals()
  })

  it('keeps row and selection state on the declared business type', () => {
    const { wrapper, tableApi } = mountTestTable()
    const firstRow = { id: 1, code: 'first', name: 'First' }

    tableApi.addNewRow(firstRow)
    tableApi.selectedRows.value = [firstRow]

    expect(tableApi.rows.value).toEqual([firstRow])
    expect(tableApi.selectedRows.value).toEqual([firstRow])
    wrapper.unmount()
  })

  it('updates and deletes rows with strongly typed row keys', () => {
    const { wrapper, tableApi } = mountTestTable()
    tableApi.addNewRow({ id: 0, code: 'zero', name: 'Before' })

    expect(tableApi.updateExistOne({ id: 0, code: 'zero', name: 'After' })).toBe(true)
    expect(tableApi.rows.value[0]?.name).toBe('After')

    tableApi.deleteRowById(0)
    expect(tableApi.rows.value).toEqual([])
    wrapper.unmount()
  })
})
