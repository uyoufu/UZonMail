<template>
  <div>
    <q-splitter v-model="splitterModel" class="email-spliter">
      <template #before>
        <div class="q-pa-md">
          <q-tree
            :nodes="groupsData"
            node-key="_id"
            selected-color="primary"
            label-key="name"
            :selected.sync="selectedNode"
            no-connectors
            no-nodes-label="单击此处右键添加分组"
          >
            <template #default-header="prop">
              <div>
                {{ prop.node.name }}
              </div>
              <q-menu
                transition-show="scale"
                transition-hide="scale"
                touch-position
                context-menu
              >
                <q-list bordered class="rounded-borders text-teal" dense>
                  <q-item
                    v-if="!prop.node.parentId"
                    v-close-popup="2"
                    clickable
                    dense
                    @click="showNewGroupDialog(null)"
                  >
                    <q-item-section>添加组</q-item-section>
                  </q-item>

                  <q-item
                    v-close-popup="2"
                    clickable
                    dense
                    @click="showNewGroupDialog(prop.node)"
                  >
                    <q-item-section>添加子组</q-item-section>
                  </q-item>

                  <q-item
                    v-close-popup
                    clickable
                    dense
                    @click="showModifyGroupDialog(prop.node)"
                  >
                    <q-item-section>修改</q-item-section>
                  </q-item>

                  <q-item
                    v-close-popup
                    clickable
                    dense
                    @click="deleteNode(prop.node)"
                  >
                    <q-item-section>删除</q-item-section>
                  </q-item>
                </q-list>
              </q-menu>
            </template>
          </q-tree>
          <q-menu
            transition-show="scale"
            transition-hide="scale"
            touch-position
            context-menu
          >
            <q-list bordered class="rounded-borders text-primary" dense>
              <q-item
                v-if="groupsData.length === 0"
                v-close-popup
                clickable
                dense
                @click="showNewGroupDialog(null)"
              >
                <q-item-section>添加组</q-item-section>
              </q-item>
            </q-list>
          </q-menu>
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
            style="height: 100%"
          >
            <GroupEmailInfos :group="group" />
          </q-tab-panel>
        </q-tab-panels>
      </template>
    </q-splitter>

    <q-dialog v-model="isShowNewGroupDialog">
      <DialogForm
        type="create"
        :init-params="initNewGroupParams"
        @createSuccess="addNewGroup"
      />
    </q-dialog>

    <q-dialog v-model="isShowModifyGroupDialog">
      <DialogForm
        type="update"
        :init-params="initModifyGroupParams"
        @updateSuccess="modifyGroup"
      />
    </q-dialog>
  </div>
</template>

<script>
import DialogForm from '@/components/DialogForm'
import GroupEmailInfos from './groupEmailInfos.vue'

import mixinNewGroup from '../mixins/newGroup.vue'
import mixinModifyGroup from '../mixins/modifyGroup.vue'

import { getGroups, deleteGroups } from '@/api/group'

import LTT from '@/utils/list2tree'
import { notifySuccess, okCancle } from '@/components/iPrompt'

export default {
  components: { DialogForm, GroupEmailInfos },
  mixins: [mixinNewGroup, mixinModifyGroup],

  props: {
    groupType: {
      type: String,
      default: 'send'
    }
  },
  data() {
    return {
      splitterModel: 20,
      groupsOrigin: [],

      selectedNode: ''
    }
  },

  computed: {
    groupsData() {
      return this.dataTree.GetTree()
    }
  },
  watch: {
    filterTreeText(val) {
      this.$refs.groupTree.filter(val)
    }
  },

  created() {
    // Call the method to compute the data tree when the component is created
    this.computeDataTree()
  },

  async mounted() {
    // 获取所有的组
    const res = await getGroups(this.groupType)

    this.groupsOrigin = res.data
    // 选择第一个
    if (this.groupsOrigin && this.groupsOrigin.length > 0) { this.selectedNode = this.groupsOrigin[0]._id }
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
    },
    async deleteNode(data) {
      const ok = await okCancle('删除提醒', '是否删除组？')
      if (!ok) return

      // 查找当前组和其所有的子组
      const targetNodes = this.dataTree.GetCurentAndSub(data._id)

      // 开始删除
      await deleteGroups(targetNodes.map(node => node._id))

      // 删除现有的数据
      this.groupsOrigin = this.groupsOrigin.filter(
        g => targetNodes.findIndex(node => node._id === g._id) < 0
      )

      notifySuccess('删除成功')

      // 如果选中的当前节点，需要换成其它节点
      if (
        targetNodes.findIndex(node => node._id === this.selectedNode) > -1 &&
        this.groupsOrigin &&
        this.groupsOrigin.length > 0
      ) {
        this.selectedNode = this.groupsOrigin[0]._id
      }
    }
  }
}
</script>

<style>
.email-spliter {
  position: absolute;
  top: 0;
  bottom: 0;
  left: 0;
  right: 0;
}
</style>
