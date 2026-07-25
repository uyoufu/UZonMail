import type { LangKey } from 'src/i18n/types'

/** 状态标签支持的原始值类型。 */
export type StatusChipValue = string | number | boolean

/** 状态标签的显示样式和可选翻译配置。 */
export interface IStatusChipItem {
  readonly status: StatusChipValue
  readonly label?: string
  readonly labelKey?: LangKey
  readonly color?: string
  readonly textColor?: string
  readonly icon?: string
}
