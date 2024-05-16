<template>
  <q-table
    row-key="_id"
    :data="data"
    :columns="columns"
    :pagination.sync="pagination"
    :loading="loading"
    :filter="filter"
    dense
    binary-state-sort
    virtual-scroll
    class="full-heigth"
    selection="multiple"
    :selected.sync="selected"
    @request="initQuasarTable_onRequest"
  >
    <template #top>
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
    </template>
  </q-table>
</template>

<script>
import { getEmailsCount, getEmails } from '@/api/group'
import mixin_initQTable from '@/mixins/initQtable.vue'

export default {
  mixins: [mixin_initQTable],

  props: {
    group: {
      type: Object,
      default() {
        return {
          groupType: 'send'
        }
      }
    },

    value: {
      type: Array,
      default() {
        return []
      }
    }
  },
  data() {
    return {
      filter: '',
      data: [],
      selected: this.value
    }
  },

  computed: {
    columns() {
      return [
        {
          name: 'userName',
          required: true,
          label: '姓名',
          align: 'left',
          field: row => row.userName,
          sortable: true
        },
        {
          name: 'email',
          required: true,
          label: '邮箱',
          align: 'left',
          field: row => row.email,
          sortable: true
        }
      ]
    }
  },

  watch: {
    selected(val) {
      console.log('selected data:', val)
      const results = val.map(u => {
        return {
          type: 'email',
          _id: u._id,
          label: u.userName
        }
      })
      this.$emit('input', results)
    }
  },

  // async mounted() {
  //   const { data } = await getEmails(this.group._id)
  //   this.data = data || []

  //   // 修改选择的数据
  //   this.selected = this.data.filter(
  //     d => this.selected.findIndex(s => s._id === d._id) > -1
  //   )
  // },

  methods: {
    // 获取筛选的数量
    // 重载 mixin 中的方法
    async initQuasarTable_getFilterCount(filterObj) {
      const res = await getEmailsCount(this.group._id, filterObj)
      return res.data || 0
    },

    // 重载 mixin 中的方法
    // 获取筛选结果
    async initQuasarTable_getFilterList(filterObj, pagination) {
      const res = await getEmails(this.group._id, filterObj, pagination)
      return res.data || []
    }
  }
}
</script>

<style>
</style>
