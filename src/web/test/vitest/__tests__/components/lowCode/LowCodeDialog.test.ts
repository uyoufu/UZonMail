import { mount } from '@vue/test-utils'
import { defineComponent, h, ref } from 'vue'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import LowCodeDialog from 'src/components/lowCode/LowCodeDialog.vue'

const mocks = vi.hoisted(() => ({
  onDialogCancel: vi.fn(),
  onDialogHide: vi.fn(),
  onDialogOK: vi.fn()
}))

vi.mock('quasar', async (importOriginal) => {
  const original = await importOriginal()
  if (!original || typeof original !== 'object') throw new Error('Quasar module mock initialization failed')

  return {
    ...original,
    useDialogPluginComponent: Object.assign(
      () => ({
        dialogRef: ref(null),
        onDialogCancel: mocks.onDialogCancel,
        onDialogHide: mocks.onDialogHide,
        onDialogOK: mocks.onDialogOK
      }),
      { emits: ['ok', 'hide'] }
    )
  }
})

const SlotStub = defineComponent({
  setup(_, { slots }) {
    return () => h('div', slots.default?.())
  }
})

const TitleBarStub = defineComponent({
  props: {
    title: {
      type: String,
      required: true
    }
  },
  emits: ['close'],
  setup(props, { emit }) {
    return () => h('button', {
      type: 'button',
      'data-title-bar': '',
      onClick: () => emit('close')
    }, props.title)
  }
})

function mountLowCodeDialog(title = '') {
  return mount(LowCodeDialog, {
    props: {
      title,
      fields: []
    },
    global: {
      stubs: {
        LowCodeForm: true,
        QCard: SlotStub,
        QDialog: SlotStub,
        TitleBar: TitleBarStub
      }
    }
  })
}

describe('LowCodeDialog', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('cancels the dialog when the title bar is closed', async () => {
    const wrapper = mountLowCodeDialog('Edit profile')

    await wrapper.get('[data-title-bar]').trigger('click')

    expect(mocks.onDialogCancel).toHaveBeenCalledOnce()
  })

  it('does not render a title bar when no title is provided', () => {
    const wrapper = mountLowCodeDialog()

    expect(wrapper.find('[data-title-bar]').exists()).toBe(false)
  })
})
