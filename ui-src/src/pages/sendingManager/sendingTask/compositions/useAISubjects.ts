import type { IEmailCreateInfo } from "src/api/emailSending"
import { notifyError, notifyUntil } from "src/utils/dialog"
import { summarizeEmailSubjects } from "src/api/aiCopilot"
import { translateAI } from "src/i18n/helpers"

export function useAISubjects (emailInfoRef: Ref<IEmailCreateInfo>) {
  // AI 根据邮件内容生成邮件主题
  async function onGenerateSubjectsByAI () {
    if (emailInfoRef.value.body.trim() === '' && emailInfoRef.value.templates.length === 0) {
      notifyError(translateAI('bodyOrTemplateRequired'))
      return
    }

    await notifyUntil(async () => {
      const firstTemplateId = emailInfoRef.value.templates.length > 0 ? emailInfoRef.value.templates[0]!.id! : 0
      const { data: subjects } = await summarizeEmailSubjects(firstTemplateId, emailInfoRef.value.body)
      emailInfoRef.value.subjects = subjects || ''
    }, translateAI('generatingSubjects'))
  }

  return {
    onGenerateSubjectsByAI
  }
}
