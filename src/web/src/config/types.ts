import type { LogLevelNames } from 'loglevel'

export interface IAppConfig {
  baseUrl: string,
  api: string,
  signalRHub: string,
  // 日志配置
  logger: {
    level: LogLevelNames
  },
  version?: string
}

/**
 * 配置接口
 */
export interface IAppConfigsContainer {
  // 默认配置
  default: IAppConfig,
  // 生产配置
  prod?: IAppConfig,
  // 开发配置
  dev?: IAppConfig
}
