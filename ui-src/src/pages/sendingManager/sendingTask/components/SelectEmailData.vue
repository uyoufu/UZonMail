<template>
  <q-field v-model="fieldModelValue" tag="div" :label="translateSendingTask('emailData')" dense @focus="isActive = true"
    @blur="isActive = false">
    <template v-slot:before>
      <q-icon name="analytics" color="primary" />
    </template>

    <template v-slot:control>
      <div class="full-width no-outline" @dblclick="onSelectExcel">
        <div class="text-grey-7"> {{ fieldText }}</div>
      </div>
    </template>

    <template v-slot:append>
      <div class="row justify-end">
        <q-btn v-if="fieldModelValue" round dense flat icon="close" color="negative"
          @click.prevent="onRemoveSelectedFile">
          <q-tooltip>
            {{ translateSendingTask('deleteCurrentData') }}
          </q-tooltip>
        </q-btn>

        <q-btn v-if="isActive" round dense flat icon="article" color="secondary" class="q-ml-sm"
          @click.prevent="onDownloadEmailDataTemplate">
          <q-tooltip>
            {{ translateSendingTask('downloadTemplate') }}
          </q-tooltip>
        </q-btn>

        <q-btn round dense flat icon="add" class="q-ml-sm" @click.stop="onSelectExcel" color="grey-7">
          <q-tooltip>
            {{ translateSendingTask('selectData') }}
          </q-tooltip>
        </q-btn>
      </div>
    </template>
  </q-field>
</template>

<script lang="ts" setup>
import { notifyError, notifySuccess } from 'src/utils/dialog'
import logger from 'loglevel'

// 模板数据
const modelValue = defineModel({
  type: Array,
  default: () => []
})

// 选择模板数据
import type { IExcelColumnMapper } from 'src/utils/file'
import { readExcelCore, writeExcel } from 'src/utils/file'
async function onSelectExcel () {
  const { files, sheetName, data } = await readExcelCore({
    sheetIndex: 0,
    selectSheet: true,
    numberDecimalCount: 5
  })
  if (!files || !data) return

  // 检查数据的有效性
  if (data.length === 0) {
    notifyError(translateSendingTask('dataIsEmpty'))
    return
  }

  logger.debug('[SelectEmailData] selected data:', data)
  // 检查数据中每条数据是否都有收件箱
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const emptyInboxRowIndex = data.findIndex((item: Record<string, any>) => !item.inbox || item.inbox.indexOf('@') <= 0)
  if (emptyInboxRowIndex > 0) {
    logger.warn('[SelectEmailData] selected data has empty inbox:', emptyInboxRowIndex + 2, data[emptyInboxRowIndex])
    notifyError(translateSendingTask('inboxEmptyOrInvalidAtRow', { row: emptyInboxRowIndex + 2 }))
    return
  }

  const firstFile = files[0]
  fieldModelValue.value = `${firstFile!.name} - ${sheetName}`
  modelValue.value = data
}

// placeholder 显示
import { useCustomQField } from '../helper'
import { translateInboxManager, translateOutboxManager, translateProxy, translateSendingTask, translateTemplate } from 'src/i18n/helpers'
const { isActive, fieldModelValue, fieldText } = useCustomQField('请选择数据 (该项可为空)')

// 删除选择的数据
function onRemoveSelectedFile () {
  fieldModelValue.value = ''
  modelValue.value = []
}

// 下载模板：下载邮件数据模板
function getEmailSendingExcelDataMapper (): IExcelColumnMapper[] {
  return [
    {
      headerName: translateInboxManager('col_inbox'),
      fieldName: 'inbox',
      required: true
    },
    {
      headerName: translateInboxManager('col_inboxName'),
      fieldName: 'inboxName'
    },
    {
      headerName: translateOutboxManager('col_outbox'),
      fieldName: 'outbox'
    },
    {
      headerName: translateOutboxManager('col_outboxName'),
      fieldName: 'outboxName'
    },
    {
      headerName: translateSendingTask('subject'),
      fieldName: 'subject'
    },
    {
      headerName: translateSendingTask('body'),
      fieldName: 'body'
    },
    {
      headerName: translateSendingTask('ccRecipients'),
      fieldName: 'cc'
    },
    {
      headerName: translateSendingTask('bccRecipients'),
      fieldName: 'bcc'
    },
    {
      headerName: translateTemplate('templateName'),
      fieldName: 'templateName'
    },
    {
      headerName: translateTemplate('templateId'),
      fieldName: 'templateId'
    },
    {
      headerName: translateProxy('proxyId'),
      fieldName: 'proxyId'
    },
    {
      headerName: translateSendingTask('attachments'),
      fieldName: 'attachmentNames'
    },
    {
      headerName: translateSendingTask('customVariables'),
      fieldName: 'other'
    }
  ]
}
async function onDownloadEmailDataTemplate () {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const data: any[] = [
    {
      inbox: translateSendingTask('example_inbox'),
      inboxName: translateSendingTask('example_inboxName'),
      outbox: translateSendingTask('example_outbox'),
      outboxName: translateSendingTask('example_outboxName'),
      subject: translateSendingTask('example_subject'),
      body: translateSendingTask('example_body'),
      cc: translateSendingTask('example_cc'),
      bcc: translateSendingTask('example_bcc'),
      templateName: translateSendingTask('example_templateName'),
      templateId: translateSendingTask('example_templateId'),
      proxy: translateSendingTask('example_proxy'),
      other: translateSendingTask('example_other'),
      attachmentNames: translateSendingTask('example_attachmentNames')
    }
  ]
  await writeExcel(data, {
    fileName: translateSendingTask('sendingDataTemplateFileName'),
    sheetName: translateSendingTask('sendingDataTemplateSheetName'),
    mappers: getEmailSendingExcelDataMapper()
  })

  notifySuccess(translateSendingTask('templateDownloadSuccess'))
}
</script>

<style lang="scss" scoped></style>
