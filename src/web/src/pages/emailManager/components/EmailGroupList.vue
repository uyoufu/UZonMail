<template>
  <q-list dense class="column no-wrap justify-start">
    <q-item class="plain-list__item text-primary bg-grey-11" v-ripple>
      <q-item-section avatar class="q-pr-none">
        <q-icon :name="header.icon" />
      </q-item-section>
      <q-item-section class="q-px-lg q-py-sm text-bold">
        {{ header.label }}
        <AsyncTooltip v-if="!readonly" :tooltip="translateEmailGroup('youCanRightClickToAddNewGroup')" />
      </q-item-section>
      <ContextMenu v-if="!readonly" :items="headerContextMenuItems" />
      <q-item-section v-if="!readonly" side>
        <q-btn icon="add" dense flat size="md" @click.stop="onCreateEmailGroup">
          <AsyncTooltip :tooltip="translateEmailGroup('newGroup')" />
        </q-btn>
      </q-item-section>
    </q-item>

    <q-item class="plain-list__item q-mt-xs">
      <SearchInput v-model="filter" dense />
    </q-item>

    <q-list class="col scroll-y hover-scroll" dense>
      <GroupListItem
        v-for="group in extraItems"
        :key="group.name"
        :group="group"
        :selectable="selectable"
        :readonly="readonly"
        :context-menu-items="itemContextMenuItems"
        @click="onItemClick"
        @selection-change="onItemCheckboxClicked"
      />

      <template v-if="isFiltering">
        <GroupListItem
          v-for="group in matchedGroups"
          :key="group.id"
          :group="group"
          :selectable="selectable"
          :readonly="readonly"
          :context-menu-items="itemContextMenuItems"
          @click="onItemClick"
          @selection-change="onItemCheckboxClicked"
        />
      </template>
      <draggable
        v-else
        v-model="groupItems"
        item-key="id"
        :disabled="readonly"
        handle=".group-drag-handle"
        @end="onGroupsReordered"
      >
        <template #item="{ element: group }">
          <GroupListItem
            :group="group"
            :selectable="selectable"
            :readonly="readonly"
            :context-menu-items="itemContextMenuItems"
            draggable
            @click="onItemClick"
            @selection-change="onItemCheckboxClicked"
          />
        </template>
      </draggable>
    </q-list>
  </q-list>
</template>

<script lang="ts" setup>
import type { PropType } from 'vue'
import draggable from 'vuedraggable'
import { t, translateEmailGroup, translateGlobal } from 'src/i18n/helpers'
import {
  EmailGroupCategory,
  createEmailGroup,
  deleteEmailGroupById,
  getEmailGroups,
  reorderEmailGroups,
  updateEmailGroup,
  type EmailGroupCategory as EmailGroupCategoryValue,
  type IEmailGroup
} from 'src/api/emailGroup'
import type { IEmailGroupListItem, IFlatHeader } from './types'
import ContextMenu from 'src/components/contextMenu/ContextMenu.vue'
import AsyncTooltip from 'src/components/asyncTooltip/AsyncTooltip.vue'
import SearchInput from 'src/components/searchInput/SearchInput.vue'
import { ContextMenuIcon, type IContextMenuItem } from 'src/components/contextMenu/types'
import type { IPopupDialogParams } from 'src/components/lowCode/types'
import { LowCodeFieldType } from 'src/components/lowCode/types'
import { showDialog } from 'src/components/lowCode/PopupDialog'
import { confirmOperation, notifySuccess } from 'src/utils/dialog'

