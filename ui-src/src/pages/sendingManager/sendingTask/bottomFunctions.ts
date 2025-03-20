/* eslint-disable @typescript-eslint/no-explicit-any */
// 底部按钮
import OkBtn from 'src/components/quasarWrapper/buttons/OkBtn.vue'
import CommonBtn from 'src/components/quasarWrapper/buttons/CommonBtn.vue'
import { notifyError, notifySuccess } from 'src/utils/dialog'
import type { IEmailCreateInfo } from 'src/api/emailSending';
import { sendEmailNow, sendSchedule } from 'src/api/emailSending'
import { useUserInfoStore } from 'src/stores/user'

import { showComponentDialog } from 'src/components/popupDialog/PopupDialog'
import PreviewEmailSendingBody from './components/PreviewEmailSendingBody.vue'
import SendingProgress from '../sendingProgress/SendingProgress.vue'
import SelectScheduleDate from './components/SelectScheduleDate.vue'

import logger from 'loglevel'

/**
 * 使用底部功能定义
 * @param emailInfo
 * @returns
 */
export function useBottomFunctions (emailInfo: Ref<IEmailCreateInfo>) {
  // 数据验证
  const needUpload = ref(false)
  const userInfoStore = useUserInfoStore()

  // 存在全局的 body
  const existGlobalBody = computed(() => {
    return emailInfo.value.templates.length || emailInfo.value.body
  })
  // 验证 excel 数据
  interface IValidateExcelDataResult {
    inboxStatus: number, // 0 全部不存在，1 部分存在，2 所有数据都存在
    outboxStatus: number,
    bodyStatus: number,
    inboxSet: Set<string>
  }
  // 验证 excel 数据，参数必须不能为空
  function validateExcelData (dataList: Record<string, any>[]): IValidateExcelDataResult {
    const inboxSet = new Set<string>()
    // 验证收件箱
    let inboxesCount = 0, outboxesCount = 0, bodiesCount = 0
    for (const data of dataList) {
      if (data.inbox) {
        inboxesCount++
        inboxSet.add(data.inbox)
      }

      if (data.outbox) outboxesCount++
      if (data.templateId || data.templateName || data.body) bodiesCount++
    }

    logger.debug(`[NewEmail] inbox count: ${inboxesCount}, outbox count: ${outboxesCount}, body count: ${bodiesCount}, data count: ${dataList.length}`)

    return {
      inboxStatus: formateExcelDataValidateResult(inboxesCount, dataList.length),
      outboxStatus: formateExcelDataValidateResult(outboxesCount, dataList.length),
      bodyStatus: formateExcelDataValidateResult(bodiesCount, dataList.length),
      inboxSet
    }
  }
  function formateExcelDataValidateResult (count: number, total: number) {
    if (count >= total) return 2
    if (count <= 0) return 0
    return 1
  }
  function validateParamsWhenNoExcelData () {
    if (!existGlobalBody.value) {
      notifyError('邮件模板和正文必须有一个不为空')
      return false
    }

    if (!emailInfo.value.outboxes.length && !emailInfo.value.outboxGroups.length) {
      notifyError('请选择发件人')
      return false
    }

    if (!emailInfo.value.inboxes.length && !emailInfo.value.inboxGroups.length) {
      notifyError('请选择收件人')
      return false
    }

    return true
  }
  // 数据验证
  function validateParams () {
    console.log('email info:', emailInfo.value)
    if (!emailInfo.value.subjects) {
      notifyError('请填写邮件主题')
      return false
    }

    // 没有 excel 数据时
    if (!emailInfo.value.data.length) {
      if (!validateParamsWhenNoExcelData()) return false
    } else {
      // 有数据的情况
      const vdDataResult = validateExcelData(emailInfo.value.data)
      // 用户选择了收件箱，要判断发件箱是否在数据表格中出现
      if (emailInfo.value.inboxes.length > 0) {
        const inboxSet = vdDataResult.inboxSet
        const inboxesCount = inboxSet.size
        emailInfo.value.inboxes.forEach(x => inboxSet.add(x.email))
        const inboxesNowCount = inboxSet.size
        if (inboxesNowCount > inboxesCount) {
          notifySuccess('除了数据外，您选择了额外的收件箱')
          // 说明选择了额外的收件箱，还要验证非数据的情况
          if (!validateParamsWhenNoExcelData()) return false
        }
      }

      // 验证其它情况
      const { inboxStatus, outboxStatus, bodyStatus } = vdDataResult
      if (inboxStatus !== 2) {
        notifyError('请保证每条数据都有 inbox (收件人邮箱)')
        return false
      }

      if (emailInfo.value.outboxes.length === 0 && outboxStatus < 2) {
        // 没有发件
        notifyError('数据中发件箱缺失，请在数据中指定发件箱或选择发件箱')
        return false
      }

      if (!existGlobalBody.value && bodyStatus < 2) {
        // 没有发件
        notifyError('数据中正文缺失，请在数据中指定邮件正文 或 选择模板 或 填写正文')
        return false
      }
    }

    if (needUpload.value) {
      notifyError('请单击附件上传按钮上传附件')
      return false
    }
    return true
  }

  // 立即发送
  async function onSendNowClick () {
    if (!validateParams()) return
    // console.log('email info:', emailInfo.value)
    // 将数据传到后台发送
    notifySuccess('开始发送...')

    await showComponentDialog(SendingProgress, {
      title: '发送进度',
      sendingApi: async () => {
        return await sendEmailNow(Object.assign({ smtpPasswordSecretKeys: userInfoStore.smtpPasswordSecretKeys }, emailInfo.value))
      }
    })
  }

  // 定时发送
  async function onScheduleSendClick () {
    if (!validateParams()) return
    console.log('email info:', emailInfo.value)

    // 选择日期
    const { ok, data } = await showComponentDialog<string>(SelectScheduleDate)
    if (!ok) return

    // 将数据传到后台发送
    await sendSchedule(Object.assign({ smtpPasswordSecretKeys: userInfoStore.smtpPasswordSecretKeys }, emailInfo.value, {
      scheduleDate: data
    }))

    notifySuccess('定时发送已预约')
  }

  // 预览
  async function onPreviewClick () {
    if (!validateParams()) return

    // 预览功能在本机实现
    // 1. 正文优先级：用户数据/正文 > 用户数据/模板 > 界面/正文 > 界面/模板
    // 2. 变量参数只能从用户数据中提取
    // 3. 主题优先级: 用户数据/主题 > 界面/主题
    await showComponentDialog(PreviewEmailSendingBody, {
      emailCreateInfo: emailInfo.value
    })
  }

  return {
    OkBtn,
    CommonBtn,
    needUpload,
    onSendNowClick,
    onScheduleSendClick,
    onPreviewClick
  }
}
