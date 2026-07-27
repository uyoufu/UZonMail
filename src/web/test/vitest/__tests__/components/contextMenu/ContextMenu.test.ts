import { flushPromises, mount } from '@vue/test-utils'
import { computed, defineComponent, h, ref } from 'vue'
import { afterAll, beforeAll, describe, expect, it, vi } from 'vitest'
import ContextMenu from 'src/components/contextMenu/ContextMenu.vue'
import type { IActionContext, IContextMenuItem } from 'src/components/contextMenu/types'

interface TestContextValue {
  id: number
  name: string
}

const SlotStub = defineComponent({
  setup(_, { slots }) {
    return () => h('div', slots.default?.())
  }
})

function mountContextMenu(
  items: IContextMenuItem<TestContextValue>[],
  value: TestContextValue,
  selectedValues: TestContextValue[] = []
) {
  // Vue Test Utils 会丢失泛型 SFC 的类型参数，转换仅限定在测试挂载边界。
  const mountItems = items as unknown as IContextMenuItem<object>[]
  return mount(ContextMenu, {
    props: { items: mountItems, value, selectedValues },
    global: {
      stubs: {
        QMenu: SlotStub,
        QList: SlotStub,
        QItem: SlotStub,
        QIcon: true,
        QItemSection: SlotStub,
        AsyncTooltip: true
      }
    }
  })
}

describe('ContextMenu', () => {
  beforeAll(() => {
    vi.stubGlobal('computed', computed)
    vi.stubGlobal('ref', ref)
  })

  afterAll(() => {
    vi.unstubAllGlobals()
  })

  it('uses the current value when there is no selection', async () => {
    const currentValue = { id: 1, name: 'current' }
    let receivedContext: IActionContext<TestContextValue> | undefined
    const onClick = vi.fn((_value: TestContextValue, context: IActionContext<TestContextValue>) => {
      receivedContext = context
    })
    const wrapper = mountContextMenu([{ name: 'open', label: 'Open', onClick }], currentValue)

    await wrapper.get('.active-item').trigger('click')
    await flushPromises()

    expect(onClick).toHaveBeenCalledWith(currentValue, expect.any(Object))
    expect(receivedContext?.targetValues).toEqual([currentValue])
  })

  it('prefers all selected values and can clear the parent selection', async () => {
    const currentValue = { id: 1, name: 'current' }
    const selectedValues = [
      { id: 2, name: 'selected-1' },
      { id: 3, name: 'selected-2' }
    ]
    let receivedContext: IActionContext<TestContextValue> | undefined
    const items: IContextMenuItem<TestContextValue>[] = [{
      name: 'delete',
      label: 'Delete',
      onClick: (_value, context) => {
        receivedContext = context
      }
    }]
    const wrapper = mountContextMenu(items, currentValue, selectedValues)

    await wrapper.get('.active-item').trigger('click')
    await flushPromises()

    expect(receivedContext?.targetValues).toEqual(selectedValues)
    receivedContext?.clearSelection()
    expect(wrapper.emitted('update:selectedValues')).toEqual([[[]]])
  })

  it('keeps existing single-argument menu callbacks compatible', async () => {
    const currentValue = { id: 1, name: 'current' }
    const onClick = vi.fn((value: TestContextValue) => {
      expect(value).toBe(currentValue)
    })
    const wrapper = mountContextMenu([{ name: 'open', label: 'Open', onClick }], currentValue)

    await wrapper.get('.active-item').trigger('click')
    await flushPromises()

    expect(onClick).toHaveBeenCalledWith(currentValue, expect.any(Object))
  })
})