const GroupListItem = defineComponent({
  name: 'GroupListItem',
  components: { AsyncTooltip, ContextMenu },
  props: {
    group: { type: Object as PropType<IEmailGroupListItem>, required: true },
    selectable: { type: Boolean, required: true },
    readonly: { type: Boolean, required: true },
    draggable: { type: Boolean, default: false },
    contextMenuItems: {
      type: Array as PropType<IContextMenuItem<IEmailGroupListItem>[]>,
      required: true
    }
  },
  emits: ['click', 'selection-change'],
  template: `
    <q-item class="plain-list__item q-my-xs" clickable v-ripple :active="group.active"
      active-class="text-secondary" @click="$emit('click', group)">
      <div class="row justify-between no-wrap items-center full-width">
        <div class="row justify-start items-center no-wrap overflow-hidden">
          <q-icon v-if="draggable" class="group-drag-handle cursor-move q-mr-xs" name="drag_indicator" size="xs" />
          <q-icon v-if="group.icon" color="primary" :name="group.icon" size="sm" />
          <q-checkbox v-if="selectable && group.selectable !== false" dense v-model="group.selected"
            color="secondary" class="q-ml-sm" keep-color @click.stop @update:model-value="$emit('selection-change', group)" />
          <div class="q-ml-sm ellipsis">{{ group.label }}<AsyncTooltip :tooltip="group.label" /></div>
        </div>
        <q-badge v-if="group.accountCount !== undefined" class="q-ml-sm" color="grey-7" :label="group.accountCount" />
      </div>
      <ContextMenu v-if="!readonly" :items="contextMenuItems" :value="group" />
    </q-item>
  `
})

const modelValue = defineModel<IEmailGroupListItem>()
const selectedValues = defineModel<IEmailGroupListItem[]>('selected', { default: () => [] })
const props = defineProps({
  extraItems: { type: Array as PropType<IEmailGroupListItem[]>, default: () => [] },
  readonly: { type: Boolean, default: false },
  groupCategory: {
    type: Number as PropType<EmailGroupCategoryValue>,
    default: EmailGroupCategory.EmailAccount
  },
  selectable: { type: Boolean, default: false },
  contextMenuItems: {
    type: Array as PropType<IContextMenuItem<IEmailGroupListItem>[]>,
    default: () => []
  }
})

const header = computed<IFlatHeader>(() => props.groupCategory === EmailGroupCategory.EmailAccount
  ? { label: t('accountManagement.emailAccount.group'), icon: 'group' }
  : { label: t('accountManagement.recipient.group'), icon: 'group' })
const filter = ref('')
const groupItems = ref<IEmailGroupListItem[]>([])
const isFiltering = computed(() => filter.value.trim().length > 0)
const matchedGroups = computed(() => {
  const normalizedFilter = filter.value.trim().toLowerCase()
  if (!normalizedFilter) return groupItems.value
  return groupItems.value.filter(group => group.name.toLowerCase().includes(normalizedFilter))
})

function onItemCheckboxClicked (emailGroup: IEmailGroupListItem) {
  if (emailGroup.selected) {
    if (!selectedValues.value.some(group => group.id === emailGroup.id)) selectedValues.value.push(emailGroup)
    return
  }
  selectedValues.value = selectedValues.value.filter(group => group.id !== emailGroup.id)
}

function activeGroup (group: IEmailGroupListItem) {
  for (const candidate of [...props.extraItems, ...groupItems.value]) candidate.active = candidate === group
  modelValue.value = group
}

function onItemClick (group: IEmailGroupListItem) {
  activeGroup(group)
}

async function loadGroups () {
  const { data: groups } = await getEmailGroups(props.groupCategory)
  const currentGroupId = modelValue.value?.id
  groupItems.value = groups.map(group => toListItem(group))
  const selectedGroup = groupItems.value.find(group => group.id === currentGroupId)
    ?? groupItems.value[0]
    ?? props.extraItems[0]
  if (selectedGroup) activeGroup(selectedGroup)
}

function toListItem (group: IEmailGroup): IEmailGroupListItem {
  return {
    ...group,
    label: group.name,
    active: false,
    selected: selectedValues.value.some(selectedGroup => selectedGroup.id === group.id)
  }
}

async function onGroupsReordered () {
  const groupIds = groupItems.value.flatMap(group => group.id === undefined ? [] : [group.id])
  if (groupIds.length !== groupItems.value.length) return
  await reorderEmailGroups(props.groupCategory, groupIds)
}

