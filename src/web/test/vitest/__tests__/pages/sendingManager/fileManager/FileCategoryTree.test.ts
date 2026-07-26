import { flushPromises, mount } from '@vue/test-utils'
import { computed, defineComponent, h, onMounted, ref, type PropType } from 'vue'
import { afterAll, beforeAll, describe, expect, it, vi } from 'vitest'
import type { IFileCategory } from 'src/api/fileCategory'
import FileCategoryTree from 'src/pages/sendingManager/fileManager/FileCategoryTree.vue'

vi.mock('vue-i18n', () => ({
  useI18n: () => ({
    t: (key: string) => {
      const translations: Record<string, string> = {
        'fileManager.allFiles': '全部文件',
        'fileManager.defaultCategory': '默认分类'
      }
      return translations[key] ?? key
    }
  })
}))

vi.mock('src/api/fileCategory', () => ({
  createFileCategory: vi.fn(),
  deleteFileCategory: vi.fn(),
  getFileCategories: vi.fn().mockResolvedValue({
    data: [
      { id: 1, name: '项目资料', sort: 1, isDefault: false },
      { id: 2, name: 'Default', sort: 2, isDefault: true }
    ]
  }),
  moveFileCategory: vi.fn(),
  renameFileCategory: vi.fn()
}))

vi.mock('src/utils/dialog', () => ({
  confirmOperation: vi.fn(),
  showDialog: vi.fn()
}))

const DraggableTreeStub = defineComponent({
  name: 'DraggableTree',
  props: {
    data: {
      type: Array as PropType<IFileCategory[]>,
      required: true
    }
  },
  setup(props, { slots }) {
    return () => h('div', props.data.map(category => h(
      'div',
      { 'data-category-id': category.id },
      slots.default?.({ data: category })
    )))
  }
})

describe('FileCategoryTree', () => {
  beforeAll(() => {
    vi.stubGlobal('computed', computed)
    vi.stubGlobal('onMounted', onMounted)
    vi.stubGlobal('ref', ref)
  })

  afterAll(() => {
    vi.unstubAllGlobals()
  })

  it('uses name as the only node display field', async () => {
    const wrapper = mount(FileCategoryTree, {
      global: {
        stubs: {
          DraggableTree: DraggableTreeStub,
          QBtn: true,
          QIcon: true,
          QSeparator: true,
          QTooltip: true
        }
      }
    })
    await flushPromises()

    const treeNodes = wrapper.getComponent(DraggableTreeStub).props('data')
    expect(treeNodes[0]).toMatchObject({ id: 0, name: '全部文件' })
    expect(treeNodes.every(category => !('label' in category))).toBe(true)
    expect(wrapper.get('[data-category-id="0"]').text()).toContain('全部文件')
    expect(wrapper.get('[data-category-id="1"]').text()).toContain('项目资料')
    expect(wrapper.get('[data-category-id="2"]').text()).toContain('默认分类')
  })
})
