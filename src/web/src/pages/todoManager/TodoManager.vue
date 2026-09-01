<template>
  <div class="todo-workspace">
    <aside class="todo-list-pane">
      <div class="todo-toolbar"><div class="text-subtitle1 text-weight-medium col">{{ t('pages.todoManagement.title') }}</div><CommonBtn icon="add" :tooltip="t('pages.todoManagement.add')" @click="onNewTask" /></div>
      <q-tabs v-model="statusFilter" dense align="left" active-color="primary" indicator-color="primary"><q-tab name="active" :label="t('pages.todoManagement.active')" /><q-tab name="completed" :label="t('pages.todoManagement.completed')" /></q-tabs>
      <q-list separator class="todo-list">
        <q-item v-for="task in filteredTasks" :key="task.id" clickable :active="task.id === selectedTask?.id" active-class="bg-blue-1 text-primary" @click="selectTask(task)">
          <q-item-section avatar><q-icon :name="task.kind === TodoTaskKind.MailFollowUp ? 'forward_to_inbox' : 'task_alt'" :color="priorityColor(task.priority)" /></q-item-section>
          <q-item-section><q-item-label class="text-weight-medium" lines="2">{{ task.title }}</q-item-label><q-item-label caption>{{ task.dueAtUtc ? formatDate(task.dueAtUtc) : t('pages.todoManagement.noDueDate') }}</q-item-label></q-item-section>
          <q-item-section side><q-badge outline :color="priorityColor(task.priority)">{{ priorityLabel(task.priority) }}</q-badge></q-item-section>
        </q-item>
      </q-list>
    </aside>

    <main v-if="selectedTask?.mailBranch" class="branch-pane">
      <header class="section-header"><div class="text-subtitle1 text-weight-medium ellipsis">{{ selectedTask.mailBranch.branchSubject }}</div><div class="text-caption text-grey-7">{{ t('pages.todoManagement.independentThread') }}</div></header>
      <q-scroll-area class="branch-scroll"><div class="branch-stream">
        <article v-for="message in selectedTask.mailBranch.messages" :key="message.id" class="branch-message" :class="message.direction === MailMessageDirection.Outgoing ? 'outgoing' : ''">
          <div class="branch-bubble" @click="expandedMessageIds.add(message.id)"><div class="row justify-between no-wrap q-gutter-sm"><strong class="ellipsis">{{ message.subject }}</strong><span class="text-caption">{{ formatDate(message.occurredAtUtc) }}</span></div><MailBody v-if="expandedMessageIds.has(message.id)" :message-id="message.id" class="q-mt-sm" /><div v-else class="text-caption text-grey-7 q-mt-xs"><q-icon name="expand_more" /> {{ t('pages.receivingManagement.expand') }}</div></div>
        </article>
        <div v-if="selectedTask.mailBranch.messages.length === 0" class="branch-empty"><q-icon name="call_split" size="40px" /><span>{{ t('pages.todoManagement.newThreadReady') }}</span></div>
      </div></q-scroll-area>
      <section class="branch-composer"><q-input v-model="mailSubject" dense outlined :label="t('pages.receivingManagement.subject')" class="q-mb-sm" /><q-editor v-model="mailBody" min-height="100px" :placeholder="t('pages.receivingManagement.writeReply')" :toolbar="editorToolbar" /><div class="row justify-end q-mt-sm"><CommonBtn icon="send" :label="t('pages.receivingManagement.send')" :loading="sending" @click="onSendBranchMail" /></div></section>
    </main>

    <section v-if="selectedTask" class="detail-pane">
      <header class="section-header row items-center"><div class="text-subtitle1 text-weight-medium col">{{ t('pages.todoManagement.details') }}</div><CommonBtn icon="delete" flat color="negative" :tooltip="t('common.delete')" @click="onDelete" /></header>
      <div class="detail-form q-gutter-md">
        <q-input v-model="taskDraft.title" outlined dense :label="t('pages.todoManagement.taskTitle')" />
        <q-input v-model="taskDraft.description" outlined dense type="textarea" autogrow :label="t('pages.todoManagement.description')" />
        <q-select v-model="taskDraft.status" outlined dense emit-value map-options :label="t('pages.todoManagement.status')" :options="statusOptions" />
        <q-select v-model="taskDraft.priority" outlined dense emit-value map-options :label="t('pages.todoManagement.priority')" :options="priorityOptions" />
        <q-input v-model="taskDraft.dueAtLocal" outlined dense type="datetime-local" :label="t('pages.todoManagement.dueAt')" />
        <div v-if="selectedTask.mailBranch" class="source-info"><div class="text-caption text-grey-7">{{ t('pages.todoManagement.sourceMessages') }}</div><div>{{ selectedTask.mailBranch.sourceMessageIds.length }}</div></div>
        <CommonBtn icon="save" class="full-width" :label="t('common.save')" @click="onSave" />
      </div>
    </section>
    <div v-else class="empty-state"><q-icon name="checklist" size="48px" /><span>{{ t('pages.todoManagement.selectTask') }}</span></div>

    <q-dialog v-model="showCreateDialog"><q-card class="dialog-card"><q-card-section class="text-subtitle1">{{ t('pages.todoManagement.add') }}</q-card-section><q-card-section class="q-gutter-md"><q-input v-model="newTask.title" outlined dense autofocus :label="t('pages.todoManagement.taskTitle')" /><q-input v-model="newTask.description" outlined dense type="textarea" :label="t('pages.todoManagement.description')" /><q-select v-model="newTask.priority" outlined dense emit-value map-options :label="t('pages.todoManagement.priority')" :options="priorityOptions" /><q-input v-model="newTask.dueAtLocal" outlined dense type="datetime-local" :label="t('pages.todoManagement.dueAt')" /></q-card-section><q-card-actions align="right"><CommonBtn icon="add_task" :label="t('common.create')" @click="onCreate" /></q-card-actions></q-card></q-dialog>
  </div>
