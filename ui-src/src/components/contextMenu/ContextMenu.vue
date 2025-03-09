<template>
  <q-menu v-model="qMenuModel" context-menu transition-show="scale" transition-hide="rotate" auto-close
    @before-show="beforeContextMenuShow" @before-hide="onMenuBeforeHide">
    <q-list dense style="overflow: hidden;">
      <q-item v-for="(item, index) in contextItems" :key="item.name" clickable @click="onMenuItemClick($event, item)"
        :class="getItemClass(item, index)" dense class="active-item">
        <div class="row justify-start items-center">
          <q-icon v-if="item.icon" :color="item.color" :name="item.icon" class="q-mr-sm"></q-icon>
          <q-item-section>
            {{ item.label }}
          </q-item-section>
        </div>
        <AsyncTooltip :tooltip="item.tooltip" :color="item.color" anchor="center right" self="center left" />
      </q-item>
    </q-list>
  </q-menu>
</template>

<script lang="ts" setup>
import AsyncTooltip from 'src/components/asyncTooltip/AsyncTooltip.vue'

import type { IContextMenuItem } from './types'
const props = defineProps({
  items: {
    type: Array as PropType<IContextMenuItem[]>,
    required: true
  },
  value: {
    type: Object,
    default: () => { return {} }
  },
  // 目标元素的 class
  // 只有匹配时，才会显示右键菜单
  targetClass: {
    type: String,
    default: undefined
  }
})

// 对定义进行处理
const colors = ['primary', 'secondary', 'accent']
const contextItems = computed(() => {
  // 如果有 vif,则执行 vif 判断是否显示
  const results = props.items.filter(x => !x.vif || x.vif(props.value))
  // 对结果赋予颜色
  results.forEach((item, index) => {
    if (item.color) return

    // 若没有颜色，则挑选与前后不一样的颜色
    const colorsTemp: (string | undefined)[] = []
    if (index > 0) colorsTemp.push(results[index - 1]?.color)
    if (index < results.length - 1) colorsTemp.push(results[index + 1]?.color)
    item.color = colors.find(x => !colorsTemp.includes(x))
  })
  return results
})

// 获取菜单项显示样式
function getItemClass (contextItem: IContextMenuItem, index: number) {
  if (contextItem.color) return `text-${contextItem.color}`
  const colors = ['text-primary', 'text-secondary', 'text-accent']
  return colors[index % 3]
}

// #region 菜单的显示和样式控制
const qMenuModel = ref(false)
const targetRowElement = ref<HTMLElement | null>(null)
// 递归向上查找，判断元素是否包含指定的class
function hasClass (element: HTMLElement, className: string) {
  if (element.classList.contains(className)) return true
  if (element.parentElement) return hasClass(element.parentElement, className)
  return false
}
function beforeContextMenuShow (evt: Event) {
  const trElement = getTableRowElement(evt.target as HTMLElement)
  if (trElement) {
    trElement.classList.add('table-row__keep-hover')
    targetRowElement.value = trElement
  }

  // 没有目标类时，不进行控制
  if (props.targetClass && hasClass(evt.target as HTMLElement, props.targetClass)) {
    qMenuModel.value = false
  }
}

// 获取表格行元素
function getTableRowElement (element: HTMLElement | null) {
  if (!element) return
  // 判断 element 是否是行
  if (element.tagName.toLowerCase() === 'tr') return element
  return getTableRowElement(element.parentElement)
}

function onMenuBeforeHide () {
  // 移除样式 .table-row__keep-hover
  if (targetRowElement.value) {
    targetRowElement.value.classList.remove('table-row__keep-hover')
    targetRowElement.value = null
  }
}
// #endregion

// #region 执行菜单命令
async function onMenuItemClick (event: Event, item: IContextMenuItem) {
  // 其它菜单设置成不可点击
  // 当前菜单增加执行动画
  try {
    console.log('执行菜单命令', event, item, props.value)
    // 开始执行
    if (typeof item.onClick === 'function') {
      const result = await item.onClick(props.value)
      if (result === false) return
    }
    onMenuBeforeHide()
    // 关闭菜单
    qMenuModel.value = false
  } catch {
    // 异常时，不关闭菜单
  } finally {
    // 重置所有的动画状态
  }
}
// #endregion
</script>

<style lang="scss" scoped>
.active-item {
  &:hover {
    // 放大
    scale: 1.1;
    transition: all 0.3s;
  }
}
</style>
