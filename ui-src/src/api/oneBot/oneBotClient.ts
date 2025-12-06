import HttpClient from "src/api/base/httpClient"

/**
 * 用于向本地请求 oneBot 相关的 api
 */
export const httpClient = new HttpClient({
  notifyError: true,
  baseUrl: "http://localhost:3000",
  api: '',
  removeResponseInterceptors: true
})
