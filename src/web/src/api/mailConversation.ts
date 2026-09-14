import { httpClient } from 'src/api/base/httpClient'

export enum MailConversationType { Direct, Group }
export enum MailMessageDirection { Incoming, Outgoing }
export enum MailReplyMode { Reply, ReplyAll }

export interface IMailTag { id: number, name: string, color: string }
export interface IMailContact { id: number, email: string, displayName?: string, tags: IMailTag[] }
export interface IReceivingAccount { emailAccountId: number, receivingAccountId: number, email: string, name?: string, protocol: number, status: number, lastSuccessfulSyncAtUtc?: string, lastError?: string, isSupported: boolean }
export interface IMailConversation { id: number, emailAccountId: number, emailAccount: string, conversationType: MailConversationType, displayTitle: string, lastMessageAtUtc: string, lastMessagePreview?: string, unreadCount: number, participants: IMailContact[] }
export interface IMailAddress { email: string, displayName?: string }
export interface IMailAttachment { id: number, fileName: string, contentType: string, size?: number, isOutgoingFileUsage: boolean }
export interface IMailMessage { id: number, direction: MailMessageDirection, subject?: string, previewText?: string, occurredAtUtc: string, isRead: boolean, replyToMessageId?: number, hasThreadReplies: boolean, sendingStatus?: number, from: IMailAddress[], to: IMailAddress[], cc: IMailAddress[], attachments: IMailAttachment[] }
export interface IMailContent { conversationMessageId: number, htmlBody?: string, textBody?: string, attachments: IMailAttachment[] }
export interface IMailThreadNeighbors { previousMessage?: IMailMessage, nextMessages: IMailMessage[] }
export interface IMailDeliveryHop { receivedHeader: string, ipAddresses: string[] }
export interface IMailMessageMetadata {
  conversationMessageId: number
  direction: MailMessageDirection
  subject?: string
  from: IMailAddress[]
  sender: IMailAddress[]
  replyTo: IMailAddress[]
  to: IMailAddress[]
  cc: IMailAddress[]
  sentAtUtc?: string
  receivedAtUtc?: string
  size?: number
  senderTimeZoneOffset?: string
  internetMessageId?: string
  inReplyToMessageIds: string[]
  referenceMessageIds: string[]
  deliveryHops: IMailDeliveryHop[]
}
export interface ISendMailRequest { replyToMessageId?: number, replyMode: MailReplyMode, subject: string, htmlBody: string, attachmentFileUsageIds: number[] }

export function getReceivingAccounts () { return httpClient.get<IReceivingAccount[]>('/receiving-management/accounts') }
export function synchronizeReceivingAccount (receivingAccountId: number) { return httpClient.post(`/receiving-management/accounts/${receivingAccountId}/sync`) }
export function getMailConversations (params: { emailAccountId?: number, tagId?: number, unreadOnly?: boolean, filter?: string, limit?: number }) { return httpClient.get<IMailConversation[]>('/mail-conversations', { params }) }
export function getMailMessages (conversationId: number) { return httpClient.get<IMailMessage[]>(`/mail-conversations/${conversationId}/messages`, { params: { limit: 100 } }) }
export function getMailContent (messageId: number) { return httpClient.get<IMailContent>(`/mail-conversations/messages/${messageId}/content`) }
export function getMailThreadNeighbors (messageId: number) { return httpClient.get<IMailThreadNeighbors>(`/mail-conversations/messages/${messageId}/thread-neighbors`) }
export function getMailMessageMetadata (messageId: number) { return httpClient.get<IMailMessageMetadata>(`/mail-conversations/messages/${messageId}/metadata`) }
export function markMailConversationRead (conversationId: number) { return httpClient.post(`/mail-conversations/${conversationId}/read`) }
export function sendMailConversationMessage (conversationId: number, request: ISendMailRequest) { return httpClient.post(`/mail-conversations/${conversationId}/messages`, { data: request }) }
export function getMailTags () { return httpClient.get<IMailTag[]>('/mail-tags') }
export function createMailTag (name: string, color: string) { return httpClient.post<IMailTag>('/mail-tags', { data: { name, color } }) }
export function setMailContactTags (contactId: number, tagIds: number[]) { return httpClient.put(`/mail-tags/contacts/${contactId}`, { data: { tagIds } }) }
