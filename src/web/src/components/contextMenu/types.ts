/* eslint-disable @typescript-eslint/no-explicit-any */
export interface IContextMenuItem<T = Record<string, any>> {
  name: string,
  label: string,
  tooltip?: string | string[] | ((params?: T) => Promise<string[]>),
  color?: string,
  icon?: string, // 图标
  // 当返回 false 时，右键菜单不会退出
  onClick: (value: T) => Promise<void | boolean> | void | boolean,
  vif?: (value: T) => boolean,
}
