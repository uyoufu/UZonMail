<template>
  <q-dialog ref='dialogRef' @hide="onDialogHide">
    <q-card class='column justify-start q-pa-sm width-800 height-500'>
      <div class="text-subtitle1 text-primary">
        主题：{{ emailSubject }}
      </div>

      <div class="text-secondary">
        收件人: {{ currentInbox }}
      </div>

      <q-separator class="q-mb-sm" />

      <div class="q-pb-md col hover-scroll full-width" v-html="emailBody"></div>

      <q-pagination v-if="pagesCount > 1" class="self-center q-pt-sm" v-model="currentPage" :max="pagesCount"
        :max-pages="6" boundary-numbers size="md" padding="0px" />
    </q-card>
  </q-dialog>
</template>

<script lang='ts' setup>
// 预览邮件发件正文
// 为了与后端结果保持一致，预览的变量替换部分在后端处理

/**
 * warning: 该组件一个弹窗的示例，不可直接使用
 * 参考：http://www.quasarchs.com/quasar-plugins/dialog#composition-api-variant
 */

import { useDialogPluginComponent } from 'quasar'
defineEmits([
  // 必需；需要指定一些事件
  // （组件将通过useDialogPluginComponent()发出）
  ...useDialogPluginComponent.emits
])
const { dialogRef, onDialogHide /* , onDialogOK, onDialogCancel */ } = useDialogPluginComponent()

import { IEmailCreateInfo, ISendingItemPreview, previewSendingItem } from 'src/api/emailSending'
const props = defineProps({
  emailCreateInfo: {
    type: Object as PropType<IEmailCreateInfo>,
    required: true
  }
})

// 分页
const pagesCount = ref(0)
const currentPage = ref(1)

// #region 预览邮件发件正文逻辑
// 若用户数据中有收件箱，则要与收件箱中的数据进行合并
let inboxes = [...props.emailCreateInfo.inboxes]
console.log('emailCreateInfo.data', props.emailCreateInfo.data)
const userDataInboxes = props.emailCreateInfo.data.map((item) => item.inbox)
  .filter(x => x)
if (userDataInboxes.length > 0) {
  inboxes.push(...userDataInboxes.map(x => ({ email: x } as IInbox)))
}
// 去重
inboxes = Array.from(new Set(inboxes))
pagesCount.value = inboxes.length

import { useInstanceRequestCache } from 'src/api/base/httpCache'
import { getEmailTemplateById, getEmailTemplateByIdOrName } from 'src/api/emailTemplate'
import { IInbox } from 'src/api/emailBox';
const cacheKey = useInstanceRequestCache()
// 主题
function getSubjects () {
  // 通过分号，换行符进行分隔
  // 先将所有的 ; 和 ； 替换为 \n
  const regex = /;|；/gm
  const subject = props.emailCreateInfo.subjects.replace(regex, '\n')
  return subject.split('\n').filter(x => x)
}
const subjects = getSubjects()
// eslint-disable-next-line @typescript-eslint/no-explicit-any
function applyVariablesToTemplate (userData?: Record<string, any>, templateContent?: string) {
  if (!templateContent || !userData) return templateContent

  // 应用变量
  for (const key of Object.keys(userData)) {
    const regex = new RegExp(`{{\\s*${key}\\s*}}`, 'gm')
    templateContent = templateContent.replace(regex, userData[key])
  }
  return templateContent
}

async function getEmailBody (inbox: string, inboxIndex: number) {
  // 正文优先级：用户数据/正文 > 用户数据/模板 > 界面/正文 > 界面/模板
  const userData = props.emailCreateInfo.data.find((item) => item.inbox === inbox)
  let templateContent = ''
  if (userData) {
    // 查找模板
    // 若数据中有模板，则使用数据模板
    if (userData.body) {
      templateContent = userData.body
    } else if (userData.templateId || userData.templateName) {
      const { data } = await getEmailTemplateByIdOrName(userData.templateId, userData.templateName, cacheKey)
      templateContent = data.content
    }
  }

  // 说明没有变量，直接从界面正文中获取
  if (!templateContent) templateContent = props.emailCreateInfo.body

  if (!templateContent && props.emailCreateInfo.templates.length > 0) {
    // 从模板中读取
    // 查找模板
    const templateIndex = inboxIndex % props.emailCreateInfo.templates.length
    const template = props.emailCreateInfo.templates[templateIndex]
    const { data } = await getEmailTemplateById(template!.id as number, cacheKey)

    // 返回模板数据
    templateContent = data.content
  }

  return applyVariablesToTemplate(userData, templateContent) || '正文为空'
}

const currentInbox = ref('')
const emailBody = ref('')
const emailSubject = ref('')
watch(currentPage, async () => {
  const index = currentPage.value - 1
  const inbox = inboxes[index]!.email as string

  // subject 优先级：Excel数据/subject  > 界面/主题
  const userData = props.emailCreateInfo.data.find((x) => x.inbox === inbox)
  if (userData && userData.subject) emailSubject.value = userData.subject
  else {
    emailSubject.value = subjects[index % subjects.length] as string
  }
  if (userData && userData.inbox) currentInbox.value = userData.inbox
  else currentInbox.value = inbox

  const body = await getEmailBody(currentInbox.value, index)
  emailBody.value = body || ''

  // 从服务器解析变量
  const previewData: ISendingItemPreview = {
    inbox,
    subject: emailSubject.value,
    body: emailBody.value,
    data: userData || {},
    // cc 与 bcc 暂不适配
    ccBoxes: [],
    bccBoxes: []
  }
  const { data: previewResult } = await previewSendingItem(previewData)
  emailSubject.value = previewResult.subject
  emailBody.value = previewResult.body
}, {
  immediate: true
})
// #endregion
</script>

<style lang='scss' scoped></style>
