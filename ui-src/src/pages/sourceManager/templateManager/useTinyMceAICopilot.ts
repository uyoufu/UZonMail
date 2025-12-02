
import type { IPopupDialogParams } from 'src/components/popupDialog/types'
import { PopupDialogFieldType } from 'src/components/popupDialog/types'
import { translateAI } from 'src/i18n/helpers'
import { notifyError, notifySuccess, notifyUntil, showDialog } from 'src/utils/dialog'
import { generateEmailBody, enhanceEmailBody } from 'src/api/aiCopilot'


export function useTinyMceAICopilot (tinymceEditorValueRef: Ref<string>) {
  const copilotRunningTip = ref('')

  // 生成邮件正文
  async function onGenerateContentByCopilot () {
    const popupParams: IPopupDialogParams = {
      title: translateAI('contentGeneration'),
      oneColumn: true,
      fields: [
        {
          name: 'prompt',
          label: translateAI('prompt'),
          type: PopupDialogFieldType.textarea,
          required: true
        }
      ]
    }

    const userInput = await showDialog(popupParams)
    if (!userInput.ok)
      return

    if (!userInput.data.prompt) {
      notifyError(translateAI('pleaseInputPrompt'))
      return
    }

    copilotRunningTip.value = translateAI('generatingEmailContent')

    const result = await notifyUntil(async () => {
      // 根据提示词生成内容
      const { data: emailBody } = await generateEmailBody(userInput.data.prompt as string)
      return emailBody
    }, translateAI('generatingEmailContent'))
    // 重置提示
    copilotRunningTip.value = ''
    // 判断结果
    if (!result) return

    // 将生成的内容覆盖到编辑器中
    tinymceEditorValueRef.value = result
  }

  // 优化邮件正文
  async function onEnhanceContentByCopilot () {
    if (tinymceEditorValueRef.value.trim() === '') {
      notifyError(translateAI('pleaseInputPrompt'))
      return
    }

    copilotRunningTip.value = translateAI('generatingEmailContent')

    // 根据提示词生成内容
    const { data: result, ok } = await enhanceEmailBody(tinymceEditorValueRef.value)
    // 重置提示
    copilotRunningTip.value = ''

    // 判断结果
    if (!ok) return

    // 将生成的内容覆盖到编辑器中
    tinymceEditorValueRef.value = result

    notifySuccess(translateAI('contentEnhancementSuccess'))
  }

  return { copilotRunningTip, onGenerateContentByCopilot, onEnhanceContentByCopilot }
}
