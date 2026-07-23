/**
 * 自定义 q-field 参数
 * @param placeholder
 */
export function useCustomQField (placeholder: string) {
  const fieldModelValue = ref('')
  const isActive = ref(false)
  const placeholderValue = computed(() => {
    if (!fieldModelValue.value && isActive.value) {
      return placeholder
    }
    return ''
  })

  const fieldText = computed(() => {
    return fieldModelValue.value || placeholderValue.value
  })
  return {
    fieldModelValue,
    isActive,
    placeholder: placeholderValue,
    fieldText
  }
}
