import { navbar } from "vuepress-theme-hope";

export default navbar(
  [
    {
      text: '首页',
      link: '/',
      icon:'home'
    },
    {
      text: '使用文档',
      link: '/get-started',
      icon:'book',
      prefix: '/guide/',
      children: [
        {
          text: '开始使用',
          link: 'README.md'
        },
        {
          text: '视频介绍',
          link: '/video-introduction'
        },
        {
          text: "Demo演示",
          link:"https://uzon-mail.223434.xyz:2234/"
        }       
      ]
    },   
    {
      text: '软件下载',
      link: '/versions',
      icon:'download'
    },
    {
      text: '联系我们',
      link: 'contact-us.md',
      icon:'message'
    },
    {
      text: '赞助支持',
      link: '/sponsor',
      icon:'thumbs-up'
    },
    {
      text: '致谢名单',
      link: '/thanks-list',
      icon:'heart'
    },
    {
      text: '版本对比',
      link: '/price',
      icon:'code-compare'
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
);
