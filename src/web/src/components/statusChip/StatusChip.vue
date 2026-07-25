<template>
  <q-chip
    v-bind="$attrs"
    dense
    outline
    square
    :color="statusStyle.color"
    :text-color="statusStyle.textColor"
    :icon="statusStyle.icon"
    :label="statusStyle.label"
  >
    <slot />
  </q-chip>
</template>

<script lang="ts" setup>
import { camelCase } from 'lodash'
import { t } from 'src/i18n/helpers'
import type { LangKey } from 'src/i18n/types'
import { computed } from 'vue'
import type { IStatusChipItem, StatusChipValue } from './types'

/** 根据业务状态显示本地化标签和语义颜色。 */
defineOptions({
  name: 'StatusChip',
  inheritAttrs: false
})

const BUILT_IN_STATUS_STYLES = [
  { status: 'created', color: 'primary' },
  { status: 'pending', color: 'accent' },
  { status: 'sending', color: 'secondary' },
  { status: 'waitingForQuotaReset', color: 'orange' },
  { status: 'success', color: 'secondary' },
  { status: 'failed', color: 'negative' },
  { status: 'pause', color: 'orange' },
  { status: 'paused', color: 'orange' },
  { status: 'stopped', color: 'grey' },
  { status: 'finish', color: 'secondary' },
  { status: 'completed', color: 'secondary' },
  { status: 'cancel', color: 'grey' },
  { status: 'canceled', color: 'grey' },
  { status: true, color: 'positive' },
  { status: false, color: 'negative' },
  { status: 'independent', color: 'primary' },
  { status: 'subUser', color: 'negative' },
  { status: 'normal', color: 'primary' },
  { status: 'forbiddenLogin', color: 'negative' },
  { status: 'read', color: 'positive' },
  { status: 'instant', color: 'primary' },
  { status: 'scheduled', color: 'orange' },
  { status: 'invalid', color: 'negative' },
  { status: 'valid', color: 'positive' },
  { status: 'unsubscribed', color: 'negative' },
  { status: 'subscribed', color: 'positive' },
  { status: 'blacklist', color: 'negative' },
  { status: 'whitelist', color: 'positive' },
  { status: 'unverified', color: 'negative' },
  { status: 'verified', color: 'positive' },
  { status: 'running', color: 'secondary' },
  { status: 'unknown', color: 'negative' },
  { status: 'inProgress', color: 'info' }
] as const satisfies readonly IStatusChipItem[]

const FALLBACK_COLORS = ['primary', 'secondary', 'accent', 'negative', 'info', 'orange', 'positive', 'grey'] as const
const STATUS_TRANSLATION_KEY_PREFIX = 'components.statusChip' as const
const builtInStatusKeys = new Set(BUILT_IN_STATUS_STYLES.map(({ status }) => camelCase(String(status))))

const props = withDefaults(
  defineProps<{
    /** 需要展示的原始业务状态。 */
    status: StatusChipValue
    /** 覆盖内置状态或定义自定义状态的显示配置。 */
    statusStyles?: readonly IStatusChipItem[]
  }>(),
  {
    statusStyles: () => []
  }
)

const statusStylesMap = computed(() => {
  const stylesMap = new Map<string, IStatusChipItem>()
  for (const builtInStyle of BUILT_IN_STATUS_STYLES) {
    stylesMap.set(camelCase(String(builtInStyle.status)), builtInStyle)
  }

  for (const customStyle of props.statusStyles) {
    const normalizedStatus = camelCase(String(customStyle.status))
    const builtInStyle = stylesMap.get(normalizedStatus)
    stylesMap.set(normalizedStatus, { ...builtInStyle, ...customStyle })
  }

  return stylesMap
})

const statusStyle = computed(() => {
  const normalizedStatus = camelCase(String(props.status))
  const configuredStyle = statusStylesMap.value.get(normalizedStatus)

  return {
    color: configuredStyle?.color ?? getColorFromHash(hashStringToNumber(normalizedStatus), FALLBACK_COLORS),
    textColor: configuredStyle?.textColor,
    icon: configuredStyle?.icon,
    label: getStatusLabel(normalizedStatus, configuredStyle)
  }
})

/** 按公开配置、内置翻译和原始值的优先级解析显示标签。 */
function getStatusLabel(normalizedStatus: string, configuredStyle?: IStatusChipItem): string {
  if (configuredStyle?.labelKey) return t(configuredStyle.labelKey)
  if (configuredStyle?.label !== undefined) return configuredStyle.label
  if (builtInStatusKeys.has(normalizedStatus)) {
    return t(`${STATUS_TRANSLATION_KEY_PREFIX}.${normalizedStatus}` as LangKey)
  }

  return String(props.status)
}

/** 将状态文本转换为稳定的数值哈希，用于选择未知状态的颜色。 */
function hashStringToNumber(statusText: string): number {
  let hash = 0
  for (let index = 0; index < statusText.length; index++) {
    hash = statusText.charCodeAt(index) + ((hash << 5) - hash)
  }
  return hash
}

/** 从候选色板中稳定选择一个颜色。 */
function getColorFromHash(hash: number, colors: readonly string[]): string {
  const index = Math.abs(hash) % colors.length
  return colors[index] as string
}
</script>
