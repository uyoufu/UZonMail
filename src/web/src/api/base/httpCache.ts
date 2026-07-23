import type { IAxiosRequestConfig } from './types'

/* eslint-disable @typescript-eslint/no-explicit-any */
class HttpCache {
  // 缓存器名称
  cacheKey: string
  private cache: Map<string, any> = new Map()

  constructor (cacheKey: string) {
    this.cacheKey = cacheKey
  }

  public get (key: string) {
    return this.cache.get(key)
  }

  /**
   * 设置缓存, key 是请求参数
   * @param key
   * @param value
   */
  public set<T = any> (key: string, value: T) {
    this.cache.set(key, value)
  }

  public delete (key: string) {
    this.cache.delete(key)
  }

  public clear () {
    this.cache.clear()
  }
}

/**
 * 缓存管理器
 */
class HttpCacheManager {
  private cache: Map<string, HttpCache> = new Map()

  public getCacheManager (key: string) {
    return this.cache.get(key)
  }

  /**
   * 添加缓存器
   * @param key
   * @param value
   */
  public setCacheManager (cacheStore: HttpCache) {
    this.cache.set(cacheStore.cacheKey, cacheStore)
  }

  public deleteCacheManager (key: string) {
    this.cache.delete(key)
  }
}

const cacheManager = new HttpCacheManager()
// 增加一个全局的缓存器
const globalCache = new HttpCache('global')
cacheManager.setCacheManager(globalCache)

// 导出缓存管理器
export default cacheManager

/**
 * 通过随机的唯一 id 创建一个缓存器
 * 并添加到缓存管理器中
 */
export function createHttpCache () {
  const cacheKey = `${Date.now()}_${Math.random().toString(16).substring(2)}`
  const cacheStore = new HttpCache(cacheKey)
  cacheManager.setCacheManager(cacheStore)
  return cacheStore
}

/**
 * 从 cache 中获取缓存的数据
 * @param config
 * @returns
 */
export function getDataFromCache<R, D = any> (url: string, config?: IAxiosRequestConfig<D>): { ok: boolean, data: R } {
  if (!config || !config.cacheKey) return { ok: false, data: null as R }
  const key = url + JSON.stringify(config.params)
  const cacheStore = cacheManager.getCacheManager(config.cacheKey)
  if (!cacheStore) return { ok: false, data: null as R }
  const cachedData = cacheStore.get(key)
  if (cachedData === undefined) return { ok: false, data: null as R }
  return { ok: true, data: cachedData as R }
}

/**
 * 将 data 保存到缓存中
 * @param config
 * @param data
 * @returns
 */
export function setDataToCache<R> (url: string, config: IAxiosRequestConfig | undefined, data: R) {
  if (!config || !config.cacheKey) return

  const key = url + JSON.stringify(config.params)
  const cacheStore = cacheManager.getCacheManager(config.cacheKey)
  if (!cacheStore) return
  cacheStore.set(key, data)
}

/**
 * 使用实例级别的请求缓存
 * 需要在每个实例中去调用,然后请求时，附带该 key 即可
 */
export function useInstanceRequestCache () {
  const cache = createHttpCache()

  onUnmounted(() => {
    // 清空缓存
    cacheManager.deleteCacheManager(cache.cacheKey)
  })

  return cache.cacheKey
}
