import { httpClient } from './oneBotClient'


export interface IGroupInfo {
  group_id: number
  group_name: string,
  member_count: number,
  [key: string]: unknown
}

export interface IUserInfo {
  user_id: string
  nickname: string,
  is_robot: boolean
}

/**
 * 获取群列表
 * @returns
 */
export function getGroupList () {
  return httpClient.post<IGroupInfo[]>('/get_group_list')
}

/**
 * 获取群成员列表
 * @param group_id
 * @returns
 */
export function getGroupMemberList (group_id: number) {
  return httpClient.post<IUserInfo[]>('/get_group_member_list', {
    data: {
      group_id
    }
  })
}
