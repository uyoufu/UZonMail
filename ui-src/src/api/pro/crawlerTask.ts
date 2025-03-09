
import { httpClientPro } from 'src/api//base/httpClient'
import type { IRequestPagination } from 'src/compositions/types'

export enum CrawlerType {
  TikTokEmail
}

/**
 * 爬虫状态
 */
export enum CrawlerStatus {
  /// <summary>
  /// 已创建
  /// </summary>
  created,

  /// <summary>
  /// 进行中
  /// </summary>
  running,

  /// <summary>
  /// 已停止
  /// </summary>
  stopped,
}

/**
 * 爬虫任务接口
 */
export interface ICrawlerTaskInfo {
  id?: number, // 爬虫任务id
  userId?: number, // 用户id
  name: string, // 任务名称
  type: CrawlerType, // 爬虫类型
  status: CrawlerStatus, // 任务状态
  description?: string, // 任务描述
  proxyId: number, // 代理id
  deadline: string, // 任务截止时间
  startDate: string,
  endDate: string,
  count: number,
}

/**
 * 获取爬虫任务数量
 * @param groupId
 * @param filter
 */
export function getCrawlerTaskInfosCount (filter: string | undefined) {
  return httpClientPro.get<number>('/crawler-task-info/filtered-count', {
    params: {
      filter
    }
  })
}

/**
 * 获取爬虫任务数据
 * @param groupId
 * @param filter
 * @param pagination
 * @returns
 */
export function getCrawlerTaskInfosData (filter: string | undefined, pagination: IRequestPagination) {
  return httpClientPro.post<ICrawlerTaskInfo[]>('/crawler-task-info/filtered-data', {
    params: {
      filter
    },
    data: pagination
  })
}

/**
 * 获取爬虫任务的 count 信息
 * @param crawlerTaskIds
 * @returns
 */
export function getCrawlerTaskCountInfos (crawlerTaskIds: number[]) {
  return httpClientPro.post<ICrawlerTaskInfo[]>('/crawler-task-info/count-infos', {
    data: crawlerTaskIds
  })
}

/**
 * 创建爬虫任务
 * @param data
 * @returns
 */
export function createCrawlerTaskInfo (data: ICrawlerTaskInfo) {
  return httpClientPro.post<ICrawlerTaskInfo>('/crawler-task-info', {
    data
  })
}

/**
 * 更新爬虫任务设置
 * @param crawlerTaskId
 * @param data
 * @returns
 */
export function updateCrawlerTaskInfo (crawlerTaskId: number, data: ICrawlerTaskInfo) {
  return httpClientPro.put<boolean>(`/crawler-task-info/${crawlerTaskId}`, {
    data
  })
}

/**
 * 删除爬虫任务
 * @param crawlerTaskId
 * @returns
 */
export function deleteCrawlerTaskInfo (crawlerTaskId: number) {
  return httpClientPro.delete<boolean>(`/crawler-task-info/${crawlerTaskId}`)
}

/**
 * 启动爬虫任务
 * @param crawlerTaskId
 * @returns
 */
export function startCrawlerTask (crawlerTaskId: number) {
  return httpClientPro.put<boolean>(`/crawler-task-info/${crawlerTaskId}/start`)
}

/**
 * 停止爬虫任务
 * @param crawlerTaskId
 * @returns
 */
export function stopCrawlerTask (crawlerTaskId: number) {
  return httpClientPro.put<boolean>(`/crawler-task-info/${crawlerTaskId}/stop`)
}

/**
 * 获取爬虫任务数量
 * @param groupId
 * @param filter
 */
export function getCrawlerTaskResultsCount (crawlerTaskId: number, filter: string | undefined) {
  return httpClientPro.get<number>(`/crawler-task-info/${crawlerTaskId}/results/filtered-count`, {
    params: {
      filter,
      crawlerTaskId
    }
  })
}

/**
 * 获取爬虫任务数据
 * @param groupId
 * @param filter
 * @param pagination
 * @returns
 */
export function getCrawlerTaskResultsData (crawlerTaskId: number, filter: string | undefined, pagination: IRequestPagination) {
  return httpClientPro.post<ICrawlerTaskInfo[]>(`/crawler-task-info/${crawlerTaskId}/results/filtered-data`, {
    params: {
      filter,
      crawlerTaskId
    },
    data: pagination
  })
}

/**
 * 保存爬虫结果到收件箱
 * @param crawlerTaskId
 * @returns 保存成功的收件箱id
 */
export function saveCrawlerResultsAsInbox (crawlerTaskId: number) {
  return httpClientPro.post<number[]>(`/crawler-task-info/${crawlerTaskId}/inbox-group`)
}
