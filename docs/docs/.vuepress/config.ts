import { defineUserConfig } from 'vuepress'

import theme from './theme.js'

export default defineUserConfig({
  base: '/',
  lang: 'zh-CN',
  locales: {
    '/': {
      lang: 'zh-CN',
      title: '宇正群邮',
      description:
        '宇正群邮是一款开源的邮件群发软件，提供邮件群发、邮件营销、邮箱爬取、任意变量等诸多功能。支持所有类型邮箱账号，包括Outlook的OAuth2。原生企业级品质，支持多端用户，支持Windows、Linux、MacOS等操作系统, 支持服务器部署。原生多线程并发，支持多账号同时使用，性能强劲。多年持续迭代更新优化，已被外贸营销、教育培训、财务会计等多个行业广泛使用。'
    },
    '/en/': {
      lang: 'en-US',
      title: 'UzonMail',
      description:
        'UzonMail is an open-source mass email software that provides various functions such as mass emailing, email marketing, mailbox scraping, and arbitrary variables. It supports all types of email accounts, including Outlook OAuth2. With enterprise-grade quality, it supports multi-end users and is compatible with Windows, Linux, MacOS, and other operating systems, as well as server deployment. Featuring native multi-threading concurrency, it allows simultaneous use of multiple accounts with strong performance. With years of continuous iteration and optimization, it has been widely used in various industries such as foreign trade marketing, education and training, and finance and accounting.'
    }
  },
  title: '宇正群邮',
  description:
    '宇正群邮是一款开源的邮件群发软件，提供邮件群发、邮件营销、邮箱爬取、任意变量等诸多功能。支持所有类型邮箱账号，包括Outlook的OAuth2。原生企业级品质，支持多端用户，支持Windows、Linux、MacOS等操作系统, 支持服务器部署。原生多线程并发，支持多账号同时使用，性能强劲。多年持续迭代更新优化，已被外贸营销、教育培训、财务会计等多个行业广泛使用。',
  theme,
  head: [
    [
      'title',
      {},
      '宇正群邮 - 邮件群发|邮件群发软件|邮件营销软件|开源邮件群发|最好用的邮件群发软件'
    ],
    // 百度验证
    [
      'meta',
      {
        name: 'baidu-site-verification',
        content: 'codeva-TLGtlr39fQ'
      }
    ]
  ]
  // plugins: []
  // 和 PWA 一起启用
  // shouldPrefetch: false,
})
