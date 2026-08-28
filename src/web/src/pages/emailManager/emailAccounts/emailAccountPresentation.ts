import { ReceivingAccountStatus, ReceivingProtocol, SenderAccountStatus, SendingProtocol } from 'src/api/accountEnums'
import type { IEmailAccount } from 'src/api/emailAccounts'
import type { IStatusChipItem } from 'src/components/statusChip/types'
import { t } from 'src/i18n/helpers'

const AccountValidationStatus = {
  Valid: 'valid',
  Invalid: 'invalid',
  Unverified: 'unverified',
  Paused: 'paused'
} as const

type AccountValidationStatus = (typeof AccountValidationStatus)[keyof typeof AccountValidationStatus]

/** 返回账户已启用能力对应的协议标签。 */
export function accountProtocols(account: IEmailAccount): string[] {
  const protocols: string[] = []
  if (account.sender) protocols.push(senderProtocolLabel(account.sender.protocol))
  if (account.receiving) protocols.push(receivingProtocolLabel(account.receiving.protocol))
  return [...new Set(protocols)]
}

export function senderStatusLabel(status: SenderAccountStatus): AccountValidationStatus {
  return status === SenderAccountStatus.Valid
    ? AccountValidationStatus.Valid
    : status === SenderAccountStatus.Invalid
      ? AccountValidationStatus.Invalid
      : AccountValidationStatus.Unverified
}

export function receivingStatusLabel(status: ReceivingAccountStatus): AccountValidationStatus {
  if (status === ReceivingAccountStatus.Active) return AccountValidationStatus.Valid
  if (status === ReceivingAccountStatus.Paused) return AccountValidationStatus.Paused
  return status === ReceivingAccountStatus.Unverified
    ? AccountValidationStatus.Unverified
    : AccountValidationStatus.Invalid
}

export function senderValidationStatusStyles(
  protocol: SendingProtocol,
  status: SenderAccountStatus
): IStatusChipItem[] {
  return [
    createValidationStatusStyle(
      senderStatusLabel(status),
      senderProtocolLabel(protocol),
      'accountManagement.emailAccount.senderValidationStatus'
    )
  ]
}

export function receivingValidationStatusStyles(
  protocol: ReceivingProtocol,
  status: ReceivingAccountStatus
): IStatusChipItem[] {
  return [
    createValidationStatusStyle(
      receivingStatusLabel(status),
      receivingProtocolLabel(protocol),
      'accountManagement.emailAccount.receivingValidationStatus'
    )
  ]
}

export function accountValidationText(account: IEmailAccount): string {
  return [
    account.sender ? senderStatusLabel(account.sender.status) : '',
    account.receiving ? receivingStatusLabel(account.receiving.status) : ''
  ]
    .filter(Boolean)
    .join(', ')
}

function createValidationStatusStyle(
  status: AccountValidationStatus,
  protocol: string,
  translationKey:
    | 'accountManagement.emailAccount.senderValidationStatus'
    | 'accountManagement.emailAccount.receivingValidationStatus'
): IStatusChipItem {
  return {
    status,
    label: t(translationKey, { protocol, status: validationStatusText(status) })
  }
}

function validationStatusText(status: AccountValidationStatus): string {
  if (status === AccountValidationStatus.Valid) return t('components.statusChip.valid')
  if (status === AccountValidationStatus.Invalid) return t('components.statusChip.invalid')
  if (status === AccountValidationStatus.Paused) return t('components.statusChip.paused')
  return t('components.statusChip.unverified')
}

function senderProtocolLabel(protocol: SendingProtocol): string {
  return protocol === SendingProtocol.Smtp ? 'SMTP' : 'Microsoft Graph'
}

function receivingProtocolLabel(protocol: ReceivingProtocol): string {
  return protocol === ReceivingProtocol.Imap ? 'IMAP' : 'Microsoft Graph'
}
