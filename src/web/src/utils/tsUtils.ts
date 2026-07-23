/**
 * settimeout 的异步版本
 * @param timeout
 */
export async function setTimeoutAsync (timeout: number = 0) {
  await new Promise((resolve) => {
    setTimeout(() => {
      resolve(true)
    }, timeout)
  })
}
