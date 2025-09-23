import type { ExtendedRouteRecordRaw } from './types'
const NormalLayout = () => import('../layouts/normalLayout/normalLayout.vue')
const SinglePageLayout = () => import('../layouts/singlePageLayout/singlePageLayout.vue')

/**
 * 使用说明
 * 1- name 应与组件名一样，在 setup 中与文件名一样，才会有缓存
 * 2- noCache: true 不缓存
 */

// 根据权限显示的 routes
export const dynamicRoutes: ExtendedRouteRecordRaw[] = [
  {
    name: 'IndexHome',
    path: '/',
    component: NormalLayout,
    meta: {
      label: '首页',
      icon: 'home',
      // 不缓存
      noCache: false
    },
    // 填绝对路径，若是相对路径，则相对于当前路由
    redirect: '/index',
    children: [
      {
        name: 'IndexPage',
        path: 'index',
        meta: {
          label: '首页',
          icon: 'home'
        },
        component: () => import('pages/dashboard/DashboardIndex.vue')
      }
    ]
  },
  {
    name: 'User',
    path: '/user',
    component: NormalLayout,
    meta: {
      label: '用户信息',
      icon: 'person',
      noTag: true,
      noMenu: true
    },
    redirect: '/user/profile',
    children: [
      {
        name: 'profileIndex',
        path: 'profile',
        meta: {
          icon: 'menu',
          label: '个人资料'
        },
        component: () => import('pages/user/profileIndex.vue')
      }
    ]
  },
  {
    name: 'EmailManagement',
    path: '/email-management',
    component: NormalLayout,
    meta: {
      label: '邮箱管理',
      icon: 'alternate_email'
    },
    redirect: '/email-manager/out-box',
    children: [
      {
        name: 'outboxManager',
        path: 'out-box',
        meta: {
          icon: 'forward_to_inbox',
          label: '发件箱'
        },
        component: () => import('pages/emailManager/outbox/outboxManager.vue')
      },
      {
        name: 'inboxManager',
        path: 'in-box',
        meta: {
          icon: 'mark_email_unread',
          label: '收件箱'
        },
        component: () => import('pages/emailManager/inbox/inboxManager.vue')
      }
    ]
  },
  {
    name: 'TemplateManagement',
    path: '/template',
    component: NormalLayout,
    meta: {
      label: '模板数据',
      icon: 'article'
    },
    redirect: '/template-manage/index',
    children: [
      {
        name: 'emailTemplates',
        path: 'index',
        meta: {
          icon: 'chrome_reader_mode',
          label: '模板管理',
          noCache: true
        },
        component: () => import('pages/sourceManager/templateManager/emailTemplates.vue')
      },
      {
        name: 'variableManager',
        path: 'variable',
        meta: {
          icon: 'data_object',
          label: '变量管理',
          noCache: true,
          access: ['professional']
        },
        component: () => import('pages/sourceManager/variableManager/variableManagerIndex.vue')
      },
      {
        name: 'templateEditor',
        path: 'editor',
        meta: {
          icon: 'article',
          label: '模板编辑',
          noMenu: true
        },
        component: () => import('pages/sourceManager/templateManager/templateEditor.vue')
      }
    ]
  },
  {
    name: 'SendManage',
    path: '/send-management',
    component: NormalLayout,
    meta: {
      label: '发件管理',
      icon: 'send'
    },
    redirect: '/send-management/new-task',
    children: [
      {
        name: 'sendingTask',
        path: 'new-task',
        meta: {
          icon: 'add_box',
          label: '新建发件'
        },
        component: () => import('pages/sendingManager/sendingTask/sendingTask.vue')
      },
      {
        name: 'IpWarmUpManager',
        path: 'ip-warm-up-manager',
        meta: {
          icon: 'autorenew',
          label: 'IP 预热',
          access: ['professional']
        },
        component: () => import('pages/sendingManager/ipWarmUp/IpWarmUpManager.vue')
      },
      {
        name: 'SendHistory',
        path: 'history',
        meta: {
          icon: 'schedule_send',
          label: '历史发件'
        },
        component: () => import('pages/sendingManager/sendHistory/sendHistory.vue')
      },
      {
        name: 'SendDetailTable',
        path: 'task-detail',
        meta: {
          noMenu: true,
          icon: 'schedule_send',
          label: '发件明细'
        },
        component: () => import('pages/sendingManager/sendHistory/SendDetailTable.vue')
      },
      {
        name: 'AttachmentManager',
        path: 'attachment-manager',
        meta: {
          icon: 'cloud_upload',
          label: '附件管理'
        },
        component: () => import('pages/sendingManager/fileManager/AttachmentManager.vue')
      },
    ]
  },
  {
    name: 'SendingStatistics',
    path: '/sending-statistics',
    component: NormalLayout,
    meta: {
      label: '统计报表',
      icon: 'analytics',
      access: ['enterprise']
    },
    redirect: '/statistics/anchor-report',
    children: [
      {
        name: 'AnchorReport',
        path: 'anchor-report',
        meta: {
          icon: 'mark_email_read',
          label: '阅读统计'
        },
        component: () => import('pages/statisticsReport/EmailAnchor.vue')
      },
      {
        name: 'UnsubscribeReport',
        path: 'unsubscribe-report',
        meta: {
          icon: 'unsubscribe',
          label: '退订统计'
        },
        component: () => import('pages/statisticsReport/UnsubscribeReport.vue')
      }
    ]
  },
  {
    name: 'EmailCrawler',
    path: '/email-crawler',
    component: NormalLayout,
    meta: {
      label: '邮件爬虫',
      icon: 'bug_report',
      access: ['enterprise']
    },
    redirect: '/email-crawler/task',
    children: [
      {
        name: 'TickTokDevice',
        path: 'tiktok-device',
        meta: {
          icon: 'devices',
          label: 'TikTok设备'
        },
        component: () => import('pages/emailCrawler/tikTokDevice/TikTokDeviceManager.vue')
      },
      {
        name: 'CrawlerTask',
        path: 'task',
        meta: {
          icon: 'flag',
          label: '爬虫任务'
        },
        component: () => import('pages/emailCrawler/crawlerTask/CrawlerTask.vue')
      },
      {
        name: 'CrawlerResult',
        path: 'tasks/:id',
        meta: {
          icon: 'contact_mail',
          label: '爬取结果',
          noMenu: true
        },
        component: () => import('pages/emailCrawler/crawlerResult/CrawlerResult.vue')
      }
    ]
  },
  {
    name: 'System',
    path: '/system',
    component: NormalLayout,
    meta: {
      label: '系统设置',
      icon: 'settings_suggest'
    },
    redirect: '/system/user-manager',
    children: [
      {
        name: 'BasicSetting',
        path: 'basic-setting',
        meta: {
          icon: 'tune',
          label: '基础设置',
          access: ['organizationAdmin']
        },
        component: () => import('pages/systemSetting/basicSetting/BasicSettings.vue')
      },
      {
        name: 'ProxyManager',
        path: 'proxy',
        meta: {
          icon: 'public',
          label: '代理管理'
        },
        component: () => import('pages/systemSetting/proxyManager/ProxyManager.vue')
      },
      {
        name: 'UserManager',
        path: 'user-manager',
        meta: {
          icon: 'manage_accounts',
          label: '用户管理',
          access: ['admin', 'professional']
        },
        component: () => import('pages/systemSetting/userManager/UserManager.vue')
      },
      {
        name: 'PermissionManager',
        path: 'permission',
        meta: {
          icon: 'badge',
          label: '权限管理',
          access: ['admin', 'enterprise']
        },
        redirect: '/system/permission/code',
        children: [
          {
            name: 'FunctionManager',
            path: 'code',
            meta: {
              icon: 'fingerprint',
              label: '功能管理'
            },
            component: () => import('pages/systemSetting/permission/functionManager/PermissionCode.vue')
          },
          {
            name: 'Role',
            path: 'role',
            meta: {
              icon: 'emoji_people',
              label: '角色管理'
            },
            component: () => import('pages/systemSetting/permission/roleManager/RoleManager.vue')
          },
          {
            name: 'UserRoleManager',
            path: 'user-role',
            meta: {
              icon: 'supervised_user_circle',
              label: '用户角色'
            },
            component: () => import('pages/systemSetting/permission/userRoleManager/UserRole.vue')
          }
        ]
      },
      {
        name: 'ApiAccess',
        path: 'api-access',
        meta: {
          icon: 'key',
          label: 'API 授权',
          access: ['admin', 'enterprise']
        },
        component: () => import('pages/systemSetting/apiAccess/ApiAccess.vue')
      },
      {
        name: 'SoftwareLicense',
        path: 'license',
        meta: {
          icon: 'emoji_events',
          label: '软件许可',
          access: ['admin'],
          denies: ['noProPlugin'],
          noTag: true
        },
        component: () => import('pages/systemSetting/license/LicenseManager.vue')
      }
    ]
  },
  {
    name: 'Sponsor',
    path: '/sponsor',
    component: NormalLayout,
    meta: {
      label: '支持作者',
      icon: 'thumb_up',
      denies: ['professional', 'enterprise']
    },
    redirect: '/sponsor/author',
    children: [
      {
        name: 'SponsorAuthor',
        path: 'author',
        meta: {
          icon: 'thumb_up',
          label: '支持作者',
          noTag: true
        },
        component: () => import('pages/sponsor/sponsorAuthor.vue')
      }
    ]
  },
  {
    name: 'Help',
    path: '/help',
    component: NormalLayout,
    meta: {
      label: '帮助文档',
      icon: 'settings_suggest'
    },
    redirect: '/help/start-guide',
    children: [
      {
        name: 'StartGuide',
        path: 'start-guide',
        component: () => import('pages/help/StartGuide.vue'),
        meta: {
          icon: 'tips_and_updates',
          label: '使用说明',
          noTag: true
        }
      }
    ]
  }
]

