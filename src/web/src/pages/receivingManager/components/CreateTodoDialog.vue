<template>
  <q-dialog ref="dialogRef" @hide="onDialogHide">
    <q-card class="full-width q-ma-sm">
      <TitleBar :title="t('pages.receivingManagement.createTodo')" @close="onDialogCancel" />
      <q-card-section class="q-pa-sm q-gutter-sm">
        <q-input v-model="todoTitle" dense outlined hide-bottom-space :label="t('pages.todoManagement.title')" />
        <q-input v-model="todoDescription" dense outlined hide-bottom-space type="textarea" autogrow
          :label="t('pages.todoManagement.description')" />
        <q-input v-model="todoDueAt" dense outlined hide-bottom-space type="datetime-local" :label="t('pages.todoManagement.dueAt')" />
        <div class="row justify-end">
          <CommonBtn icon="add_task" :label="t('common.create')" :disable="!todoTitle.trim()" @click="onCreate" />
        </div>
      </q-card-section>
    </q-card>
  </q-dialog>
</template>

<script setup lang="ts">
import { useDialogPluginComponent } from 'quasar'
import CommonBtn from 'src/components/buttons/CommonBtn.vue'
import TitleBar from 'src/components/windowLike/TitleBar.vue'
import { createMailTodoTask, TodoTaskPriority, TodoTaskStatus } from 'src/api/todoTask'
import type { IMailConversation } from 'src/api/mailConversation'
import { useI18n } from 'vue-i18n'

const props = defineProps<{
  conversation: IMailConversation
  messageIds: number[]
}>()
defineEmits([...useDialogPluginComponent.emits])

const { t } = useI18n()
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent()
const todoTitle = ref(props.conversation.displayTitle)
const todoDescription = ref('')
const todoDueAt = ref('')

async function onCreate() {
  if (!todoTitle.value.trim()) return
  await createMailTodoTask({
    title: todoTitle.value.trim(),
    description: todoDescription.value,
    status: TodoTaskStatus.Pending,
    priority: TodoTaskPriority.Normal,
    dueAtUtc: todoDueAt.value ? new Date(todoDueAt.value).toISOString() : undefined,
    sourceConversationId: props.conversation.id,
    sourceMessageIds: props.messageIds,
    branchSubject: todoTitle.value.trim()
  })
  onDialogOK()
}
</script>
