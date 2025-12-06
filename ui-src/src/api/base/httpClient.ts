/* eslint-disable @typescript-eslint/no-explicit-any */
import type { AxiosInstance, AxiosResponse } from 'axios'
import axios from 'axios'
import logger from 'loglevel'

import type { IAxiosRequestConfig, IHttpClientOptions, IResponseData } from './types'
import { useUserInfoStore } from 'src/stores/user'
import { StatusCode } from 'status-code-enum'
import { notifyError } from 'src/utils/dialog'

import { getDataFromCache, setDataToCache } from './httpCache'

import { useConfig } from 'src/config'

/**
 * HttpClient 封装
 */
export default class HttpClient {
  private _options: IHttpClientOptions
  private _axios: AxiosInstance

  public get axios () {
    return this._axios
  }

  constructor(options: IHttpClientOptions) {
    this._options = options
    this._axios = this.createAxios()

    if (!options.removeRequestInterceptors)
      this.setRequestInterceptors(this._axios)
    if (!options.removeResponseInterceptors)
      this.setResponseInterceptors(this._axios)
  }

  // 获取基础 url
  private getBaseUrl (): string {
    const config = useConfig()
    const baseUrl = this._options.baseUrl || config.baseUrl
    const api = (this._options.api === null || this._options.api === undefined)
      ? config.api : this._options.api
    const finalBaseUrl = `${baseUrl}${api}`
    logger.debug('[HttpClient] BaseUrl:', finalBaseUrl)
    return finalBaseUrl
  }

  // 创建 axios 实例
  private createAxios () {
    return axios.create({
      baseURL: this.getBaseUrl(),
      responseType: 'json'
    })
  }

  // 添加请求拦截器
  private setRequestInterceptors (axiosInstance: AxiosInstance) {
    axiosInstance.interceptors.request.use((config) => {
      const store = useUserInfoStore()
      // 自动添加 token
      config.headers.Authorization = 'Bearer ' + store.token
      return config
    },
      (error) => {
        return Promise.reject(error as Error)
      })
  }

  // 添加响应拦截器
  private setResponseInterceptors (axiosInstance: AxiosInstance) {
    axiosInstance.interceptors.response.use(async (response) => {
      // 有可能后端返回的是流
      if (response.headers['content-type'] === 'application/octet-stream') {
        console.log('response is stream:', response)
        return response
      }


      const data = response.data as IResponseData<any>
      logger.debug('[HttpClient] Response data:', data)
      // eslint-disable-next-line @typescript-eslint/no-unsafe-enum-comparison
      if (data.code === StatusCode.SuccessOK)
        return response

      // 非 200 状态码，进行提示
      const config = response.config as IAxiosRequestConfig<any>

      // 处理错误
      if (this._options.notifyError && !config.stopNotifyError) {
        // 提示错误
        notifyError(data.message)
      }

      // 允许通过错误，返回结果
      if (config.passError)
        return response

      // 返回错误
      // eslint-disable-next-line @typescript-eslint/prefer-promise-reject-errors
      return Promise.reject(data)

    },
      // 当 response.status 不是 200 时触发
      async (error) => {
        logger.error('[HttpClient] Response error:', error)
        if (!error.response && error.code) {
          // 当不阻止错误通知时，进行提示
          if (!error.config.stopNotifyError)
            notifyError(error.code)
          return Promise.reject(error as Error)
        }

        const response = error.response as AxiosResponse
        // eslint-disable-next-line @typescript-eslint/no-unsafe-enum-comparison
        if (response.status === StatusCode.ClientErrorUnauthorized) {
          // 退出登录
          await this.logout()
          return Promise.reject(error as Error)
        }

        if (!error.config.stopNotifyError) {
          if (!response.data) {
            // 其它错误，进行提示，后端返回的错误，都会进行消息展示
            notifyError(response.statusText)
          } else {
            notifyError(error.message)
          }
        }

        return Promise.reject(error as Error)
      })
  }

  // 退出登录
  private async logout () {
    const store = useUserInfoStore()
    await store.logout()
  }

  // 格式化配置
  private formatConfig (config?: IAxiosRequestConfig): IAxiosRequestConfig {
    const baseURL = this.getBaseUrl()

    const newConfig = {
      baseURL
    }
    return Object.assign(newConfig, config)
  }

  // #region 对请求返回值的data进行解构，方便前端使用
  private destructureAxiosResponse<R> (response: AxiosResponse<IResponseData<R>>): IResponseData<R> {
    let data = response.data
    // console.log('destructureAxiosResponse:', response)
    // 如果是流，要单独处理
    if (response.headers['content-type'] === 'application/octet-stream') {
      data = {
        data: response.data as unknown as R,
        code: StatusCode.SuccessOK,
        message: 'ok',
        ok: true
      }
    }
    data.axiosResponse = response

    return data
  }

