<template>
  <q-dialog ref='dialogRef' @hide="onDialogHide">
    <q-card class='column items-center items-start'>
      <SelectProxyDialogTable v-model:selected="selected" />

      <div class="row justify-end q-mb-sm full-width q-pr-sm">
        <OkBtn @click="onOkBtnClicked" tooltip="确认选择" />
      </div>
    </q-card>
  </q-dialog>
</template>

<script lang='ts' setup>
/**
 * warning: 该组件是一个弹窗的示例，不可直接使用
 * 参考：http://www.quasarchs.com/quasar-plugins/dialog#composition-api-variant
 */

import { useDialogPluginComponent } from 'quasar'
defineEmits([
  // 必需；需要指定一些事件
  // （组件将通过useDialogPluginComponent()发出）
  ...useDialogPluginComponent.emits
])
const { dialogRef, onDialogHide, onDialogOK /*, onDialogCancel */ } = useDialogPluginComponent()

import SelectProxyDialogTable from './SelectProxyDialogTable.vue'
// #region 代理选择相关

const props = defineProps({
  proxyIds: {
    type: Array as () => string[],
    default: () => []
  },
})

import OkBtn from 'src/components/quasarWrapper/buttons/OkBtn.vue'
const selected: Ref<{ id: string }[]> = ref(props.proxyIds.map(x => ({ id: x })))
function onOkBtnClicked () {
  onDialogOK({
    proxyIds: selected.value.map(x => x.id)
  })
}
// #endregion
</script>

<style lang='scss' scoped></style>