async function onCreateEmailGroup () {
  const dialogParams: IPopupDialogParams = {
    title: translateEmailGroup('newGroup'),
    oneColumn: true,
    fields: [
      { name: 'name', label: translateEmailGroup('field_name'), value: '', type: LowCodeFieldType.text, required: true },
      { name: 'description', label: translateEmailGroup('field_description'), value: '', type: LowCodeFieldType.textarea }
    ]
  }
  const result = await showDialog<{ name: string, description?: string }>(dialogParams)
  if (!result.ok) return

  const { data: group } = await createEmailGroup({
    icon: 'group',
    name: result.data.name,
    description: result.data.description,
    category: props.groupCategory
  })
  const listItem = toListItem(group)
  groupItems.value.push(listItem)
  activeGroup(listItem)
  notifySuccess(translateEmailGroup('newGroupSuccess'))
}

async function modifyGroup (emailGroup: IEmailGroupListItem) {
  if (emailGroup.id === undefined) return
  const dialogParams: IPopupDialogParams = {
    title: translateEmailGroup('modifyEmailGroup'),
    oneColumn: true,
    fields: [
      { name: 'name', label: translateEmailGroup('field_name'), value: emailGroup.name, type: LowCodeFieldType.text, required: true },
      { name: 'description', label: translateEmailGroup('field_description'), value: emailGroup.description, type: LowCodeFieldType.textarea }
    ]
  }
  const result = await showDialog<{ name: string, description?: string }>(dialogParams)
  if (!result.ok) return
  const { data: group } = await updateEmailGroup(emailGroup.id, result.data)
  Object.assign(emailGroup, toListItem(group), { active: emailGroup.active })
  notifySuccess(t('accountManagement.updated'))
}

async function deleteGroup (emailGroup: IEmailGroupListItem) {
  if (emailGroup.id === undefined) return
  const confirmed = await confirmOperation(
    translateGlobal('deleteConfirmation'),
    translateEmailGroup('deleteGroupConfirm', { groupName: emailGroup.label })
  )
  if (!confirmed) return
  await deleteEmailGroupById(emailGroup.id)
  const groupIndex = groupItems.value.findIndex(group => group.id === emailGroup.id)
  groupItems.value.splice(groupIndex, 1)
  const nextGroup = groupItems.value[Math.max(0, groupIndex - 1)] ?? props.extraItems[0]
  if (nextGroup) activeGroup(nextGroup)
  notifySuccess(translateEmailGroup('deleteGroupSuccess', { groupName: emailGroup.label }))
}

const headerContextMenuItems = computed<IContextMenuItem<IEmailGroupListItem>[]>(() => [
  {
    name: 'add',
    label: translateGlobal('new'),
    tooltip: translateEmailGroup('newEmailGroup'),
    icon: ContextMenuIcon.add,
    onClick: onCreateEmailGroup
  }
])
const itemContextMenuItems = computed<IContextMenuItem<IEmailGroupListItem>[]>(() => [
  ...props.contextMenuItems,
  ...headerContextMenuItems.value,
  {
    name: 'modify',
    label: translateGlobal('modify'),
    tooltip: translateEmailGroup('modifyCurrentGroup'),
    icon: ContextMenuIcon.edit,
    onClick: modifyGroup
  },
  {
    name: 'delete',
    label: translateGlobal('delete'),
    color: 'negative',
    tooltip: translateEmailGroup('deleteCurrentGroup'),
    icon: ContextMenuIcon.delete,
    vif: group => !group.isDefault,
    onClick: deleteGroup
  }
])

watch(() => props.groupCategory, loadGroups)
onMounted(loadGroups)
defineExpose({ reloadGroups: loadGroups })
</script>

<style lang="scss" scoped>
.plain-list__item {
  :deep(.q-item__section--avatar) {
    min-width: auto !important;
  }
}
</style>
