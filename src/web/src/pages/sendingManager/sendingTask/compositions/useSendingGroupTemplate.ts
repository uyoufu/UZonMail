import { useRoute } from "vue-router"
import logger from 'loglevel'
import _ from 'lodash'

import { getSendingGroup } from 'src/api/sendingGroup'
import type { IEmailCreateInfo } from "src/api/emailSending"
import type { IFileObject, IObsUploadedResult } from "src/utils/file"

import { format } from 'quasar'

/**
 * 使用发送模板创建发送任务
 */
export function useSendingGroupTemplate (emailInfo: Ref<IEmailCreateInfo>) {
  const sendingGroupTemplateId = ref('')

  const route = useRoute()
  onMounted(async () => {
    sendingGroupTemplateId.value = route.query.sendingGroupTemplateId as string
    if (!sendingGroupTemplateId.value) {
      logger.debug('[useSendingGroupTemplate] sendingGroupTemplateId is empty')
      return
    }

    // 获取模板数据
    const { data } = await getSendingGroup(sendingGroupTemplateId.value)
    if (!data) return

    emailInfo.value = Object.assign(emailInfo.value,
      _.pick(data, 'subjects', 'templates', 'outboxes', 'data', 'outboxGroups', 'inboxGroups', 'inboxes', 'ccBoxes', 'body', 'sendBatch', 'proxyIds'))

    // 恢复 id
    emailInfo.value.attachments = data.attachments.map(x => {
      const fileObject = x.fileObject as IFileObject
      return {
        __fileName: x.fileName,
        __sha256: fileObject.sha256,
        __key: x.fileName,
        __size: format.humanStorageSize(fileObject.size),
        __progressLabel: '0.00%',
        __fileUsageId: x.id,
        name: x.fileName,
        size: fileObject.size,
      } as IObsUploadedResult
    })

    logger.debug('[useSendingGroupTemplate] emailInfo', emailInfo.value)
  })
}
