<template>
  <q-dialog ref="dialogRef" @hide="onDialogHide">
    <q-card class="duplicate-recipient-dialog column">
      <q-card-section class="text-subtitle1">
        {{ translateSendingTask('duplicateRecipientsDialogTitle') }}
      </q-card-section>

      <q-table :rows="duplicateRecipients" :columns="columns" row-key="recipientEmail" dense flat hide-bottom />

      <q-card-actions align="right">
        <OkBtn :label="translateSendingTask('closeDuplicateRecipientsDialog')" @click="onCloseClick" />
      </q-card-actions>
    </q-card>
  </q-dialog>
</template>

<script lang="ts" setup>
import type { QTableColumn } from 'quasar'
import { useDialogPluginComponent } from 'quasar'
import OkBtn from 'src/components/quasarWrapper/buttons/OkBtn.vue'
import { translateSendingTask } from 'src/i18n/helpers'
import type { IDuplicateRecipientSummary } from './compositions/duplicateRecipient'

/** 展示上传 Excel 中重复收件人汇总信息的弹窗。 */
defineOptions({
  name: 'DuplicateRecipientDialog'
})

defineProps({
  duplicateRecipients: {
    type: Array as PropType<IDuplicateRecipientSummary[]>,
    default: () => []
  }
})

defineEmits([...useDialogPluginComponent.emits])
const { dialogRef, onDialogHide, onDialogOK } = useDialogPluginComponent()

const columns = computed<QTableColumn[]>(() => [
  {
    name: 'recipientName',
    label: translateSendingTask('duplicateRecipientName'),
    align: 'left',
    field: 'recipientName'
  },
  {
    name: 'recipientEmail',
    label: translateSendingTask('duplicateRecipientEmail'),
    align: 'left',
    field: 'recipientEmail'
  },
  {
    name: 'sendingCount',
    label: translateSendingTask('duplicateRecipientSendingCount'),
    align: 'right',
    field: 'sendingCount'
  }
])

/** 关闭只读的重复收件人详情弹窗。 */
function onCloseClick (): void {
  onDialogOK()
}
</script>

<style lang="scss" scoped>
.duplicate-recipient-dialog {
  min-width: 560px;
  max-width: min(92vw, 860px);
}
</style>
