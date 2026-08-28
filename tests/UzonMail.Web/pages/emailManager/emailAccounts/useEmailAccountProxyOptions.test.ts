import { defineComponent, h, ref } from 'vue'
import { mount } from '@vue/test-utils'
import { afterAll, beforeAll, beforeEach, describe, expect, it, vi } from 'vitest'
import { useEmailAccountProxyOptions } from 'src/pages/emailManager/emailAccounts/useEmailAccountProxyOptions'

const mocks = vi.hoisted(() => ({
  getUsableProxies: vi.fn()
}))

vi.mock('src/api/proxy', () => ({
  getUsableProxies: mocks.getUsableProxies
}))

type ProxyOptionsApi = ReturnType<typeof useEmailAccountProxyOptions>

function mountProxyOptions() {
  let proxyOptionsApi: ProxyOptionsApi | undefined
  const host = defineComponent({
    setup() {
      proxyOptionsApi = useEmailAccountProxyOptions()
      return () => h('div')
    }
  })
  const wrapper = mount(host)
  if (!proxyOptionsApi) throw new Error('Proxy options were not initialized')
  return { wrapper, proxyOptionsApi }
}

describe('useEmailAccountProxyOptions', () => {
  beforeAll(() => {
    vi.stubGlobal('ref', ref)
  })

  afterAll(() => vi.unstubAllGlobals())

  beforeEach(() => {
    vi.clearAllMocks()
    mocks.getUsableProxies.mockResolvedValue({
      data: [
        {
          id: 11,
          name: 'US outbound',
          isActive: true,
          url: 'http://proxy.example.test:8080',
          userId: 1,
          organizationId: 2
        }
      ]
    })
  })

  it('loads usable database proxies once when the selector opens', async () => {
    const { wrapper, proxyOptionsApi } = mountProxyOptions()

    await proxyOptionsApi.onProxyPopupShow()
    await proxyOptionsApi.onProxyPopupShow()

    expect(mocks.getUsableProxies).toHaveBeenCalledTimes(1)
    expect(proxyOptionsApi.proxyOptions.value).toMatchObject([{ id: 11, name: 'US outbound' }])
    expect(proxyOptionsApi.isLoadingProxyOptions.value).toBe(false)
    wrapper.unmount()
  })
})
