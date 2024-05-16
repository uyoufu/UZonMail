<template>
  <div class="column q-pa-md template-main">
    <div class="row q-gutter-sm">
      <q-btn
        dense
        color="primary"
        class="q-mb-md self-center q-pl-sm q-pr-sm"
        @click="jumpToTemplateEditor()"
      >新增</q-btn>

      <q-btn
        dense
        color="primary"
        class="q-mb-md self-center q-pl-sm q-pr-sm"
        @click="selectFile"
      >导入</q-btn>
      <input
        id="fileInput"
        type="file"
        style="display: none"
        accept="text/html"
        @change="fileSelected"
      >
    </div>

    <div class="row q-gutter-sm">
      <q-card
        v-for="temp in data"
        :key="temp._id"
        flat
        bordered
        class="q-pa-xs column"
        style="width: 400px; max-height: 300px"
      >
        <div class="text-overline">{{ temp.name }}</div>

        <q-img class="rounded-borders template-image" :src="temp.imageUrl" />

        <div class="row justify-between q-mt-sm">
          <div>创建时间:{{ temp.createDate | formatDate }}</div>
          <div class="row q-gutter-sm">
            <q-btn
              color="primary"
              class="self-center"
              size="sm"
              dense
              @click="jumpToTemplateEditor(temp._id)"
            >编辑</q-btn>
            <q-btn
              color="primary"
              class="self-center"
              size="sm"
              dense
              @click="viewTemplate(temp.imageUrl)"
            >查看</q-btn>
            <q-btn
              color="negative"
              class="self-center"
              size="sm"
              dense
              @click="deleteTemplate(temp._id)"
            >删除</q-btn>
          </div>
        </div>
      </q-card>
    </div>

    <q-dialog v-model="isShowTemplateDialog" persistent>
      <q-card style="max-width: none">
        <q-layout
          view="lHh lpr lFf"
          container
          style="height: 400px; width: 600px"
          class="shadow-2 rounded-borders"
        >
          <q-header elevated class="bg-teal">
            <div class="text-subtitle1 q-pa-sm">{{ selectedFileName }}</div>
          </q-header>

          <q-footer elevated class="bg-teal">
            <div class="row justify-end q-ma-sm q-gutter-sm">
              <q-btn v-close-popup color="warning" size="sm">取消</q-btn>
              <q-btn
                color="primary"
                size="sm"
                :loading="isSavingTemplate"
                @click="confirmTemplate"
              >确认</q-btn>
            </div>
          </q-footer>

          <q-page-container>
            <div
              id="capture"
              style="background-color: white"
              v-html="templateHtml"
            />
          </q-page-container>
        </q-layout>
      </q-card>
    </q-dialog>
  </div>
</template>

<script>
import toImage from '@/utils/html2image'

import { newTemplate, getTemplates, deleteTemplate } from '@/api/template'
import moment from 'moment'
import { notifySuccess, okCancle } from '@/components/iPrompt'

import 'viewerjs/dist/viewer.css'
import { api as viewerApi } from 'v-viewer'

import ModifyTemplate from './mixins/modifyTemplate.vue'

export default {
  filters: {
    formatDate(date) {
      if (!date) return ''

      return moment(date).format('YYYY-MM-DD')
    }
  },
  mixins: [ModifyTemplate],
  data() {
    return {
      templateHtml: '',
      isShowTemplateDialog: false,
      selectedFileName: '',
      data: [],
      isSavingTemplate: false
    }
  },

  async mounted() {
    const res = await getTemplates()
    this.data = res.data
  },

  methods: {
    // 选择文件
    selectFile() {
      const elem = document.getElementById('fileInput')
      elem.click()
      elem.value = ''
    },

    async fileSelected(e) {
      console.log('fileSelected:', e)
      // 判断是否选择了文件
      if (e.target.files.length === 0) {
        return
      }

      // 获取选择的文件
      const file = e.target.files[0]
      this.selectedFileName = file.name
      this.templateHtml = await this.readExcelData(file)
    },

    async readExcelData(file) {
      return new Promise((resolve, reject) => {
        const reader = new FileReader()
        reader.onload = e => {
          // 获取html
          const result = e.target.result
          this.isShowTemplateDialog = true
          resolve(result)
        }
        reader.onerror = () => {
          reject(false)
        }

        reader.readAsText(file)
      })
    },

    // 确认邮件
    async confirmTemplate() {
      this.isSavingTemplate = true

      this.$nextTick(async() => {
        const imageUrl = await toImage(document.getElementById('capture'))

        // 发送模板
        const res = await newTemplate(
          this.selectedFileName,
          imageUrl,
          this.templateHtml
        )

        // 添加到集合
        this.data.push(res.data)

        notifySuccess('添加成功')

        this.isSavingTemplate = false
        this.isShowTemplateDialog = false
      })
    },

    // 查看邮件
    viewTemplate(imageUrl) {
      // 在新标签中打开
      viewerApi({
        options: {
          title: false
        },
        images: [imageUrl]
      })
    },

    // 删除邮件
    async deleteTemplate(id) {
      const ok = await okCancle('是否删除模板？')
      if (!ok) return

      const res = await deleteTemplate(id)
      console.log('deleteTemplate:', res)
      // 删除成功后，清除显示
      const index = this.data.findIndex(d => d._id === id)
      if (index > -1) this.data.splice(index, 1)

      notifySuccess('删除成功')
    }
  }
}
</script>

<style lang='scss'>
.template-main {
  .template-image {
    flex: 1;
  }

  .template-editor-dialog {
    max-width: none;
    .q-dialog__inner {
      max-width: none !important;
    }
  }
}
</style>
