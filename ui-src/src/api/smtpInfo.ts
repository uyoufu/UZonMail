import { httpClient } from './base/httpClient'
import type { ConnectionSecurity } from './emailBox'


export interface ISmtpInfo {
  domain: string,
  host: string,
  port: number,
  connectionSecurity: ConnectionSecurity
}

/**
 * 更新 SMTP 信息库，方便下次猜测
 * @param smtpInfo
 * @returns
 */
export function updateSmtpInfo (smtpInfo: ISmtpInfo) {
  return httpClient.put('/smtp-info', {
    data: smtpInfo
  })
}

/**
 * 猜测 SMTP 信息
 * @param email
 * @returns
 */
export function guessSmtpInfoGet (email: string) {
  return httpClient.get<ISmtpInfo>('/smtp-info/guess', {
    params: {
      email
    }
  })
}

/**
 * 猜测多个 SMTP 信息
 * @param emails
 * @returns
 */
export function guessSmtpInfoPost (emails: string[]) {
  return httpClient.post<ISmtpInfo[]>('/smtp-info/guess', {
    data: emails
  })
}
