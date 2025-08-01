import { hopeTheme } from 'vuepress-theme-hope'

import navbar from './navbar.js'
import sidebar from './sidebar.js'

export default hopeTheme({
  hostname: 'https://mail.uzoncloud.com/',

  author: {
    name: 'uyoufu',
    url: 'https://uyoufu.uzoncloud.com/'
  },

  iconAssets: 'fontawesome-with-brands',

  logo: '/images/logo.svg',

  repo: 'uyoufu/UZonMail',

  docsDir: 'src',

  // 导航栏布局
  // https://theme-hope.vuejs.press/zh/guide/layout/navbar.html#%E5%B8%83%E5%B1%80%E9%85%8D%E7%BD%AE
  navbarLayout: {
    start: ['Brand'],
    center: ['Links'],
    end: ['Language', 'Repo', 'Outlook', 'Search']
  },

  // navbarAutoHide: 'always',
  hideSiteNameOnMobile: false,

  // 导航栏
  navbar,

  // 侧边栏
  sidebar,

  // 页脚
  footer: 'Apache-2.0 license | Copyright © 2021-persent UZonMail',
  displayFooter: true,

  // 加密配置
  encrypt: {
    config: {
      '/demo/encrypt.html': ['1234']
    }
  },

  // 多语言配置
  // metaLocales: {
  //   editLink: "在 GitHub 上编辑此页",
  // },

  // 如果想要实时查看任何改变，启用它。注: 这对更新性能有很大负面影响
  // hotReload: true,

  // 此处开启了很多功能用于演示，你应仅保留用到的功能。
  markdown: {
    align: true,
    attrs: true,
    breaks: true,
    codeTabs: true,
    component: true,
    demo: true,
    figure: true,
    gfm: true,
    imgLazyload: true,
    imgSize: true,
    include: true,
    linkify: true,
    mark: true,
    plantuml: true,
    spoiler: true,
    stylize: [
      {
        matcher: 'Recommended',
        replacer: ({ tag }) => {
          if (tag === 'em')
            return {
              tag: 'Badge',
              attrs: { type: 'tip' },
              content: 'Recommended'
            }
        }
      }
    ],
    sub: true,
    sup: true,
    tabs: true,
    tasklist: true,
    vPre: true

    // 取消注释它们如果你需要 TeX 支持
    // markdownMath: {
    //   // 启用前安装 katex
    //   type: "katex",
    //   // 或者安装 mathjax-full
    //   type: "mathjax",
    // },

    // 如果你需要幻灯片，安装 @vuepress/plugin-revealjs 并取消下方注释
    // revealjs: {
    //   plugins: ["highlight", "math", "search", "notes", "zoom"],
    // },

    // 在启用之前安装 chart.js
    // chartjs: true,

    // insert component easily

    // 在启用之前安装 echarts
    // echarts: true,

    // 在启用之前安装 flowchart.ts
    // flowchart: true,

    // 在启用之前安装 mermaid
    // mermaid: true,

    // playground: {
    //   presets: ["ts", "vue"],
    // },

    // 在启用之前安装 @vue/repl
    // vuePlayground: true,

    // 在启用之前安装 sandpack-vue3
    // sandpack: true,
  },

  // 在这里配置主题提供的插件
  plugins: {
    seo: true,
    sitemap: true,

    // 注意: 仅用于测试! 你必须自行生成并在生产环境中使用自己的评论服务
    comment: {
      provider: 'Waline',
      serverURL: 'https://uzonmail-comment.uzoncloud.com'
    },

    components: {
      components: ['Badge', 'VPCard', 'BiliBili', 'PDF']
    }

    // 如果你需要 PWA。安装 @vuepress/plugin-pwa 并取消下方注释
    // pwa: {
    //   favicon: "/favicon.ico",
    //   cacheHTML: true,
    //   cacheImage: true,
    //   appendBase: true,
    //   apple: {
    //     icon: "/assets/icon/apple-icon-152.png",
    //     statusBarColor: "black",
    //   },
    //   msTile: {
    //     image: "/assets/icon/ms-icon-144.png",
    //     color: "#ffffff",
    //   },
    //   manifest: {
    //     icons: [
    //       {
    //         src: "/assets/icon/chrome-mask-512.png",
    //         sizes: "512x512",
    //         purpose: "maskable",
    //         type: "image/png",
    //       },
    //       {
    //         src: "/assets/icon/chrome-mask-192.png",
    //         sizes: "192x192",
    //         purpose: "maskable",
    //         type: "image/png",
    //       },
    //       {
    //         src: "/assets/icon/chrome-512.png",
    //         sizes: "512x512",
    //         type: "image/png",
    //       },
    //       {
    //         src: "/assets/icon/chrome-192.png",
    //         sizes: "192x192",
    //         type: "image/png",
    //       },
    //     ],
    //     shortcuts: [
    //       {
    //         name: "Demo",
    //         short_name: "Demo",
    //         url: "/demo/",
    //         icons: [
    //           {
    //             src: "/assets/icon/guide-maskable.png",
    //             sizes: "192x192",
    //             purpose: "maskable",
    //             type: "image/png",
    //           },
    //         ],
    //       },
    //     ],
    //   },
    // },
  }
})
