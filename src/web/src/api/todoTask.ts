import { httpClient } from 'src/api/base/httpClient'
import type { IMailMessage, ISendMailRequest } from './mailConversation'

export enum TodoTaskKind { Normal, MailFollowUp }
export enum TodoTaskStatus { Pending, InProgress, Completed, Archived }
export enum TodoTaskPriority { Low, Normal, High, Urgent }
export interface ITodoMailBranch { id: number, sourceConversationId: number, branchSubject: string, sourceMessages: IMailMessage[], messages: IMailMessage[] }
export interface ITodoTask { id: number, kind: TodoTaskKind, title: string, description?: string, status: TodoTaskStatus, priority: TodoTaskPriority, dueAtUtc?: string, completedAtUtc?: string, updatedAtUtc: string, mailBranch?: ITodoMailBranch }
export interface IUpsertTodoTask { title: string, description?: string, status: TodoTaskStatus, priority: TodoTaskPriority, dueAtUtc?: string }

export function getTodoTasks () { return httpClient.get<ITodoTask[]>('/todo-tasks') }
export function createTodoTask (request: IUpsertTodoTask) { return httpClient.post<ITodoTask>('/todo-tasks', { data: request }) }
export function createMailTodoTask (request: IUpsertTodoTask & { sourceConversationId: number, sourceMessageId: number, branchSubject?: string }) { return httpClient.post<ITodoTask>('/todo-tasks/mail', { data: request }) }
export function updateTodoTask (taskId: number, request: IUpsertTodoTask) { return httpClient.put<ITodoTask>(`/todo-tasks/${taskId}`, { data: request }) }
export function deleteTodoTask (taskId: number) { return httpClient.delete(`/todo-tasks/${taskId}`) }
export function sendTodoMailMessage (taskId: number, request: ISendMailRequest) { return httpClient.post(`/todo-tasks/${taskId}/messages`, { data: request }) }
