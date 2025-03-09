export default {
  failed: '操作失败',
  success: '操作成功',
  confirm: '确认',
  cancel: '取消',
  edit: '编辑',
  delete: '删除',
  confirmOperation: '操作确认',
  deleteConfirm: '删除确认',
  cancelOperation: '取消操作',
  unsuscribePage: {
    unsubscribe: '取消订阅',
    unsubscribed: '已取消订阅'
  },
  languageRequired: '语言是必填项',
  htmlContentRequired: 'Html 内容是必填项',
  unsubscribePageHasAreadyExist: '该语言的取消订阅页面已存在',
  deleteSuccess: '删除成功',
  updateSuccess: '更新成功',

  columns: {
    index: '序号',
    name: '名称',
    description: '描述',
  },

  outboxManager: {
    doDeleteAllInvalidOutboxes: '是否删除当前组中所有验证失败的发件箱？',
    editCurrentOutbox: '编辑当前发件箱',
    deleteCurrentOrSelection: '删除当前或选中的发件箱',
    validate: '验证',
    sendTestToMe: '向自己发送一封邮件，以此测试发件箱的有效性',
    validateBatch: '批量验证',
    validateAllUnverifiedInGroup: '批量验证当前组中所有未验证的邮箱',
    isConfirmValidateBatch: '是否验证当前组中所有未验证的邮箱？',
    validating: '验证中',
    validateBatchSuccess: '批量验证结束',
    validateFailed: '{email} 验证失败, 原因: {reason}',

    deleteInvalid: '删除无效',
    deleteCurrentGroupInvalidOutboxes: '删除当前组中验证失败的发件箱',
    deleteOutbox: '删除发件箱',
    isDeleteCurrentOutbox: '是否删除发件箱: {email}？',
    isDeleteSelectedOutboxes: '是否删除选中的 {count} 个发件箱？',
    deleteSuccess: '删除成功, 共删除 {count} 项',

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
    invalid: '无效',
    unsubscribed: '取消订阅',
    running: '运行中',
    unknown: '未知'
  }
}
