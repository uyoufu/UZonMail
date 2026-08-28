import type { ConnectionSecurity } from './accountEnums'
import { httpClient } from './base/httpClient'

export interface IImapInfo {
  domain: string
  host: string
  port: number
  connectionSecurity: ConnectionSecurity
}

/** 根据邮箱地址推断 IMAP 连接参数。 */
export function guessImapInfoGet(email: string) {
  return httpClient.get<IImapInfo>('/imap-info/guess', { params: { email } })
}

/** 批量推断邮箱域对应的 IMAP 连接参数。 */
export function guessImapInfoPost(emails: string[]) {
  return httpClient.post<IImapInfo[]>('/imap-info/guess', { data: emails })
}
