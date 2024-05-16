import html2canvas from 'html2canvas'

async function toImage(elem) {
  // 生成模板预览图
  const canvas = await html2canvas(elem, {
    scale: 1, // 缩放比例,默认为1
    allowTaint: false, // 是否允许跨域图像污染画布
    useCORS: true, // 是否尝试使用CORS从服务器加载图像
    // width: '500', //画布的宽度
    // height: '500', //画布的高度
    backgroundColor: 'white' // 画布的背景色，默认为透明
  })
  // 将canvas转为base64格式
  const imageUrl = canvas.toDataURL('image/png')

  return imageUrl
}

export default toImage
