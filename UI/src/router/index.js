import Vue from 'vue'
import Router from 'vue-router'

Vue.use(Router)

/* Layout */
import Layout from '@/layout'

/**
 * Note: sub-menu only appear when route children.length >= 1
 * Detail see: https://panjiachen.github.io/vue-element-admin-site/guide/essentials/router-and-nav.html
 *
 * hidden: true                   if set true, item will not show in the sidebar(default is false)
 * alwaysShow: true               if set true, will always show the root menu
 *                                if not set alwaysShow, when item has more than one children route,
 *                                it will becomes nested mode, otherwise not show the root menu
 * redirect: noRedirect           if set noRedirect will no redirect in the breadcrumb
 * name:'router-name'             the name is used by <keep-alive> (must set!!!)
 * meta : {
    roles: ['admin','editor']    control the page roles (you can set multiple roles)
    title: 'title'               the name show in sidebar and breadcrumb (recommend set)
    icon: 'svg-name'/'el-icon-x' the icon show in the sidebar
    breadcrumb: false            if set false, the item will hidden in breadcrumb(default is true)
    activeMenu: '/example/list'  if set path, the sidebar will highlight the path you set
  }
 */

/**
 * constantRoutes
 * a base page that does not have permission requirements
 * all roles can be accessed
 */

export const constantRoutes = [
  {
    path: '/login',
    component: () => import('@/views/login/index'),
    hidden: true
  },

  {
    path: '/404',
    component: () => import('@/views/404'),
    hidden: true
  },
  {
    path: '/lang',
    component: () => import('@/views/login/index')
  },
  {
    path: '/',
    component: Layout,
    redirect: '/dashboard',
    children: [
      {
        path: 'dashboard',
        name: 'Dashboard',
        component: () => import('@/views/dashboard/index'),
        meta: { title: 'home', icon: 'dashboard' }
      }
    ]
  },

  {
    path: '/profile',
    component: Layout,
    redirect: '/index',
    children: [
      {
        path: 'index',
        name: 'Profile',
        hidden: true,
        component: () => import('@/views/profile/index'),
        meta: { title: 'profile', icon: 'dashboard' }
      }
    ]
  },

  {
    path: '/setting',
    component: Layout,
    children: [
      {
        path: 'index',
        name: 'Setting',
        component: () => import('@/views/setting/index'),
        meta: { title: 'settings', icon: 'setting' }
      }
    ]
  },

  {
    path: '/email',
    component: Layout,
    meta: { title: 'emailManagement', icon: 'data' },
    redirect: '/email/send-box',
    children: [
      {
        path: 'send-box',
        name: 'SendBox',
        component: () => import('@/views/email/send'),
        meta: { title: 'outbox', icon: 'outbox', noCache: false }
      },
      {
        path: 'receive-box',
        name: 'ReceiveBox',
        component: () => import('@/views/email/receive'),
        meta: { title: 'inbox', icon: 'inbox', noCache: false }
      },
      {
        path: 'template',
        name: 'Template',
        component: () => import('@/views/email/template'),
        meta: { title: 'template', icon: 'template-f' }
      },
      {
        path: 'template-editor',
        name: 'TemplateEditor',
        hidden: true,
        component: () => import('@/views/email/templateEditor'),
        meta: { title: 'editTemplate', icon: 'form' }
      }
    ]
  },

  {
    path: '/send',
    component: Layout,
    meta: { title: 'sendManagement', icon: 'send' },
    redirect: '/send/index',
    children: [
      {
        path: 'index',
        name: 'SendIndex',
        component: () => import('@/views/send/index'),
        meta: { title: 'newSend', icon: 'add' }
      },
      {
        path: 'history',
        name: 'SendHistory',
        component: () => import('@/views/send/history'),
        meta: { title: 'sendHistory', icon: 'history' }
      }
    ]
  },

  {
    path: 'readme',
    component: Layout,
    children: [
      {
        path: 'https://galensgan.github.io/posts/2020/2QMK677.html',
        meta: { title: 'docs', icon: 'book' }
      }
    ]
  },

  // 404 page must be placed at the end !!!
  { path: '*', redirect: '/404', hidden: true }
]

const createRouter = () =>
  new Router({
    // mode: 'history', // require service support
    scrollBehavior: () => ({ y: 0 }),
    routes: constantRoutes
  })

const router = createRouter()

// Detail see: https://github.com/vuejs/vue-router/issues/1234#issuecomment-357941465
export function resetRouter() {
  const newRouter = createRouter()
  router.matcher = newRouter.matcher // reset router
}

export default router
