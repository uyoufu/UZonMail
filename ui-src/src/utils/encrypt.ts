import CryptoJS from 'crypto-js'

/**
 * 使用 ase 加密
 * @param key 是一个16进制的字符串
 * @param iv 是一个16进制的字符串
 * @param data 是一个字符串或者对象
 * @returns
 */
export function aes (key: string, iv: string, data: string | object) {
  if (typeof data === 'object') {
    // 如果传入的data是json对象，先转义为json字符串
    try {
      data = JSON.stringify(data)
    } catch (error) {
      console.log('error:', error)
    }
  }
  if (!data) return ''

  // 统一将传入的字符串转成UTF8编码
  const dataWordArray = CryptoJS.enc.Utf8.parse(data as string) // 需要加密的数据
  const keyHex = CryptoJS.enc.Utf8.parse(key) // 秘钥
  const ivHex = CryptoJS.enc.Utf8.parse(iv) // 偏移量
  const encrypted = CryptoJS.AES.encrypt(dataWordArray, keyHex, {
    iv: ivHex,
    mode: CryptoJS.mode.CBC, // 加密模式
    padding: CryptoJS.pad.Pkcs7
  })
  const encryptedVal = encrypted.ciphertext.toString()
  return encryptedVal //  返回加密后的值
}

/**
 * 解密数据
 * @param key
 * @param iv
 * @param ciphertext
 * @returns
 */
export function deAes (key: string, iv: string, ciphertext: string) {
  // console.log('解密：', key, iv, ciphertext)
  /*
    传入的key和iv需要和加密时候传入的key一致
  */

  // 统一将传入的字符串转成UTF8编码
  const encryptedHexStr = CryptoJS.enc.Hex.parse(ciphertext)
  ciphertext = CryptoJS.enc.Base64.stringify(encryptedHexStr)
  const keyHex = CryptoJS.enc.Utf8.parse(key) // 秘钥
  const ivHex = CryptoJS.enc.Utf8.parse(iv) // 偏移量
  const decrypt = CryptoJS.AES.decrypt(ciphertext, keyHex, {
    iv: ivHex,
    mode: CryptoJS.mode.CBC,
    padding: CryptoJS.pad.Pkcs7
  })
  const result = decrypt.toString(CryptoJS.enc.Utf8)
  // console.log('结果:', result)
  return result
}

/**
 * 计算 sha256
 * @param data
 * @returns
 */
export function sha256 (data: string) {
  return CryptoJS.SHA256(data).toString()
}

/**
 * md5 加密
 * @param data
 * @returns
 */
export function md5 (data: string) {
  return CryptoJS.MD5(data).toString()
}

/**
 * 使用明文的 password 生成 SmtpPasswordSecretKeys
 * @param password
 * @returns
 */
export function getSmtpPasswordSecretKeys (password: string) {
  const md5Password = md5(password)
  const key = md5Password
  if (!key || key.length < 16) {
    return {
      key,
      iv: key
    }
  }

  return {
    key, iv: key.substring(0, 16)
  }
}
