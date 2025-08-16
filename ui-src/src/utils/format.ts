import dayjs from 'dayjs'
import utc from 'dayjs/plugin/utc'
import logger from 'loglevel'

dayjs.extend(utc)

/**
 * 格式化字符串型日期
 * @param dateStr
 * @param format
 * @returns
 */
export function formatDate (dateStr: string | undefined | null, format = 'YYYY-MM-DD HH:mm:ss') {
  if (!dateStr) return ''

  if (dateStr.startsWith('0001')) return ''
  if (dateStr.startsWith('9999')) return ''

  if (typeof format !== 'string') format = 'YYYY-MM-DD HH:mm:ss'
  logger.debug('[format] formatDate:', dateStr, format)
  return dayjs.utc(dateStr).local().format(format)
}
