import { httpClientPro } from 'src/api//base/httpClient'
import type { IRequestPagination } from 'src/compositions/types'
import type { IEmailCreateInfo } from '../emailSending'

export enum IpWarmUpUpStatus {
  /// <summary>
  /// 计划创建完成，等待开始
  /// </summary>
  Created = 0,
  /// <summary>
  /// 计划正在进行中
  /// </summary>
  InProgress = 1,
  /// <summary>
  /// 计划暂停中
  /// </summary>
  Paused = 2,
  /// <summary>
  /// 计划已经完成
  /// </summary>
  Completed = 3,
  /// <summary>
  /// 计划被取消
  /// </summary>
  Canceled = 4,
}

export interface IIpWarmUpUpPlan {
  userId: number
  name: string
  subjects: string[]
  templateIds: number[]
  outboxIds: number[]
  inboxIds: number[]
  ccIds: number[]
  bccIds: number[]
  attachmentIds: number[]
  data?: object[],
  body?: string
  startDate: Date
  endDate: Date
  tasksCount: number
  status: IpWarmUpUpStatus
}

/**
 * 立即发件
 * @param userId
 * @param type
 * @returns
 */
export function createIpWarmUpPlan (data: IEmailCreateInfo) {
  return httpClientPro.post<boolean>('/ip-warm-up-plan', {
    data
  })
}

/**
 * 获取发件邮箱数量
 * @param groupId
 * @param filter
 */
export function getIpWarmUpPlanCount (filter: string | undefined) {
  return httpClientPro.get<number>('/ip-warm-up-plan/filtered-count', {
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
export function getIpWarmUpPlanData (filter: string | undefined, pagination: IRequestPagination) {
  return httpClientPro.post<IIpWarmUpUpPlan[]>('/ip-warm-up-plan/filtered-data', {
    params: {
      filter
    },
    data: pagination
  })
}
