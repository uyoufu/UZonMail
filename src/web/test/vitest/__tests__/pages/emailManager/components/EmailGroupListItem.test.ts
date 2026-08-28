import { mount } from '@vue/test-utils'
import { defineComponent, h } from 'vue'
import { describe, expect, it } from 'vitest'
import EmailGroupListItem from 'src/pages/emailManager/components/EmailGroupListItem.vue'
import type { IEmailGroupListItem } from 'src/pages/emailManager/components/types'

const CheckboxStub = defineComponent({
  props: {
    modelValue: {
      type: Boolean,
      default: false
    }
  },
  emits: ['update:modelValue'],
  setup(props, { emit }) {
    return () => h('input', {
      type: 'checkbox',
      checked: props.modelValue,
      onChange: (event: Event) => emit('update:modelValue', (event.target as HTMLInputElement).checked)
    })
  }
})

function createGroup(): IEmailGroupListItem {
  return {
    id: 1,
    name: 'Priority accounts',
    label: 'Priority accounts',
    icon: 'group',
    order: 1,
    accountCount: 3,
    selected: false,
    active: true
  }
}

function mountGroupListItem(group: IEmailGroupListItem) {
  return mount(EmailGroupListItem, {
    props: {
      group,
      selectable: true,
      readonly: true,
      draggable: true,
      contextMenuItems: []
    },
    global: {
      stubs: {
        AsyncTooltip: true,
        ContextMenu: true,
        QCheckbox: CheckboxStub
      }
    }
  })
}

describe('EmailGroupListItem', () => {
  it('renders a Quasar item instead of an unresolved q-item element', () => {
    const wrapper = mountGroupListItem(createGroup())

    expect(wrapper.element.tagName).not.toBe('Q-ITEM')
    expect(wrapper.classes()).toContain('q-item')
    expect(wrapper.text()).toContain('Priority accounts')
    expect(wrapper.find('.group-drag-handle').exists()).toBe(true)
    expect(wrapper.find('.q-badge').text()).toBe('3')
  })

  it('emits row and selection changes with the current group', async () => {
    const group = createGroup()
    const wrapper = mountGroupListItem(group)

    await wrapper.trigger('click')
    await wrapper.get('input[type="checkbox"]').setValue(true)

    expect(wrapper.emitted('click')).toEqual([[group]])
    expect(group.selected).toBe(false)
    expect(wrapper.emitted('selection-change')).toEqual([[group, true]])
  })
})
