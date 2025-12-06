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
    appName: '宇正群邮',

    order: '序号',
    failed: '失败',
    success: '操作成功',
    confirm: '确认',
    cancel: '取消',
    modify: '修改',
    edit: '编辑',
    delete: '删除',
    new: '新增',
    add: '添加',
    confirmOperation: '操作确认',
    deleteConfirmation: '删除确认',
    warning: '警告',
    notice: '注意',
    cancelOperation: '取消操作',
    languageRequired: '语言是必填项',
    htmlContentRequired: 'Html 内容是必填项',
    unsubscribePageHasAreadyExist: '该语言的取消订阅页面已存在',
    deleteSuccess: '删除成功',
    updateSuccess: '更新成功',
    pleaseInputNumber: '请输入数字',
    import: '导入',
    importing: '导入中',
    export: '导出',
    exporting: '导出中',
    validate: '验证',
    validateMultiple: '批量验证',
    yes: '是',
    no: '否',
    empty: '无',
    save: '保存'
  },

  // #endregion

  // #region components
  components: {
    // 清空
    clear: '清空',
    // 移除已上传文件
    removeUploadedFile: '移除已上传文件',
    // 选择文件
    selectFile: '选择文件',
    // 上传
    upload: '上传',
    // 中止上传
    abortUpload: '中止上传',
    // 等待上传中...
    waitingForUpload: '等待上传中...',
    // 剩余
    remain: '剩余',
    // 正在计算 ${callbackData.file.name} 哈希值
    calculatingFileHash: '正在计算 {fileName} 哈希值',
    // 正在上传 ${file.name}
    uploadingFile: '正在上传 {fileName}',
  },
  // #endregion

  // #region utils
  utils: {
    // `... 等共 ${labels.length} ${unit}`
    totalItems: '... 等共 {count} {unit}',
    item: '项',

    // 未找到文件
    file_fileNotFound: '未找到文件',
    // 未检测到文件,可能是用户已取消
    file_noFileDetected: '未检测到文件,可能是用户已取消',
    // 指定 Worksheet
    file_specifyWorksheet: '指定 WorkSheet',
    // 请选择 Worksheet
    file_pleaseSelectWorksheet: '请选择 WorkSheet',
    // 字段 ${field} 不能为空
    file_fieldCannotBeEmpty: '字段 {field} 不能为空',
    // 严格模式下, mappers 不能为空
    file_mappersCannotBeEmptyInStrictMode: '严格模式下, mappers 不能为空',
    // 第 ${rowIndex} 行数据中，${map.headerName} 列不能为空
    file_fieldCannotBeEmptyAtRow: '第 {rowIndex} 行数据中，{field} 列不能为空',
    // 正在计算 hash 值...
    file_calculatingHash: '正在计算 hash 值...',
    // 文件 ${file.name} sha256 值为：
    file_fileHashCalculated: '文件 {fileName} sha256 值为：',
    // hash 已校验, 等待上传
    file_hashVerifiedWaitingUpload: 'hash 已校验, 等待上传',
    // 下载地址不合法,应以 http 开头
    file_invalidDownloadUrl: '下载地址不合法,应以 http 开头',
    // 正在解析文件...
    file_parsingFile: '正在解析文件...',
    // 下载完成
    file_downloadCompleted: '下载完成',
    // 下载失败
    file_downloadFailed: '下载失败',
    // ${fileHandle.name} 下载中...
    file_downloadingFile: '{fileName} 下载中...',
    // ${ext} 文件
    file_fileExtension: '{ext} 文件',
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
    tiktokCrawler: 'TikTok爬虫',
    qqGroupMembersGetter: 'QQ群友获取',
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
    new: '新增',
    newItem: '新增项',
    delete: '删除',
    deleteItem: '删除项',
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

  // 模板相关
  template: {
    templateName: '模板名称',
    templateId: '模板ID',
    // 返回模板管理
    backToTemplateManager: '返回模板管理',
    // 输入模板名称
    pleaseInputTemplateName: '请输入模板名称',
    saveTemplate: '保存模板',
    savingTemplate: '正在保存模板...',
    saveTemplateSuccess: '模板保存成功',
    // 保存失败: 无法生成缩略图
    saveFailedCannotGenerateThumbnail: '保存失败: 无法生成缩略图',
    // 当前正文结构异常，无法生成缩略图
    saveFailedCurrentContentStructureInvalid: '当前正文结构异常，无法生成缩略图',
    templateEditorPlaceholder: "在此处输入模板内容, 变量使用 {'{{  }}'} 号包裹, 例如 {'{{ variableName }}'}"
  },

  // AI 相关
  ai: {
    generateBodyWithCopilot: '使用 Copilot 生成正文',
    contentGeneration: '正文生成',
    contentEnhancement: '内容优化',
    prompt: '提示词',
    pleaseInputPrompt: '请输入提示词',
    generatingEmailContent: 'Copilot 正在生成邮件内容, 请稍候... 可能需要几分钟时间',
    contentEnhancementSuccess: '内容优化成功',
    summaryEmailSubjects: '使用 AI 根据邮件内容生成主题',
    bodyOrTemplateRequired: '正文或模板不能为空',
    generatingSubjects: 'AI 正在生成主题, 请稍候... 可能需要几分钟时间',
  },

  // 代理
  proxy: {
    proxyId: '代理ID',
  },

  // #region 邮箱管理
  // 邮箱组
  emailGroup: {
    inboxGroup: '收件箱组',
    outboxGroup: '发件箱组',

    newGroup: '新建组',
    newEmailGroup: '新建邮箱组',
    newGroupSuccess: '新建组成功',
    youCanRightClickToAddNewGroup: '可单击右键添加新组',

    field_name: '组名',
    field_description: '描述',
    field_order: "@:global.order",

    deleteGroupAndInboxesConfirm: '即将删除组【{groupName}】及其所有收件箱, 是否继续？',
    deleteGroupSuccess: '删除组 {groupName} 成功',
    modifyCurrentGroup: '修改当前组',
    modifyEmailGroup: '修改邮箱组',
    deleteCurrentGroup: '删除当前组',
  },


  // 收件箱管理
  inboxManager: {
    importFromTxt: 'Txt 导入',
    importFromTxtTooltip: "支持格式: 邮箱, 用户名, 最小发件间隔 \n 三者可以任意顺序排列 \n 例如: test{'@'}gmail.com, uzonmail, 1 \n 其中最小发件间隔单位为小时",
    inboxText: '收件箱文本',
    importFromTxtPlaceholder: '每行一个收件箱',

    inbox: '收件箱',

    newInbox: '新建收件箱',
    newInboxSuccess: '新建收件箱成功',
    addGroupFirst: '请先添加分组',
    editInbox: '修改收件箱',
    editCurrentInbox: '修改当前收件箱',
    updateInboxSuccess: '更新收件箱成功',
    validateCurrentOrSelectedInboxes: '验证当前或选中的收件箱',

    inboxTemplate: '收件箱模板',
    exportInboxTemplate: '导出收件箱模板',
    export_EmailColumnTooltip: '邮箱(导入时，请删除该行数据)',
    templateDonwloadSuccess: '模板下载成功',
    noInboxesInThisGroupForExporting: '当前组中没有可导出的收件箱',

    importInbox: '导入收件箱',
    importInboxSuccess: '导入收件箱成功',
    ctx_import: '导入',
    ctx_importInboxToCurrentGroup: '向当前组中导入收件箱',
    ctx_export: '导出',
    ctx_exportInboxToCurrentGroup: '导出当前组中的收件箱',
    ctx_deleteDelivered: '删除投递',
    ctx_deleteDeliveredInboxesInCurrentGroup: '删除当前组中所有已成功投递的收件箱',

    availableImportDataNotFound: '未找到可导入的数据',
    emptyDataAtRow: '第 {row} 行数据为空',
    emailFormatInvalid: '邮箱格式错误: {email}',
    noValidImportData: '未找到有效的导入数据, 请检查表头是否正确',
    confirmImportWithErrors: '部分数据格式错误，是否继续导入？',
    importCancelled: '导入已取消',

    validateAllInvalidInboxesInCurrentGroup: '批量验证当前组中的所有未验证过的收件箱',

    deleteCurrentInbox: '删除当前收件箱',
    ctx_deleteInvalid: '删除无效',
    deleteAllInvalidInboxesInCurrentGroup: '删除当前组中所有验证无效的收件箱',
    isDeleteEmailOf: '是否删除收件箱: {email} ?',
    isDeleteAllDeliveredInboxesInCurrentGroup: '是否删除当前组中所有已成功投递的收件箱？',

    col_email: '收件箱',
    col_inbox: '收件箱',
    col_inboxName: '收件箱姓名',
    col_name: '收件人姓名',
    col_description: '描述',
    col_minInboxCooldownHours: '最小收件间隔(小时)',
    col_lastSuccessDeliveryDate: '最后成功发件时间',
  },

  // 发件箱管理
  outboxManager: {
    col_order: '@:global.order',
    col_email: '发件箱',
    col_outbox: '发件箱',
    col_outboxName: '发件人姓名',
    col_smtpHost: 'SMTP地址',
    col_smtpPort: 'SMTP端口',
    col_smtpUserName: 'SMTP用户名',
    col_smtpPassword: 'SMTP密码',
    col_enableSSL: 'SSL',
    col_description: '描述',
    col_proxy: '代理',
    col_status: '验证',
    col_replyToEmails: '回信收件人',

    ifSameAsEmailUseEmpty: '可为空，若为空，则使用发件邮箱作用用户名',
    smtpPasswordIsRequired: 'smtp密码不能为空',
    ifEmptyProxyUseSystemSettings: '为空时使用系统设置',
    outlookDelegateAuthorizationSkippedNonExchangeSmtp: ' 检测到 Outlook 邮箱，但 SMTP 地址不是 Exchange 地址，跳过委托授权',
    existingRefreshTokenNoNeedDelegateAuthorization: '该邮箱已换取 refreshToken, 无需进行委托授权',
    outlookDelegateAuthorizationSkippedEncryptedPassword: '检测到该发件箱的密码为加密状态，跳过委托授权',
    detectedExchangeEmailStartingOutlookDelegateAuthorization: '检测到个人 Outlook 邮箱，需要进行委托授权，请批准弹出的授权窗口',
    failedToGetAuthorizationUrl: '获取委托授权地址失败，请稍后重试',
    allowPopupWindowsForAuthorization: '请允许弹出窗口以进行授权',
    delegateCopleted: '委托授权结束',

    decryptionFailedDueToKeyChange: '密钥变动,解密失败。请重新输入 smtp 密码',

    importFromExcel: 'Excel导入',
    importOutboxForCurrentGroupFromExcel: '从Excel导入发件箱到当前组',

    importFromTxt: 'Txt导入',
    importOutboxForCurrentGroupFromTxt: '从Txt导入发件箱到当前组',
    importFromTxtTooltip: "支持格式:\n1. 邮箱, 密码: test{'@'}gmail.com, 1234\n2. 邮箱, 密码, smtp地址, smtp端口: test{'@'}gmail.com, 1234, smtp.gmail.com, 465",
    noItemsToImport: '没有可导入项',
    emaiEmptyAtRow: '第 {row} 行邮箱为空',
    emailFormatInvalid: '邮箱格式错误: {email}',
    noValidImportData: '未找到有效的导入数据, 请检查表头是否正确',
    confirmImportWithErrors: '部分数据格式错误，是否继续导入？',
    importCancelled: '导入已取消',
    importOutboxSuccess: '导入发件箱成功, 共导入 {count} 项',

    exportOutboxesInThisGroup: '导出当前组中的发件箱',
    noItemsToExport: '没有可导出项',
    // 修复：避免嵌套引号导致的语法错误，使用拼接保留 i18n 引用格式
    exportFileName: '{groupName}-' + '@:outboxManager.col_email' + '.xlsx',
    exportColumn_email: '填写发件邮箱(导入时，请删除该行数据)',
    exportColumn_name: '填写发件人名称(可选)',
    exportColumn_userName: '填写 smtp 用户名，若与邮箱一致，则设置不填写',
    exportColumn_password: '填写 smtp 密码',
    exportColumn_smtpHost: '填写 smtp 地址',
    exportColumn_description: '描述(可选)',
    exportColumn_proxy: '格式为：http://username:password@domain:port(可选)',
    exportColumn_replyToEmails: '回信收件人(多个使用逗号分隔)',
    outboxTemplateFileName: '发件箱模板.xlsx',
    outboxTemplateDownloadSuccess: '发件箱模板下载成功',

    doDeleteAllInvalidOutboxes: '是否删除当前组中所有验证失败的发件箱？',
    editCurrentOutbox: '编辑当前发件箱',
    deleteCurrentOrSelection: '删除当前或选中的发件箱',
    validate: '验证',
    sendTestToMe: '验证当前或选中发件箱',
    validateBatch: '批量验证',
    validateAllUnverifiedInGroup: '批量验证当前组中所有未验证的邮箱',
    isConfirmValidateBatch: '是否验证当前组中所有未验证的邮箱？',
    validatingEmail: '正在验证发件箱: {email}',
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
    newOutbox: '新增发件箱',
    pleaseAddGroupFirst: '请先添加组',
    downloadOutboxTemplate: '下载发件箱模板',
    importOutbox: '导入发件箱',
    delegateAuthorizationCompleted: '委托授权结束',
    validationSuccessful: '验证成功',
    validationFailedWithMessage: '验证失败: {message}',

    modifyOutboxTitle: '修改发件箱 / {email}',
    newOutboxTitle: '新建发件箱 / {groupName}',
    newOutboxSuccess: '新建发件箱成功',
    updateOutboxSuccess: '更新发件箱成功',
  },
  // #endregion

  // #region 发件任务
  sendingTask: {
    subject: '主题',
    template: '模板',
    // 选择模板
    selectTemplateTooltip: '请选择模板 (模板与正文需至少有一个不为空)',
    templateUnit: '个模板',

    subjectPlaceholder: '请输入邮件主题(若需要随机主题，多个主题之间请使用分号 ; 进行分隔或者单独一行)',
    sender: '发件人',
    senderPlaceholder: '请选择发件箱或者发件组 (必须)',
    recipients: '收件人',
    recipientsPlaceholder: '请选择收件箱或者收件箱组 (必须)',
    mergeToSend: '合并',

    body: '正文',
    // 自定义变量
    customVariables: '自定义变量',

    // 模板中的默认值
    example_inbox: '收件箱(导入时，请删除该行数据)',
    example_inboxName: '收件人姓名(可选)',
    example_outbox: '发件箱(可选)',
    example_outboxName: '发件人姓名(可选)',
    example_subject: '主题(可选)',
    example_body: '内容(可选)',
    example_cc: '抄送(多个逗号分隔,可选)',
    example_bcc: '密送(多个逗号分隔,可选)',
    example_templateName: '模板名称(可选)',
    example_templateId: '模板id(可选)',
    example_proxy: '代理Id(可选)',
    example_other: '可以继续增加列，作为自定义字段(可选)',
    example_attachmentNames: '附件名称(多个逗号分隔,可选)',

    // 发件数据模板.xlsx
    sendingDataTemplateFileName: '发件数据模板.xlsx',
    // 发件数据
    sendingDataTemplateSheetName: '发件数据',
    // 下载模板
    templateDownloadSuccess: '模板下载成功',

    // 抄送人
    ccRecipients: '抄送人',
    // 请选择抄送人 (可选)
    ccRecipientsPlaceholder: '请选择抄送人 (可选)',
    bccRecipients: '密送人',
    // 请选择密送人 (可选)
    bccRecipientsPlaceholder: '请选择密送人 (可选)',
    // 附件
    attachments: '附件',
    // 预览
    btn_preview: '预览',
    btn_previewTooltip: '预览发件正文',
    // 定时
    btn_schedule: '定时',
    btn_scheduleTooltip: '定时发件',
    // 发送
    btn_send: '发送',
    btn_sendTooltip: '立即发送',
    btn_warmup: '预热',
    btn_warmupTooltip: 'IP 预热发件',
    sendBatchTooltips: ['若有多个发件人,将其合并到一封邮件中发送', '启用后无法对单个发件箱进行重发', '一般不建议启用'].join('\n'),
    // 邮件模板和正文必须有一个不为空
    emailTemplateAndBodyRequired: '邮件模板和正文必须有一个不为空',
    // 请选择发件人
    pleaseSelectSender: '请选择发件人',
    // 请选择收件人
    pleaseSelectRecipients: '请选择收件人',
    // 请填写邮件主题
    pleaseInputEmailSubject: '请填写邮件主题',
    // 除了数据外，您选择了额外的收件箱
    notifyExtraInboxSelected: '除了数据外，您选择了额外的收件箱',
    // 请保证每条数据都有 inbox (收件人邮箱)
    pleaseEnsureEachDataHasInbox: '请保证每条数据都有 inbox (收件人邮箱)',
    // 数据中发件箱缺失，请在数据中指定发件箱或选择发件箱
    outboxMissingInData: '数据中发件箱缺失，请在数据中指定发件箱或选择发件箱',
    // 数据中正文缺失，请在数据中指定邮件正文 或 选择模板 或 填写正文
    bodyMissingInData: '数据中正文缺失，请在数据中指定邮件正文 或 选择模板 或 填写正文',
    // 请单击附件上传按钮上传附件
    pleaseUploadAttachments: '请单击附件上传按钮上传附件',
    // 开始发送...
    sendingStarted: '开始发送...',
    // 发送进度
    sendingProgress: '发送进度',
    // 定时发送已预约
    scheduledSendingBooked: '定时发送已预约',
    // 数据
    emailData: '数据',
    // 删除当前数据
    deleteCurrentData: '删除当前数据',
    // 下载模板
    downloadTemplate: '下载模板',
    // 选择数据
    selectData: '选择数据',
    // 数据为空，请重新选择
    dataIsEmpty: '数据为空，请重新选择',
    // 第 ${emptyInboxRowIndex + 2} 行数据的收件箱为空或格式不正确，请检查后重新选择
    inboxEmptyOrInvalidAtRow: '第 {row} 行数据的收件箱为空或格式不正确，请检查后重新选择',
  },
  // #endregion

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

  unsubscribePage: {
    unsubscribe: '取消订阅',
    unsubscribed: '已取消订阅'
  },

  // qq 获取器
  qqGetter: {
    qqNumber: 'QQ号码',
    qqNickname: 'QQ昵称',
    save: '保存',
    saveAsInbox: '保存为收件箱',
    selectGroup: '选择群',
    // 是否将当前 QQ 群中的所有成员保存为收件箱？
    confirmSaveAllMembersAsInboxes: '是否将当前 QQ 群中的 {count} 个成员保存为收件箱？',
    saveMembersAsInboxesSuccess: '群成员保存为收件箱成功',
  },

  basicSettings: {
    tabUser: '用户',
    tabUserTooltip: '用户设置，该设置会覆盖组织设置',
    tabOrganization: '组织',
    tabOrganizationTooltip: '组织设置，该设置会覆盖系统设置',
    tabSystem: '系统',
    tabSystemTooltip: '系统设置，所有用户和组织默认应用该设置',

    // #region AI 设置
    aiSettings: 'AI 设置',
    addAiKeyToEnableAiFeatures: '添加 AI 密钥以启用 AI 功能',
    enableAIFeatures: '启用 AI 功能',
    enableAIFeaturesTooltip: '启用后，可使用 AI 功能来优化邮件内容和发送效果',

    aiProviderType: 'AI 提供商类型',
    aiProviderTooltip: '选择 AI 提供商类型，例如 OpenAI, 目前仅支持 OpenAI 或者兼容 API 的提供商',
    endPoint: 'AI 端点',
    aiKey: 'AI 密钥',
    enterYourAiKey: '输入您的 AI 密钥',
    aiKeyTooltip: '支持 OpenAI 兼容的 API Key，例如 OpenAI, Azure OpenAI 等等',
    aiModel: 'AI 模型',
    aiModelTooltip: '选择使用的 AI 模型，例如 gpt-3.5-turbo, gpt-4 等等',
    maxTokens: '最大令牌数',

    btn_saveSetting: '保存设置',
    btn_saveSettingTooltip: '保存 AI 设置',
    notify_updateAISettingsTitle: '更新 AI 设置',
    notify_updateAISettingsUSuccess: '更新 AI 设置成功',
    // #endregion

  }
}
