import type { IFunctionResult } from 'src/types'

/* eslint-disable @typescript-eslint/no-explicit-any */
export enum PopupDialogFieldType {
  text = 'text',
  textarea = 'textarea',
  boolean = 'boolean',
  selectOne = 'selectOne',
  selectMany = 'selectMany',
  date = 'date',
  password = 'password',
  number = 'number',
  time = 'time',
  email = 'email',
  tel = 'tel',
  file = 'file',
  url = 'url',
  datetimeLocal = 'datetime-local',
  search = 'search',
  editor = 'editor',
}

/**
 * 弹出菜单项
 */
export interface IPopupDialogField {
  type?: PopupDialogFieldType,
  name: string, // 用于返回值的字段名
  label: string, // 显示的名称
  placeholder?: string, // 占位内容
  value?: any, // 默认值
  options?: Array<Record<string, any>> | string[] | [], // 多选或单选时的选项
  optionLabel?: string, // 选项的显示字段
  optionValue?: string, // 选项的值字段
  optionTooltip?: string, // 选项的提示字段
  mapOptions?: boolean,
  emitValue?: boolean, // 是否返回值
  icon?: string, // 图标
  required?: boolean, // 是否必须
  validate?: (value: any, parsedValue: any) => Promise<IFunctionResult>, // 验证函数
  parser?: (value: any) => any, // 解析函数,在返回时，对数据进行转换
  tooltip?: Array<any> | ((params?: object) => Promise<string[]>) | string, // 提示
  disable?: boolean, // 是否禁用，一般用于仅显示数据,
  disableAutogrow?: boolean, // 当为 textarea 时，是否自动增长
  classes?: string, // 自定义样式
}

/**
 * 自定义弹出框按钮
 */
export interface ICustomPopupButton {
  label: string,
  color: string,
  onClick: (value: Record<string, any>) => Promise<void>
}

export interface IOnSetupParams {
  fieldsModel: Ref<Record<string, any>>,
  fields: ComputedRef<Array<IPopupDialogField>>,
}

/**
 * 弹出框参数
 */
export interface IPopupDialogParams {
  title?: string, // 标题
  // 字段定义
  fields: Array<IPopupDialogField>,
  // 数据源
  dataSet?: Record<string, Array<string | number | boolean | object> | Promise<any[]>>,
  // 用于数据验证
  validate?: (data: Record<string, any>) => Promise<IFunctionResult>,
  // 窗体保持
  persistent?: boolean,
  // ok 最后执行的逻辑
  onOkMain?: (params: Record<string, any>) => Promise<void | boolean>,
  // 只有一列
  oneColumn?: boolean,
  customBtns?: ICustomPopupButton[],
  // 在 setup 中调用
  onSetup?: (params: IOnSetupParams) => void,
}

/**
 * 对话框返回的结果
 */
export interface IDialogResult<T = Record<string, any>> extends IFunctionResult {
  data: T
}
