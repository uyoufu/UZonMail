import { httpClientPro } from 'src/api/base/httpClient'
import type { IGroupInfo, IUserInfo } from 'src/api/oneBot/group'

/**
 * 生成一个永久的文件读取器
 * @returns
 */
export function saveQQMemberAsInbox (qqGroup: IGroupInfo, users: IUserInfo[]) {
  return httpClientPro.put<string>('/qq-members/as-inbox', {
    data: {
      group: {
        groupId: qqGroup.group_id,
        groupName: qqGroup.group_name
      },
      users: users.map(x => ({
        userId: x.user_id,
        nickname: x.nickname
      }
      )),
    }
  })
}
