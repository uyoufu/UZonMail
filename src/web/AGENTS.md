# 前端项目规范

## 项目结构与模块组织

基于 Quasar、Vue 3、TypeScript 和 Bun 构建。

**目录结构**

应用代码位于 `src/` 下，包含以下模块：

- `api/` 存放 HTTP 客户端
- `boot/` 初始化应用服务
- `components/` 存放跨页面或全局复用的 UI
- `compositions/` 存放跨页面或全局复用的 Vue 3 组合式 API
- `layouts/` 定义页面外壳
- `pages/` 存放路由级页面，页面私有组件和 composable 默认随页面放置
- `router/` 管理导航
- `stores/` 存放 Pinia 状态
- `utils/` 存放共享工具
- `public/` 存放静态资源
- `assets/` 存放源码资源，如图片、字体等
- `tests/` 存放测试代码
- `tests/vitest/` 存放 Vitest 测试代码
- `tests/cypress/` 存放 Cypress 端到端测试代码
- `tests/components/` 存放组件测试规格

**模块组织**

- 路由级页面按路由层级组织目录结构，保存到 `src/pages/` 目录下
- 子目录使用 camelCase 命名风格; 入口组件为 `${PageName}Index.vue`, 使用 PascalCase 命名
- 页面内组件和 composable 放到对应页面目录下, components 目录还可以嵌套存在 components 和 compositions 目录
- 仅与具体业务无关的公共组件和全局复用的 composable，放到 `src/components/` 和 `src/compositions/` 目录中
- 页面入口组件中，不包含具体业务逻辑，仅对所有的组件和 composable 进行初始化和聚合
- 页面中不使用 q-page 作为根元素，已经在 layout 中使用了 q-page

## 代码风格

TS 中，使用 `as const` 定义常量, 禁止使用字符串联合类型，示例如下：

```TypeScript
  // 定义状态常量, 代替枚举
const Status = {
  Pending: 'PENDING',
  Fulfilled: 'FULFILLED',
  Rejected: 'REJECTED',
  } as const

// 定义类型
type Status = typeof Status[keyof typeof Status]
```

## 函数位置与回调边界

- 函数应声明在能够完整承担其职责的最窄作用域中，并尽量靠近首次直接调用位置；不要为了让页面集中展示所有动作而将实现提升到更外层
- 若函数仅被一个页面私有组件或 composable 使用，且由该组件或 composable 决定调用时机和业务语义，则函数应内聚在该组件或 composable 中
- 禁止纯透传回调：不要在外层声明业务函数后，仅通过 `Actions`、`Callbacks` 对象或单独参数传入，再原样赋给 `onClick`、事件监听器或其它接口；改成单独传入函数也不能解决职责错位
- 是否传入函数取决于调用方是否需要选择行为：固定业务行为由被调用模块实现；只有调用方需要提供可替换算法、策略或能力时，才使用函数参数
- 页面私有 composable 可以直接使用稳定的 router、store、API 和 dialog 依赖；优先传入必要的数据、ref 或配置，不要让页面组装整套业务动作
- 若被调用模块必须修改调用方私有状态，可以传入一个职责明确的最小能力回调；不要因此引入覆盖整个模块操作面的回调集合
- 通用基础设施的策略参数、Vue 组件的 props/emits、框架事件回调，以及确有多种实现的依赖接口，不受纯透传限制。例如 `useQTable` 的 `onRequest` 是由调用方提供查询策略，属于合理的函数参数
- 合理的一次性回调应内联或紧邻调用位置声明；当框架可能传入额外参数时，使用箭头函数显式转发所需参数，避免依赖隐含的签名兼容
- 若函数内聚后使组件或 composable 同时承担多个无关职责，应继续按业务关注点拆分模块，而不是恢复为大量回调注入

```TypeScript
// 不推荐：页面只声明动作，再由 composable 原样转交
/** 在页面层打开报告工作区。 */
async function onOpenReport(report: CalcReport): Promise<void> {
  await router.push(`/calc-report/${report.reportOid}/workspace`)
}

const { items } = useReportContextMenu({ open: onOpenReport })

// 推荐：菜单 composable 拥有固定的“打开报告”行为
/** 创建报告右键菜单并内聚菜单的固定业务行为。 */
export function useReportContextMenu() {
  const router = useRouter()

  /** 打开报告工作区。 */
  async function onOpenReport(report: CalcReport): Promise<void> {
    await router.push(`/calc-report/${report.reportOid}/workspace`)
  }

  const items = [{ name: 'open', label: t('calcWorkspace.openWorkspace'), onClick: onOpenReport }]
  return { items }
}
```

