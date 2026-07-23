import { subscribeOne } from 'src/signalR/signalR'
import { UzonMailClientMethods } from 'src/signalR/types'
import { notifyAny } from 'src/utils/dialog'
import logger from 'loglevel'

export enum NotifyType {
  Info,
  Success,
  Warning,
  Error
}


export interface INotifyMessage {
  message?: string,
  type: NotifyType,
  title?: string
}

/**
 * 从服务器接收通知
 */
export function useNotifyRegister () {
  function receivedNotify (message: INotifyMessage) {
    logger.debug('[signalR] receive message from server:', message)

    notifyAny({
      message: message.message,
      type: NotifyType[message.type].toLowerCase(),
    })
  }

  subscribeOne(UzonMailClientMethods.notify, receivedNotify)
}
