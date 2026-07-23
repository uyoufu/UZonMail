<template>
  <q-table class="full-height" :rows="rows" :columns="columns" row-key="id" virtual-scroll
    v-model:pagination="pagination" dense :loading="loading" :filter="filter" binary-state-sort
    @request="onTableRequest">
    <template v-slot:top-left>
      <q-select outlined v-model="selectedGroup" :options="groupInfoOptions" :label="t('qqGetter.selectGroup')"
        option-label="group_name" dense options-dense use-input input-debounce="300" @filter="filterFn">
        <template v-slot:option="{ itemProps, opt }">
          <q-item v-bind="itemProps">
            <q-item-section>
              {{ opt.group_name }}
            </q-item-section>
            <q-item-section side>
              {{ opt.member_count }}
            </q-item-section>
          </q-item>
        </template>
      </q-select>

      <CreateBtn :label="t('qqGetter.save')" :tooltip="t('qqGetter.saveAsInbox')" class="q-ml-sm"
        @click="onSaveGroupMembersAsInbox" />
    </template>

    <template v-slot:top-right>
      <SearchInput v-model="filter" />
    </template>

    <template v-slot:body-cell-index="props">
      <QTableIndex :props="props" />
    </template>

    <template v-slot:body-cell-userId="props">
      <q-td :props="props">
        {{ props.value }}
      </q-td>
    </template>
  </q-table>
</template>

<script lang="ts" setup>
import type { QTableColumn } from 'quasar'
import { useQTable, useQTableIndex } from 'src/compositions/qTableUtils'
import type { IRequestPagination, TTableFilterObject } from 'src/compositions/types'
import SearchInput from 'src/components/searchInput/SearchInput.vue'
import type { IGroupInfo, IUserInfo } from 'src/api/oneBot/group'

import { t } from 'src/i18n/helpers'

const { indexColumn, QTableIndex } = useQTableIndex()
const columns: QTableColumn[] = [
  indexColumn,
  {
    name: 'user_id',
    required: true,
    label: 'QQ',
    align: 'left',
    field: 'user_id',
    sortable: true
  },
  {
    name: 'nickname',
    required: true,
    label: '昵称',
    align: 'left',
    field: 'nickname',
    sortable: true
  },
]


function filterRowsLocal (filter?: string) {
  return allGroupMembers.value.filter(x => {
    if (!filter || filter === '') {
      return true
    }

    return x.nickname.includes(filter) || x.user_id.toString().includes(filter)
  })
}


function getRowsNumberCount (filterObj: TTableFilterObject) {
  return filterRowsLocal(filterObj.filter).length
}

function onRequest (filterObj: TTableFilterObject, pagination: IRequestPagination) {
  // 进行过滤
  return filterRowsLocal(filterObj.filter).slice(pagination.skip, pagination.limit)
}

const { pagination, rows, filter, onTableRequest, loading, refreshTable } = useQTable({
  getRowsNumberCount,
  onRequest
})

// #region 群列表选项
const groupInfos: Ref<IGroupInfo[]> = ref([])
const selectedGroup: Ref<IGroupInfo> = ref({
  group_id: 0,
  group_name: '无',
  member_count: 0,
})
const groupInfoOptions: Ref<IGroupInfo[]> = ref([])

import { getGroupList, getGroupMemberList } from 'src/api/oneBot/group'
import { confirmOperation, notifyError, notifySuccess } from 'src/utils/dialog'
onMounted(async () => {
  // 获取群列表
  const { data, retcode, message } = await getGroupList()
  if (retcode !== 0) {
    notifyError(message)
    return
  }

  if (data && data.length > 0) {
    selectedGroup.value = data[0]!
    groupInfos.value = data
  }
})

function filterFn (inputValue: string, update: (callbackFn: () => void) => void) {
  if (inputValue === '') {
    update(() => {
      groupInfoOptions.value = groupInfos.value
    })
    return
  }

  update(() => {
    groupInfoOptions.value = groupInfos.value.filter((group) => {
      return group.group_name.includes(inputValue)
    })
  })
}
// #endregion

// #region 获取群用户
const allGroupMembers: Ref<IUserInfo[]> = ref([])
watch(selectedGroup, async (newGroup) => {
  if (newGroup.group_id < 1) return

  // 获取群成员列表
  const { data, message, retcode } = await getGroupMemberList(newGroup.group_id)
  if (retcode !== 0) {
    notifyError(message)
    return
  }

  // 过滤掉机器人
  allGroupMembers.value = data.filter(x => !x.is_robot)

  // 更新数据
  refreshTable()
})
// #endregion

// #region 保存数据
import { saveQQMemberAsInbox } from 'src/api/pro/qqMembers'
async function onSaveGroupMembersAsInbox () {
  const confirm = await confirmOperation(t('qqGetter.saveAsInbox'),
    t('qqGetter.confirmSaveAllMembersAsInboxes', {
      count: allGroupMembers.value.length,
    }))
  if (!confirm) return

  // 保存到服务器
  await saveQQMemberAsInbox(selectedGroup.value, allGroupMembers.value)
  notifySuccess(t('qqGetter.saveMembersAsInboxesSuccess'))
}
// #endregion
</script>

<style lang="scss" scoped></style>
