// 滚动条样式
const contentStyle = {
  // backgroundColor: 'rgba(0,0,0,0.02)',
  // color: '#555'
  height: '100%'
}

const contentActiveStyle = {
  // backgroundColor: '#eee',
  // color: 'black'
}

// 滚动条样式
const thumbStyle = {
  right: '2px',
  borderRadius: '4px',
  backgroundColor: '#8b8b8b',
  width: '6px',
  opacity: '0.75'
}

export function useScrollAreaStyle () {
  // 滚动条样式
  return {
    contentStyle,
    contentActiveStyle,
    thumbStyle
  }
}
