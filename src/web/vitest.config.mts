import { defineConfig, type UserConfig } from 'vite'
import { fileURLToPath } from 'node:url'
import type { InlineConfig } from 'vitest'
import vue from '@vitejs/plugin-vue'
import { quasar, transformAssetUrls } from '@quasar/vite-plugin'
import tsconfigPaths from 'vite-tsconfig-paths'

const config = {
  resolve: {
    alias: {
      src: fileURLToPath(new URL('./src', import.meta.url))
    },
    dedupe: ['vue', 'vue-i18n', 'vitest', '@vue/test-utils']
  },
  server: {
    fs: {
      allow: ['../..']
    }
  },
  test: {
    environment: 'happy-dom',
    setupFiles: 'test/vitest/setup-file.ts',
    include: [
      // Matches vitest tests in any subfolder of 'src' or into 'test/vitest/__tests__'
      // Matches all files with extension 'js', 'jsx', 'ts' and 'tsx'
      'src/**/*.vitest.{test,spec}.{js,mjs,cjs,ts,mts,cts,jsx,tsx}',
      'test/vitest/__tests__/**/*.{test,spec}.{js,mjs,cjs,ts,mts,cts,jsx,tsx}',
      '../../tests/UzonMail.Web/**/*.{test,spec}.{js,mjs,cjs,ts,mts,cts,jsx,tsx}'
    ]
  },
  plugins: [
    vue({
      template: { transformAssetUrls }
    }),
    quasar({
      sassVariables: 'src/quasar-variables.scss'
    }),
    tsconfigPaths()
  ]
} satisfies UserConfig & { test: InlineConfig }

// https://vitejs.dev/config/
export default defineConfig(config)
