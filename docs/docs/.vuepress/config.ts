import { defineUserConfig } from "vuepress";

import theme from "./theme.js";

export default defineUserConfig({
  base: "/",

  lang: 'zh-CN',
  title: '宇正群邮',
  description: '一个开源强大的邮件批量发送系统',

  theme,

  // 和 PWA 一起启用
  // shouldPrefetch: false,
});
