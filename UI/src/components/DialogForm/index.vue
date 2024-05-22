<template>
  <q-card class="q-pa-sm" style="width: 400px">
    <div class="text-subtitle1 q-mb-sm">{{ initParams.title }}</div>

    <div class="column q-gutter-sm">
      <q-input
        v-for="field in fields"
        :key="field.name"
        v-model="data[field.name]"
        clearable
        clear-icon="close"
        outlined
        :type="field.type"
        :label="field.label"
        dense
        :readonly="field.readonly"
      />

      <div class="row justify-end q-gutter-sm">
        <q-btn
          v-close-popup
          :size="btn_cancel.size"
          :color="btn_cancel.color"
          :label="btn_cancel.label"
          :dense="btn_cancel.dense"
        />

        <q-btn
          :size="btn_confirm.size"
          :color="btn_confirm.color"
          :label="btn_confirm.label"
          :dense="btn_confirm.dense"
          @click="confirm"
        />
      </div>
    </div>
  </q-card>
</template>

<script>
/**
 * 说明
 * 该模块为通用的增加和修改模块
 */

import { button } from '@/themes/index'
import { notifyError } from '@/components/iPrompt'

import _ from 'lodash'

export default {
  props: {
    initParams: {
      type: Object,
      require: true,
      default: null
    },

    // 类型，有 create,update 两种
    type: {
      type: String,
      default: 'create'
    },

    initData: {
      type: Object,
      default: null
    }
  },

  data() {
    return {
      data: {}
    }
  },

  computed: {
    btn_confirm() {
      return button.btn_confirm
    },
    btn_cancel() {
      return button.btn_cancel
    },
    fields() {
      const fields = this.initParams.fields.filter(f => !f.hidden)
      console.log('fields:', fields)
      return fields
    },

    isUpdate() {
      return this.type === 'update'
    }
  },
  created() {
    console.log('iform create data:', this.initParams)
    // 处理默认数据
    for (const field of this.initParams.fields) {
      if (field.default) {
        // 判断是否是方法
        if (typeof field.default === 'function') {
          this.$set(this.data, field.name, field.default())
        } else {
          this.$set(this.data, field.name, field.default)
        }
      }
    }

    // console.log('form created data:', this.data)

    // // 加载编辑的初始数据
    // if (this.isUpdate && this.initData) {
    //   this.data = _.cloneDeep(this.initData)

    //   for (const field of this.initParams.update.fields) {
    //     if (field.type === 'date') {
    //       this.data[field.name] = moment(this.data[field.name]).format('YYYY-MM-DD')
    //     }
    //   }
    // }
  },

  mounted() {},

  methods: {
    async confirm() {
      // 判断数据的必要性
      for (const field of this.fields) {
        if (field.required && !this.data[field.name]) {
          notifyError(`${field.label} 为空`)
          return
        }
      }

      if (!this.initParams.interceptApi && !this.initParams.api) {
        throw new Error(`需要传递 api`)
      }

      const result = await this[`${this.type}Doc`]()

      if (result.code !== 200) {
        notifyError(result.message)
      }
    },

    // 新建
    async createDoc() {
      const createData = _.cloneDeep(this.data)

      if (this.initParams.handler && this.initParams.handler.before) {
        if (!this.initParams.handler.before(createData)) {
          return {
            code: 200,
            message: '前置操作失败'
          }
        }
      }

      console.log('createDoc data:', createData)

      // 拦截api响应，直接返回数据
      if (this.initParams.interceptApi) {
        this.$emit(`${this.type}Success`, this.createData)
        return {
          code: 200,
          message: '在外部处理更新'
        }
      }

      const result = await this.initParams.api(createData)

      // 如果成功了，则提示
      if (result.code === 200) {
        // 添加完成后，清空数据
        this.data = {}

        // 关闭窗体
        this.$emit(`${this.type}Success`, result.data)
      }

      return result
    },

    // 更新
    async updateDoc() {
      // 判断是否有 _id,如果没有，不进行更新
      if (!this.data._id && !this.data.id) {
        return {
          code: 204,
          message: '没有找到更新的_id'
        }
      }

      // 整理更新的数据
      const updateData = {}
      this.initParams.fields.forEach(field => {
        if (!field.readonly) {
          updateData[field.name] = this.data[field.name]
        }
      })

      // 前置操作
      if (this.initParams.handler && this.initParams.handler.before) {
        if (!this.initParams.handler.before(updateData)) {
          return {
            code: 200,
            message: '前置操作失败'
          }
        }
      }

      // 拦截api响应，直接返回数据
      if (this.initParams.interceptApi) {
        this.$emit(
          `${this.type}Success`,
          Object.assign({}, this.data, updateData)
        )
        return {
          code: 200,
          message: '在外部处理更新'
        }
      }

      // console.log('updatedoc:', updateData, this.data)
      const result = await this.initParams.api(
        updateData._id || updateData.id,
        updateData
      )

      // 如果成功了，则提示
      if (result.code === 200) {
        // console.log(`${this.type}Success`, updateData)

        // 关闭窗体
        this.$emit(`${this.type}Success`, updateData, result.data)

        // 添加完成后，清空数据
        this.data = {}
      }

      return result
    }
  }
}
</script>

<style>
</style>
