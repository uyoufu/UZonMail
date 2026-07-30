interface Window {
  chrome?: {
    webview?: {
      hostObjects?: {
        sync?: {
          uzonMailLocale?: {
            GetCurrentLocale: () => string
          }
        }
        uzonMailUpdater?: {
          BeginUpdate: () => Promise<boolean>
        }
        uzonMailLocale?: {
          SetCurrentLocale: (locale: string) => Promise<boolean>
        }
      }
    }
  }
}
