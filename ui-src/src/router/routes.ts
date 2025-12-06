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
      label: 'dashboard', // '首页',
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
          label: 'dashboard',
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
      label: 'userInfo',
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
          label: 'profile'
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
      label: 'emailManagement',
      icon: 'alternate_email'
    },
    redirect: '/email-manager/out-box',
    children: [
      {
        name: 'outboxManager',
        path: 'out-box',
        meta: {
          icon: 'forward_to_inbox',
          label: 'outbox'
        },
        component: () => import('pages/emailManager/outbox/outboxManager.vue')
      },
      {
        name: 'inboxManager',
        path: 'in-box',
        meta: {
          icon: 'mark_email_unread',
          label: 'inbox'
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
      label: 'templateData',
      icon: 'article'
    },
    redirect: '/template-manage/index',
    children: [
      {
        name: 'EmailTemplates',
        path: 'index',
        meta: {
          icon: 'chrome_reader_mode',
          label: 'templateManagement',
          noCache: true
        },
        component: () => import('pages/sourceManager/templateManager/emailTemplates.vue')
      },
      {
        name: 'VariableManager',
        path: 'variable',
        meta: {
          icon: 'data_object',
          label: 'variableManagement',
          noCache: true,
          access: ['professional']
        },
        component: () => import('pages/sourceManager/variableManager/variableManagerIndex.vue')
      },
      {
        name: 'TemplateEditor',
        path: 'editor',
        meta: {
          icon: 'article',
          label: 'templateEditor',
          noMenu: true
        },
        component: () => import('pages/sourceManager/templateManager/templateEditor.vue')
      }
    ]
  },
  {
    name: 'SendManager',
    path: '/send-management',
    component: NormalLayout,
    meta: {
      label: 'sendingManagement',
      icon: 'send'
    },
    redirect: '/send-management/new-task',
    children: [
      {
        name: 'SendingTask',
        path: 'new-task',
        meta: {
          icon: 'add_box',
          label: 'newSending'
        },
        component: () => import('pages/sendingManager/sendingTask/sendingTask.vue')
      },
      {
        name: 'IpWarmUpManager',
        path: 'ip-warm-up-manager',
        meta: {
          icon: 'autorenew',
          label: 'ipWarmUp',
          noCache: true,
          access: ['professional'],
        },
        component: () => import('pages/sendingManager/ipWarmUp/IpWarmUpManager.vue')
      },
      {
        name: 'SendHistory',
        path: 'history',
        meta: {
          icon: 'schedule_send',
          label: 'sendHistory'
        },
        component: () => import('pages/sendingManager/sendHistory/sendHistory.vue')
      },
      {
        name: 'SendDetailTable',
        path: 'task-detail',
        meta: {
          noMenu: true,
          icon: 'schedule_send',
          label: 'sendDetail'
        },
        component: () => import('pages/sendingManager/sendHistory/SendDetailTable.vue')
      },
      {
        name: 'AttachmentManager',
        path: 'attachment-manager',
        meta: {
          icon: 'cloud_upload',
          label: 'attachmentManagement',
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
      label: 'statisticsReport',
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
          label: 'readStatistics'
        },
        component: () => import('pages/statisticsReport/EmailAnchor.vue')
      },
      {
        name: 'UnsubscribeReport',
        path: 'unsubscribe-report',
        meta: {
          icon: 'unsubscribe',
          label: 'unsubscribeStatistics'
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
      label: 'emailCrawler',
      icon: 'bug_report',
      access: ['professional']
    },
    redirect: '/email-crawler/tiktok/task',
    children: [
      {
        name: 'TiktokCrawler',
        path: 'tiktok',
        meta: {
          icon: 'devices',
          label: 'tiktokCrawler',
          access: ['enterprise']
        },
        children: [
          {
            name: 'TickTokDevice',
            path: 'device',
            meta: {
              icon: 'devices',
              label: 'tiktokDevice'
            },
            component: () => import('pages/emailCrawler/tiktok/tikTokDevice/TikTokDeviceManager.vue')
          },
          {
            name: 'CrawlerTask',
            path: 'task',
            meta: {
              icon: 'flag',
              label: 'crawlerTask'
            },
            component: () => import('pages/emailCrawler/tiktok/crawlerTask/CrawlerTask.vue')
          },
          {
            name: 'CrawlerResult',
            path: 'tasks/:id',
            meta: {
              icon: 'contact_mail',
              label: 'crawlerResult',
              noMenu: true
            },
            component: () => import('pages/emailCrawler/tiktok/crawlerResult/CrawlerResult.vue')
          }
        ]
      },
      {
        name: 'QQCrawler',
        path: 'qq',
        meta: {
          icon: 'groups',
          label: 'qqGroupMembersGetter',
          access: ['professional']
        },
        children: [
          {
            name: 'TickTokDevice2',
            path: 'device',
            meta: {
              icon: 'groups',
              label: 'qqGroupMembersGetter'
            },
            component: () => import('pages/emailCrawler/qqOnebot/qqGroupMemberGetter.vue')
          }
        ]
      },
    ]
  },
  {
    name: 'System',
    path: '/system',
    component: NormalLayout,
    meta: {
      label: 'systemSettings',
      icon: 'settings_suggest'
    },
    redirect: '/system/user-manager',
    children: [
      {
        name: 'BasicSetting',
        path: 'basic-setting',
        meta: {
          icon: 'tune',
          label: 'basicSettings',
          access: ['organizationAdmin']
        },
        component: () => import('pages/systemSetting/basicSetting/BasicSettings.vue')
      },
      {
        name: 'ProxyManager',
        path: 'proxy',
        meta: {
          icon: 'public',
          label: 'proxyManagement',
        },
        component: () => import('pages/systemSetting/proxyManager/ProxyManager.vue')
      },
      {
        name: 'UserManager',
        path: 'user-manager',
        meta: {
          icon: 'manage_accounts',
          label: 'userManagement',
          access: ['admin', 'professional']
        },
        component: () => import('pages/systemSetting/userManager/UserManager.vue')
      },
      {
        name: 'PermissionManager',
        path: 'permission',
        meta: {
          icon: 'badge',
          label: 'permissionManagement',
          access: ['admin', 'enterprise']
        },
        redirect: '/system/permission/code',
        children: [
          {
            name: 'FunctionManager',
            path: 'code',
            meta: {
              icon: 'fingerprint',
              label: 'functionManagement'
            },
            component: () => import('pages/systemSetting/permission/functionManager/PermissionCode.vue')
          },
          {
            name: 'Role',
            path: 'role',
            meta: {
              icon: 'emoji_people',
              label: 'roleManagement'
            },
            component: () => import('pages/systemSetting/permission/roleManager/RoleManager.vue')
          },
          {
            name: 'UserRoleManager',
            path: 'user-role',
            meta: {
              icon: 'supervised_user_circle',
              label: 'userRole'
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
          label: 'apiAccess',
          access: ['admin', 'enterprise']
        },
        component: () => import('pages/systemSetting/apiAccess/ApiAccess.vue')
      },
      {
        name: 'SoftwareLicense',
        path: 'license',
        meta: {
          icon: 'emoji_events',
          label: 'softwareLicense',
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
      label: 'sponsorAuthor',
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
          label: 'sponsorAuthor',
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
      label: 'helpDoc',
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
          label: 'startGuide',
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
      label: 'login',
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
      label: 'singlePages',
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
          label: 'unsubscribe',
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
      label: 'exception',
      icon: 'error',
      noMenu: true, // 在菜单中隐藏
      noTag: true // 在标签中隐藏
    },
    component: () => import('pages/ErrorNotFound.vue')
  }
]
