<template>
  <q-splitter v-model="splitterModel" class="email-selector">
    <template #before>
      <div class="q-pa-xs">
        <q-tree
          :nodes="groupsData"
          node-key="_id"
          selected-color="primary"
          label-key="name"
          :selected.sync="selectedNode"
          no-connectors
          tick-strategy="leaf"
          :ticked.sync="tickedNodes"
        />
      </div>
    </template>

    <template #after>
      <q-tab-panels
        v-model="selectedNode"
        animated
        transition-prev="jump-up"
        transition-next="jump-up"
        style="height: 100%"
      >
        <q-tab-panel
          v-for="group in groupsOrigin"
          :key="group._id"
          :name="group._id"
          class="q-pa-none"
          style="height: 100%"
        >
          <EmailTable v-model="tickedUsers" :group="group" />
        </q-tab-panel>
      </q-tab-panels>
    </template>
  </q-splitter>
</template>

<script>
import EmailTable from './emailsTable.vue'

import { getGroups } from '@/api/group'
import LTT from '@/utils/list2tree'

export default {
  components: { EmailTable },

  props: {
    groupType: {
      type: String,
      default: 'send'
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
      splitterModel: 30,
      groupsOrigin: [],
      dataTree: [],
      selectedNode: '',

      tickedNodes: this.value
        .filter(v => v.type === 'group')
        .map(item => item._id),
      tickedUsers: this.value.filter(v => v.type !== 'group')
    }
  },
  computed: {
    groupsData() {
      return this.dataTree
    }
  },
  watch: {
    tickedNodes(val) {
      // 转换数据格式
      const results = this.groupsOrigin
        .filter(g => val.findIndex(t => t === g._id) > -1)
        .map(g => {
          return {
            type: 'group',
            _id: g._id,
            label: g.name
          }
        })

      results.push(...this.tickedUsers)

      console.log('tickedNodes:', results)
      this.$emit('input', results)
    },

    tickedUsers(val) {
      console.log('tickedUsers:', val)
      const results = this.groupsOrigin
        .filter(g => this.tickedNodes.findIndex(t => t === g._id) > -1)
        .map(g => {
          return {
            type: 'group',
            _id: g._id,
            label: g.name
          }
        })

      results.push(...val)

      this.$emit('input', results)
    }
  },
  created() {
  },
  async mounted() {
    console.log('this.value:', this.value)
    // 获取所有的组
    const res = await getGroups(this.groupType)

    this.groupsOrigin = res.data
    // 选择第一个
    if (this.groupsOrigin && this.groupsOrigin.length > 0) { this.selectedNode = this.groupsOrigin[0]._id }
    this.computeDataTree()
  },
  methods: {
    computeDataTree() {
      // This method is responsible for computing the tree structure
      const ltt = new LTT(this.groupsOrigin, {
        key_id: '_id',
        key_parent: 'parentId',
        key_child: 'children',
        empty_children: true
      })

      this.dataTree = ltt.GetTree()
    }
  }
}
</script>

<style lang="scss">
.email-selector {
  width: 600px;
  height: 400px;
  background: white;
}
</style>
