export default {
  system: {
    language: "en_US",
  },
  login: {
    title: "Searching for him thousands of times, nothing to send and drifting alone",
    username: "Username",
    password: "Password",
    login: "Login",
    usernameRequired: "Username cannot be empty",
    passwordRequired: "Password cannot be empty",
    language: "System Language:",
  },
  template: {
    new: "New",
    save: "Save",
    saveAs: "Save As",
    close: "Close",
  },
  langSwitchSuccess: "Language switched successfully",
  dashboard: "Dashboard",
  home: "Home",
  docs: "Documentation",
  profile: "My Profile",
  emailManagement: "Email Management",
  outbox: "Outbox",
  inbox: "Inbox",
  template: "Template",
  editTemplate: "Edit Template",
  sendManagement: "Send Management",
  newSend: "New Send",
  sendHistory: "Send History",
  settings: "Settings",
  logout: "Logout",
  arrivalRate: "Arrival Rate",
  receiptsStatistics: "Receipts Statistics",
  sendInterval: "Send Interval:",
  sendIntervalTooltip: "The interval range between two consecutive emails sent from a single outbox, will fluctuate randomly within this range",
  maxEmailsPerDay: "Max Emails Per Day:",
  maxEmailsPerDayTooltip: "Daily email sending limit for a single outbox. 0 means unlimited",
  second: "Second",
  unlimited: "Unlimited",
  autoResend: "Auto Resend",
  autoResendTooltip: "Automatically resend if sending fails, with a maximum of 5 retries.",
  sendWithImageAndHtml: "Send with Image and HTML",
  sendWithImageAndHtmlTooltip: "Convert half of the emails to images (images may be lost during conversion)",
  avatarTooltip: "Click to select an image",
  uploadedAvatarError: "Failed to upload avatar",
  successModified: "Modified successfully",
  new: "New",
  import: "Import",
  search: "Search",
  clear: "Clear",
  importExcel: "Import from Excel",
  createdTime: "Created Time:",
  modifiedTime: "Modified Time:",
  edit: "Edit",
  view: "View",
  delete: "Delete",
  modify: "Modify",
  confirm: "Confirm",
  cancel: "Cancel",
  addSuccess: "Added successfully",
  addFailed: "Failed to add",
  deleteConfirm: "Delete this item?",
  delete_success: "Deleted successfully",
  editSuccess: "Modified successfully",
  nodesLabel: "Right-click here to add a group",
  addNode: "Add Group",
  addSubNode: "Add Sub Group",
  deleteNodeTooltip: "Delete Warning",
  deleteNodeConfirm: "Delete this group?",
  table: {
    senderId: "Sender ID",
    emailId: "Email ID",
    parentId: "Parent Group ID",
    parentName: "Parent Group",
    groupId: "Group ID",
    groupType: "Group Type",
    subGroupName: "Sub Group Name",
    description: "Description",
    userName: "Name",
    email: "Email",
    smtp: "SMTP Server Address",
    password: "SMTP Password",
    maxEmailsPerDay: "Max Emails Per Day",
    operation: "Operation",
    create: "Create",
    trans: "Transfer",
    trans_history: "Transfer History",
    detail: "Detail",
    detail_tooltip: "View detailed information",
    modify: "Modify",
    delete: "Delete",
    delete_tooltip: "Delete",
    remove: "Remove",
    remove_tooltip: "Remove",
    preview: "Preview",
    preview_tooltip: "Preview",
    version_manager: "Version Management",
    version_manager_tooltip: "Version Management",
    upload: "Upload",
    upload_tooltip: "Upload",
    download: "Download",
    download_tooltip: "Download",
    createDate: "Date",
    subject: "Subject",
    templateName: "Template",
    senderIdsLength: "Total Outboxes",
    receiverIdsLength: "Total Inboxes",
    status: "Status",
    operations: "Operations",
    senderName: "Sender Name",
    senderEmail: "Sender Email",
    receiverName: "Receiver Name",
    receiverEmail: "Receiver Email",
    isSent: "Status",
    sent_state_success: "Success",
    sent_state_fail: "Failure",
    sendMessage: "Reason",
    tryCount: "Retry Count",
    index:"index",
  },
  button: {
    create: "Create",
    new: "New",
    save: "Save",
    confirm: "Confirm",
    cancel: "Cancel",
    delete: "Delete",
    modify: "Modify",
    add: "Add",
    import: "Import",
    export: "Export",
    remove: "Remove",
    select: "Select",
    close: "Close",
    projectLogin: "Project Login",
    storageIn: "Storage In",
    storageInTooltip: "New items storage",
    download: "Download",
    preview: "Preview",
    back: "Back",
  },
  btnSettings: "Settings",
  newOutbox: "New Outbox",
  newInbox: "New Inbox",
  modifyOutbox: "Modify Outbox",
  modifyInbox: "Modify Inbox",
  deleteEmailInfoConfirm: "Delete this email information?",
  clearGroupConfirm: "Clear all emails in this group?",
  allClearSuccess: "All cleared",
  subject: "Subject:",
  subject_tooltip: "Email subject, cannot be empty",
  sender: "Sender:",
  sender_tooltip: "If the sender is empty, a random outbox will be used",
  select_sender: "Select Sender",
  receiver: "Receiver:",
  receiver_tooltip: "If sending according to the defined recipient in the data, the recipient field must be empty",
  select_receiver: "Select Receiver",
  copy_to: "Cc:",
  copy_to_tooltip: "If the Cc is empty, the usernames in copyToEmails from the data will be used as the Cc",
  select_copy_to: "Select Cc",
  template_name: "Template:",
  select_template: "Please select a template",
  excel_file_name: "Data:",
  select_excel_file: "Select Excel",
  attachment: "Attachment:",
  select_attachment: "Select Attachment",
  content: "Content:",
  preview: "Preview",
  send: "Send",
  schedule_send: "Schedule",
  send_to: "Send to:",
  current: "Current:",
  total: " / Total:",
  sending_progress: "Sent:",
  previous_item: "Previous",
  next_item: "Next",
  close: "Close",
  subject_required: "Please enter the subject",
  receiver_required: "Please select a receiver or enter data (one of them must be present)",
  send_dialog_title: "Send Confirmation<em>!</em>",
  selected_receiver_count: "Selected Receivers: {count}",
  data_receiver_count: "Data Receivers: {count}",
  actual_receiver_count: "Actual Receivers: {count}",
  continue_confirmation: "Do you want to continue?",
  send_status1: "Sending finished, success",
  send_status2: "Initialized but not sent",
  send_status4: "Sending...",
  send_status8: "Resending...",
  send_status16: "Paused",
  send_status32: "Sending image",
  send_status64: "Sending HTML",
  delete_history_group_confirm: "Delete this sending record?",
  get_history_detail_error: "Failed to get history details",
  resend_confirmation: "Do you want to resend all failed items? A total of {count} items.",
  no_items_to_send: "No items to send",
  start_label: "Starting...",
  resend: "Resend",
  mail_sent_successfully: "Mail sent successfully",
}
