import { ContextMenuIcon, type IActionContext, type IContextMenuItem } from "src/components/contextMenu/types"
import type { IPopupDialogParams } from "src/components/lowCode/types"
import { LowCodeFieldType } from "src/components/lowCode/types"
import { notifySuccess, showDialog } from "src/utils/dialog"

import type { IJsFunctionDefinition } from 'src/api/pro/jsFunctionDefinition';
import { upsertJsFunctionDefinition, deleteJsFunctionDefinitionsData, testJsFunctionDefinitionsData } from 'src/api/pro/jsFunctionDefinition'
import type { addNewRowType, deleteRowByIdType } from "src/compositions/qTableUtils"

import logger from 'loglevel'
import { t } from 'src/i18n/helpers'

export function useVariableDefinitionContext (
  addNewRow: addNewRowType<IJsFunctionDefinition>,
  deleteRowById: deleteRowByIdType<IJsFunctionDefinition>
) {
  const dataSourceContextMenuItems = computed<IContextMenuItem<IJsFunctionDefinition>[]>(() => [
    {
      name: 'test',
      label: t('pages.variableManager.test'),
      tooltip: t('pages.variableManager.testVariableDefinition'),
      icon: ContextMenuIcon.science,
      onClick: onTestVariableDefinition
    },
    {
      name: 'edit',
      label: t('pages.variableManager.edit'),
      tooltip: t('pages.variableManager.editVariableDefinition'),
      icon: ContextMenuIcon.edit,
      onClick: onUpdateVariableDefinition
    },
    {
      name: 'delete',
      label: t('pages.variableManager.delete'),
      tooltip: t('pages.variableManager.deleteVariableDefinitions'),
      color: 'negative',
      icon: ContextMenuIcon.delete,
      onClick: onDeleteDataSource,
    }
  ])

  async function onTestVariableDefinition (data: IJsFunctionDefinition) {
    const { data: result } = await testJsFunctionDefinitionsData(data.id)
    logger.debug('[useVariableDefinitionContext] 测试变量定义结果:', JSON.stringify(result, null, 2))

    notifySuccess(t('pages.variableManager.testResult', { name: data.name, result: String(result) }))
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

    notifySuccess(t('pages.variableManager.updateSuccess'))
  }

  async function onUpsertJsVariableDefinition (data?: IJsFunctionDefinition) {
    const popupParams: IPopupDialogParams = {
      title: data ? t('pages.variableManager.editVariableTitle', { name: data.name }) : t('pages.variableManager.createVariable'),
      oneColumn: true,
      fields: [
        {
          name: 'name',
          label: t('pages.variableManager.variableName'),
          tooltip: t('pages.variableManager.variableNameTooltip'),
          value: data?.name || '',
          required: true,
          validate: (val: string) => {
            // 变量名只能是字母、数字和下划线,不能以数字开头
            const regex = /^[a-zA-Z_][a-zA-Z0-9_]*$/
            return {
              ok: regex.test(val),
              message: t('pages.variableManager.variableNameInvalid')
            }
          },
        },
        {
          name: 'description',
          label: t('pages.variableManager.description'),
          tooltip: t('pages.variableManager.variableDescription'),
          value: data?.description || '',
        },
        {
          name: 'functionBody',
          label: t('pages.variableManager.expression'),
          tooltip: [t('pages.variableManager.expressionTooltip'), t('pages.variableManager.expressionDataAccess')],
          type: LowCodeFieldType.textarea,
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

  async function onDeleteDataSource (
    _cursorDefinition: IJsFunctionDefinition,
    { targetValues, clearSelection }: IActionContext<IJsFunctionDefinition>
  ) {
    // 开始删除
    await deleteJsFunctionDefinitionsData(targetValues.map((definition) => definition.id))
    // 移除已经删除的数据
    targetValues.forEach((definition) => {
      deleteRowById(definition.id)
    })
    clearSelection()

    notifySuccess(t('pages.variableManager.deleteSuccess'))
  }

  return {
    onNewVariableDefinition,
    dataSourceContextMenuItems
  }
}
