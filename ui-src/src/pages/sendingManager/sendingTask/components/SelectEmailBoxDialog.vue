<template>
  <q-dialog ref='dialogRef' @hide="onDialogHide" :persistent='true'>
    <q-card class='column no-wrap q-pa-sm height-0 select-email-box-dialog'>
      <div class="col row items-start">
        <EmailGroupList v-show="!isCollapseGroupList" v-model="emailGroupRef" :extra-items="categoryTopItems" readonly
          :groupType="groupType" selectable class="q-mr-sm card-like full-height" v-model:selected="selectedGroups" />

        <q-table class="col full-height" :rows="rows" :columns="columns" row-key="id" virtual-scroll
          v-model:pagination="pagination" dense :loading="loading" :filter="filter" binary-state-sort
          @request="onTableRequest" selection="multiple" v-model:selected="selectedEmails">
          <template v-slot:top-left>
            <div class="row justify-start q-gutter-sm">
              <CreateBtn v-if="showNewTempInboxBtn" tooltip="新增临时收件箱" @click="onNewTempInboxClick" />
            </div>
          </template>

          <template v-slot:top-right>
            <SearchInput v-model="filter" />
          </template>

          <template v-slot:body-cell-index="props">
            <QTableIndex :props="props" />
          </template>
        </q-table>

        <CollapseLeft v-model="isCollapseGroupList" :style="collapseStyleRef" />
      </div>

      <div class="row justify-end items-center q-mt-md">
        <CancelBtn class="q-mr-sm" @click="onDialogCancel" />
        <OkBtn @click="onOKClick" />
      </div>
    </q-card>
  </q-dialog>
</template>

<script lang='ts' setup>
import { IInbox, getInboxesCount, getInboxesData, getOutboxesCount, getOutboxesData, createUngroupedInbox } from 'src/api/emailBox'
import { IEmailGroupListItem } from 'src/pages/emailManager/components/types'
// props 定义
const props = defineProps({
  emailBoxType: {
    // 0-发件箱 1-收件箱
    type: Number as PropType<0 | 1>,
    default: 0
  },
  initEmailBoxes: {
    type: Array as PropType<IInbox[]>,
    default: () => []
  },
  initEmailGroups: {
    type: Array as PropType<IEmailGroupListItem[]>,
    default: () => []
  }
})
const { emailBoxType } = toRefs(props)
const groupType = computed(() => {
  return emailBoxType.value + 1 as 1 | 2
})

/**
 * warning: 该组件一个弹窗的示例，不可直接使用
 * 参考：http://www.quasarchs.com/quasar-plugins/dialog#composition-api-variant
 */

import { QTable, QTableColumn, useDialogPluginComponent } from 'quasar'
defineEmits([
  // 必需；需要指定一些事件
  // （组件将通过useDialogPluginComponent()发出）
  ...useDialogPluginComponent.emits
])
const { dialogRef, onDialogHide, onDialogOK, onDialogCancel } = useDialogPluginComponent()

// 分类列表
import EmailGroupList from 'pages/emailManager/components/EmailGroupList.vue'

const emailGroupRef: Ref<IEmailGroupListItem> = ref({
  name: 'selected',
  order: 0,
  label: '已选中'
})
const showNewTempInboxBtn = computed(() => {
  return emailBoxType.value === 1 && emailGroupRef.value.name === 'selected'
})
const categoryTopItems: Ref<IEmailGroupListItem[]> = ref([
  {
    name: 'selected',
    order: -1,
    icon: 'task_alt',
    label: '已选邮箱',
    selectable: false
  }
])

// 表格定义与数据请求
import { useQTable, useQTableIndex } from 'src/compositions/qTableUtils'
const { indexColumn, QTableIndex } = useQTableIndex()
const columns: QTableColumn[] = [
  indexColumn,
  {
    name: 'email',
    required: true,
    label: '邮箱',
    align: 'left',
    field: 'email',
    sortable: true
  },
  {
    name: 'description',
    required: true,
    label: '描述',
    align: 'left',
    field: 'description',
    sortable: true
  }
]
import { IRequestPagination, TTableFilterObject } from 'src/compositions/types'
// 选择结果
const selectedEmails: Ref<IInbox[]> = ref([])
// 更新选择结果
selectedEmails.value.push(...props.initEmailBoxes)
const selectedGroups: Ref<IEmailGroupListItem[]> = ref([])
selectedGroups.value.push(...props.initEmailGroups)

// 若是选择已选中，需要单独处理
async function getRowsNumberCount (filterObj: TTableFilterObject) {
  if (emailGroupRef.value.name === 'selected') {
    if (!filterObj.filter) return selectedEmails.value.length
    const regex = new RegExp(filterObj.filter, 'i')
    return selectedEmails.value.filter(x => x.email.match(regex)).length
  }
  if (props.emailBoxType === 0) {
    const { data } = await getOutboxesCount(emailGroupRef.value.id, filterObj.filter)
    return data
  }

  const { data } = await getInboxesCount(emailGroupRef.value.id, filterObj.filter)
  return data
}
async function onRequest (filterObj: TTableFilterObject, pagination: IRequestPagination) {
  if (emailGroupRef.value.name === 'selected') {
    let results = selectedEmails.value
    if (filterObj.filter) {

      const regex = new RegExp(filterObj.filter, 'i')
      results = results.filter(x => x.email.match(regex))
    }

    // 排序并分页
    return results.slice(pagination.skip, pagination.skip + pagination.limit)
  }

  if (props.emailBoxType === 0) {
    const { data } = await getOutboxesData(emailGroupRef.value.id, filterObj.filter, pagination)
    return data
  }

  const { data } = await getInboxesData(emailGroupRef.value.id, filterObj.filter, pagination)
  return data
}
const { pagination, rows, filter, onTableRequest, loading, refreshTable, addNewRow } = useQTable({
  getRowsNumberCount,

  onRequest
})
watch(emailGroupRef, () => {
  // 组切换时，触发更新
  refreshTable()
})

// 新增收件箱
import { showNewInboxDialog } from 'src/pages/emailManager/inbox/headerFunctions'
import { notifyError } from 'src/utils/dialog'

async function onNewTempInboxClick () {
  // 打开输入框
  const { ok, data } = await showNewInboxDialog('临时收件')
  if (!ok) return

  // 若在选择集中，提示错误
  if (selectedEmails.value.some(x => x.email === data.email)) {
    notifyError('已存在相同邮箱')
    return
  }

  // 向默认组中添加邮箱
  const { data: newInbox } = await createUngroupedInbox(data)

  // 向当前数据中添加
  addNewRow(newInbox)
  // 向当选择集中添加
  selectedEmails.value.push(newInbox)
}

// 底部确认
import CancelBtn from 'src/components/quasarWrapper/buttons/CancelBtn.vue'
import OkBtn from 'src/components/quasarWrapper/buttons/OkBtn.vue'
function onOKClick () {
  const result = {
    selectedEmails: selectedEmails.value,
    selectedGroups: selectedGroups.value
  }
  // console.log('dialog closed:', result)
  onDialogOK(result)
}

// #region 左侧分组开关
import { useTableCollapseLeft } from 'src/components/collapseLeft/useCollapseLeft'
const outboxTableRef = ref<InstanceType<typeof QTable> | undefined>()
const { CollapseLeft, collapseStyleRef, isCollapseGroupList } = useTableCollapseLeft(outboxTableRef, 14)
// 默认展示
isCollapseGroupList.value = false
// #endregion
</script>

<style lang='scss' scoped>
.select-email-box-dialog {
  min-width: min(800px, 100%);
  min-height: min(600px, 100%);
}
</style>
