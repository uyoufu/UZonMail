export default {
  // #region 通用
  failed: '操作失败',
  success: '操作成功',
  confirm: '确认',
  cancel: '取消',
  edit: '编辑',
  delete: '删除',
  confirmOperation: '操作确认',
  deleteConfirm: '删除确认',
  cancelOperation: '取消操作',
  languageRequired: '语言是必填项',
  htmlContentRequired: 'Html 内容是必填项',
  unsubscribePageHasAreadyExist: '该语言的取消订阅页面已存在',
  deleteSuccess: '删除成功',
  updateSuccess: '更新成功',
  // #endregion

  // #region 全局通用
  global: {
    order: '序号',

    failed: '失败',
    success: '操作成功',
    confirm: '确认',
    cancel: '取消',
    edit: '编辑',
    delete: '删除',
    confirmOperation: '操作确认',
    deleteConfirm: '删除确认',
    cancelOperation: '取消操作',
    languageRequired: '语言是必填项',
    htmlContentRequired: 'Html 内容是必填项',
    unsubscribePageHasAreadyExist: '该语言的取消订阅页面已存在',
    deleteSuccess: '删除成功',
    updateSuccess: '更新成功',
  },

  // #endregion

  // #region 路由
  routes: {
    dashboard: '首页',
    userInfo: '用户信息',
    profile: '个人资料',
    emailManagement: '邮箱管理',
    outbox: '发件箱',
    inbox: '收件箱',
    templateData: '模板数据',
    templateManagement: '模板管理',
    variableManagement: '变量管理',
    templateEditor: '模板编辑',
    sendingManagement: '发件管理',
    newSending: '新建发件',
    ipWarmUp: 'IP 预热',
    sendHistory: '历史发件',
    sendDetail: '发件明细',
    attachmentManagement: '附件管理',
    statisticsReport: '统计报表',
    readStatistics: '阅读统计',
    unsubscribeStatistics: '退订统计',
    emailCrawler: '邮件爬虫',
    tiktokDevice: 'TikTok设备',
    crawlerTask: '爬虫任务',
    crawlerResult: '爬取结果',
    systemSettings: '系统设置',
    basicSettings: '基础设置',
    proxyManagement: '代理管理',
    userManagement: '用户管理',
    permissionManagement: '权限管理',
    functionManagement: '功能管理',
    roleManagement: '角色管理',
    userRole: '用户角色',
    apiAccess: 'API 授权',
    softwareLicense: '软件许可',
    sponsorAuthor: '支持作者',
    helpDoc: '帮助文档',
    startGuide: '使用说明',
    login: '用户登录',
    singlePages: '单页面',
    unsubscribe: 'Unsubscribe',
    exception: '异常',
  },
  // #endregion

  // #region 按钮
  buttons: {
    new: '新建',
    newItem: '新建项',

    delete: '删除',
    deletetItem: '删除项',

    save: '保存',

    cancel: '取消',
    cancelCurrentOperation: '取消当前操作',

    export: '导出',
    exportData: '导出数据',

    import: '导入',
    importData: '导入数据',

    confirm: '确认',
    confirmCurrentOperation: '确认当前操作',
  },
  // #endregion

  // #region 登录页
  loginPage: {
    userName: '用户名',
    password: '密码',
    signIn: '登录',
    version: '版本',
    client: '客户端',
    server: '服务器',
    pleaseInputUserName: '请输入用户名',
    pleaseInputPassword: '请输入密码',
  },
  // #endregion

  // #region 首页
  dashboardPage: {
    outboxCount: '发件箱数量',
    emailCount: '邮箱数量',
    count: '数量',
    outboxStatsTitle: '发件箱统计',
    inboxStatsTitle: '收件箱统计',
    monthlySendingStatsTitle: '每月发件统计',
  },
  // #endregion

  // #region 邮箱管理
  // 邮箱组
  emailGroup: {
    inboxGroup: '收件箱组',
    outboxGroup: '发件箱组',

    newGroup: '新建组',

    field_name: '组名',
    field_description: '描述',
    field_order: "{'global.order'}",
  },


  // 收件箱管理
  inboxManager: {
    importFromTxt: 'Txt 导入',
    importFromTxtTooltip: "支持格式: 邮箱, 用户名, 最小发件间隔 \n 三者可以任意顺序排列 \n 例如: test{'@'}gmail.com, uzonmail, 1 \n 其中最小发件间隔单位为小时",

    inbox: '收件箱',

    newInbox: '新建收件箱',
    addGroupFirst: '请先添加分组',

    exportInboxTemplate: '导出收件箱模板',
    importInbox: '导入收件箱',

    ctx_import: '导入',
    ctx_importInboxToCurrentGroup: '向当前组中导入收件箱',
    ctx_export: '导出',
    ctx_exportInboxToCurrentGroup: '导出当前组中的收件箱',

    col_email: '邮箱',
    col_name: '收件人姓名',
    col_description: '描述',
    col_minInboxCooldownHours: '最小收件间隔(小时)',
    col_lastSuccessDeliveryDate: '最后成功发件时间',

    noInboxesInThisGroup: '当前组中没有可导出的收件箱',
  },
  // #endregion

  // 表格列
  columns: {
    index: '序号',
    name: '名称',
    description: '描述',
  },

  outboxManager: {
    importFromTxt: 'Txt 导入',
    importFromTxtTooltip: "支持格式:\n1. 邮箱, 密码: test{'@'}gmail.com, 1234\n2. 邮箱, 密码, smtp地址, smtp端口: test{'@'}gmail.com, 1234, smtp.gmail.com, 465",

    doDeleteAllInvalidOutboxes: '是否删除当前组中所有验证失败的发件箱？',
    editCurrentOutbox: '编辑当前发件箱',
    deleteCurrentOrSelection: '删除当前或选中的发件箱',
    validate: '验证',
    sendTestToMe: '验证当前或选中发件箱',
    validateBatch: '批量验证',
    validateAllUnverifiedInGroup: '批量验证当前组中所有未验证的邮箱',
    isConfirmValidateBatch: '是否验证当前组中所有未验证的邮箱？',
    validating: '验证中',
    validateBatchSuccess: '批量验证结束',
    validateFailed: '{email} 验证失败, 原因: {reason}',

    deleteInvalid: '删除无效',
    deleteCurrentGroupInvalidOutboxes: '删除当前组中验证失败的发件箱',
    deleteOutbox: '删除发件箱',
    isDeleteCurrentOutbox: '是否删除发件箱: {email} ?',
    isDeleteSelectedOutboxes: '是否删除选中的 {count} 个发件箱？',
    deleteSuccess: '删除成功, 共删除 {count} 项',

    outlookDelegateAuthorization: 'Outlook授权',

    col_outbox: '发件箱',
    col_outboxUserName: '名称(发件人姓名)',
    col_smtpAddress: 'SMTP地址',
    col_smtpPort: 'SMTP端口',
    col_smtpUserName: 'SMTP用户名',
    col_smtpPassword: 'SMTP密码',
    col_ssl: 'SSL',
    col_proxy: '代理',
    col_status: '验证',
  },

  outboxStatus: {
    none: '未验证',
    success: '成功',
    failed: '失败',
    unknown: '未知状态',
  },

  statusChip: {
    created: '新建',
    pending: '等待中',
    sending: '发送中',
    success: '成功',
    failed: '失败',
    pause: '暂停',
    stopped: '已停止',
    finish: '完成',
    cancel: '取消',
    true: '是',
    false: '否',
    independent: '独立',
    subUser: '子账户',
    normal: '正常',
    forbiddenLogin: '禁用',
    read: '已读',
    instant: '即时',
    scheduled: '定时',
    unsubscribed: '取消订阅',
    running: '运行中',
    unknown: '未知',
    unverified: '未验证',
    valid: '有效',
    invalid: '无效',
    inProgress: '进行中',
  },

  sendingProgress: {
    title: '发送进度',
  },

  sendDetail: {
    resend: '重新发送',
    delete: '删除'
  },

  unsuscribePage: {
    unsubscribe: '取消订阅',
    unsubscribed: '已取消订阅'
  },
}
