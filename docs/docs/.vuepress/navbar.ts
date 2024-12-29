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
      children: [
        {
          text: '开始使用',
          link: '/get-started'
        },
        {
          text: '视频介绍',
          link: '/video-introduction'
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
