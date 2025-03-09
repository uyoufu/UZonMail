<template>
  <q-chip v-bind="$attrs" square dense :color="statusStyle.color" :text-color="statusStyle.textColor"
    :label="statusStyle.label">
    <slot name="default"></slot>
  </q-chip>
</template>

<script lang="ts" setup>
import { useI18n } from 'vue-i18n'
const { t } = useI18n()

import { IStatusChipItem } from './types'
const defaultStatusStyles = [
  { status: 'created', label: '新建', color: 'primary', textColor: 'white', icon: '' },
  { status: 'pending', label: '等待中', color: 'accent', textColor: 'white', icon: '' },
  { status: 'sending', label: '发送中', color: 'secondary', textColor: 'white', icon: '' },
  { status: 'success', label: '成功', color: 'secondary', textColor: 'white', icon: '' },
  { status: 'failed', label: '失败', color: 'negative', textColor: 'white', icon: '' },
  { status: 'pause', label: '暂停', color: 'orange', textColor: 'white', icon: '' },
  { status: 'stopped', label: '已停止', color: 'grey', textColor: 'white', icon: '' },
  { status: 'finish', label: '完成', color: 'secondary', textColor: 'white', icon: '' },
  { status: 'cancel', label: '取消', color: 'grey', textColor: 'white', icon: '' },
  { status: 'true', label: '是', color: 'positive', textColor: 'white', icon: '' },
  { status: 'false', label: '否', color: 'negative', textColor: 'white', icon: '' },
  { status: 'independent', label: '独立', color: 'primary', textColor: 'white', icon: '' },
  { status: 'subUser', label: '子账户', color: 'negative', textColor: 'white', icon: '' },
  { status: 'normal', label: '正常', color: 'primary', textColor: 'white', icon: '' },
  { status: 'forbiddenLogin', label: '禁用', color: 'negative', textColor: 'white', icon: '' },
  { status: 'read', label: '已读', color: 'positive', textColor: 'white', icon: '' },
  { status: 'instant', label: '即时', color: 'primary', textColor: 'white', icon: '' },
  { status: 'scheduled', label: '定时', color: 'warning', textColor: 'white', icon: '' },
  { status: 'invalid', label: '无效', color: 'negative', textColor: 'white', icon: '' },
  { status: 'unsubscribed', label: '取消订阅', color: 'negative', textColor: 'white', icon: '' },
  { status: 'running', label: '运行中', color: 'secondary', textColor: 'white', icon: '' },
  { status: 'unknown', label: '未知', color: 'negative', textColor: 'white', icon: '' }
]
const props = defineProps({
  status: {
    type: [String, Number, Boolean],
    required: true
  },

  statusStyles: {
    type: Array as PropType<IStatusChipItem[]>,
    default: () => []
  }
})

// 将 props.statusStyles 进行格式化
const colors = ['primary', 'secondary', 'accent', 'negative', 'info', 'warning', 'positive', 'white', 'grey-3']
const statusStylesMap = computed(() => {
  const result: Record<string, IStatusChipItem> = {}

  const fullStatusStyles = [...defaultStatusStyles, ...props.statusStyles]
  for (let i = 0; i < fullStatusStyles.length; i++) {
    const item = fullStatusStyles[i]

    // 修改颜色
    if (!item.color) {
      item.color = colors[i % colors.length]
    }
    if (!item.textColor) {
      item.textColor = 'white'
    }
    if (!item.label) item.label = String(item.status)
    result[String(item.status).toLowerCase()] = item
  }
  return result
})

import _ from 'lodash'
const statusStyle = computed(() => {
  const statusStr = String(props.status).toLowerCase()
  const statusLabel = t(`statusChip.${String(props.status)}`)

  const statusMap = statusStylesMap.value[statusStr]
  if (!statusMap) {
    return {
      status: 'unknown',
      color: 'negative',
      label: status.toUpperCase(),
      textColor: 'white'
    }
  }

  // 克隆一个新对象，避免修改原对象
  const result = Object.assign({}, statusMap, { label: statusLabel || statusMap.label })
  return result
})
</script>

<style lang="scss" scoped></style>