// 静态 routes
export const constantRoutes: ExtendedRouteRecordRaw[] = [
  {
    name: 'Login',
    path: '/login',
    component: () => import('src/pages/login/loginIndex.vue'),
    meta: {
      label: '用户登录',
      icon: 'login',
      noMenu: true, // 在菜单中隐藏
      noTag: true // 在标签中隐藏
    }
  },
  {
    name: 'SinglePages',
    path: '/pages',
    component: SinglePageLayout,
    meta: {
      label: '单页面',
      icon: 'view_carousel',
      noMenu: true,
      noTag: true
    },
    children: [
      {
        name: 'UnsubscribePage',
        path: 'unsubscribe/pls-give-me-a-shot',
        component: () => import('src/pages/unsubscribe/UnsubscribePage.vue'),
        meta: {
          label: 'Unsubscribe',
          icon: 'unsubscribe',
          noMenu: true,
          noTag: true,
          // 允许匿名访问
          anoymous: true
        }
      }
    ]
  }
]

// 放到最后添加
export const exceptionRoutes: ExtendedRouteRecordRaw[] = [
  // Always leave this as last one,
  // but you can also remove it
  // 异常处理
  {
    name: 'exception',
    path: '/:catchAll(.*)*',
    meta: {
      label: '异常',
      icon: 'error',
      noMenu: true, // 在菜单中隐藏
      noTag: true // 在标签中隐藏
    },
    component: () => import('pages/ErrorNotFound.vue')
  }
]
