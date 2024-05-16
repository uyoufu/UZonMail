<template>
  <q-table v-bind="$attrs" :dense="dense" v-on="$listeners">
    <!--示例: 在封装组件中增加插槽，通过后备内容进行自定义，方便父组件覆盖当前插槽-->
    <template #top="props">
      <slot name="top" v-bind="props">
        <q-space />
        <q-input
          v-model="filter"
          dense
          debounce="300"
          placeholder="搜索"
          color="primary"
        >
          <template #append>
            <q-icon name="search" />
          </template>
        </q-input>
      </slot>
    </template>

    <!--根据父类插槽定义，传递插槽到被封装组件-->
    <template v-for="slotName in scopedSlotsName" #[slotName]="props">
      <!-- v-bind 是向插槽中传递参数，使得父类的插槽可以使用-->
      <slot :name="slotName" v-bind="props" />
    </template>
  </q-table>
</template>

<script>
import mixin_initQTable from '@/mixins/initQtable.vue'

export default {
  mixins: [mixin_initQTable],

  // 默认为 true,为 true 时会把 `$attrs` 对象上没在子组件 `props` 中声明的属性加在子组件的根 `html` 标签上。
  inheritAttrs: false,

  props: {
    // 设置组件的默认值
    dense: {
      type: Boolean,
      default() {
        return true
      }
    },

    // 数据
    data: {
      type: Array,
      default() {
        return []
      }
    }
  },

  data() {
    return {
      filter: 'filter2'
    }
  },

  computed: {
    attrs() {
      // 因为 $attrs 不包含 $props 中的值，在此处对属性进行合并，然后供被封装组件使用
      // 由于 Object.assign 是浅复制，所以不会影响字段的 getter 和 setter
      return Object.assign({}, this.$attrs, this.$props)
    },

    // 作用域内的插槽名称
    scopedSlotsName() {
      let keys = Object.keys(this.$scopedSlots)
      // 过滤掉以$开头的字段,$ 开头的是 vue 框架的值
      keys = keys.filter(key => !key.startsWith('$'))
      // 过滤掉已经添加插槽名称
      const existSlotNames = ['top']
      keys = keys.filter(key => !existSlotNames.includes(key))

      return keys
    }
  },

  watch: {
    data(newValue) {
      // 向外传递值
      this.$emit('update:data', newValue)
    }
  }
}
</script>

<style scoped>
</style>>