</template>

<script setup lang="ts">
import dayjs from 'dayjs'
import CommonBtn from 'src/components/buttons/CommonBtn.vue'
import MailBody from 'src/pages/receivingManager/components/MailBody.vue'
import { MailMessageDirection, MailReplyMode } from 'src/api/mailConversation'
import { createTodoTask, deleteTodoTask, getTodoTasks, sendTodoMailMessage, TodoTaskKind, TodoTaskPriority, TodoTaskStatus, updateTodoTask, type ITodoTask } from 'src/api/todoTask'
import { confirmOperation, notifyError, notifySuccess } from 'src/utils/dialog'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()
const tasks = ref<ITodoTask[]>([])
const selectedTask = ref<ITodoTask>()
const statusFilter = ref<'active' | 'completed'>('active')
const expandedMessageIds = ref(new Set<number>())
const sending = ref(false)
const mailSubject = ref('')
const mailBody = ref('')
const showCreateDialog = ref(false)
const editorToolbar = [['bold', 'italic', 'underline'], ['unordered', 'ordered'], ['link'], ['undo', 'redo']]
const createDraft = () => ({ title: '', description: '', status: TodoTaskStatus.Pending, priority: TodoTaskPriority.Normal, dueAtLocal: '' })
const taskDraft = ref(createDraft())
const newTask = ref(createDraft())
const filteredTasks = computed(() => tasks.value.filter(task => statusFilter.value === 'completed' ? task.status === TodoTaskStatus.Completed : task.status !== TodoTaskStatus.Completed && task.status !== TodoTaskStatus.Archived))
const statusOptions = computed(() => [{ label: t('pages.todoManagement.pending'), value: TodoTaskStatus.Pending }, { label: t('pages.todoManagement.inProgress'), value: TodoTaskStatus.InProgress }, { label: t('pages.todoManagement.completed'), value: TodoTaskStatus.Completed }, { label: t('pages.todoManagement.archived'), value: TodoTaskStatus.Archived }])
const priorityOptions = computed(() => [TodoTaskPriority.Low, TodoTaskPriority.Normal, TodoTaskPriority.High, TodoTaskPriority.Urgent].map(value => ({ label: priorityLabel(value), value })))

