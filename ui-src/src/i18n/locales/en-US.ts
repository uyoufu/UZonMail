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
    appName: 'UZonMail',

    order: 'Order',
    failed: 'Action failed',
    success: 'Action was successful',
    confirm: 'Confirm',
    cancel: 'Cancel',
    modify: 'Modify',
    edit: 'Edit',
    delete: 'Delete',
    new: 'New',
    add: 'Add',
    confirmOperation: 'Confirm operation',
    deleteConfirmation: 'Delete confirmation',
    cancelOperation: 'Cancel operation',
    languageRequired: 'Language is required',
    htmlContentRequired: 'HTML content is required',
    unsubscribePageHasAreadyExist: 'Unsubscribe page for this language already exists',
    deleteSuccess: 'Deletion successful',
    updateSuccess: 'Update successful',
    pleaseInputNumber: 'Please input number',
    warning: 'Warning',
    notice: 'Notice',
    import: 'Import',
    importing: 'Importing',
    export: 'Export',
    exporting: 'Exporting',
    validate: 'Validate',
    validateMultiple: 'Batch Validate',
    yes: 'Yes',
    no: 'No',
    empty: 'Empty',
  },
  // #endregion

  // #region components
  components: {
    // Clear
    clear: 'Clear',
    // Remove uploaded file
    removeUploadedFile: 'Remove uploaded file',
    // Select file
    selectFile: 'Select file',
    // Upload
    upload: 'Upload',
    // Abort upload
    abortUpload: 'Abort upload',
    // Waiting for upload...
    waitingForUpload: 'Waiting for upload...',
    // Remaining
    remain: 'Remain',
    // Calculating hash for ${fileName}
    calculatingFileHash: 'Calculating hash for {fileName}',
    // Uploading ${fileName}
    uploadingFile: 'Uploading {fileName}',
  },
  // #endregion

  // #region utils
  utils: {
    // `... 等共 ${labels.length} ${unit}`
    totalItems: '... and a total of {count} {unit}',
    item: 'item',
  },
  // #endregion
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

  template: {
    templateName: 'Template Name',
    templateId: 'Template ID',
    backToTemplateManager: 'Back to Template Manager',
    pleaseInputTemplateName: 'Please input template name',
    saveTemplate: 'Save Template',
    savingTemplate: 'Saving template...',
    saveTemplateSuccess: 'Template saved successfully',
    saveFailedCannotGenerateThumbnail: 'Save failed: Cannot generate thumbnail',
    saveFailedCurrentContentStructureInvalid: 'Current content structure is invalid, cannot generate thumbnail',
    templateEditorPlaceholder: "Enter template content here, variables are wrapped in {'{{ }}'}, for example {'{{ variableName }}'}",
  },

  proxy: {
    proxyId: 'Proxy ID',
  },

  // #region 邮箱管理
  emailGroup: {
    inboxGroup: 'Inbox Group',
    outboxGroup: 'Outbox Group',

    newGroup: 'New Group',
    newEmailGroup: 'New Email Group',
    newGroupSuccess: 'New group success',
    youCanRightClickToAddNewGroup: 'You can right click to add a new group',

    field_name: 'Group Name',
    field_description: 'Description',
    field_order: "@:{'global.order'}",

    willDeleteGroupAndInboxes: 'Will delete group and all its inboxes',
    deleteGroupAndInboxesConfirm: 'Will delete group [{groupName}] and all its inboxes, continue?',
    deleteGroupSuccess: 'Delete group {groupName} success',
    modifyCurrentGroup: 'Modify current group',
    modifyEmailGroup: 'Modify email group',
    deleteCurrentGroup: 'Delete current group',
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
    ctx_deleteDelivered: 'Delete Delivered',
    ctx_deleteDeliveredInboxesInCurrentGroup: 'Delete all delivered inboxes in the current group',

    col_email: 'Email',
    col_inbox: 'Inbox',
    col_inboxName: 'Inbox Name',
    col_name: 'Recipient Name',
    col_description: 'Description',
    col_minInboxCooldownHours: 'Minimum Inbox Cooldown (Hours)',
    col_lastSuccessDeliveryDate: 'Last Successful Delivery Date',

    noInboxesInThisGroupForExporting: 'No inboxes available for export in this group',
    inboxText: 'Inbox Text',
    importFromTxtPlaceholder: 'One inbox per line',
    newInboxSuccess: 'New inbox created successfully',
    editInbox: 'Edit Inbox',
    editCurrentInbox: 'Edit Current Inbox',
    updateInboxSuccess: 'Inbox updated successfully',
    validateCurrentOrSelectedInboxes: 'Validate current or selected inboxes',
    inboxTemplate: 'Inbox Template',
    export_EmailColumnTooltip: 'Email (please delete this row when importing)',
    templateDonwloadSuccess: 'Template download successful',
    importInboxSuccess: 'Inbox imported successfully',
    availableImportDataNotFound: 'No importable data found',
    emptyDataAtRow: 'Row {row} data is empty',
    emailFormatInvalid: 'Invalid email format: {email}',
    noValidImportData: 'No valid import data found, please check the header',
    confirmImportWithErrors: 'Some data has format errors, continue importing?',
    importCancelled: 'Import cancelled',
    validateAllInvalidInboxesInCurrentGroup: 'Batch validate all unverified inboxes in the current group',
    deleteCurrentInbox: 'Delete current inbox',
    ctx_deleteInvalid: 'Delete Invalid',
    deleteAllInvalidInboxesInCurrentGroup: 'Delete all invalid inboxes in the current group',
    isDeleteEmailOf: 'Delete inbox: {email} ?',
    isDeleteAllDeliveredInboxesInCurrentGroup: 'Delete all delivered inboxes in the current group?',
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

    noItemsToExport: 'No items to export',

    outlookDelegateAuthorization: 'Outlook authorization',
    newOutbox: 'New Outbox',
    pleaseAddGroupFirst: 'Please add group first',
    downloadOutboxTemplate: 'Download Outbox Template',
    importOutbox: 'Import Outbox',
    delegateAuthorizationCompleted: 'Delegate authorization completed',
    validationSuccessful: 'Validation successful',
    validationFailedWithMessage: 'Validation failed: {message}',
    modifyOutboxTitle: 'Modify Outbox / {email}',
    outlookSmtpNotExchange: 'Detected Outlook email, but SMTP address is not Exchange address, skipping delegate authorization',
    personalOutlookNeedsAuthorization: 'Detected personal Outlook email, delegate authorization required, please approve',
    refreshTokenAlreadyObtained: 'This email has already obtained refreshToken, no need for delegate authorization',
    failedToGetAuthorizationUrl: 'Failed to get delegate authorization URL, please try again later',

    col_order: '@:global.order',
    col_outbox: 'Outbox',
    col_outboxName: 'Name (Sender Name)',
    col_smtpHost: 'SMTP Address',
    col_smtpPort: 'SMTP Port',
    col_smtpUserName: 'SMTP Username',
    col_smtpPassword: 'SMTP Password',
    col_enableSSL: 'SSL',
    col_proxy: 'Proxy',
    col_description: 'Description',
    col_status: 'Validation',
    col_email: 'Outbox',
    col_replyToEmails: 'Reply To Emails',

    ifSameAsEmailUseEmpty: 'Leave empty if same as email, otherwise use as username',
    smtpPasswordIsRequired: 'SMTP password is required',
    ifEmptyProxyUseSystemSettings: 'Use system settings if empty',
    outlookDelegateAuthorizationSkippedNonExchangeSmtp: 'Detected Outlook email, but SMTP address is not Exchange address, skipping delegate authorization',
    existingRefreshTokenNoNeedDelegateAuthorization: 'This email has already obtained refreshToken, no need for delegate authorization',
    detectedExchangeEmailStartingOutlookDelegateAuthorization: 'Detected personal Outlook email, delegate authorization required, please approve the authorization window',
    allowPopupWindowsForAuthorization: 'Please allow popup windows for authorization',
    delegateCopleted: 'Delegate authorization completed',
    decryptionFailedDueToKeyChange: 'Decryption failed due to key change. Please re-enter SMTP password',
    importFromExcel: 'Import from Excel',
    importOutboxForCurrentGroupFromExcel: 'Import outboxes to current group from Excel',
    importOutboxForCurrentGroupFromTxt: 'Import outboxes to current group from TXT',
    noItemsToImport: 'No items to import',
    emailFormatInvalid: 'Invalid email format: {email}',
    noValidImportData: 'No valid import data found, please check the header',
    confirmImportWithErrors: 'Some data has format errors, continue importing?',
    importCancelled: 'Import cancelled',
    emaiEmptyAtRow: 'Email empty at row {row}',
    importOutboxSuccess: 'Import outbox success, imported {count} items',
    exportOutboxesInThisGroup: 'Export outboxes in this group',
    exportFileName: '{groupName}-' + '@:outboxManager.col_email' + '.xlsx',
    exportColumn_email: 'Fill in outbox email (please delete this row when importing)',
    exportColumn_name: 'Fill in sender name (optional)',
    exportColumn_userName: 'Fill in SMTP username, leave blank if same as email',
    exportColumn_password: 'Fill in SMTP password',
    exportColumn_smtpHost: 'Fill in SMTP address',
    exportColumn_description: 'Description (optional)',
    exportColumn_proxy: 'Format: http://username:password@domain:port (optional)',
    exportColumn_replyToEmails: 'Reply to emails (separate with commas)',
    outboxTemplateFileName: 'Outbox template.xlsx',
    outboxTemplateDownloadSuccess: 'Outbox template download success',
    validatingEmail: 'Validating outbox: {email}',
    newOutboxTitle: 'New Outbox / {groupName}',
    newOutboxSuccess: 'New outbox success',
    updateOutboxSuccess: 'Update outbox success',
  },


  // #region Sending Task
  sendingTask: {
    subjectPlaceholder: 'Please input email subject (if you need random subjects, separate multiple subjects with a semicolon ; or place each on its own line)',
    subject: 'Subject',
    template: 'Template',
    // select template
    selectTemplateTooltip: 'Please select a template (template and body must have at least one non-empty)',
    templateUnit: 'templates',
    sender: 'Sender',
    senderPlaceholder: 'Please select an outbox or outbox group (required)',
    recipients: 'Recipients',
    recipientsPlaceholder: 'Please select inboxes or inbox groups (required)',
    mergeToSend: 'Merge',
    ccRecipients: 'CC',
    ccRecipientsPlaceholder: 'Please select CC recipients (optional)',
    attachments: 'Attachments',
    btn_preview: 'Preview',
    btn_previewTooltip: 'Preview email body',
    btn_schedule: 'Schedule',
    btn_scheduleTooltip: 'Schedule sending',
    btn_send: 'Send',
    btn_sendTooltip: 'Send immediately',
    btn_warmup: 'Warm-up',
    btn_warmupTooltip: 'IP warm-up sending',
    sendBatchTooltips: ['If there are multiple senders, they will be merged into one email', 'Once enabled you cannot resend for a single outbox', "Not generally recommended"].join('\n'),
    emailTemplateAndBodyRequired: 'Either email template or body is required',
    pleaseSelectSender: 'Please select a sender',
    pleaseSelectRecipients: 'Please select recipients',
    pleaseInputEmailSubject: 'Please input email subject',
    notifyExtraInboxSelected: 'You selected extra inboxes in addition to the data',
    pleaseEnsureEachDataHasInbox: 'Please ensure each data row has an inbox (recipient email)',
    outboxMissingInData: 'Outbox missing in data, please specify outbox in data or select an outbox',
    bodyMissingInData: 'Body missing in data, please specify email body in data or select a template or enter the body',
    body: 'Body',
    // custom variables
    customVariables: 'Custom Variables',
    pleaseUploadAttachments: 'Please click the attachments upload button to upload attachments',
    sendingStarted: 'Sending started...',
    sendingProgress: 'Sending progress',
    scheduledSendingBooked: 'Scheduled sending booked',
    // examples for template columns
    example_inbox: 'Inbox (please delete this row when importing)',
    example_inboxName: 'Recipient Name (optional)',
    example_outbox: 'Outbox (optional)',
    example_outboxName: 'Sender Name (optional)',
    example_subject: 'Subject (optional)',
    example_body: 'Content (optional)',
    example_cc: 'CC (comma separated, optional)',
    example_bcc: 'BCC (comma separated, optional)',
    example_templateName: 'Template Name (optional)',
    example_templateId: 'Template ID (optional)',
    example_proxy: 'Proxy ID (optional)',
    example_other: 'You can add more columns as custom fields (optional)',
    example_attachmentNames: 'Attachment names (comma separated, optional)',
    // sending data template
    sendingDataTemplateFileName: 'Sending Data Template.xlsx',
    sendingDataTemplateSheetName: 'Sending Data',
    templateDownloadSuccess: 'Template download successful',
    // data
    emailData: 'Data',
    // delete current data
    deleteCurrentData: 'Delete current data',
    // download template
    downloadTemplate: 'Download Template',
    // select data
    selectData: 'Select Data',
    // data empty
    dataIsEmpty: 'Data is empty, please reselect',
    // inbox empty or invalid at row
    inboxEmptyOrInvalidAtRow: 'Inbox at row {row} is empty or invalid, please check and reselect',
    bccRecipients: 'BCC',
    bccRecipientsPlaceholder: 'Please select BCC recipients (optional)',
  },
  // #endregion


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
  }
  ,
  // keep old spelling from zh file
  unsuscribePage: {
    unsubscribe: 'Unsubscribe',
    unsubscribed: 'Unsubscribed'
  }
}
