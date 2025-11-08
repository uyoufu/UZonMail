// 一些 Label 工具函数

import { translateUtils } from "src/i18n/helpers"

/**
 * 生成摘要标签
 * 示例：张三,李四... 等共 5 人
 * @param labels
 * @param maxCount
 * @param unit
 * @returns
 */
export function createAbstractLabel (labels: string[], maxCount: number, unit?: string): string {
  if (labels.length <= maxCount) {
    return labels.join(',')
  } else {
    return labels.slice(0, maxCount).join(',') + translateUtils('totalItems', {
      count: labels.length,
      unit: unit || translateUtils('item')
    })
  }
}
