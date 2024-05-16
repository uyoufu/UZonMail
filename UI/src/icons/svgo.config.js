module.exports = {
  plugins: [
    {
      name: 'preset-default',
      params: {
        overrides: {
          removeViewBox: false
        }
      },
      removeAttrs: {
        attrs: ['fill', 'fill-rule']
      }
    }
  ]
}
