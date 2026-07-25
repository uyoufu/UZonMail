import { httpClient } from 'src/api/base/httpClient'

export type FileCategoryPlacement = 'Before' | 'After' | 'Inside'

export interface IFileCategory {
  id: number
  parentId?: number
  name: string
  sort: number
  isDefault: boolean
}

/** 获取当前用户的完整文件分类树数据。 */
export function getFileCategories () {
  return httpClient.get<IFileCategory[]>('/file-category/all')
}

/** 创建文件分类。 */
export function createFileCategory (name: string, parentId?: number) {
  return httpClient.post<IFileCategory>('/file-category', {
    data: { name, parentId }
  })
}

/** 重命名文件分类。 */
export function renameFileCategory (categoryId: number, name: string) {
  return httpClient.put<IFileCategory>(`/file-category/${categoryId}`, {
    data: { name }
  })
}

/** 移动文件分类。 */
export function moveFileCategory (categoryId: number, targetCategoryId: number, placement: FileCategoryPlacement) {
  return httpClient.put<boolean>(`/file-category/${categoryId}/position`, {
    data: { targetCategoryId, placement }
  })
}

/** 删除空文件分类。 */
export function deleteFileCategory (categoryId: number) {
  return httpClient.delete<boolean>(`/file-category/${categoryId}`)
}