## 组件设计

- 将视图拆分为职责聚焦的子组件，每个组件只负责一个清晰关注点
- 保持组件之间低耦合
- 同类逻辑重复出现两次及以上时，提取为工具函数或 composable
- 若单个组件的职责多于 3 个时，应将其拆分为多个 composable
- 页面中的右键菜单应在单独的 composable 中实现

## 组件复用

1. 通用表格展示先读取 src/components/tableComponents/README.md，再以 src/components/tableComponents/tableExample.vue 为初始化模板，复用 src/compositions/qTableUtils
2. 右键菜单组件，读取 src/components/contextMenu/README.md 使用说明进行复用
3. 若要使用弹窗，读取 src/components/lowCode/README.md 使用说明进行复用
4. 对于按钮，优先复用 `src/web/src/components/buttons/` 下实际存在的语义按钮组件；没有合适的语义按钮时使用 `CommonBtn`，不要在页面中直接新增 `q-btn`。
5. 将既有 `q-btn` 重构为 `CommonBtn` 时，若按钮内包含 `q-tooltip`，删除嵌套的 `q-tooltip`，并将提示内容通过 `CommonBtn` 的 `:tooltip` 属性传入。
6. `src/web/src/components/buttons/` 下的按钮组件已自动注册，在模板中直接使用，不需要手动导入。
7. `src/web/src/pages/login/loginIndex.vue` 中的登录按钮是保留原 `q-btn` 实现的明确例外。

## Quasar 组件使用

1. 采用 `dense` 样式，提高布局密度。 `<q-select dense options-dense>` q-select 需要同步设置 `options-dense`
2. 边框采用 `outlined` 样式

## 通用工具函数

通过工具函数定义于 src/utils/ 目录中。

- `src/utils/format.ts formatDate`：统一使用该函数格式化日期

## 数据请求与更新

1. 不要在 OnMounted 中请求所有数据，而是按需获取
2. 对于仅按钮、右键菜单回调时才需要的数据，在对应事件处理函数中请求
3. 当表格中只有一个数据项变化时，不要刷新整个表格，优先使用 `useQTable` 返回的 `addNewRow`、`updateExistOne`、`deleteRowById` 更新本地行

## 弹窗与反馈

弹窗的详细使用见文档 `src/components/lowCode/README.md`

- 表单弹窗优先使用 `src/components/lowCode`
- 长耗时操作使用 `notifyUntil` 包裹，以提供执行进度反馈
- 优先使用 `src/utils/dialog.ts` 中的提示工具，尤其是 `notifySuccess`、`notifyError`、`notifyWarning` 和 `confirmOperation`
- 需要原生 q-dialog 的复杂交互时, 使用 `src/components/lowCode/PopupDialogExample.vue` 模板定义成页面私有组件，然后在独立的 composable 组件中调用 `showComponentDialog` 函数显示弹窗
- 禁止在组件上使用 `v-model=` 来控制弹窗的显示与隐藏，而应使用 `showComponentDialog` 函数来显示弹窗


## 组合式 API

- 按功能组织独立 composable，例如 `useSaveXxx`、`useDialogXxx` 或 `useTableXxx`
- 每个 composable 只聚焦一个职责
- 将相关状态和处理函数放在一起，例如 `onSaveXxx`、`isSaving`、`canSaveXxx`
- 解构 `const { xxx } = useXxx()` 时，暴露字段应保持精简；超过约 10 个字段时，优先拆分 composable，而不是包装返回值

## 命名

- 布尔状态使用 `is`、`can` 或 `has` 前缀
- 组件事件和动作处理函数使用 `on` 前缀
- 命名应直接表达意图，方便项目级检索

## 样式

- 总体风格为卡片式、响应式、无边框的视觉设计
- 优先使用 flex 布局
- 优先使用 Quasar 工具类，例如 `text-subtitle1`、`text-primary`、`row`、`col`, 若无必要，不使用自定义 css
- Tailwind 可用于新增布局、间距、响应式和小范围样式补充，存量页面保持现有 Quasar 优先的风格
- 添加自定义样式前，先保持与现有视觉语言一致
- vue 组件中使用 scss 语法, 不使用 plain css

## 页面设计

- 页面中的表格布局建议：表头左侧显示没有数据时的必要功能，如新增，上传等; 表头右侧显示搜索; 其它功能通过表格行的右键菜单来实现，对于重复性的操作，要支持多选

