/* eslint-disable @typescript-eslint/no-explicit-any */
import type { AxiosRequestConfig, AxiosResponse } from 'axios'

/**
 * 初始化化 Client 的参数
 */
export interface IHttpClientOptions {
  // 提示错误
  notifyError?: boolean,
  // 重写配置中的 baseUrl
  baseUrl?: string,
  // 重写配置中的 api
  api?: string,

  // 默认不移除
  removeRequestInterceptors?: boolean,
  removeResponseInterceptors?: boolean
}

/**
 * 返回值参数
 */
export interface IResponseData<T> {
  axiosResponse?: AxiosResponse,
  data: T,
  code: number,
  message: string,
  ok: boolean,
  status?: string,
  retcode?: number
}

/**
 * 自定义请求类型
 * 增加缓存策略
 */
export interface IAxiosRequestConfig<T = any> extends AxiosRequestConfig<T> {
  // 缓存的 Key
  cacheKey?: string,
  // 如果返回值非 200, 不触发错误，直接返回
  passError?: boolean,
  stopNotifyError?: boolean
}
