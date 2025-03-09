import { httpClient } from 'src/api//base/httpClient'
import type { IRequestPagination } from 'src/compositions/types'

/**
 * 邮箱模板
 */
export interface IEmailTemplate {
  id?: number,
  name: string,
  content: string,
  description?: string,
  thumbnail?: string,
}

/**
 * 获取模板数量
 * @param filter
 * @returns
 */
export function getEmailTemplatesCount (filter?: string) {
  return httpClient.get<number>('/email-template/filtered-count', {
    params: {
      filter
    }
  })
}

/**
 * 获取模板数据
 * @param filter
 * @param pagination
 * @returns
 */
export function getEmailTemplatesData (filter: string | undefined, pagination: IRequestPagination) {
  return httpClient.post<IEmailTemplate[]>('/email-template/filtered-data', {
    params: {
      filter
    },
    data: pagination
  })
}

/**
 * 更新或者创建模板
 * @param eailTemplate
 * @returns
 */
export function upsertEmailTemplate (emailTemplate: IEmailTemplate) {
  return httpClient.post<IEmailTemplate>('/email-template', {
    data: emailTemplate
  })
}

/**
 * 删除邮箱模板
 * @param emailTemplateId
 * @returns
 */
export function deleteEmailTemplate (emailTemplateId: number) {
  return httpClient.delete<IEmailTemplate>(`/email-template/${emailTemplateId}`)
}

/**
 * 通过 id 获取邮箱模板
 * @param emailTemplateId
 * @returns
 */
export function getEmailTemplateById (emailTemplateId: number, cacheKey?: string) {
  return httpClient.get<IEmailTemplate>(`/email-template/${emailTemplateId}`, {
    cacheKey
  })
}

/**
 * 通过 id 或者 name 来获取邮箱模板
 * @param templateId
 * @param templateName
 * @param cacheKey
 * @returns
 */
export function getEmailTemplateByIdOrName (templateId?: number, templateName?: string, cacheKey?: string) {
  return httpClient.get<IEmailTemplate>('/email-template/by-id-or-name', {
    params: {
      templateId,
      templateName
    },
    cacheKey
  })
}