function selectTask (task: ITodoTask) {
  selectedTask.value = task; expandedMessageIds.value = new Set(); taskDraft.value = { title: task.title, description: task.description || '', status: task.status, priority: task.priority, dueAtLocal: task.dueAtUtc ? dayjs(task.dueAtUtc).format('YYYY-MM-DDTHH:mm') : '' }; mailSubject.value = task.mailBranch?.branchSubject || task.title; mailBody.value = ''
}
async function loadTasks (selectedId?: number) { tasks.value = (await getTodoTasks()).data; const target = tasks.value.find(task => task.id === (selectedId || selectedTask.value?.id)) || tasks.value[0]; if (target) selectTask(target); else selectedTask.value = undefined }
function onNewTask () { newTask.value = createDraft(); showCreateDialog.value = true }
async function onCreate () { if (!newTask.value.title.trim()) return; const result = await createTodoTask(toRequest(newTask.value)); showCreateDialog.value = false; await loadTasks(result.data.id); notifySuccess(t('pages.todoManagement.created')) }
async function onSave () { if (!selectedTask.value || !taskDraft.value.title.trim()) return; await updateTodoTask(selectedTask.value.id, toRequest(taskDraft.value)); await loadTasks(selectedTask.value.id); notifySuccess(t('pages.todoManagement.saved')) }
async function onDelete () { if (!selectedTask.value || !await confirmOperation(t('common.delete'), t('pages.todoManagement.deleteConfirm'))) return; await deleteTodoTask(selectedTask.value.id); selectedTask.value = undefined; await loadTasks() }
async function onSendBranchMail () {
  if (!selectedTask.value?.mailBranch || !mailSubject.value.trim() || !mailBody.value.trim()) { notifyError(t('pages.receivingManagement.completeReply')); return }
  sending.value = true
  try { await sendTodoMailMessage(selectedTask.value.id, { replyMode: MailReplyMode.ReplyAll, subject: mailSubject.value, htmlBody: mailBody.value, attachmentFileUsageIds: [] }); mailBody.value = ''; await loadTasks(selectedTask.value.id); notifySuccess(t('pages.receivingManagement.sent')) } finally { sending.value = false }
}
function toRequest (draft: ReturnType<typeof createDraft>) { return { title: draft.title, description: draft.description, status: draft.status, priority: draft.priority, dueAtUtc: draft.dueAtLocal ? new Date(draft.dueAtLocal).toISOString() : undefined } }
function formatDate (date: string) { return dayjs(date).format('YYYY-MM-DD HH:mm') }
function priorityColor (priority: TodoTaskPriority) { return ({ [TodoTaskPriority.Low]: 'grey-7', [TodoTaskPriority.Normal]: 'primary', [TodoTaskPriority.High]: 'orange-8', [TodoTaskPriority.Urgent]: 'negative' })[priority] }
function priorityLabel (priority: TodoTaskPriority) { return t(`pages.todoManagement.priority${TodoTaskPriority[priority]}`) }
onMounted(() => loadTasks())
</script>

<style scoped lang="scss">
.todo-workspace { height: calc(100vh - 98px); min-height: 540px; display: grid; grid-template-columns: 300px minmax(360px, 1fr) 320px; border: 1px solid $grey-4; background: white; overflow: hidden; }
.todo-list-pane,.branch-pane,.detail-pane { min-width: 0; min-height: 0; display: flex; flex-direction: column; border-right: 1px solid $grey-4; }
.detail-pane { border-right: 0; }
.todo-toolbar,.section-header { min-height: 58px; padding: 10px 14px; border-bottom: 1px solid $grey-4; }
.todo-toolbar { display: flex; align-items: center; }
.todo-list { flex: 1; overflow: auto; }
.branch-scroll { flex: 1; min-height: 0; background: $grey-2; }
.branch-stream { padding: 16px; display: flex; flex-direction: column; gap: 12px; }
.branch-message { max-width: 88%; }.branch-message.outgoing { align-self: flex-end; }.branch-bubble { padding: 12px; border: 1px solid $grey-4; border-radius: 6px; background: white; cursor: pointer; }.outgoing .branch-bubble { background: #e7f6ea; border-color: #b8dfc0; }
.branch-composer { padding: 10px 12px; border-top: 1px solid $grey-4; }.branch-empty,.empty-state { min-height: 240px; display: flex; flex-direction: column; gap: 10px; align-items: center; justify-content: center; color: $grey-7; }.empty-state { grid-column: 2 / 4; }.detail-form { padding: 16px; overflow: auto; }.source-info { display: flex; justify-content: space-between; padding: 10px 0; border-bottom: 1px solid $grey-4; }.dialog-card { width: min(520px, 92vw); border-radius: 6px; }
@media (max-width: 1199px) { .todo-workspace { grid-template-columns: 280px minmax(340px, 1fr); }.detail-pane { position: absolute; right: 16px; top: 82px; bottom: 16px; width: 320px; background: white; box-shadow: 0 4px 18px rgba(0,0,0,.14); z-index: 2; } }
@media (max-width: 767px) { .todo-workspace { grid-template-columns: 180px 1fr; height: calc(100vh - 82px); }.detail-pane { width: min(320px, 86vw); }.branch-composer { padding: 8px; } }
</style>
