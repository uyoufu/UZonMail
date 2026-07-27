import { mount } from '@vue/test-utils'
import { defineComponent, h, nextTick, onMounted, ref, shallowRef, watch } from 'vue'
import { afterAll, beforeAll, beforeEach, describe, expect, it, vi } from 'vitest'
import { CollapsePositionSide, useTableCollapsePosition } from 'src/components/collapseIcon/useCollapsePosition'

let onResize: ResizeObserverCallback | undefined

vi.mock('@vueuse/core', () => ({
  useResizeObserver: (_target: unknown, callback: ResizeObserverCallback) => {
    onResize = callback
    return { isSupported: ref(true), stop: vi.fn() }
  }
}))

function createRect(left: number, right: number): DOMRect {
  return {
    x: left,
    y: 0,
    width: right - left,
    height: 100,
    top: 0,
    right,
    bottom: 100,
    left,
    toJSON: () => ({})
  }
}

async function mountPositionHarness(positionSide: typeof CollapsePositionSide[keyof typeof CollapsePositionSide]) {
  const positioningElement = document.createElement('div')
  const tableElement = document.createElement('div')
  let tableRect = createRect(120, 700)
  vi.spyOn(positioningElement, 'getBoundingClientRect').mockImplementation(() => createRect(20, 820))
  vi.spyOn(tableElement, 'getBoundingClientRect').mockImplementation(() => tableRect)
  Object.defineProperty(tableElement, 'offsetParent', { configurable: true, value: positioningElement })

  let collapseStyle: ReturnType<typeof useTableCollapsePosition>['collapseStyleRef'] | undefined
  const Harness = defineComponent({
    setup() {
      const tableRef = ref({ $el: tableElement })
      const result = useTableCollapsePosition(tableRef as never, positionSide, 10)
      collapseStyle = result.collapseStyleRef
      return () => h('div')
    }
  })
  const wrapper = mount(Harness)
  await nextTick()

  return {
    collapseStyle: collapseStyle!,
    setTableRect: (left: number, right: number) => {
      tableRect = createRect(left, right)
    },
    unmount: () => wrapper.unmount()
  }
}

describe('useTableCollapsePosition', () => {
  beforeAll(() => {
    vi.stubGlobal('nextTick', nextTick)
    vi.stubGlobal('onMounted', onMounted)
    vi.stubGlobal('ref', ref)
    vi.stubGlobal('shallowRef', shallowRef)
    vi.stubGlobal('watch', watch)
  })

  afterAll(() => {
    vi.unstubAllGlobals()
  })

  beforeEach(() => {
    onResize = undefined
  })

  it('updates the left position when the table moves after a resize', async () => {
    const harness = await mountPositionHarness(CollapsePositionSide.left)
    expect(harness.collapseStyle.value.left).toBe('110px')

    harness.setTableRect(180, 700)
    onResize?.([], {} as ResizeObserver)

    expect(harness.collapseStyle.value.left).toBe('170px')
    harness.unmount()
  })

  it('updates the right position when the table edge changes after a resize', async () => {
    const harness = await mountPositionHarness(CollapsePositionSide.right)
    expect(harness.collapseStyle.value.right).toBe('130px')

    harness.setTableRect(120, 760)
    onResize?.([], {} as ResizeObserver)

    expect(harness.collapseStyle.value.right).toBe('70px')
    harness.unmount()
  })
})
