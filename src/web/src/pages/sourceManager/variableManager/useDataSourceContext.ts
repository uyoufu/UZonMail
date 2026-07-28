import type { IJsVariableSource } from "src/api/pro/jsVariable"
import { ContextMenuIcon, type IActionContext, type IContextMenuItem } from "src/components/contextMenu/types"
import type { IPopupDialogParams } from "src/components/lowCode/types"
import { LowCodeFieldType } from "src/components/lowCode/types"
import { notifySuccess, showDialog } from "src/utils/dialog"

import { upsertJsVariableSource, deleteJsVariableSourcesData } from 'src/api/pro/jsVariable'
import type { addNewRowType, deleteRowByIdType } from "src/compositions/qTableUtils"

import logger from 'loglevel'
import { t } from 'src/i18n/helpers'

export function useDataSourceContext (
  addNewRow: addNewRowType<IJsVariableSource>,
  deleteRowById: deleteRowByIdType<IJsVariableSource>
) {
  const dataSourceContextMenuItems = computed<IContextMenuItem<IJsVariableSource>[]>(() => [
    {
      name: 'edit',
      label: t('pages.variableManager.edit'),
      tooltip: t('pages.variableManager.editDataSource'),
      icon: ContextMenuIcon.edit,
      onClick: onUpdateDataSource
    },
    {
      name: 'delete',
      label: t('pages.variableManager.delete'),
      tooltip: t('pages.variableManager.deleteDataSources'),
      color: 'negative',
      icon: ContextMenuIcon.delete,
      onClick: onDeleteDataSource,
    }
  ])

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

    notifySuccess(t('pages.variableManager.updateSuccess'))
  }


  async function onUpsertDataSource (dataSource?: IJsVariableSource) {
    const popupParams: IPopupDialogParams = {
      title: dataSource ? t('pages.variableManager.editDataSourceTitle', { name: dataSource.name }) : t('pages.variableManager.createDataSource'),
      oneColumn: true,
      fields: [
        {
          name: 'name',
          label: t('pages.variableManager.name'),
          tooltip: t('pages.variableManager.dataSourceName'),
          value: dataSource?.name || '',
          required: true,
        },
        {
          name: 'description',
          label: t('pages.variableManager.description'),
          tooltip: t('pages.variableManager.dataSourceDescription'),
          value: dataSource?.description || '',
        },
        {
          name: 'value',
          label: t('pages.variableManager.dataSourceValue'),
          tooltip: [t('pages.variableManager.dataSourceFormat'), t('pages.variableManager.dataSourceFormatSingle'), t('pages.variableManager.dataSourceFormatArray'), t('pages.variableManager.dataSourceFormatObject')],
          type: LowCodeFieldType.textarea,
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
                message: t('pages.variableManager.dataSourceRequired')
              }
            }

            try {
              JSON.parse(value)
            } catch (e) {
              logger.error('数据源格式错误:', e)
              return {
                ok: false,
                message: t('pages.variableManager.dataSourceInvalid')
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

  async function onDeleteDataSource (
    _cursorDataSource: IJsVariableSource,
    { targetValues, clearSelection }: IActionContext<IJsVariableSource>
  ) {
    // 开始删除
    await deleteJsVariableSourcesData(targetValues.map((dataSource) => dataSource.id))
    // 移除已经删除的数据
    targetValues.forEach((dataSource) => {
      deleteRowById(dataSource.id)
    })
    clearSelection()

    notifySuccess(t('pages.variableManager.deleteSuccess'))
  }

  return {
    onNewDataSource,
    dataSourceContextMenuItems
  }
}
