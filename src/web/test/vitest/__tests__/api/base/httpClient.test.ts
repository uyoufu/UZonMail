import {
  AxiosError,
  AxiosHeaders,
  type AxiosRequestConfig,
  type AxiosResponse,
  type InternalAxiosRequestConfig
} from 'axios'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import HttpClient, { HttpClientError } from 'src/api/base/httpClient'
import type { IResponseData } from 'src/api/base/types'

vi.mock('loglevel', () => ({ default: { debug: vi.fn(), error: vi.fn() } }))

const mocks = vi.hoisted(() => ({
  logout: vi.fn(),
  notifyError: vi.fn()
}))

vi.mock('src/config', () => ({ useConfig: () => ({ baseUrl: 'http://localhost', api: '/api/v1' }) }))
vi.mock('src/stores/user', () => ({ useUserInfoStore: () => ({ token: 'token', logout: mocks.logout }) }))
vi.mock('src/utils/dialog', () => ({ notifyError: mocks.notifyError }))

function createAxiosResponse<T>(config: InternalAxiosRequestConfig, data: T, status: number = 200): AxiosResponse<T> {
  return {
    data,
    status,
    statusText: status === 200 ? 'OK' : 'Server Error',
    headers: new AxiosHeaders({ 'content-type': 'application/json' }),
    config
  }
}

function useAdapter(client: HttpClient, adapter: NonNullable<AxiosRequestConfig['adapter']>) {
  client.axios.defaults.adapter = adapter
}

describe('HttpClient', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('rejects business failures as HttpClientError with backend context', async () => {
    const client = new HttpClient({ notifyError: true })
    const responseData: IResponseData<null> = {
      data: null,
      code: 400,
      message: 'business failed',
      ok: false
    }
    useAdapter(client, (config) => Promise.resolve(createAxiosResponse(config, responseData)))

    const request = client.get('/failure')

    await expect(request).rejects.toMatchObject({
      name: 'HttpClientError',
      code: 400,
      message: 'business failed',
      responseData
    })
    expect(mocks.notifyError).toHaveBeenCalledWith('business failed')
  })

  it('keeps passError resolved while preserving the response envelope', async () => {
    const client = new HttpClient({ notifyError: true })
    const responseData: IResponseData<null> = {
      data: null,
      code: 400,
      message: 'allowed failure',
      ok: false
    }
    useAdapter(client, (config) => Promise.resolve(createAxiosResponse(config, responseData)))

    const result = await client.get('/allowed-failure', { passError: true })

    expect(result).toMatchObject(responseData)
    expect(result.axiosResponse).toBeDefined()
  })

  it('normalizes HTTP errors and uses the backend message', async () => {
    const client = new HttpClient({ notifyError: true })
    const responseData: IResponseData<null> = {
      data: null,
      code: 503,
      message: 'service unavailable',
      ok: false
    }
    useAdapter(client, (config) => {
      const response = createAxiosResponse(config, responseData, 503)
      return Promise.reject(
        new AxiosError('Request failed with status code 503', AxiosError.ERR_BAD_RESPONSE, config, undefined, response)
      )
    })

    const request = client.get('/http-failure')

    await expect(request).rejects.toBeInstanceOf(HttpClientError)
    await expect(request).rejects.toMatchObject({ code: 503, message: 'service unavailable' })
    expect(mocks.notifyError).toHaveBeenCalledWith('service unavailable')
  })

  it('normalizes network errors and respects stopNotifyError', async () => {
    const client = new HttpClient({ notifyError: true })
    useAdapter(client, (config) => Promise.reject(new AxiosError('Network Error', AxiosError.ERR_NETWORK, config)))

    const request = client.get('/network-failure', { stopNotifyError: true })

    await expect(request).rejects.toMatchObject({ code: AxiosError.ERR_NETWORK, message: 'Network Error' })
    expect(mocks.notifyError).not.toHaveBeenCalled()
  })
})
