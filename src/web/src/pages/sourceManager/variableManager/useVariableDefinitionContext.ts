import type { IContextMenuItem } from "src/components/contextMenu/types"
import type { IPopupDialogParams } from "src/components/popupDialog/types"
import { PopupDialogFieldType } from "src/components/popupDialog/types"
import { notifySuccess, showDialog } from "src/utils/dialog"

import type { IJsFunctionDefinition } from 'src/api/pro/jsFunctionDefinition';
import { upsertJsFunctionDefinition, deleteJsFunctionDefinitionsData, testJsFunctionDefinitionsData } from 'src/api/pro/jsFunctionDefinition'
import type { addNewRowType, deleteRowByIdType, getSelectedRowsType } from "src/compositions/qTableUtils"

import logger from 'loglevel'

export function useVariableDefinitionContext (addNewRow: addNewRowType,
  getSelectedRows: getSelectedRowsType,
  deleteRowById: deleteRowByIdType) {
  const dataSourceContextMenuItems: IContextMenuItem<IJsFunctionDefinition>[] = [
    {
      name: 'test',
      label: '测试',
      tooltip: '测试变量定义结果',
      onClick: onTestVariableDefinition
    },
    {
      name: 'edit',
      label: '编辑',
      tooltip: '编辑变量定义',
      onClick: onUpdateVariableDefinition
    },
    {
      name: 'delete',
      label: '删除',
      tooltip: '删除当前或选中项的变量定义',
      color: 'negative',
      onClick: onDeleteDataSource,
    }
  ]

  async function onTestVariableDefinition (data: IJsFunctionDefinition) {
    const { data: result } = await testJsFunctionDefinitionsData(data.id)
    logger.debug('[useVariableDefinitionContext] 测试变量定义结果:', JSON.stringify(result, null, 2))

    notifySuccess(`${data.name} 测试结果: ${result}`)
  }

  async function onNewVariableDefinition () {
    const newDoc = await onUpsertJsVariableDefinition()
    if (!newDoc) return

    // 保存到当前数据中
    addNewRow(newDoc)
  }

  async function onUpdateVariableDefinition (data: IJsFunctionDefinition) {
    const newDoc = await onUpsertJsVariableDefinition(data)
    if (!newDoc) return

    addNewRow(newDoc)

    notifySuccess('更新成功')
  }

  async function onUpsertJsVariableDefinition (data?: IJsFunctionDefinition) {
    const popupParams: IPopupDialogParams = {
      title: data ? `编辑变量 / ${data.name}` : '新增变量',
      oneColumn: true,
      fields: [
        {
          name: 'name',
          label: '变量名',
          tooltip: '变量名称',
          value: data?.name || '',
          required: true,
          validate: (val: string) => {
            // 变量名只能是字母、数字和下划线,不能以数字开头
            const regex = /^[a-zA-Z_][a-zA-Z0-9_]*$/
            return {
              ok: regex.test(val),
              message: '变量名只能包含字母、数字和下划线，且不能以数字开头'
            }
          },
        },
        {
          name: 'description',
          label: '描述',
          tooltip: '变量描述',
          value: data?.description || '',
        },
        {
          name: 'functionBody',
          label: '表达式',
          tooltip: ['表达式为 JavaScript 代码块', '系统数据通过 uzonData 变量访问'],
          type: PopupDialogFieldType.textarea,
          value: data?.functionBody || '',
          required: true
        }
      ]
    }

    const result = await showDialog(popupParams)
    if (!result.ok) return

    const newData: IJsFunctionDefinition = Object.assign({}, data, result.data)
    const { data: newDoc } = await upsertJsFunctionDefinition(newData)
    return newDoc
  }

  async function onDeleteDataSource (dataSource: IJsFunctionDefinition) {
    const selectedRows = getSelectedRows(dataSource)
    // 开始删除
    await deleteJsFunctionDefinitionsData(selectedRows.selectedRows.value.map(x => x.id))
    // 移除已经删除的数据
    selectedRows.selectedRows.value.forEach(row => {
      deleteRowById(row.id)
    })
    selectedRows.selectedRows.value = []

    notifySuccess('删除成功')
  }

  return {
    onNewVariableDefinition,
    dataSourceContextMenuItems
  }
}
