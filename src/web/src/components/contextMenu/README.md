# ContextMenu 使用说明

基于 Quasar `q-menu` 的右键菜单封装，支持单值与多选两种场景。组件不抛出事件，菜单行为由 `items` 中每个菜单项的 `onClick` 定义。

## 基本用法

```vue
<template>
  <q-chip>
    <span>{{ tag.label }}</span>
    <ContextMenu :items="tagContextItems" :value="tag" />
  </q-chip>
</template>

<script lang="ts" setup>
import ContextMenu from 'src/components/contextMenu/ContextMenu.vue'
import type { IContextMenuItem } from 'src/components/contextMenu/types'

interface TagItem {
  label: string
  fullPath: string
}

const tagContextItems: IContextMenuItem<TagItem>[] = [
  {
    name: 'close',
    label: '关闭',
    tooltip: '关闭当前标签',
    icon: 'close',
    onClick: async (tag) => closeTag(tag)
  },
  {
    name: 'closeOther',
    label: '关闭其他',
    vif: (tag) => tag.fullPath !== '/',
    onClick: async (tag) => closeOtherTags(tag)
  }
]
</script>
```

## 多选与批量操作

通过 `v-model:selectedValues` 传入多选数组，`onClick` 收到的 `context.targetValues` 即为完整选择项（未选择时回退为 `[value]`）。可调用 `context.clearSelection()` 让父级清空选择。

```vue
<template>
  <q-table v-model:selected="selectedRows" :rows="rows" :columns="columns" selection="multiple">
    <template #body-cell-actions="props">
      <q-td :props="props">
        <ContextMenu :items="rowContextItems" :value="props.row" v-model:selected-values="selectedRows" />
      </q-td>
    </template>
  </q-table>
</template>

<script lang="ts" setup>
import { ref } from 'vue'
import ContextMenu from 'src/components/contextMenu/ContextMenu.vue'
import type { IContextMenuItem } from 'src/components/contextMenu/types'

interface RowItem {
  id: number
  name: string
}
const selectedRows = ref<RowItem[]>([])

const rowContextItems: IContextMenuItem<RowItem>[] = [
  {
    name: 'delete',
    label: '删除',
    icon: 'delete',
    when: 'onlyMulti',
    onClick: async (row, ctx) => {
      await batchDelete(ctx.targetValues)
      ctx.clearSelection()
    }
  },
  {
    name: 'view',
    label: '查看',
    when: 'onlySingle',
    onClick: async (row) => viewRow(row)
  }
]
</script>
```

## Props / Model

| 参数             | 类型                    | 必填 | 默认值      | 说明                                                                       |
| ---------------- | ----------------------- | ---- | ----------- | -------------------------------------------------------------------------- |
| `items`          | `IContextMenuItem<T>[]` | 是   | -           | 菜单项配置列表。                                                           |
| `value`          | `T`                     | 否   | `{}`        | 当前业务对象，传给 `vif` 和 `onClick`。                                    |
| `targetClass`    | `string`                | 否   | `undefined` | 目标元素 class 控制项：右键目标或其父级匹配时才会显示菜单。                |
| `selectedValues` | `T[]`（v-model）        | 否   | `[]`        | 多选值；传入后 `targetValues` 使用该数组，未传入或为空时回退为 `[value]`。 |

## 类型定义

`IContextMenuItem<T>` 与 `IActionContext<T>` 定义于 `types.ts`：

```ts
export const ContextMenuWhen = {
  any: 'any',
  onlyMulti: 'onlyMulti',
  onlySingle: 'onlySingle'
} as const

export interface IContextMenuItem<T = Record<string, any>> {
  name: string
  label: string
  tooltip?: string | string[] | ((params?: T) => Promise<string[]>)
  color?: string
  icon?: string
  when?: keyof typeof ContextMenuWhen
  onClick: (value: T, context: IActionContext<T>) => Promise<void | boolean> | void | boolean
  vif?: (value: T) => boolean
}

export interface IActionContext<TValue> {
  readonly targetValues: readonly TValue[]
  clearSelection: () => void
}
```

## 菜单项字段

| 字段      | 类型                                                        | 必填 | 说明                                                                                                                       |
| --------- | ----------------------------------------------------------- | ---- | -------------------------------------------------------------------------------------------------------------------------- |
| `name`    | `string`                                                    | 是   | 唯一标识，同时作为列表渲染 `key`。                                                                                         |
| `label`   | `string`                                                    | 是   | 菜单显示文本。                                                                                                             |
| `tooltip` | `string \| string[] \| ((params?: T) => Promise<string[]>)` | 否   | 传给 `AsyncTooltip` 显示。                                                                                                 |
| `color`   | `string`                                                    | 否   | 文字/图标颜色。未设置时在 `primary`、`secondary`、`accent` 中自动分配，并尽量与相邻项不同。                                |
| `icon`    | `string`                                                    | 否   | Quasar 图标名。                                                                                                            |
| `when`    | `'any' \| 'onlyMulti' \| 'onlySingle'`                      | 否   | 按选中数量过滤：默认 `any`；`onlyMulti` 仅选中数 > 1 时显示；`onlySingle` 仅选中数 ≤ 1 时显示（含未选）。                  |
| `onClick` | `(value: T, context: IActionContext<T>) => …`               | 是   | 点击时执行。`value` 为当前 `value`，`context.targetValues` 为本次应处理的值集合，`context.clearSelection()` 清空父级选择。 |
| `vif`     | `(value: T) => boolean`                                     | 否   | 控制菜单项是否显示，`false` 时不渲染。                                                                                     |

## 点击与关闭

- `onClick` 可同步或异步。
- 返回 `false` 时菜单不关闭；正常完成则关闭。
- 抛出异常时菜单保持打开，不向外抛错，也不提示。
- 多选下 `targetValues` 始终为当前选中集合（未选时回退为 `[value]`）。

## 条件显示

`vif`（按 `value`）与 `when`（按选中数量）是与关系，均通过时才渲染。未定义时分别默认显示 / 总是显示。

## 注意事项

- 右键目标在表格 `tr` 内时，显示期间会给该行添加 `table-row__keep-hover`，隐藏时移除，用于维持 hover 样式。
- `onClick` 内部异常不会向上抛出，错误提示需自行处理。
- `name` 须稳定且唯一，避免渲染 `key` 冲突。

仓库内使用示例：`src/frontend/src/layouts/components/tags/tagsView.vue`。
