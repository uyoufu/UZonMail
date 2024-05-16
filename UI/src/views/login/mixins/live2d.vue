<script>
export default {
  mounted() {
    const waifu = document.getElementById('waifu')
    if (!waifu) {
      this.loadJs()
      this.loadCss()
    }
  },
  methods: {
    loadJs() {
      const s = document.createElement('script')
      s.type = 'text/javascript'
      s.src =
        'https://cdn.jsdelivr.net/gh/stevenjoezhang/live2d-widget@latest/autoload.js'
      document.body.appendChild(s)
    },
    loadCss() {
      const s = document.createElement('link')
      s.rel = 'stylesheet'
      s.href =
        'https://cdn.jsdelivr.net/npm/font-awesome/css/font-awesome.min.css'
      document.body.appendChild(s)
    }
  }
}
</script>
