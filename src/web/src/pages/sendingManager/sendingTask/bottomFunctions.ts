/* eslint-disable @typescript-eslint/no-explicit-any */
// 底部按钮
import OkBtn from 'src/components/quasarWrapper/buttons/OkBtn.vue'
import CommonBtn from 'src/components/quasarWrapper/buttons/CommonBtn.vue'
import { notifyError, notifySuccess } from 'src/utils/dialog'
import { formatDateToUTC } from 'src/utils/format'
import type { IEmailCreateInfo } from 'src/api/emailSending'
import { sendEmailNow, sendSchedule } from 'src/api/emailSending'

import { showComponentDialog } from 'src/components/lowCode/PopupDialog'
import PreviewSendingDialog from './components/PreviewSendingDialog.vue'
import SendingProgress from '../sendingProgress/SendingProgress.vue'
import SelectScheduleDate from './components/SelectScheduleDate.vue'

import logger from 'loglevel'
import { translateSendingTask } from 'src/i18n/helpers'

/**
 * 使用底部功能定义
 * @param emailInfo
 * @returns
 */
export function useBottomFunctions (emailInfo: Ref<IEmailCreateInfo>) {
  // 数据验证
  const needUpload = ref(false)
  // 存在全局的 body
  const existGlobalBody = computed(() => {
    return emailInfo.value.templates.length || emailInfo.value.body
  })
  // 验证 excel 数据
  interface IValidateExcelDataResult {
    recipientStatus: number, // 0 全部不存在，1 部分存在，2 所有数据都存在
    senderStatus: number,
    bodyStatus: number,
    recipientEmailSet: Set<string>
  }
  // 验证 excel 数据，参数必须不能为空
  function validateExcelData (dataList: Record<string, any>[]): IValidateExcelDataResult {
    const recipientEmailSet = new Set<string>()
    // 验证收件箱
    let recipientsCount = 0, senderAccountsCount = 0, bodiesCount = 0
    for (const data of dataList) {
      if (data.recipientEmail) {
        recipientsCount++
        recipientEmailSet.add(data.recipientEmail)
      }

      if (data.senderEmail) senderAccountsCount++
      if (data.templateId || data.templateName || data.body) bodiesCount++
    }

    logger.debug(`[NewEmail] recipient count: ${recipientsCount}, senderAccount count: ${senderAccountsCount}, body count: ${bodiesCount}, data count: ${dataList.length}`)

    return {
      recipientStatus: formatExcelValidationStatus(recipientsCount, dataList.length),
      senderStatus: formatExcelValidationStatus(senderAccountsCount, dataList.length),
      bodyStatus: formatExcelValidationStatus(bodiesCount, dataList.length),
      recipientEmailSet
    }
  }
  function formatExcelValidationStatus (count: number, total: number) {
    if (count >= total) return 2
    if (count <= 0) return 0
    return 1
  }
  function validateParamsWhenNoExcelData () {
    if (!existGlobalBody.value) {
      notifyError(translateSendingTask('emailTemplateAndBodyRequired'))
      return false
    }

    if (!emailInfo.value.senderAccounts.length && !emailInfo.value.senderAccountGroups.length) {
      notifyError(translateSendingTask('pleaseSelectSender'))
      return false
    }

    if (!emailInfo.value.recipients.length && !emailInfo.value.recipientContactGroups.length) {
      notifyError(translateSendingTask('pleaseSelectRecipients'))
      return false
    }

    return true
  }
  // 数据验证
  function validateSendingTaskParams () {
    logger.debug('[sendingTask] validateSendingTaskParams email info:', emailInfo.value)
    if (!emailInfo.value.subjects) {
      notifyError(translateSendingTask('pleaseInputEmailSubject'))
      return false
    }

    // 没有 excel 数据时
    if (!emailInfo.value.data.length) {
      if (!validateParamsWhenNoExcelData()) return false
    } else {
      // 有数据的情况
      const vdDataResult = validateExcelData(emailInfo.value.data)
      // 用户选择了收件箱，要判断发件箱是否在数据表格中出现
      if (emailInfo.value.recipients.length > 0) {
        const recipientEmailSet = vdDataResult.recipientEmailSet
        const recipientsCount = recipientEmailSet.size
        emailInfo.value.recipients.forEach(x => recipientEmailSet.add(x.email))
        const recipientsNowCount = recipientEmailSet.size
        if (recipientsNowCount > recipientsCount) {
          notifySuccess(translateSendingTask('notifyExtraRecipientSelected'))
          // 说明选择了额外的收件箱，还要验证非数据的情况
          if (!validateParamsWhenNoExcelData()) return false
        }
      }

      // 验证其它情况
      const { recipientStatus, senderStatus, bodyStatus } = vdDataResult
      if (recipientStatus !== 2) {
        notifyError(translateSendingTask('pleaseEnsureEachDataHasRecipient'))
        return false
      }

      if (emailInfo.value.senderAccounts.length === 0 && emailInfo.value.senderAccountGroups.length === 0 && senderStatus < 2) {
        // 没有发件
        notifyError(translateSendingTask('senderAccountMissingInData'))
        return false
      }

      if (!existGlobalBody.value && bodyStatus < 2) {
        // 没有发件
        notifyError(translateSendingTask('bodyMissingInData'))
        return false
      }
    }

    if (needUpload.value) {
      notifyError(translateSendingTask('pleaseUploadAttachments'))
      return false
    }
    return true
  }

  // 立即发送
  async function onSendNowClick () {
    if (!validateSendingTaskParams()) return
    // console.log('email info:', emailInfo.value)
    // 将数据传到后台发送
    notifySuccess(translateSendingTask('sendingStarted'))

    await showComponentDialog(SendingProgress, {
      title: translateSendingTask('sendingProgress'),
      sendingApi: async () => {
        return await sendEmailNow(emailInfo.value)
      }
    })
  }

  // 定时发送
  async function onScheduleSendClick () {
    if (!validateSendingTaskParams()) return
    logger.debug('email info:', emailInfo.value)

    // 选择日期
    const { ok, data: scheduleDate } = await showComponentDialog<string>(SelectScheduleDate)
    if (!ok) return

    // 将数据传到后台发送
    await sendSchedule(emailInfo.value, formatDateToUTC(scheduleDate))

    notifySuccess(translateSendingTask('scheduledSendingBooked'))
  }

  // 预览
  async function onPreviewClick () {
    if (!validateSendingTaskParams()) return

    // 预览功能在本机实现
    // 1. 正文优先级：用户数据/正文 > 用户数据/模板 > 界面/正文 > 界面/模板
    // 2. 变量参数只能从用户数据中提取
    // 3. 主题优先级: 用户数据/主题 > 界面/主题
    await showComponentDialog(PreviewSendingDialog, {
      emailCreateInfo: emailInfo.value
    })
  }

  return {
    OkBtn,
    CommonBtn,
    needUpload,
    onSendNowClick,
    onScheduleSendClick,
    onPreviewClick,
    validateSendingTaskParams
  }
}
