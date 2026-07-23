/* eslint-disable @typescript-eslint/no-explicit-any */

import { httpClient } from 'src/api//base/httpClient'
import type { AppSettingType } from './appSetting'

export enum AIProviderType {
  None,
  OpenAI,
}

export enum AppSettingStatus {
  Disabled = 0,
  Enabled = 1,
  Ignored = 2,
}

/**
 * AI 提供商设置
 */
export interface AIProviderSetting {
  status: AppSettingStatus,
  providerType: AIProviderType,
  endpoint: string,
  apiKey: string,
  model: string,
  maxTokens: number,
  [key: string]: any
}

// #region 设置相关
/**
 * 获取 ai 设置
 * @param type 设置级别
 * @returns
 */
export function getAICopilotSetting (type: AppSettingType) {
  return httpClient.get<AIProviderSetting>('/ai-setting', {
    params: {
      type
    },
    passError: true
  })
}


/**
 * 更新 ai 设置
 * @param settings
 * @param type
 * @returns
 */
export function updateAICopilotSetting (settings: AIProviderSetting, type: AppSettingType) {
  return httpClient.put<boolean>('/ai-setting', {
    data: settings,
    params: {
      type
    },
    passError: true
  })
}

// #endregion


// #region email copilot
/**
 * generate email body
 * @param userPrompt
 * @returns
 */
export function generateEmailBody (userPrompt: string) {
  return httpClient.post<string>('/email-copilot/email/body/generation', {
    data: {
      userPrompt
    },
    passError: true
  })
}

/**
 * enhance email body
 * @param body
 * @returns
 */
export function enhanceEmailBody (body: string) {
  return httpClient.post<string>('/email-copilot/email/body/enhancement', {
    data: {
      body
    },
    passError: true
  })
}

/**
 * 对邮件内容进行主题总结
 * @param templateId 可选，使用的模板 ID
 * @param body 邮件内容，与 templateId 二选一
 * @returns 每个主题一行
 */
export function summarizeEmailSubjects (templateId: number, body: string) {
  return httpClient.post<string>('/email-copilot/email/subjects/generation', {
    data: {
      templateId,
      body
    }
  })
}
// #endregion
