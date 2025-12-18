import { httpClient } from './base/httpClient'
import type { ConnectionSecurity } from './emailBox'


export interface ISmtpInfo {
  domain: string,
  host: string,
  port: number,
  connectionSecurity: ConnectionSecurity
}

/**
 * 猜测 SMTP 信息
 * @param email
 * @returns
 */
export function GuessSmtpInfoGet (email: string) {
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
export function GuessSmtpInfoPost (emails: string[]) {
  return httpClient.post<ISmtpInfo[]>('/smtp-info/guess', {
    data: emails
  })
}
