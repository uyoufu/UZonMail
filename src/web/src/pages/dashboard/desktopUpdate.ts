interface IDesktopUpdater {
  BeginUpdate: () => Promise<boolean> | boolean
}

/** 获取 WebView 注入的桌面端更新器。 */
export function getDesktopUpdater (): IDesktopUpdater | undefined {
  return window.chrome?.webview?.hostObjects?.uzonMailUpdater
}

/** 判断当前页面是否由具备更新能力的桌面端宿主承载。 */
export function isDesktopClient (): boolean {
  return getDesktopUpdater() !== undefined
}

/** 请求桌面端宿主启动独立更新器。 */
export async function startDesktopUpdate (): Promise<boolean> {
  const updater = getDesktopUpdater()
  if (!updater) return false
  return await updater.BeginUpdate()
}
