/* eslint-disable @typescript-eslint/no-explicit-any */
export interface IFunctionResult {
  ok: boolean,
  message?: string,
  data?: any
}


// 递归类型，用于获取所有嵌套键作为字符串路径
export type NestedKeys<T> = T extends object
  ? {
    [K in keyof T]: K extends string | number
    ? K | `${K}.${NestedKeys<T[K]>}`
    : never
  }[keyof T]
  : never
