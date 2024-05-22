<template>
  <q-dialog ref="dialog" @hide="onDialogHide">
    <q-card class="q-dialog-plugin">
      <!--
        ...内容
        ...使用q-card-section展现它?
      -->
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
              tick-strategy="strict"
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
              class="q-pa-none column"
              style="height: 100%"
            >
              <EmailTable
                v-model="tickedUsers"
                :group="group"
                style="flex: 1"
              />
            </q-tab-panel>
          </q-tab-panels>
        </template>
      </q-splitter>

      <!-- 按钮示例 -->
      <q-card-actions align="right">
        <q-btn color="negative" :label="$t('cancel')" dense @click="onCancelClick" />
        <q-btn color="primary" :label="$t('confirm')" dense @click="onOKClick" />
      </q-card-actions>
    </q-card>
  </q-dialog>
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

      selectedNode: '',

      tickedNodes: this.value
        .filter(v => v.type === 'group')
        .map(item => item._id),
      tickedUsers: this.value.filter(v => v.type !== 'group')
    }
  },

  computed: {
    groupsData() {
      // 将所有的组解析成树的结构
      const ltt = new LTT(this.groupsOrigin, {
        key_id: '_id',
        key_parent: 'parentId',
        key_child: 'children',
        empty_children: true
      })

      // this.dataTree = ltt

      return ltt.GetTree()
    },

    // 选择的结果数据
    ticketResults() {
      // 转换数据格式
      const results = this.groupsOrigin
        .filter(g => this.tickedNodes.findIndex(t => t === g._id) > -1)
        .map(g => {
          return {
            type: 'group',
            _id: g._id,
            label: g.name
          }
        })

      results.push(...this.tickedUsers)

      console.log('tickedNodes:', results)
      return results
    }
  },

  // 原模块的v-model使用
  // watch: {
  //   tickedNodes(val) {
  //     // 转换数据格式
  //     const results = this.groupsOrigin
  //       .filter(g => val.findIndex(t => t === g._id) > -1)
  //       .map(g => {
  //         return {
  //           type: 'group',
  //           _id: g._id,
  //           label: g.name
  //         }
  //       })

  //     results.push(...this.tickedUsers)

  //     console.log('tickedNodes:', results)
  //     this.$emit('input', results)
  //   },

  //   tickedUsers(val) {
  //     console.log('tickedUsers:', val)
  //     const results = this.groupsOrigin
  //       .filter(g => this.tickedNodes.findIndex(t => t === g._id) > -1)
  //       .map(g => {
  //         return {
  //           type: 'group',
  //           _id: g._id,
  //           label: g.name
  //         }
  //       })

  //     results.push(...val)

  //     this.$emit('input', results)
  //   }
  // },

  async mounted() {
    console.log('this.value:', this.value)
    // 获取所有的组
    const res = await getGroups(this.groupType)

    this.groupsOrigin = res.data
    // 选择第一个
    if (this.groupsOrigin && this.groupsOrigin.length > 0) {
      this.selectedNode = this.groupsOrigin[0]._id
    }
  },

  methods: {
    // 以下方法是必需的
    // (不要改变它的名称 --> "show")
    show() {
      this.$refs.dialog.show()
    },

    // 以下方法是必需的
    // (不要改变它的名称 --> "hide")
    hide() {
      this.$refs.dialog.hide()
    },

    onDialogHide() {
      // QDialog发出“hide”事件时
      // 需要发出
      this.$emit('hide')
    },

    onOKClick() {
      // 按OK，在隐藏QDialog之前
      // 发出“ok”事件（带有可选的有效负载）
      // 是必需的
      this.$emit('ok', { data: this.ticketResults })
      // 或带有有效负载：this.$emit('ok', { ... })

      // 然后隐藏对话框
      this.hide()
    },

    onCancelClick() {
      // 我们只需要隐藏对话框
      this.hide()
    }
  }
}
</script>

<style lang='scss'>
.q-dialog-plugin {
  max-width: none !important;
  width: 600px;

  .email-selector {
    background: white;
    height: 400px;
  }
}
</style>
