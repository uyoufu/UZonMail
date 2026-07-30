import { defineBoot } from '#q-app/wrappers'
import { restoreColorTheme } from 'src/stores/theme'

/** 在应用渲染前恢复浏览器保存的品牌色主题 */
export default defineBoot(() => {
  restoreColorTheme()
})
