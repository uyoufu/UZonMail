
/**
 * 可以在这里配置一些全局的配置
 * 这个配置是编译时使用，若在运行时修改配置，请修改 public/app.config.js
 */

import type { IAppConfigsContainer } from 'src/config/types'

export default {
  // 默认配置
  default: {
    baseUrl: 'http://localhost:9000',
    api: '/api/v1',
    signalRHub: '/hubs/uzonMailHub',
    // 日志配置
    logger: {
      level: 'info'
    },
    version: '0.15.1.0'
  },

  // 生产配置
  prod: {
    baseUrl: 'http://localhost:22345',
    api: '/api/v1',
    signalRHub: '/hubs/uzonMailHub',
    // 日志配置
    logger: {
      level: 'info'
    }
  },

  // 开发配置
  dev: {
    baseUrl: 'http://localhost:9000',
    api: '/api/v1',
    signalRHub: '/hubs/uzonMailHub',
    // 日志配置
    logger: {
      level: 'debug'
    }
  }

} as IAppConfigsContainer
