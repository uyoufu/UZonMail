# lowCode 对话框

`lowCode` 目录提供基于 Quasar Dialog 的低代码表单弹窗。调用方通过字段配置生成表单，用户确认后返回表单模型。

## 推荐入口

新代码优先从统一弹窗工具导入：

```ts
import { showDialog, showComponentDialog, showHtmlDialog2 } from 'src/utils/dialog'
import { LowCodeFieldType } from 'src/components/lowCode/types'
```

也可以直接从组件目录导入：

```ts
import { showDialog } from 'src/components/lowCode/PopupDialog'
import { LowCodeFieldType } from 'src/components/lowCode/types'
```

`showDialog<T>()` 会打开 `LowCodeForm`，返回 `Promise<IDialogResult<T>>`：

```ts
const result = await showDialog<{ name: string; age: number }>({
  title: '编辑用户',
  oneColumn: true,
  fields: [
    {
      name: 'name',
      label: '姓名',
      type: LowCodeFieldType.text,
      required: true
    },
    {
      name: 'age',
      label: '年龄',
      type: LowCodeFieldType.number,
      value: 18,
      parser: (value) => Number(value)
    },
    {
      name: 'role',
      label: '角色',
      type: LowCodeFieldType.selectOne,
      value: 'user',
      options: [
        { label: '管理员', value: 'admin' },
        { label: '普通用户', value: 'user' }
      ],
      optionLabel: 'label',
      optionValue: 'value',
      emitValue: true,
      mapOptions: true
    }
  ],
  validate: (values) => ({
    ok: Number(values.age) > 0,
    message: '年龄必须大于 0'
  })
})

if (!result.ok) return

await updateUser(result.data)
```

取消或关闭弹窗时返回 `{ ok: false, data: {} }`。

默认情况下不要传入 `onOkMain`。弹窗只负责收集和校验表单数据；调用方应等待 `showDialog` 返回，确认
`result.ok` 后再执行后端提交。这样可以让弹窗职责和业务提交流程保持清晰。

只有后端提交失败时必须保留当前弹窗、让用户继续修改表单的场景，才通过 `onOkMain` 提交。例如修改密码时，
旧密码错误后需要保留弹窗：

```ts
const result = await showDialog<{ oldPassword: string; newPassword: string }>({
  title: '修改密码',
  fields: [
    { name: 'oldPassword', label: '旧密码', type: LowCodeFieldType.password },
    { name: 'newPassword', label: '新密码', type: LowCodeFieldType.password }
  ],
  onOkMain: async (values) => {
    const response = await changeUserPassword(values.oldPassword, values.newPassword)
    return response.data
  }
})

if (!result.ok) return
notifySuccess('密码修改成功')
```

`onOkMain` 返回 `false` 或抛出异常时不会确认关闭弹窗；成功完成且未返回 `false` 时才会关闭并返回表单数据。

## 字段配置

字段使用 `ILowCodeField` 描述。常用字段如下：

| 字段              | 说明                                                           |
| ----------------- | -------------------------------------------------------------- |
| `name`            | 返回模型中的字段名，必填                                       |
| `label`           | 表单显示名称，必填                                             |
| `type`            | 字段类型；未传入时 `showDialog` 会补为 `LowCodeFieldType.text` |
| `placeholder`     | 输入占位内容                                                   |
| `hint`            | 输入控件底部的辅助说明                                         |
| `autofocus`       | 弹窗打开后是否自动聚焦该字段                                   |
| `value`           | 初始值                                                         |
| `options`         | `selectOne` / `selectMany` 的候选项                            |
| `optionLabel`     | 选项对象中用于显示的字段名，默认按组件逻辑读取 `label`         |
| `optionValue`     | 选项对象中作为值的字段名                                       |
| `optionTooltip`   | 选项对象中作为提示的字段名                                     |
| `mapOptions`      | 透传给 Quasar `q-select` 的 `map-options`                      |
| `emitValue`       | 透传给 Quasar `q-select` 的 `emit-value`                       |
| `icon`            | 输入框前置图标                                                 |
| `required`        | 是否必填                                                       |
| `parser`          | 确认时先对当前字段值做转换                                     |
| `validate`        | 字段级校验函数                                                 |
| `visible`         | `false` 时隐藏；函数返回 `false` 时隐藏                        |
| `tooltip`         | 字段提示内容，传给 `AsyncTooltip`                              |
| `disable`         | 是否禁用                                                       |
| `disableAutoGrow` | `textarea` 是否禁用自动增高                                    |
| `classes`         | 追加到字段容器的样式类                                         |

