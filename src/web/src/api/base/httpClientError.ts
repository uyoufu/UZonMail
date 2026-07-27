import type { AxiosResponse } from 'axios'
import type { IResponseData } from './types'

export const HttpClientErrorCode = {
  invalidResponse: 'INVALID_RESPONSE',
  unknown: 'UNKNOWN_ERROR'
} as const

interface HttpClientErrorOptions {
  responseData?: IResponseData<unknown>
  axiosResponse?: AxiosResponse
  cause?: unknown
}

/** 表示 HttpClient 归一化后的业务、HTTP 或网络请求失败。 */
export class HttpClientError extends Error {
  readonly code: number | string
  readonly responseData?: IResponseData<unknown>
  readonly axiosResponse?: AxiosResponse

  constructor(message: string, code: number | string, options: HttpClientErrorOptions = {}) {
    super(message, { cause: options.cause })
    this.name = 'HttpClientError'
    this.code = code
    this.responseData = options.responseData
    this.axiosResponse = options.axiosResponse
  }
}
