// This is just an example,
// so you can safely delete all default props below

export default {
  // #region General
  failed: 'Action failed',
  success: 'Action was successful',
  confirm: 'Confirm',
  cancel: 'Cancel',
  edit: 'Edit',
  delete: 'Delete',
  confirmOperation: 'Confirm operation',
  deleteConfirm: 'Delete confirmation',
  cancelOperation: 'Cancel operation',
  languageRequired: 'Language is required',
  htmlContentRequired: 'HTML content is required',
  unsubscribePageHasAreadyExist: 'Unsubscribe page for this language already exists',
  deleteSuccess: 'Deletion successful',
  updateSuccess: 'Update successful',
  // #endregion

  // #region Global
  global: {
    order: 'Order',
    failed: 'Action failed',
    success: 'Action was successful',
    confirm: 'Confirm',
    cancel: 'Cancel',
    edit: 'Edit',
    delete: 'Delete',
    confirmOperation: 'Confirm operation',
    deleteConfirm: 'Delete confirmation',
    cancelOperation: 'Cancel operation',
    languageRequired: 'Language is required',
    htmlContentRequired: 'HTML content is required',
    unsubscribePageHasAreadyExist: 'Unsubscribe page for this language already exists',
    deleteSuccess: 'Deletion successful',
    updateSuccess: 'Update successful',
  },
  // #endregion

  // #region Routes
  routes: {
    dashboard: 'Dashboard',
    userInfo: 'User Info',
    profile: 'Profile',
    emailManagement: 'Email Management',
    outbox: 'Outbox',
    inbox: 'Inbox',
    templateData: 'Template Data',
    templateManagement: 'Template Management',
    variableManagement: 'Variable Management',
    templateEditor: 'Template Editor',
    sendingManagement: 'Sending Management',
    newSending: 'New Sending',
    ipWarmUp: 'IP Warm-up',
    sendHistory: 'Send History',
    sendDetail: 'Send Detail',
    attachmentManagement: 'Attachment Management',
    statisticsReport: 'Statistics Report',
    readStatistics: 'Read Statistics',
    unsubscribeStatistics: 'Unsubscribe Statistics',
    emailCrawler: 'Email Crawler',
    tiktokDevice: 'TikTok Device',
    crawlerTask: 'Crawler Task',
    crawlerResult: 'Crawler Result',
    systemSettings: 'System Settings',
    basicSettings: 'Basic Settings',
    proxyManagement: 'Proxy Management',
    userManagement: 'User Management',
    permissionManagement: 'Permission Management',
    functionManagement: 'Function Management',
    roleManagement: 'Role Management',
    userRole: 'User Role',
    apiAccess: 'API Access',
    softwareLicense: 'Software License',
    sponsorAuthor: 'Sponsor Author',
    helpDoc: 'Help Document',
    startGuide: 'Start Guide',
    login: 'Login',
    singlePages: 'Single Pages',
    unsubscribe: 'Unsubscribe',
    exception: 'Exception',
  },
  // #endregion

  // #region 按钮
  buttons: {
    new: 'New',
    newItem: 'New Item',

    delete: 'Delete',
    deletetItem: 'Delete Item',

    save: 'Save',

    cancel: 'Cancel',
    cancelCurrentOperation: 'Cancel Current Operation',

    export: 'Export',
    exportData: 'Export Data',

    import: 'Import',
    importData: 'Import Data',

    confirm: 'Confirm',
    confirmCurrentOperation: 'Confirm Current Operation',
  },
  // #endregion


  // #region Login Page
  loginPage: {
    userName: 'Username',
    password: 'Password',
    signIn: 'Sign In',
    version: 'Version',
    client: 'Client',
    server: 'Server',
    pleaseInputUserName: 'Please enter username',
    pleaseInputPassword: 'Please enter password',
  },
  // #endregion

  // #region dashboard page
  dashboardPage: {
    outboxCount: 'Outbox Count',
    emailCount: 'Email Count',
    count: 'Count',
    outboxStatsTitle: 'Outbox Stats',
    inboxStatsTitle: 'Inbox Stats',
    monthlySendingStatsTitle: 'Monthly Sending Stats',
  },
  // #endregion

  // #region 邮箱管理
  emailGroup: {
    inboxGroup: 'Inbox Group',
    outboxGroup: 'Outbox Group',

    newGroup: 'New Group',

    field_name: 'Group Name',
    field_description: 'Description',
    field_order: "{'global.order'}",
  },

  // 收件箱管理
  inboxManager: {
    importFromTxt: 'Import from TXT',
    importFromTxtTooltip: "Supported format: Email, Username, Minimum Sending Interval \n Can be in any order \n Example: test{'@'}gmail.com, uzonmail, 1 \n Where minimum sending interval is in hours",

    inbox: 'Inbox',

    newInbox: 'New Inbox',
    addGroupFirst: 'Please add group first',

    exportInboxTemplate: 'Export Inbox Template',
    importInbox: 'Import Inbox',

    ctx_import: 'Import',
    ctx_importInboxToCurrentGroup: 'Import inboxes to current group',
    ctx_export: 'Export',
    ctx_exportInboxToCurrentGroup: 'Export inboxes from current group',

    col_email: 'Email',
    col_name: 'Recipient Name',
    col_description: 'Description',
    col_minInboxCooldownHours: 'Minimum Inbox Cooldown (Hours)',
    col_lastSuccessDeliveryDate: 'Last Successful Delivery Date',

    noInboxesInThisGroup: 'No inboxes available for export in this group',
  },
  // #endregion

  // Table Columns
  columns: {
    index: 'Index',
    name: 'Name',
    description: 'Description',
  },

  outboxManager: {
    importFromTxt: 'Import from TXT',
    importFromTxtTooltip: "Supported formats:\n1. Email, Password: test{'@'}gmail.com, 1234\n2. Email, Password, SMTP Address, SMTP Port: test{'@'}gmail.com, 1234, smtp.gmail.com, 465",

    doDeleteAllInvalidOutboxes: 'Do you want to delete all invalid outboxes in the current group?',
    editCurrentOutbox: 'Edit current outbox',
    deleteCurrentOrSelection: 'Delete current or selected outboxes',
    validate: 'Validate',
    sendTestToMe: 'Validate current or selected outboxes',
    validateBatch: 'Batch validate',
    validateAllUnverifiedInGroup: 'Batch validate all unverified emails in the current group',
    isConfirmValidateBatch: 'Do you want to validate all unverified emails in the current group?',
    validating: 'Validating',
    validateBatchSuccess: 'Batch validation completed',
    validateFailed: '{email} validation failed, reason: {reason}',

    deleteInvalid: 'Delete invalid',
    deleteCurrentGroupInvalidOutboxes: 'Delete invalid outboxes in the current group',
    deleteOutbox: 'Delete outbox',
    isDeleteCurrentOutbox: 'Do you want to delete outbox: {email}?',
    isDeleteSelectedOutboxes: 'Do you want to delete the selected {count} outboxes?',
    deleteSuccess: 'Deletion successful, total deleted: {count}',

    outlookDelegateAuthorization: 'Outlook authorization',

    col_outbox: 'Outbox',
    col_outboxUserName: 'Name (Sender Name)',
    col_smtpAddress: 'SMTP Address',
    col_smtpPort: 'SMTP Port',
    col_smtpUserName: 'SMTP Username',
    col_smtpPassword: 'SMTP Password',
    col_ssl: 'SSL',
    col_proxy: 'Proxy',
    col_status: 'Validation',
  },


  outboxStatus: {
    none: 'Unverified',
    success: 'Success',
    failed: 'Failed',
    unknown: 'Unknown status',
  },

  statusChip: {
    created: 'Created',
    pending: 'Pending',
    sending: 'Sending',
    success: 'Success',
    failed: 'Failed',
    pause: 'Paused',
    stopped: 'Stopped',
    finish: 'Finished',
    cancel: 'Canceled',
    true: 'Yes',
    false: 'No',
    independent: 'Independent',
    subUser: 'Sub-account',
    normal: 'Normal',
    forbiddenLogin: 'Disabled',
    read: 'Read',
    instant: 'Instant',
    scheduled: 'Scheduled',
    unsubscribed: 'Unsubscribed',
    running: 'Running',
    unknown: 'Unknown',
    unverified: 'Unverified',
    valid: 'Valid',
    invalid: 'Invalid',
    inProgress: 'In Progress',
  },

  sendingProgress: {
    title: 'Sending Progress',
  },

  sendDetail: {
    resend: 'Resend',
    delete: 'Delete'
  },

  unsubscribePage: {
    unsubscribe: 'Unsubscribe',
    unsubscribed: 'Unsubscribed'
  },
}
