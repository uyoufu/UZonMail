import request from '@/utils/request'

export function getUserSettings() {
  return request({
    url: '/setting',
    method: 'get'
  })
}

export function updateSendInterval(min, max) {
  return request({
    url: '/setting/send-interval',
    method: 'put',
    data: {
      sendInterval: {
        min,
        max
      }
    }
  })
}

export function updateIsAutoResend(isAutoResend) {
  return request({
    url: '/setting/auto-resend',
    method: 'put',
    data: {
      isAutoResend
    }
  })
}

// 更新图文重发
export function updateSendWithImageAndHtml(sendWithImageAndHtml) {
  return request({
    url: '/setting/send-with-image-html',
    method: 'put',
    data: {
      sendWithImageAndHtml
    }
  })
}

// 更新每日最大发件量
export function updateMaxEmailsPerDay(maxCount) {
  return request({
    url: '/setting/max-emails-per-day',
    method: 'put',
    data: {
      maxCount
    }
  })
}
