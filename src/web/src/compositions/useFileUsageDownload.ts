import { useFileSystemAccess } from '@vueuse/core'
import { getFileReaderId, getFileStreamByReaderId } from 'src/api/fileReader'
import { useConfig } from 'src/config'
import { notifySuccess } from 'src/utils/dialog'
import { saveFileSmart } from 'src/utils/file'
import { useI18n } from 'vue-i18n'

export interface IDownloadableFileUsage {
  id: number
  displayName: string
}

/**
 * 下载当前用户拥有的逻辑文件，并在支持时使用浏览器原生文件保存器。
 */
export function useFileUsageDownload() {
  const config = useConfig()
  const { t } = useI18n()

  async function downloadFileUsage(fileUsage: IDownloadableFileUsage): Promise<void> {
    const { data: fileReaderId } = await getFileReaderId(fileUsage.id)
    const extension = fileUsage.displayName.split('.').pop() || ''
    const fileSystemAccess = useFileSystemAccess({
      dataType: ref<'Text' | 'ArrayBuffer' | 'Blob'>('ArrayBuffer'),
      types: [
        {
          description: fileUsage.displayName,
          accept: { '*/*': extension ? [`.${extension}`] : [] }
        }
      ],
      excludeAcceptAllOption: true
    })

    if (fileSystemAccess.isSupported.value) {
      await fileSystemAccess.create({ suggestedName: fileUsage.displayName })
      const { data } = await getFileStreamByReaderId(fileReaderId)
      fileSystemAccess.data.value = data
      await fileSystemAccess.save()
    } else {
      await saveFileSmart(fileUsage.displayName, `${config.baseUrl}${config.api}/file-reader/${fileReaderId}/stream`)
    }
    notifySuccess(t('fileManager.downloadSuccess'))
  }

  return { downloadFileUsage }
}
