
import { httpClientPro } from 'src/api//base/httpClient'
import type { IRequestPagination } from 'src/compositions/types'

/**
 * 爬虫任务接口
 */
export interface ITikTokDevice {
  id?: number,
  name: string, // 名称
  description?: string, // 描述
  deviceId: number, // 代理id
  odinId: string, // 任务截止时间
  isShared: boolean, // 是否共享
}

/**
 * 获取设备数量
 * @param filter
 * @returns
 */
export function getUserTikTokDevicesCount (filter: string | undefined) {
  return httpClientPro.get<number>('/tiktok-device/filtered-count', {
    params: {
      filter
    }
  })
}

/**
 * 获取设备数据
 * @param filter
 * @param pagination
 * @returns
 */
export function getUserTikTokDevicesData (filter: string | undefined, pagination: IRequestPagination) {
  return httpClientPro.post<ITikTokDevice[]>('/tiktok-device/filtered-data', {
    params: {
      filter
    },
    data: pagination
  })
}

/**
 * 获取用户可访问的所有设备
 * @returns
 */
export function getAllUserTikTokDevices () {
  return httpClientPro.get<ITikTokDevice[]>('/tiktok-device/all')
}

/**
 * 创建设备
 * @param data
 * @returns
 */
export function createTikTokDevice (data: ITikTokDevice) {
  return httpClientPro.post<ITikTokDevice>('/tiktok-device', {
    data
  })
}

/**
 * 更新设置
 * @param deviceId
 * @param data
 * @returns
 */
export function updateTikTokDevice (deviceId: number, data: ITikTokDevice) {
  return httpClientPro.put<boolean>(`/tiktok-device/${deviceId}`, {
    data
  })
}

/**
 * 删除
 * @param deviceId
 * @returns
 */
export function deleteTikTokDevice (deviceId: number) {
  return httpClientPro.delete<boolean>(`/tiktok-device/${deviceId}`)
}

/**
 * 更新共享状态
 * @param deviceId
 * @returns
 */
export function updateTikTokDeviceIsShared (deviceId: number, isShared: boolean) {
  return httpClientPro.put<boolean>(`/tiktok-device/${deviceId}/is-shared`, {
    params: {
      isShared
    }
  })
}
