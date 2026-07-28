/* eslint-disable @typescript-eslint/no-explicit-any */
/** 右键菜单命令执行时可用的批量操作上下文。 */
export interface IActionContext<TValue> {
  /** 本次命令应处理的值；存在多选时为全部选择项，否则仅包含当前右键值。 */
  readonly targetValues: readonly TValue[]
  /** 通知值的拥有者清空当前选择。 */
  clearSelection: () => void
}

export const ContextMenuWhen = {
  any: 'any',
  onlyMulti: 'onlyMulti',
  onlySingle: 'onlySingle'
} as const

/** 右键菜单通用动作对应的 Material 图标名称 */
export const ContextMenuIcon = {
  add: 'add',
  adminPanelSettings: 'admin_panel_settings',
  block: 'block',
  cancel: 'cancel',
  checkCircle: 'check_circle',
  contentCopy: 'content_copy',
  createNewFolder: 'create_new_folder',
  delete: 'delete',
  deleteSweep: 'delete_sweep',
  download: 'download',
  driveFileMove: 'drive_file_move',
  edit: 'edit',
  factCheck: 'fact_check',
  groupAdd: 'group_add',
  groupRemove: 'group_remove',
  link: 'link',
  lockReset: 'lock_reset',
  pause: 'pause',
  personAdd: 'person_add',
  playArrow: 'play_arrow',
  receiptLong: 'receipt_long',
  replay: 'replay',
  save: 'save',
  science: 'science',
  send: 'send',
  stop: 'stop',
  uploadFile: 'upload_file',
  verified: 'verified',
  visibility: 'visibility'
} as const

/** 描述一个作用于指定业务值的右键菜单命令。 */
export interface IContextMenuItem<T = Record<string, any>> {
  name: string
  label: string
  tooltip?: string | string[] | ((params?: T) => Promise<string[]>)
  color?: string
  icon?: string // 图标
  when?: keyof typeof ContextMenuWhen
  // 当返回 false 时，右键菜单不会退出
  onClick: (value: T, context: IActionContext<T>) => Promise<void | boolean> | void | boolean
  vif?: (value: T) => boolean
}
