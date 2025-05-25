import type { IJsVariableSource } from "src/api/pro/jsVariable"
import type { IContextMenuItem } from "src/components/contextMenu/types"
import type { IPopupDialogParams } from "src/components/popupDialog/types"
import { PopupDialogFieldType } from "src/components/popupDialog/types"
import { notifySuccess, showDialog } from "src/utils/dialog"

import { upsertJsVariableSource, deleteJsVariableSourcesData } from 'src/api/pro/jsVariable'
import type { addNewRowType, deleteRowByIdType, getSelectedRowsType } from "src/compositions/qTableUtils"

import logger from 'loglevel'

export function useDataSourceContext (addNewRow: addNewRowType,
  getSelectedRows: getSelectedRowsType,
  deleteRowById: deleteRowByIdType) {
  const dataSourceContextMenuItems: IContextMenuItem<IJsVariableSource>[] = [
    {
      name: 'edit',
      label: '编辑',
      tooltip: '编辑数据源',
      onClick: onUpdateDataSource
    },
    {
      name: 'delete',
      label: '删除',
      tooltip: '删除当前数据项或选中的数据源',
      color: 'negative',
      onClick: onDeleteDataSource,
    }
  ]

  async function onNewDataSource () {
    const newDoc = await onUpsertDataSource()
    if (!newDoc) return

    // 保存到当前数据中
    addNewRow(newDoc)
  }


  async function onUpdateDataSource (dataSource: IJsVariableSource) {
    const newDoc = await onUpsertDataSource(dataSource)
    if (!newDoc) return

    addNewRow(newDoc)

    notifySuccess('更新成功')
  }


  async function onUpsertDataSource (dataSource?: IJsVariableSource) {
    const popupParams: IPopupDialogParams = {
      title: dataSource ? `编辑数据源 / ${dataSource.name}` : '新增数据源',
      oneColumn: true,
      fields: [
        {
          name: 'name',
          label: '名称',
          tooltip: '数据源名称',
          value: dataSource?.name || '',
          required: true,
        },
        {
          name: 'description',
          label: '描述',
          tooltip: '数据源描述',
          value: dataSource?.description || '',
        },
        {
          name: 'value',
          label: '数据源',
          tooltip: ['格式:', '1. 可以是单个值', '2. 可以是数组, 以 [ 开头, 以 ] 结尾', '3. 可以是对象, 以 { 开头, 以 } 结尾'],
          type: PopupDialogFieldType.textarea,
          value: dataSource ? JSON.stringify(dataSource.value, null, 2) : '',
          required: true,
          parser: (value: string) => {
            try {
              return JSON.parse(value)
            } catch {
              return ''
            }
          },
          // eslint-disable-next-line @typescript-eslint/no-explicit-any
          validate: (value: any) => {
            if (typeof value === 'string' && value.trim() === '') {
              return {
                ok: false,
                message: '数据源不能为空'
              }
            }

            try {
              JSON.parse(value)
            } catch (e) {
              logger.error('数据源格式错误:', e)
              return {
                ok: false,
                message: '数据源格式不正确，请输入有效的 JSON 格式数据'
              }
            }
            return {
              ok: true
            }
          }
        }
      ]
    }

    const result = await showDialog(popupParams)
    if (!result.ok) return

    const newData: IJsVariableSource = Object.assign({}, dataSource, result.data)
    const { data: newDoc } = await upsertJsVariableSource(newData)
    return newDoc
  }

  async function onDeleteDataSource (dataSource: IJsVariableSource) {
    const selectedRows = getSelectedRows(dataSource)
    // 开始删除
    await deleteJsVariableSourcesData(selectedRows.selectedRows.value.map(x => x.id))
    // 移除已经删除的数据
    selectedRows.selectedRows.value.forEach(row => {
      deleteRowById(row.id)
    })
    selectedRows.selectedRows.value = []

    notifySuccess('删除成功')
  }

  return {
    onNewDataSource,
    dataSourceContextMenuItems
  }
}
