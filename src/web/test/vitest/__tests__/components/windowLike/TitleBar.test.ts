import { mount } from '@vue/test-utils'
import { defineComponent, h } from 'vue'
import { describe, expect, it, vi } from 'vitest'
import TitleBar from 'src/components/windowLike/TitleBar.vue'

vi.mock('src/i18n/helpers', () => ({
  translateButton: (key: string) => key
}))

const QBarStub = defineComponent({
  props: {
    dense: {
      type: Boolean,
      default: false
    }
  },
  setup(props, { slots }) {
    return () => h('div', { 'data-q-bar': '', 'data-dense': String(props.dense) }, slots.default?.())
  }
})

const CommonBtnStub = defineComponent({
  props: {
    icon: {
      type: String,
      required: true
    }
  },
  emits: ['click'],
  setup(props, { emit }) {
    return () => h('button', {
      type: 'button',
      'data-icon': props.icon,
      onClick: () => emit('click')
    })
  }
})

describe('TitleBar', () => {
  it('renders a dense q-bar and emits close from its close button', async () => {
    const wrapper = mount(TitleBar, {
      props: { title: 'Edit profile' },
      global: {
        stubs: {
          QBar: QBarStub,
          CommonBtn: CommonBtnStub
        }
      }
    })

    expect(wrapper.get('[data-q-bar]').attributes('data-dense')).toBe('true')
    expect(wrapper.text()).toContain('Edit profile')
    expect(wrapper.get('button').attributes('data-icon')).toBe('close')

    await wrapper.get('button').trigger('click')

    expect(wrapper.emitted('close')).toEqual([[]])
  })
})
