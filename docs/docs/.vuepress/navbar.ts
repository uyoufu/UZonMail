import { navbar } from 'vuepress-theme-hope'

export const navbarZh = navbar(
  [
    {
      text: '首页',
      link: '/',
      icon: 'home'
    },
    {
      text: '使用文档',
      link: '/get-started',
      icon: 'book',
      prefix: '/guide/',
      children: [
        {
          text: '开始使用',
          link: '/guide/introduction'
        },
        // {
        //   text: '视频介绍',
        //   link: '/video-introduction'
        // },
        {
          text: 'Demo 演示',
          link: 'https://mailtest.uzoncloud.com/'
        },
        {
          text: '自建邮局',
          link: '/self-hosted/postfix'
        }
      ]
    },
    {
      text: '软件下载',
      link: '/downloads',
      icon: 'download'
    },
    {
      text: '联系我们',
      link: '/contact-us',
      icon: 'message'
    },
    {
      text: '赞助支持',
      link: '/sponsor',
      icon: 'thumbs-up'
    },
    {
      text: '致谢名单',
      link: '/thanks-list',
      icon: 'heart'
    },
    {
      text: '版本对比',
      link: '/versions',
      icon: 'code-compare'
    }
  ]
  //   [
  //   "/",
  //   "/portfolio",
  //   "/demo/",
  //   {
  //     text: "指南",
  //     icon: "lightbulb",
  //     prefix: "/guide/",
  //     children: [
  //       {
  //         text: "Bar",
  //         icon: "lightbulb",
  //         prefix: "bar/",
  //         children: ["baz", { text: "...", icon: "ellipsis", link: "" }],
  //       },
  //       {
  //         text: "Foo",
  //         icon: "lightbulb",
  //         prefix: "foo/",
  //         children: ["ray", { text: "...", icon: "ellipsis", link: "" }],
  //       },
  //     ],
  //   },
  //   {
  //     text: "V2 文档",
  //     icon: "book",
  //     link: "https://theme-hope.vuejs.press/zh/",
  //   },
  // ]
)

export const navbarEn = navbar([
  {
    text: 'Home',
    link: '/en/',
    icon: 'home'
  },
  {
    text: 'Guide',
    link: '/en/get-started',
    icon: 'book',
    prefix: '/en/guide/',
    children: [
      {
        text: 'Get Started',
        link: '/en/guide/introduction'
      },
      {
        text: 'Demo',
        link: 'https://mailtest.uzoncloud.com/'
      },
      {
        text: 'Self-hosted',
        link: '/en/self-hosted/postfix'
      }
    ]
  },
  {
    text: 'Downloads',
    link: '/en/downloads',
    icon: 'download'
  },
  {
    text: 'Contact',
    link: '/en/contact-us',
    icon: 'message'
  },
  {
    text: 'Sponsor',
    link: '/en/sponsor',
    icon: 'thumbs-up'
  },
  {
    text: 'Thanks',
    link: '/en/thanks-list',
    icon: 'heart'
  },
  {
    text: 'Versions',
    link: '/en/versions',
    icon: 'code-compare'
  }
])
