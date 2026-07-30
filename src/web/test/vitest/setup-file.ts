import { config } from '@vue/test-utils';
import { Notify, Quasar } from 'quasar';
import { vi } from 'vitest'

config.global.plugins = [[Quasar, { plugins: { Notify } }]];

// Quasar 的该模块由应用构建阶段生成，单元测试中只需透传启动回调
vi.mock('#q-app/wrappers', () => ({
  defineBoot: <T>(bootCallback: T): T => bootCallback
}))