  /**
   * 通用请求
   * @param config
   * @returns
   */
  async request<R, D = any> (config: IAxiosRequestConfig<D>): Promise<IResponseData<R>> {
    config = this.formatConfig(config)
    const responseData = await this._axios.request<R, AxiosResponse<IResponseData<R>, D>, D>(config)
    return this.destructureAxiosResponse(responseData)
  }

  /**
   * get 请求
   * @param url 地址，不包含 baseUrl
   * @param config 请求参数和配置
   * @returns
   */
  async get<R, D = any> (url: string, config?: IAxiosRequestConfig<D>): Promise<IResponseData<R>> {
    // 从 cache 中获取值
    // 如果包含 key,则从缓存中获取
    const { ok, data } = getDataFromCache<R, D>(url, config)
    if (ok) {
      return {
        data,
        code: StatusCode.SuccessOK,
        message: 'fromCache',
        ok: true
      }
    }

    config = this.formatConfig(config)
    const response = await this._axios.get<R, AxiosResponse<IResponseData<R>, D>, D>(url, config)
    const dataResult = this.destructureAxiosResponse(response)

    // 添加到缓存
    setDataToCache(url, config, dataResult.data)

    return dataResult
  }

  /**
   * delete 请求
   * @param url
   * @param config
   * @returns
   */
  async delete<R, D = any> (url: string, config?: IAxiosRequestConfig<D>): Promise<IResponseData<R>> {
    config = this.formatConfig(config)
    const responseData = await this._axios.delete<R, AxiosResponse<IResponseData<R>, D>, D>(url, config)
    return this.destructureAxiosResponse(responseData)
  }

  /**
   * head 请求
   * @param url
   * @param config
   * @returns
   */
  async head<R, D = any> (url: string, config?: IAxiosRequestConfig<D>): Promise<IResponseData<R>> {
    config = this.formatConfig(config)
    const responseData = await this._axios.head<R, AxiosResponse<IResponseData<R>, D>, D>(url, config)
    return this.destructureAxiosResponse(responseData)
  }

  /**
   * options 请求
   * @param url
   * @param config
   * @returns
   */
  async options<R, D = any> (url: string, config?: IAxiosRequestConfig<D>): Promise<IResponseData<R>> {
    config = this.formatConfig(config)
    const responseData = await this._axios.options<R, AxiosResponse<IResponseData<R>, D>, D>(url, config)
    return this.destructureAxiosResponse(responseData)
  }

  /**
   * post 请求
   * @param url
   * @param config
   * @returns
   */
  async post<R, D = any> (url: string, config?: IAxiosRequestConfig<D>): Promise<IResponseData<R>> {
    config = this.formatConfig(config)
    const responseData = await this._axios.post<R, AxiosResponse<IResponseData<R>, D>, D>(url, config?.data, config)
    return this.destructureAxiosResponse(responseData)
  }

  /**
   * put 请求
   * @param url
   * @param config
   * @returns
   */
  async put<R, D = any> (url: string, config?: IAxiosRequestConfig<D>): Promise<IResponseData<R>> {
    config = this.formatConfig(config)
    const responseData = await this._axios.put<R, AxiosResponse<IResponseData<R>, D>, D>(url, config?.data, config)
    return this.destructureAxiosResponse(responseData)
  }

  /**
   * patch 请求
   * @param url
   * @param config
   * @returns
   */
  async patch<R, D = any> (url: string, config?: IAxiosRequestConfig<D>): Promise<IResponseData<R>> {
    config = this.formatConfig(config)
    const responseData = await this._axios.patch<R, AxiosResponse<IResponseData<R>, D>, D>(url, config?.data, config)
    return this.destructureAxiosResponse(responseData)
  }

  async postForm<R, D = any> (url: string, config?: IAxiosRequestConfig<D>): Promise<IResponseData<R>> {
    config = this.formatConfig(config)
    const responseData = await this._axios.postForm<R, AxiosResponse<IResponseData<R>, D>, D>(url, config?.data, config)
    return this.destructureAxiosResponse(responseData)
  }

  async patchForm<R, D = any> (url: string, config?: IAxiosRequestConfig<D>): Promise<IResponseData<R>> {
    config = this.formatConfig(config)
    const responseData = await this._axios.patchForm<R, AxiosResponse<IResponseData<R>, D>, D>(url, config?.data, config)
    return this.destructureAxiosResponse(responseData)
  }
  // #endregion
}

/**
 * 默认的 httpClient, 前缀为 /api/v1
 */
export const httpClient = new HttpClient({
  notifyError: true
})

/**
 * pro 版本的 httpClient, 前缀为 /api/pro
 * 调用时，请先使用 userStore.hasProPlugin 判断一下
 */
export const httpClientPro = new HttpClient({
  notifyError: true,
  api: '/api/pro'
})
