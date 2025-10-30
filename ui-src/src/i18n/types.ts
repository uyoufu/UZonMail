import type { NestedKeys } from 'src/types'
import type LocaleLang from './locales/zh-CN'

// 所有支持的语言 Key 类型
export type LangKey = NestedKeys<typeof LocaleLang>
// export type LangKey = keyof typeof LocaleLang

// 路由相关的语言 Key 类型
export type RoutesLangKey = keyof typeof LocaleLang.routes

// 登录页面相关的语言 Key 类型
export type LoginPageLangKey = keyof typeof LocaleLang.loginPage

export type DashboardPageLangKey = keyof typeof LocaleLang.dashboardPage

export type InboxManagerLangKey = keyof typeof LocaleLang.inboxManager

export type outboxManagerLangKey = keyof typeof LocaleLang.outboxManager

export type ButtonLangKey = keyof typeof LocaleLang.buttons

export type EmailGroupLangKey = keyof typeof LocaleLang.emailGroup

export type GlobalLangKey = keyof typeof LocaleLang.global

export type SendingTaskLangKey = keyof typeof LocaleLang.sendingTask
