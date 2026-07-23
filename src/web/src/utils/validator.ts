/**
 * 验证是否是邮箱
 * @param emailStr
 * @returns
 */
export function isEmail (emailStr: string) {
  const reg = /^[a-zA-Z0-9_%+-]+(\.[a-zA-Z0-9_%+-]+)*@([a-zA-Z0-9-]+\.)+[a-zA-Z]{2,}$/
  return reg.test(emailStr)
}
