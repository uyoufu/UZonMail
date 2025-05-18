<template>
  <q-field v-model="fieldModelValue" tag="div" label="数据" dense @focus="isActive = true" @blur="isActive = false">
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
            删除当前数据
          </q-tooltip>
        </q-btn>

        <q-btn v-if="isActive" round dense flat icon="article" color="secondary" class="q-ml-sm"
          @click.prevent="onDownloadEmailDataTemplate">
          <q-tooltip>
            下载模板
          </q-tooltip>
        </q-btn>

        <q-btn round dense flat icon="add" class="q-ml-sm" @click.stop="onSelectExcel" color="grey-7">
          <q-tooltip>
            选择数据
          </q-tooltip>
        </q-btn>
      </div>
    </template>
  </q-field>
</template>

<script lang="ts" setup>
import { notifyError, notifySuccess } from 'src/utils/dialog'

// 模板数据
const modelValue = defineModel({
  type: Array,
  default: () => []
})

// 选择模板数据
import type { IExcelColumnMapper } from 'src/utils/file';
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
    notifyError('数据为空，请重新选择')
    return
  }

  // 检查数据中每条数据是否都有收件箱
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const hasInbox = data.every((item: Record<string, any>) => item.inbox && item.inbox.indexOf('@') > 0)
  if (!hasInbox) {
    console.log('exce data:', data)
    notifyError('请确保每条数据都有 inbox')
    return
  }

  const firstFile = files[0]
  fieldModelValue.value = `${firstFile!.name} - ${sheetName}`
  modelValue.value = data
}

// placeholder 显示
import { useCustomQField } from '../helper'
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
      headerName: 'inbox',
      fieldName: 'inbox',
      required: true
    },
    {
      headerName: 'inboxName',
      fieldName: 'inboxName'
    },
    {
      headerName: 'outbox',
      fieldName: 'outbox'
    },
    {
      headerName: 'outboxName',
      fieldName: 'outboxName'
    },
    {
      headerName: 'subject',
      fieldName: 'subject'
    },
    {
      headerName: 'body',
      fieldName: 'body'
    },
    {
      headerName: 'cc',
      fieldName: 'cc'
    },
    {
      headerName: 'bcc',
      fieldName: 'bcc'
    },
    {
      headerName: 'templateName',
      fieldName: 'templateName'
    },
    {
      headerName: 'templateId',
      fieldName: 'templateId'
    },
    {
      headerName: 'proxyId',
      fieldName: 'proxyId'
    },
    {
      headerName: 'attachmentNames',
      fieldName: 'attachmentNames'
    },
    {
      headerName: '自定义变量',
      fieldName: 'other'
    }
  ]
}
async function onDownloadEmailDataTemplate () {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const data: any[] = [
    {
      inbox: '收件箱(导入时，请删除该行数据)',
      inboxName: '收件人姓名(可选)',
      outbox: '发件箱(可选)',
      outboxName: '发件人姓名(可选)',
      subject: '主题(可选)',
      body: '内容(可选)',
      cc: '抄送(多个逗号分隔,可选)',
      bcc: '密送(多个逗号分隔,可选)',
      templateName: '模板名称(可选)',
      templateId: '模板id(可选)',
      proxy: '代理Id(可选)',
      other: '可以继续增加列，作为自定义字段(可选)',
      attachmentNames: '附件名称(多个逗号分隔,可选)'
    }
  ]
  await writeExcel(data, {
    fileName: '发件数据模板.xlsx',
    sheetName: '发件数据',
    mappers: getEmailSendingExcelDataMapper()
  })

  notifySuccess('模板下载成功')
}
</script>

<style lang="scss" scoped></style>
