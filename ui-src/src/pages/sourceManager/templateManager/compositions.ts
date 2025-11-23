import { getEmailTemplatesCount, getEmailTemplatesData } from 'src/api/emailTemplate'
import { useQTable } from 'src/compositions/qTableUtils'
import type { IRequestPagination, TTableFilterObject } from 'src/compositions/types'
import type { QBtnDropdown, QEditor } from 'quasar'
import { useQuasar } from 'quasar'

// 查看缩略图
import 'viewerjs/dist/viewer.css'
import { api as viewerApi } from 'v-viewer'
import { useConfig } from 'src/config'

// 生成邮件模板数据
export function useEmailTemplateTable () {
  async function getRowsNumberCount (filterObj: TTableFilterObject) {
    const { data } = await getEmailTemplatesCount(filterObj.filter)
    return data
  }
  async function onRequest (filterObj: TTableFilterObject, pagination: IRequestPagination) {
    const { data } = await getEmailTemplatesData(filterObj.filter as string, pagination)
    return data
  }

  const { pagination, ...others } = useQTable({
    getRowsNumberCount,
    onRequest
  })

  const config = useConfig()
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  function getTemplateImage (row: Record<string, any>) {
    if (!row) return '/icons/undraw_mailbox_re_dvds.svg'
    const url = `${config.baseUrl}/${row.thumbnail}`
    // console.log(url)
    return url
  }

  // 查看缩略图
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  function onPreviewThumbnail (row: Record<string, any>) {
    const imageUrl = getTemplateImage(row)

    viewerApi({
      options: {
        title: false,
        transition: true,
        navbar: false,
        zIndex: 9999
      },
      images: [imageUrl]
    })
  }

  return {
    pagination,
    getTemplateImage,
    onPreviewThumbnail,
    ...others
  }
}

/**
 * 获取 WYSIWYG (what you see is what you get) 编辑器配置
 */
export function useWysiwygEditor () {
  const editorDefinitions = {}
  const $q = useQuasar()
  const editorToolbar = [
    [{
      label: $q.lang.editor.align,
      icon: $q.iconSet.editor.align,
      fixedLabel: true,
      list: 'only-icons',
      options: ['left', 'center', 'right', 'justify']
    },
    // {
    //   label: $q.lang.editor.formatting,
    //   icon: $q.iconSet.editor.formatting,
    //   list: 'no-icons',
    //   options: ['p', 'code', 'h6', 'h5', 'h4', 'h3', 'h2', 'h1']
    // },
    {
      label: $q.lang.editor.fontSize,
      icon: $q.iconSet.editor.fontSize,
      fixedLabel: true,
      fixedIcon: true,
      list: 'no-icons',
      options: [
        'size-1',
        'size-2',
        'size-3',
        'size-4',
        'size-5',
        'size-6',
        'size-7'
      ]
    },
      'textColor'
    ],
    // ['left', 'center', 'right', 'justify'],
    ['bold', 'italic', 'strike', 'underline', 'link'],
    ['removeFormat', 'undo', 'redo'],
    ['viewsource']
  ]

  const foreColor = ref('#000000')
  const highlightColor = ref('#ffff00aa')
  const editorRef: Ref<QEditor | null> = ref(null)
  const textColorDropdownRef: Ref<QBtnDropdown | null> = ref(null)

  function setColor (cmd: string, name: string) {
    if (!textColorDropdownRef.value) return
    if (!editorRef.value) return
    textColorDropdownRef.value.hide()
    editorRef.value.runCmd(cmd, name)
    editorRef.value.focus()
  }

  const foreColorPalette = [
    '#ff0000',
    '#ff8000',
    '#ffff00',
    '#00ff00',
    '#00ff80',
    '#00ffff',
    '#0080ff',
    '#0000ff',
    '#8000ff',
    '#ff00ff'
  ]
  const highlightColorPalette = [
    '#ffccccaa',
    '#ffe6ccaa',
    '#ffffccaa',
    '#ccffccaa',
    '#ccffe6aa',
    '#ccffffaa',
    '#cce6ffaa',
    '#ccccffaa',
    '#e6ccffaa',
    '#ffccffaa',
    '#ff0000aa',
    '#ff8000aa',
    '#ffff00aa',
    '#00ff00aa',
    '#00ff80aa',
    '#00ffffaa',
    '#0080ffaa',
    '#0000ffaa',
    '#8000ffaa',
    '#ff00ffaa'
  ]
  return {
    editorDefinitions,
    editorToolbar,
    foreColor,
    highlightColor,
    editorRef,
    textColorDropdownRef,
    setColor,
    foreColorPalette,
    highlightColorPalette
  }
}

