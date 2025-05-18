<template>
  <q-dialog ref='dialogRef' @hide="onDialogHide" :persistent="true">
    <div class="column card-like">
      <q-table class="col" :rows="rows" row-key="id" virtual-scroll v-model:pagination="pagination" dense title="选择发件模板"
        hide-header grid :loading="loading" :filter="filter" binary-state-sort @request="onTableRequest"
        selection="multiple" v-model:selected="selected">
        <template v-slot:top-right>
          <SearchInput v-model="filter" />
        </template>

        <template v-slot:item="props">
          <div class="rounded-borders q-ma-sm hover-card" style="height: 100px; width: 200px"
            @click="onItemClick(props.row)">
            <q-img :src="getTemplateImage(props.row)" error-src="/icons/undraw_mailbox_re_dvds.svg"
              spinner-color="white" class="full-width full-height" fit="cover" position="left top">

              <template v-slot:loading>
                <q-spinner-gears color="white" />
              </template>

              <template v-slot:error>
                <div class="absolute-bottom row items-center justify-center select-template__image-title">
                  <div class="text-subtitle1 q-mr-sm">
                    {{ props.row.name }}
                  </div>
                  <div class="text-secondary">ID:{{ props.row.id }}</div>
                </div>

                <div v-if="isSelected(props.row)" class="absolute-full flex flex-center bg-transparent">
                  <q-icon name="done" color="positive" size="lg"></q-icon>
                </div>
              </template>

              <div class="absolute-bottom row items-center justify-center select-template__image-title">
                <div class="text-subtitle1 q-mr-sm">
                  {{ props.row.name }}
                </div>
                <div class="text-secondary">ID:{{ props.row.id }}</div>
              </div>

              <div v-if="isSelected(props.row)" class="absolute-full flex flex-center bg-transparent">
                <q-icon name="done" color="positive" size="lg"></q-icon>
              </div>
            </q-img>
          </div>
        </template>
      </q-table>

      <div class="row justify-end q-ma-sm">
        <CancelBtn class="q-mr-sm" @click="onDialogCancel" />
        <OkBtn tooltip="确认选择" @click="onOkBtnClick" />
      </div>
    </div>
  </q-dialog>
</template>

<script lang='ts' setup>
/**
 * warning: 该组件一个弹窗的示例，不可直接使用
 * 参考：http://www.quasarchs.com/quasar-plugins/dialog#composition-api-variant
 */
import { IEmailTemplate } from 'src/api/emailTemplate'
const props = defineProps({
  initTemplates: {
    type: Array as PropType<IEmailTemplate[]>,
    default: () => []
  }
})

import { useDialogPluginComponent } from 'quasar'
defineEmits([
  // 必需；需要指定一些事件
  // （组件将通过useDialogPluginComponent()发出）
  ...useDialogPluginComponent.emits
])

const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent<IEmailTemplate[]>()

// 模板接口
import { useEmailTemplateTable } from 'pages/templateManager/compositions'
import SearchInput from 'src/components/searchInput/SearchInput.vue'
const { pagination, rows, filter, onTableRequest, loading, getTemplateImage } = useEmailTemplateTable()

// 选择结果
const selected: Ref<IEmailTemplate[]> = ref(props.initTemplates)
function onItemClick (row: IEmailTemplate) {
  if (selected.value.some(item => item.id === row.id)) {
    selected.value = selected.value.filter(item => item.id !== row.id)
  } else {
    selected.value.push(row)
  }
}
function isSelected (row: IEmailTemplate) {
  return selected.value.some(item => item.id === row.id)
}

// 确定和取消
import OkBtn from 'src/components/quasarWrapper/buttons/OkBtn.vue'
import CancelBtn from 'src/components/quasarWrapper/buttons/CancelBtn.vue'
function onOkBtnClick () {
  // 单击确认
  onDialogOK(selected.value)
}
</script>

<style lang='scss' scoped>
.select-template__image-title {
  padding: 4px !important;
}

:deep(.q-table__grid-content) {
  overflow-y: scroll;
}
</style>
