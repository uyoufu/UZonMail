<script>
import { notifySuccess, okCancle } from '@/components/iPrompt'

import { upsertTemplate, getTemplate, newTemplate } from '@/api/template'
import _ from 'lodash'

import toImage from '@/utils/html2image'

export default {
  data() {
    return {
      template: {
        html: ''
      }
    }
  },

  methods: {
    hasChange() {
      // 获取内容
      const content = this.getContent()

      // console.log('hasChange:', content, this.lastSavedData)
      return content !== this.lastSavedData
    },

    async generateTemplate() {
      // 更新模板
      const template = _.cloneDeep(this.template)
      template.html = this.getContent()
      // 生成element
      const elem = document.createElement('div')
      elem.innerHTML = template.html
      const parent = document.getElementById('html2image')
      parent.appendChild(elem)
      // 生成缩略图
      template.imageUrl = await toImage(elem)
      // 生成后，删除节点
      parent.removeChild(elem)

      // console.log('generateTemplate:', this.template, template)
      return template
    },

    // 新增
    async newTemplate() {
      // 判断数据是否变化，且没有保存
      if (this.hasChange() && this.template._id) {
        const ok = await okCancle('模板已经被修改，是否保存并继续？')
        if (ok) {
          const template = await this.generateTemplate()
          // 更新
          await upsertTemplate(template._id, template)
        }
      }

      // 新建模板
      this.setContent('')
      // 清空数据
      this.template = {
        html: ''
      }
    },

    // 保存
    async saveTemplate() {
      // 检查是否是否有id，如果有id，调用保存，否则让用户输入邮件的主题
      if (this.template._id) {
        const ok = await okCancle('是否保存数据?')
        if (!ok) return

        // 开始保存
        const template = await this.generateTemplate()

        // 更新
        const newTempRes = await upsertTemplate(template._id, template)
        this.template = newTempRes.data
      } else {
        // 打开保存框，让用户输入主题
        const title = await this.inputTemplateName()
        if (!title) return

        // 开始保存
        const template = await this.generateTemplate()
        // 添加名称
        template.name = title
        // 更新
        const newTempRes = await newTemplate(
          template.name,
          template.imageUrl,
          template.html
        )
        this.template = newTempRes.data
      }

      notifySuccess('保存成功！')
    },

    inputTemplateName() {
      return new Promise((resolve, reject) => {
        this.$q
          .dialog({
            title: '输入名称',
            message: '请输入模板的名称：',
            prompt: {
              model: '',
              type: 'text' // optional
            },
            persistent: true,
            ok: {
              dense: true,
              color: 'warning'
            },
            cancel: {
              dense: true,
              color: 'primary'
            }
          })
          .onOk(data => {
            resolve(data)
          })
          .onCancel(() => {
            reject(false)
          })
          .onDismiss(() => {
            reject(false)
          })
      })
    },

    // 另存为
    // 新增保存
    async saveAsTemplate() {
      // 直接另存为
      // 打开保存框，让用户输入主题
      const title = await this.inputTemplateName()
      if (!title) return

      // 开始保存
      const template = await this.generateTemplate()
      // 添加名称
      template.name = title
      // 更新
      const newTempRes = await newTemplate(
        template.name,
        template.imageUrl,
        template.html
      )
      this.template = newTempRes.data
    },

    // 退出当前
    async exitEditor() {
      // 检查变动
      if (this.hasChange() && this.template._id) {
        const ok = await okCancle('模板已经被修改，是否保存再退出？')
        if (ok) {
          const template = await this.generateTemplate()
          // 更新
          await upsertTemplate(template._id, template)
        }
      }

      this.$router.push({ name: 'Template' })
    },

    async getTemplateData() {
      // 界面中查看是否有id
      const id = this.$route.query.id
      if (!id) return

      // 获取 id 模板数据
      const res = await getTemplate(id)
      this.template = res.data
      // console.log('mounted:', this.template)

      // 将模板显示到界面上
      this.$nextTick(() => this.setContent(this.template.html))
    }
  }
}
</script>
