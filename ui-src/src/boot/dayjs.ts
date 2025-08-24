import dayjs from 'dayjs'
import utc from 'dayjs/plugin/utc'
import { defineBoot } from '#q-app/wrappers'
import logger from 'loglevel'

export default defineBoot(() => {
  logger.debug('[boot] current timezone:', Intl.DateTimeFormat().resolvedOptions().timeZone)
  dayjs.extend(utc)
  logger.debug('[boot] dayjs init with utc plugin, current utc date', dayjs().utc().toISOString())
})