支持的字段类型来自 `LowCodeFieldType`：

| 类型                                                                                        | 控件            |
| ------------------------------------------------------------------------------------------- | --------------- |
| `text`、`email`、`search`、`tel`、`file`、`number`、`url`、`time`、`date`、`datetime-local` | `q-input`       |
| `textarea`                                                                                  | 多行 `q-input`  |
| `password`                                                                                  | `PasswordInput` |
| `selectOne`                                                                                 | 单选 `q-select` |
| `selectMany`                                                                                | 多选 `q-select` |
| `boolean`                                                                                   | `q-checkbox`    |
| `editor`                                                                                    | `q-editor`      |

## 确认流程

用户点击确定或在非 `textarea` 控件中按 Enter 时，组件按以下顺序处理：

1. 只处理当前可见字段。
2. 若字段设置 `required`，空值会提示错误并中止提交；`false` 会被视为有效值。
3. 若字段设置 `parser`，先用 `parser(value)` 转换字段值。
4. 若字段设置 `validate`，调用 `validate(originalValue, parsedValue, allValues)`；返回 `{ ok: false, message }` 时提示错误并恢复该字段原值。
5. 若弹窗设置全局 `validate`，调用 `validate(allValues)`。
6. 若例外场景设置了 `onOkMain`，调用 `onOkMain(allValues)`；返回 `false` 或抛出异常时阻止确认关闭。
7. 全部通过后返回 `{ ok: true, data: allValues }`。

## 布局和按钮

`showDialog` 参数主要来自 `IPopupDialogParams`：

| 参数         | 说明                                                |
| ------------ | --------------------------------------------------- |
| `title`      | 弹窗标题                                            |
| `fields`     | 字段配置数组                                        |
| `validate`   | 全局校验函数                                        |
| `persistent` | 是否阻止点击遮罩直接关闭，默认 `true`               |
| `onOkMain`   | 仅用于提交失败时必须保留当前弹窗的例外场景          |
| `oneColumn`  | 是否使用单列布局                                    |
| `customBtns` | 自定义按钮列表                                      |
| `onSetup`    | 表单初始化阶段回调，可访问 `fieldsModel` 和可见字段 |

`LowCodeForm` 还支持 `disableDefaultBtns` 隐藏默认 `ok` / `cancel` 按钮。当前 `IPopupDialogParams` 没有声明该字段，直接使用组件 props 时更适合配置它。

自定义按钮格式如下：

```ts
customBtns: [
  {
    label: '预览',
    color: 'primary',
    onClick: async (values) => {
      console.log(values)
    }
  }
]
```

## 项目内真实用法

`src/utils/file.ts` 在 Excel 文件存在多个 worksheet 时，用 `selectOne` 让用户选择工作表：

```ts
const { ok, data } = await showDialog<Record<string, any>>({
  title: translateUtils('file_specifyWorksheet'),
  fields: [
    {
      name: 'sheetName',
      type: LowCodeFieldType.selectOne,
      label: translateUtils('file_pleaseSelectWorksheet'),
      value: workbook.SheetNames[0],
      options: workbook.SheetNames
    }
  ],
  oneColumn: true
})

if (ok) {
  params.sheetIndex = workbook.SheetNames.indexOf(data.sheetName)
}
```

## HTML 弹窗

`showHtmlDialog2(title, html)` 用于展示 HTML 内容：

```ts
await showHtmlDialog2('说明', '<p>这里是 HTML 内容</p>')
```

该弹窗内部使用 `v-html` 渲染内容，调用方必须确保传入 HTML 可信，不要直接渲染未清洗的用户输入。

## 自定义弹窗

若通过配置无法实现弹窗功能，参考 src/components/quasarWrapper/PopupDialogExample.vue 模板自定义弹窗组件，然后通过 `await showComponentDialog(component, componentProps)` 显示。

## 注意事项

- `onChanged` 会在字段值变化后调用，并接收新值、旧值、完整表单模型和字段定义数组；支持异步回调，也可以在回调中联动修改其它字段的值或候选项。
- `dataSet` 当前会在初始化时解析函数值到内部 `dataSetRef`，但表单渲染逻辑没有自动把它绑定到字段 `options`。
- `visible` 支持布尔值和函数；字符串形式只适合经过外部适配层转换后的场景。
- 日期字段初始化时会格式化为 `YYYY-MM-DD`，数字字段未提供初值时默认为 `0`。
