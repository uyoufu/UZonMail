/**
 * 获取枚举的键值对数组
 * @param e
 * @returns [key, value][]
 */
export function enumEntries<T extends Record<string, string | number>> (e: T) {
  return Object.keys(e)
    .filter(k => isNaN(Number(k)))
    .map(k => [k, e[k]]) as Array<[keyof T, T[keyof T]]>
}

/**
 * 将枚举转换为选项数组
 * @param e
 * @returns
 */
export function enumToOptions<T extends Record<string, string | number>> (e: T): Array<{ label: keyof T; value: T[keyof T] }> {
  return enumEntries(e).map(([key, value]) => ({
    label: key,
    value: value
  }))
}
