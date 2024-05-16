<template>
  <div :class="{ fullscreen: fullscreen }" class="tinymce-container">
    <div id="html2image" />
    <textarea :id="tinymceId" class="tinymce-textarea" />
    <!-- <div class="editor-custom-btn-container">
      <editorImage color="#1890ff" class="editor-upload-btn" @successCBK="imageSuccessCBK" />
    </div>-->
  </div>
</template>

<script>
/**
 * docs:
 * https://panjiachen.github.io/vue-element-admin-site/feature/component/rich-editor.html#tinymce
 */
// import editorImage from './components/EditorImage'
import plugins from './plugins'
import toolbar from './toolbar'
import load from './dynamicLoadScript'

// why use this cdn, detail see https://github.com/PanJiaChen/tinymce-all-in-one
// const tinymceCDN = 'https://cdn.jsdelivr.net/npm/tinymce-all-in-one@4.9.5/tinymce.min.js'

// 有跟新闻的tiny有冲突,所以使用本地tiny test
const tinymceCDN = '/tinymce/tinymce.min.js'

import Template from './mixins/template.vue'

export default {
  name: 'Tinymce',
  // components: { editorImage },
  mixins: [Template],

  data() {
    return {
      hasInit: false,
      tinymceId: 'vue-tinymce-const-id' + Date.now(),
      fullscreen: false,

      lastSavedData: ''
    }
  },

  mounted() {
    this.init()
  },

  activated() {
    if (window.tinymce) {
      this.initTinymce()
    }
  },
  deactivated() {
    this.destroyTinymce()
  },
  destroyed() {
    this.destroyTinymce()
  },
  methods: {
    init() {
      // dynamic load tinymce from cdn
      load(tinymceCDN, err => {
        if (err) {
          this.$message.error(err.message)
          return
        }
        this.initTinymce()
      })
    },
    initTinymce() {
      const _this = this

      window.tinymce.init({
        width: '100%', //  设置富文本编辑器宽度
        height: '100%', //  设置富文本编辑器高度
        menubar: false, // 设置富文本编辑器菜单, 默认true
        branding: false, // 关闭底部官网提示 默认true
        statusbar: true, // 显示底部状态栏 默认true
        resize: false, // 调节编辑器大小 默认 true
        selector: `#${this.tinymceId}`,
        language: 'zh_CN',
        // language_url: 'https://cdn.jsdelivr.net/npm/tinymce-lang/langs/zh_CN.js', // site absolute URL

        // body_class: 'panel-body',
        // object_resizing: true, // 是否禁用表格图片大小调整
        toolbar: toolbar, // 分组工具栏控件
        // 配置菜单
        menu: {
          custom: {
            title: 'file',
            items:
              'newTemplate | saveTemplate saveAsTemplate | preview print | exitEditor'
          }
        },
        plugins: plugins, // 插件(比如: advlist | link | image | preview等)
        // end_container_on_empty_block: true, // enter键 分块
        // powerpaste_word_import: 'merge', // 是否保留word粘贴样式  clean | merge
        paste_data_images: true, // 设置为“true”将允许粘贴图像，而将其设置为“false”将不允许粘贴图像。

        // code_dialog_height: 450,
        // code_dialog_width: 600,

        // advlist_bullet_styles: 'square',
        // advlist_number_styles: 'default',

        // imagetools_cors_hosts: ['www.tinymce.com', 'codepen.io'],
        images_reuse_filename: true, // 它将告诉TinyMCE使用图片文件实际的文件名，而不是每次随即生成一个新的
        image_advtab: true, // 图片高级设置
        image_caption: true,

        // default_link_target: '_blank',
        // link_title: false,
        // nonbreaking_force_tab: true, // inserting nonbreaking space &nbsp; need Nonbreaking Space Plugin

        // autoresize_bottom_margin: 50,
        // autoresize_max_height: this.maxHeight, // 编辑区域的最大高
        // autoresize_min_height: 400, // 编辑区域的最小高度
        // autoresize_on_init: true,
        // autoresize_overflow_padding: 10,

        // // skin_url: 'https://cdn.jsdelivr.net/npm/tinymce-all-in-one@4.9.5/skins/lightgray/skin.min.css',
        // 初始化完成后调用
        init_instance_callback: editor => {
          this.hasInit = true

          _this.getTemplateData()
        },

        // 加载完成后，添加自定义菜单
        setup(editor) {
          editor.on('FullscreenStateChanged', e => {
            _this.fullscreen = e.state
          })

          editor.ui.registry.addMenuItem('newTemplate', {
            text: '新建',
            icon: 'new-document',
            onAction: _this.newTemplate.bind(_this)
          })

          editor.ui.registry.addMenuItem('saveTemplate', {
            text: '保存',
            icon: 'save',
            onAction: _this.saveTemplate.bind(_this)
          })

          editor.ui.registry.addMenuItem('saveAsTemplate', {
            text: '另存为',
            icon: 'save',
            onAction: _this.saveAsTemplate.bind(_this)
          })

          editor.ui.registry.addMenuItem('exitEditor', {
            text: '退出',
            icon: 'close',
            onAction: _this.exitEditor.bind(_this)
          })
        }

        // images_upload_handler: _this.images_upload_handler
      })
    },

    // 上传有问题，今后处理
    async images_upload_handler(blobInfo, success, failure, progress) {
      progress(0)
      // 将 blob 转成 dataUrl
      console.log('images_upload_handler:')

      const dataUrl = await this.fileReader(blobInfo.blob())
      success(dataUrl)
    },

    // blob 转 dataUrl
    fileReader(blob) {
      return new Promise((resolve, reject) => {
        const reader = new FileReader()
        reader.onload = e => {
          resolve(e.target.result)
        }
        reader.readAsDataURL(blob)
      })
    },

    destroyTinymce() {
      const tinymce = window.tinymce.get(this.tinymceId)
      if (this.fullscreen) {
        tinymce.execCommand('mceFullScreen')
      }

      if (tinymce) {
        tinymce.destroy()
      }
    },

    setContent(value) {
      console.log('setContent value:')
      window.tinymce.get(this.tinymceId).setContent(value)
      this.lastSavedData = this.getContent()
    },

    getContent() {
      return window.tinymce.activeEditor.getContent()
    },

    imageSuccessCBK(arr) {
      const _this = this
      arr.forEach(v => {
        window.tinymce
          .get(_this.tinymceId)
          .insertContent(`<img class="wscnph" src="${v.url}" >`)
      })
    }
  }
}
</script>

<style lang='scss' scoped>
.tinymce-container {
  position: absolute;
  top: 0;
  left: 0%;
  bottom: 0;
  right: 0;
  padding: 10px;

  #html2image {
    position: absolute;
    z-index: -2;
    height: 100px;
    overflow: hidden;
  }
}
.tinymce-container {
  ::v-deep .mce-fullscreen {
  z-index: 10000;
}
}
.tinymce-textarea {
  visibility: hidden;
  z-index: -1;
}
.editor-custom-btn-container {
  position: absolute;
  right: 4px;
  top: 4px;
  /*z-index: 2005;*/
}
.fullscreen .editor-custom-btn-container {
  z-index: 10000;
  position: fixed;
}
.editor-upload-btn {
  display: inline-block;
}
</style>
