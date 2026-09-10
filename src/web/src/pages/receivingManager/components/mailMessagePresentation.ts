import { MailMessageDirection, type IMailAddress, type IMailMessage } from 'src/api/mailConversation'

export function formatMailAddresses(addresses: IMailAddress[]): string {
  return addresses.map(address => address.displayName || address.email).join(', ')
}

export function formatMailAddressRoute(message: IMailMessage): string {
  const sender = formatMailAddresses(message.from)
  const recipient = formatMailAddresses(message.to)
  return sender && recipient ? `${sender} -> ${recipient}` : sender || recipient
}

export function formatMailEmailRoute(message: IMailMessage): string {
  const sender = message.from.map(address => address.email).join(', ')
  const recipient = message.to.map(address => address.email).join(', ')
  return sender && recipient ? `${sender} -> ${recipient}` : sender || recipient
}

export function getMessageAvatarLabel(message: IMailMessage): string {
  const addresses = message.direction === MailMessageDirection.Incoming ? message.from : message.to
  const displayValue = addresses[0]?.displayName || addresses[0]?.email || '?'
  return displayValue.slice(0, 1).toLocaleUpperCase()
}

export function getMessagePreview(message: IMailMessage, fallbackText: string): string {
  return message.previewText || message.subject || fallbackText
}
