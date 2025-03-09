
import { httpClient } from 'src/api//base/httpClient'

export interface IEmailCount {
  domain: string,
  count: number
}

export interface IMonthlySendingInfo {
  year: number,
  month: number,
  count: number
}

/**
 * 获取发件箱邮件统计
 */
export function getOutboxEmailCountStatistics () {
  return httpClient.get<IEmailCount[]>('/statistics/outbox')
}

/**
 * 获取收件箱邮件统计
 * @returns
 */
export function getInboxEmailCountStatistics () {
  return httpClient.get<IEmailCount[]>('/statistics/inbox')
}

/**
 * 获取每月发送邮件统计
 * @returns
 */
export function getMonthlySendingCountInfo () {
  return httpClient.get<IMonthlySendingInfo[]>('/statistics/monthly-sending')
}
