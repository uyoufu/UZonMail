interface Window {
  chrome?: {
    webview?: {
      hostObjects?: {
        uzonMailUpdater?: {
          beginUpdate: () => Promise<boolean>
        }
      }
    }
  }
}
