import dayjs from 'dayjs'
import logger from 'loglevel'

/**
 * 格式化字符串型日期
 * @param dateStr
 * @param format
 * @returns
 */
export function formatDate (dateStr: dayjs.ConfigType, format = 'YYYY-MM-DD HH:mm:ss') {
  if (!dateStr) return ''

  if (typeof dateStr === "string"
    && (dateStr.startsWith('0001') || dateStr.startsWith('9999'))) return ''

  if (typeof format !== 'string') format = 'YYYY-MM-DD HH:mm:ss'
  logger.debug('[format] formatDate:', dateStr, format)
  return dayjs.utc(dateStr).local().format(format)
}

export function formatDateToUTC (date: dayjs.ConfigType): string {
  return dayjs(date).utc().toISOString()
}
