export function useEllipsisTrimmer () {
  /**
   * 文字截取函数，支持三种方式：开头、中间、结尾
   * @param content
   * @param ellipsis
   * @param maxLength
   * @returns
   */
  function ellipsisTrimmer (content: string, ellipsis: 'start' | 'middle' | 'end', maxLength: number) {
    if (typeof content !== 'string') content = String(content)

    if (content.length <= maxLength) return content // 如果长度小于最大长度，直接返回

    // 按照方向截取
    if (ellipsis === 'start') {
      return '...' + content.slice(content.length - maxLength)
    } else if (ellipsis === 'middle') {
      const half = Math.floor(maxLength / 2)
      return content.slice(0, half) + '...' + content.slice(content.length - half)
    } else {
      return content.slice(0, maxLength) + '...'
    }
  }

  return {
    ellipsisTrimmer
  }
}
