import { defineBoot } from '#q-app/wrappers'
import log, { LogLevelNames } from 'loglevel'

// 声明全局变量
declare global {
  interface Window {
    setLogLevel: (level: LogLevelNames) => string;
  }
}

/**
 * 配置 logger
 */
export default defineBoot(() => {
  // 设置日志级别
  const logLevel = (process.env.LOG_LEVEL || 'info') as LogLevelNames
  console.log('[logger] log level is set to', logLevel)
  log.setLevel(logLevel)

  if (window) {
    // 向 windows 暴露 setLogLevel 方法
    window.setLogLevel = (level: LogLevelNames) => {
      log.setLevel(level)
      console.log(`[logger] set log level to: ${level}`)
      return 'Success!'
    }
  }
})
