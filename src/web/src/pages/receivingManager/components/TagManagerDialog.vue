<template>
  <q-dialog ref="dialogRef" @hide="onDialogHide">
    <q-card class="full-width q-ma-sm">
      <TitleBar :title="t('pages.receivingManagement.tags')" @close="onDialogCancel" />
      <q-card-section class="q-pa-sm q-gutter-sm">
        <div class="row items-center q-gutter-sm">
          <q-input v-model="newTagName" dense outlined hide-bottom-space class="col" :label="t('pages.receivingManagement.newTag')" />
          <input v-model="newTagColor" type="color" class="width-200" :aria-label="t('pages.receivingManagement.tagColor')" />
          <CommonBtn icon="add" flat :tooltip="t('pages.receivingManagement.addTag')" @click="onCreateTag" />
        </div>
        <q-scroll-area class="height-300">
          <div v-for="contact in conversation.participants" :key="contact.id" class="q-py-sm">
            <div class="text-body2 text-weight-medium q-mb-xs">{{ contact.displayName || contact.email }}</div>
            <q-option-group v-model="contactTagSelection[contact.id]" :options="tagOptions" type="checkbox" color="primary" />
          </div>
        </q-scroll-area>
        <div class="row justify-end">
          <CommonBtn icon="save" :label="t('common.save')" @click="onSave" />
        </div>
      </q-card-section>
    </q-card>
  </q-dialog>
</template>

<script setup lang="ts">
import { useDialogPluginComponent } from 'quasar'
import CommonBtn from 'src/components/buttons/CommonBtn.vue'
import TitleBar from 'src/components/windowLike/TitleBar.vue'
import { createMailTag, getMailTags, setMailContactTags, type IMailConversation, type IMailTag } from 'src/api/mailConversation'
import { useI18n } from 'vue-i18n'

const props = defineProps<{ conversation: IMailConversation }>()
defineEmits([...useDialogPluginComponent.emits])

const { t } = useI18n()
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent()
const tags = ref<IMailTag[]>([])
const contactTagSelection = ref<Record<number, number[]>>({})
const newTagName = ref('')
const newTagColor = ref('#1976d2')
const tagOptions = computed(() => tags.value.map(tag => ({ label: tag.name, value: tag.id, color: tag.color })))

async function loadTags() {
  tags.value = (await getMailTags()).data
  contactTagSelection.value = Object.fromEntries(
    props.conversation.participants.map(contact => [contact.id, contact.tags.map(tag => tag.id)])
  )
}

async function onCreateTag() {
  if (!newTagName.value.trim()) return
  tags.value.push((await createMailTag(newTagName.value.trim(), newTagColor.value)).data)
  newTagName.value = ''
}

async function onSave() {
  for (const contact of props.conversation.participants) {
    await setMailContactTags(contact.id, contactTagSelection.value[contact.id] || [])
  }
  onDialogOK()
}

onMounted(() => void loadTags())
</script>
