/**
 * 获取 public/icons 中的 svg 全名,用于在 quasar 中使用 svg 图标
 * @param svgName
 * @returns
 */
export function resolveSvgFullName (svgName: string) {
  return `img:/icons/${svgName.replace('.svg', '')}.svg`
}
