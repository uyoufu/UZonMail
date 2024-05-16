<template>
  <wrapped-table :data="data" :columns="columns" :dense="dense">
    <template #body-cell-index="props">
      <q-td>{{ props.value }}_1</q-td>
    </template>
  </wrapped-table>
</template>

<script>
import WrappedTable from './index.vue'

export default {
  components: { WrappedTable },

  data() {
    return {
      columns: [
        {
          name: 'index',
          label: '序号',
          align: 'left',
          field: 'name'
        },
        {
          name: 'name',
          align: 'center',
          label: '名称',
          field: 'name',
          sortable: true
        }
      ],
      data: [
        {
          index: 1,
          name: 'Frozen Yogurt'
        },
        {
          index: 1,
          name: 'Ice cream sandwich'
        }
      ],

      dense: false
    }
  }
}
</script>

<style scoped>
</style>
