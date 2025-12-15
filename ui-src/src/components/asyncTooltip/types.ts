// eslint-disable-next-line @typescript-eslint/no-explicit-any
export type toolTipType = Array<any> | ((params?: object) => Promise<string[]> | string[] | string) | string[] | string
