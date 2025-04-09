export enum SecurityProtocol {
  None = 1,
  SSL = 2,
  TLS = 4,
  StartTLS = 8
}

export interface ISmtpInfo {
  domain: string,
  host: string,
  port: number,
  securityProtocol: SecurityProtocol,
  enableSSL: boolean
}


import { httpClient } from './base/httpClient'

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
  return httpClient.post<ISmtpInfo>('/smtp-info/guess', {
    data: emails
  })
}
