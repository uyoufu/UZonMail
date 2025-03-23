import { OutboxStatus, type IOutbox } from "src/api/emailBox"
import { subscribeOne } from "src/signalR/signalR"
import { UzonMailClientMethods } from "src/signalR/types"

import logger from 'loglevel'
import type { updateExistOneType } from "src/compositions/qTableUtils"
import { notifyError } from "src/utils/dialog"

import { useI18n } from "vue-i18n"

/**
 * 注册 SignalR 事件
 */
export function useSignalR (updateExistOne: updateExistOneType) {
  const { t } = useI18n()

  // 注册事件
  subscribeOne(UzonMailClientMethods.outboxStatusChanged, onOutboxStatusChanged)
  function onOutboxStatusChanged (outbox: IOutbox) {
    logger.debug('[outboxSignalR] 发件箱状态变更', outbox)

    if (outbox.status !== OutboxStatus.Valid) {
      notifyError(t('outboxManager.validateFailed', {
        email: outbox.email,
        reason: outbox.validFailReason
      }))
    }

    updateExistOne(outbox)
  }
}
