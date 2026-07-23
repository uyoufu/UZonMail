<template>
  <q-input v-bind="$attrs" :dense="dense" outlined standout v-model="password" :label="label" :placeholder="placeholder"
    :type="isPwd ? 'password' : 'text'">
    <template v-if="!noIcon" v-slot:prepend>
      <q-icon name="lock" />
    </template>
    <template v-slot:append>
      <q-icon :name="isPwd ? 'visibility_off' : 'visibility'" class="cursor-pointer" @click="isPwd = !isPwd"
        :size="iconSize" />
    </template>
  </q-input>
</template>

<script lang="ts" setup>
const props = defineProps({
  dense: {
    type: Boolean,
    default: true
  },

  noIcon: {
    type: Boolean,
    default: false
  },

  label: {
    type: String,
    default: '密码'
  },

  placeholder: {
    type: String,
    default: ''
  }
})
const password = defineModel({
  type: String,
  required: true
})
const isPwd = ref(true)

const iconSize = computed(() => {
  return props.dense ? 'sm' : 'md'
})
</script>

<style lang="scss" scoped></style>
