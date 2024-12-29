/* eslint-disable @typescript-eslint/no-explicit-any */
import { QuasarContext } from '@quasar/app-vite/types/configuration/context'
import appConfigs from './app.config'
import { IAppConfig } from './types'

import _ from 'lodash'

/**
 * 具体的配置接口
 */
const config: IAppConfig = {} as IAppConfig
let isConfigInitialized = false

/**
 * 获取配置
 * @param quasarContext 可为空，为空时，表示是浏览器中使用，非空时，表示是开发中的 nodejs 环境
 * @returns
 */
export function useConfig (quasarContext?: QuasarContext): IAppConfig {
  // 当为空时，根据环境使用配置
  if (!isConfigInitialized) {
    isConfigInitialized = true

    // 参考：https://vitejs.dev/guide/env-and-mode.html
    // console.log(import.meta.env, '----', process.env)
    const isDev = quasarContext ? quasarContext.dev : import.meta.env.DEV
    // 解析 config
    Object.assign(config, appConfigs.default, isDev ? appConfigs.dev : appConfigs.prod)
  }

  return config
}

/**
 * 修改配置
 * @param newConfig
 */
export function changeConfig (newConfig: IAppConfig) {
  console.log('[config] 修改配置:', newConfig)
  _.merge(config, newConfig)
}
