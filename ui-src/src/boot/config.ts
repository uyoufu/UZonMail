import { defineBoot } from '#q-app/wrappers'
import { changeConfig } from 'src/config/index'
import { IAppConfig } from 'src/config/types'

/**
 * 配置 logger
 */
export default defineBoot(async () => {
  // 从服务器获取 /app.config.ts 获取配置
  const origin = typeof window !== 'undefined' ? window.location.origin : ''
  const fetchConfig = new Promise((resolve) => {
    if (!origin) resolve({})
    const configUrl = `${origin}/app.config.json`
    console.log('[config] ', `正在从${configUrl}获取配置`)
    fetch(configUrl)
      .then(async response => {
        const configResult = await response.json()
        console.log('[config] ', '获取远程配置成功:', configResult)
        resolve(configResult)
      }
      )
      .catch((error) => {
        console.log('[config] ', '获取配置失败，使用默认配置', error)
        resolve({})
      })
  })

  const config = (await fetchConfig) as IAppConfig
  changeConfig(config)
})
