export const SendingProtocol = {
  Smtp: 0,
  MicrosoftGraph: 1
} as const

export type SendingProtocol = typeof SendingProtocol[keyof typeof SendingProtocol]

export const ReceivingProtocol = {
  Imap: 0,
  MicrosoftGraph: 1
} as const

export type ReceivingProtocol = typeof ReceivingProtocol[keyof typeof ReceivingProtocol]

export const AuthenticationMethod = {
  Password: 0,
  OAuth2: 1
} as const

export type AuthenticationMethod = typeof AuthenticationMethod[keyof typeof AuthenticationMethod]

export const OAuthApplicationSource = {
  System: 0,
  Custom: 1
} as const

export type OAuthApplicationSource = typeof OAuthApplicationSource[keyof typeof OAuthApplicationSource]

export const SenderAccountStatus = {
  Unverified: 0,
  Valid: 1,
  Invalid: 2
} as const

export type SenderAccountStatus = typeof SenderAccountStatus[keyof typeof SenderAccountStatus]

export const RecipientValidationStatus = {
  Unverified: 0,
  Invalid: 1,
  Unknown: 2,
  Valid: 3
} as const

export type RecipientValidationStatus = typeof RecipientValidationStatus[keyof typeof RecipientValidationStatus]

export const ReceivingAccountStatus = {
  Unverified: 0,
  Active: 1,
  Paused: 2,
  AuthenticationFailed: 3,
  ConnectionFailed: 4
} as const

export type ReceivingAccountStatus = typeof ReceivingAccountStatus[keyof typeof ReceivingAccountStatus]

export const ConnectionSecurity = {
  None: 0,
  SSL: 1,
  TLS: 2,
  StartTLS: 3
} as const

export type ConnectionSecurity = typeof ConnectionSecurity[keyof typeof ConnectionSecurity]
