/* eslint-disable @typescript-eslint/no-explicit-any */
import { httpClientPro } from 'src/api//base/httpClient'
import type { IRequestPagination } from 'src/compositions/types'
import { AppSettingType } from '../appSetting'

export interface IEmailAnchor {
  userId: string,
  outboxEmail: string,
  inboxEmail: string,
  visitedCount: string,
  firstVisitDate: string,
  lastVisitDate: string,
}

/**
 * 获取发件邮箱数量
 * @param groupId
 * @param filter
 */
export function getEmailAnchorsCount (filter: string | undefined) {
  return httpClientPro.get<number>('/email-tracker/filtered-count', {
    params: {
      filter
    }
  })
}

/**
 * 获取发件邮箱数据
 * @param groupId
 * @param filter
 * @param pagination
 * @returns
 */
export function getEmailAnchorsData (filter: string | undefined, pagination: IRequestPagination) {
  return httpClientPro.post<IEmailAnchor[]>('/email-tracker/filtered-data', {
    params: {
      filter
    },
    data: pagination
  })
}

// #region  邮件跟踪设置
export interface IEmailTrackingSetting {
  enableEmailTracker: boolean
}

/**
 * 获取发送设置
 * @returns
 */
export function getEmailTrackingSetting (type: AppSettingType = AppSettingType.System) {
  return httpClientPro.get<IEmailTrackingSetting>('/email-tracker/setting', {
    params: {
      type
    }
  })
}

/**
 * 更新发送设置
 * @param trackingSetting
 * @returns
 */
export function updateEmailTrackingSetting (trackingSetting: IEmailTrackingSetting, type: AppSettingType = AppSettingType.System) {
  return httpClientPro.put<Record<string, any>>('/email-tracker/setting', {
    params: {
      type
    },
    data: trackingSetting
  })
}
// #endregion
