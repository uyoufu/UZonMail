import type { IEmailGroup } from 'src/api/emailGroup'

/**
 * 邮箱组数据结构
 */
export interface IEmailGroupListItem extends IEmailGroup {
  name: string,
  active?: boolean,
  label: string,
  side?: string
}

export interface IFlatHeader {
  label?: string,
  icon?: string,
}
