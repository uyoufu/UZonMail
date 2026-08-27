
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
 * 获取发件账户统计
 */
export function getSenderAccountCountStatistics () {
  return httpClient.get<IEmailCount[]>('/statistics/sender-accounts')
}

/**
 * 获取收件联系人统计
 * @returns
 */
export function getRecipientContactCountStatistics () {
  return httpClient.get<IEmailCount[]>('/statistics/recipient-contacts')
}

/**
 * 获取每月发送邮件统计
 * @returns
 */
export function getMonthlySendingCountInfo () {
  return httpClient.get<IMonthlySendingInfo[]>('/statistics/monthly-sending')
}
