<script>
export default {
  methods: {
    jumpToTemplateEditor(templateId) {
      // 跳转到文本编辑器
      this.$router.push({
        name: 'TemplateEditor',
        query: {
          id: templateId
        }
      })
    }
  }
}
</script>
