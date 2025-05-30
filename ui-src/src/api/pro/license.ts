
import { httpClientPro } from 'src/api//base/httpClient'

export enum LicenseType {
  None,
  /// <summary>
  /// 社区版本，免费
  /// </summary>
  Community,

  /// <summary>
  /// 专业版
  /// </summary>
  Professional,

  /// <summary>
  /// 企业版
  /// </summary>
  Enterprise,
}

export interface ILicenseInfo {
  /// <summary>
  /// 授权码
  /// </summary>
  licenseKey?: string,

  /// <summary>
  /// 激活时间
  /// </summary>
  activeDate: string,

  /// <summary>
  /// 过期时间
  /// </summary>
  expireDate: string,

  /// <summary>
  /// 最近更新日期
  /// </summary>
  lastUpdateDate: string,

  /// <summary>
  /// 授权类型
  /// </summary>
  licenseType: LicenseType
}

/**
 * 更新授权信息
 * @param licenseCode
 * @returns
 */
export function updateLicenseInfo (licenseCode: string) {
  return httpClientPro.put<ILicenseInfo>('/license', {
    params: {
      licenseCode
    }
  })
}

/**
 * 重新从服务器获取授权信息
 * 该接口只有管理员才能调用
 */
export function updateExistingLicenseInfo () {
  return httpClientPro.put<ILicenseInfo>('/license/exist')
}

/**
 * 获取授权信息
 * @returns
 */
export function getLicenseInfo () {
  return httpClientPro.get<ILicenseInfo>('/license')
}

/**
 * 移除授权信息
 * @returns
 */
export function removeLicense () {
  return httpClientPro.delete<ILicenseInfo>('/license')
}

