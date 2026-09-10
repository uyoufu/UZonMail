<template>
  <q-dialog ref="dialogRef" @hide="onDialogHide">
    <q-card class="full-width q-ma-sm">
      <TitleBar :title="t('pages.receivingManagement.insertTemplate')" @close="onDialogCancel" />
      <q-scroll-area class="height-300">
        <q-list dense class="q-pa-sm">
          <q-item v-for="template in templates" :key="template.id" clickable class="border-radius-4"
            @click="onTemplateSelect(template.content)">
            <q-item-section>{{ template.name }}</q-item-section>
          </q-item>
          <q-item v-if="!isLoading && templates.length === 0" class="text-grey-7">
            <q-item-section>{{ t('pages.receivingManagement.empty') }}</q-item-section>
          </q-item>
        </q-list>
      </q-scroll-area>
      <q-inner-loading :showing="isLoading" color="primary" />
    </q-card>
  </q-dialog>
</template>

<script setup lang="ts">
import { useDialogPluginComponent } from 'quasar'
import TitleBar from 'src/components/windowLike/TitleBar.vue'
import { getEmailTemplatesData, type IEmailTemplate } from 'src/api/emailTemplate'
import { useI18n } from 'vue-i18n'

defineEmits([...useDialogPluginComponent.emits])

const { t } = useI18n()
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent()
const templates = ref<IEmailTemplate[]>([])
const isLoading = ref(false)

async function loadTemplates() {
  isLoading.value = true
  try {
    templates.value = (await getEmailTemplatesData(undefined, {
      sortBy: 'id',
      descending: true,
      skip: 0,
      limit: 100
    })).data
  } finally {
    isLoading.value = false
  }
}

function onTemplateSelect(content: string) {
  onDialogOK(content)
}

onMounted(() => void loadTemplates())
</script>