## vue 语法补充

- 使用 `defineModel<TContextValue[]>({default: () => []})` 定义 v-model:xxx 双向属性，xxx 为属性名

## Vue 自动引入

前端在 `quasar.config.ts` 中通过 `unplugin-auto-import/vite` 自动引入 `vue`、`vue-router`、`pinia` 和 `quasar` 的常用 API。在 `.vue` 和 TypeScript 代码中使用这些来源的函数时，不需要再手动添加 import，例如 `ref`、`computed`、`watch`、`onMounted`、`useRouter`、`useRoute`、`defineStore`、`useQuasar` 等。

组件按需引入由 `unplugin-vue-components/vite` 负责，并配置了 `ElementPlusResolver()` 和 `QuasarResolver()`。在 Vue 模板中使用 Element Plus 或 Quasar 组件时，不需要手动导入组件，例如 `<ElButton>`、`<ElForm>`、`<QBtn>`、`<QTable>` 等。

项目本地组件、业务 composable、工具函数、API 方法和 store 实例仍应按实际路径显式导入，除非已在 `quasar.config.ts` 中加入对应的自动导入规则。

## 国际化与多语言

- 使用 src\i18n\helpers.ts 中的 t 函数进行多语言翻译
- 多语言字段名按模块、页面、组件组织，例如 `loginPage.title`、`routes.title`、`components.tableExample.title` 等。
- 禁止读取、编辑 `src/i18n/locales/` 目录下的文件，新增、更新、删除多语言字段应使用 `scripts/upsert_i18n.js` 脚本完成
- 单条更新：`bun run i18n:upsert -- --key loginPage.title --zh-CN "标题" --en-US "Title"`
- 批量更新： `bun run i18n:upsert -- --pair loginPage.title "标题" "Title" --pair loginPage.subtitle "副标题" "Subtitle"`，每个 `--pair` 的参数顺序固定为 key、中文、英文
- 批量删除：`bun run i18n:upsert -- --delete loginPage.title --delete loginPage.subtitle`，不存在的 key 会被忽略，`--delete` 不与新增或更新参数混用
- 对于 q-table 中的 columns 定义，使用 computed 函数返回，以支持多语言切换

## 构建、测试与开发命令

- `bun install`：使用固定的 Bun 包管理器安装依赖。
- `bun run dev`：启动 Quasar 开发服务器。
- `bun run build`：使用 `quasar build` 构建生产版本。
- `bun run lint`：对 `.js`、`.ts` 和 `.vue` 文件运行 ESLint。
- `bun run typecheck`：检查 TypeScript 编译诊断。
- `bun run test:unit`：以监听模式运行 Vitest。
- `bun run test:unit:ci`：为 CI 单次运行 Vitest。
- `bun run test:e2e:ci`：启动应用并运行 Cypress 端到端测试。
- `bun run test:component:ci`：运行 Cypress 组件测试。

## 代码风格与命名约定

遵循现有 Vue 单文件组件和 TypeScript 风格。Prettier 配置为不使用分号、使用单引号、`printWidth: 120`、不使用尾随逗号，并为箭头函数参数添加括号。ESLint 强制执行 Vue、Quasar 和 TypeScript 推荐规则，包括 TypeScript 类型使用 type-only import。文件、函数和 ref 使用描述性业务命名；布尔值优先使用 `is*`、`has*` 或 `should*`，事件回调使用 `on*`，例如 `onCancelClick`。

## 测试规范

单元测试使用 Vitest，端到端或组件行为测试使用 Cypress。Vitest 匹配 `src/**/*.vitest.{test,spec}.*` 和 `test/vitest/__tests__/**/*.{test,spec}.*`。Cypress 规格文件使用 `.cy.ts`。行为变更需要新增或更新聚焦的回归测试，并将 fixture 和 support 代码放在对应的 `test/cypress/` 或 `test/vitest/` 区域。

## 提交与 Pull Request 规范

近期提交历史使用简短的祈使句主题，并带有 `feat:`、`refactor:`、`Docs:`、`Build:`、`Chore:` 等前缀。保持提交聚焦，必要时说明受影响的模块。Pull Request 应包含简洁的变更摘要、关联 issue 或任务、测试结果，以及可见 UI 变更的截图或录屏。

## 配置与安全说明

运行时配置示例位于 `public/app.config.json` 和 `src/config/`。不要提交密钥、令牌或特定环境凭据。API 和 SignalR 变更在合并前应与后端契约保持一致。
