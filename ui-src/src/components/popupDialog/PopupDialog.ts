import type { IDialogResult, IPopupDialogParams } from './types'
import { PopupDialogFieldType } from './types'
import { Dialog } from 'quasar'
import LowCodeForm from './LowCodeForm.vue'

/**
 * 弹出对话框
 */
// eslint-disable-next-line @typescript-eslint/no-explicit-any
export async function showDialog<T = Record<string, any>> (dialogParams: IPopupDialogParams): Promise<IDialogResult<T>> {
  // 修改默认值：fields
  dialogParams.fields.forEach(field => {
    if (!field.type) field.type = PopupDialogFieldType.text
  })

  /**
  * 显示对话框并返回结果
  */
  return new Promise((resolve) => {
    Dialog.create({
      component: LowCodeForm,
      componentProps: dialogParams
    }).onOk((model) => {
      // console.log('OK', model)
      resolve({ ok: true, data: model })
    }).onCancel(() => {
      // console.log('Cancel')
      resolve({ ok: false, data: {} as T })
    }).onDismiss(() => {
      // console.log('Dismiss')
      resolve({ ok: false, data: {} as T })
    })
  })
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
export async function showComponentDialog<T = Record<string, any>> (component: Component, componentProps?: any): Promise<IDialogResult<T>> {
  return new Promise((resolve) => {
    Dialog.create({
      component,
      componentProps
    }).onOk((model) => {
      resolve({ ok: true, data: model })
    }).onCancel(() => {
      resolve({ ok: false, data: {} as T })
    }).onDismiss(() => {
      resolve({ ok: false, data: {} as T })
    })
  })
}
