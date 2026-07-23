
/**
 * 分割字符串
 * @param str
 * @param userSeparators
 * @returns
 */
export function splitString (str: string, userSeparators: string[] = []): string[] {
  const defaultSeparators = [',', '，', ';', '；', '\\s+', '\\n']
  defaultSeparators.push(...userSeparators)

  const regex = new RegExp(defaultSeparators.join('|'), 'g')
  const results = str.split(regex).filter((item) => item)
  console.log('results', results)
  return results
}
