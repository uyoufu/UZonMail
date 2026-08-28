<template>
  <q-item class="email-group-list-item q-my-xs" clickable v-ripple :active="group.active" active-class="text-secondary"
    @click="onGroupClick">
    <div class="row justify-between no-wrap items-center full-width">
      <div class="row justify-start items-center no-wrap overflow-hidden">
        <q-icon v-if="draggable" class="group-drag-handle cursor-move q-mr-xs" name="drag_indicator" size="xs" />
        <q-icon v-if="group.icon" color="primary" :name="group.icon" size="sm" />
        <q-checkbox v-if="selectable && group.selectable !== false" dense :model-value="group.selected"
          color="secondary" class="q-ml-sm" keep-color @click.stop @update:model-value="onSelectionChange" />
        <div class="q-ml-sm ellipsis">
          {{ group.label }}
          <AsyncTooltip :tooltip="group.label" />
        </div>
      </div>
      <q-badge v-if="group.accountCount !== undefined" class="q-ml-sm" color="grey-5" :label="group.accountCount" />
    </div>
    <ContextMenu v-if="!readonly" :items="contextMenuItems" :value="group" />
  </q-item>
</template>

<script lang="ts" setup>
import AsyncTooltip from 'src/components/asyncTooltip/AsyncTooltip.vue'
import ContextMenu from 'src/components/contextMenu/ContextMenu.vue'
import type { IContextMenuItem } from 'src/components/contextMenu/types'
import type { IEmailGroupListItem } from './types'

const props = withDefaults(defineProps<{
  group: IEmailGroupListItem
  selectable: boolean
  readonly: boolean
  draggable?: boolean
  contextMenuItems: IContextMenuItem<IEmailGroupListItem>[]
}>(), {
  draggable: false
})

const emit = defineEmits<{
  click: [group: IEmailGroupListItem]
  'selection-change': [group: IEmailGroupListItem, isSelected: boolean]
}>()

function onGroupClick() {
  emit('click', props.group)
}

function onSelectionChange(isSelected: boolean | null) {
  emit('selection-change', props.group, isSelected === true)
}
</script>
