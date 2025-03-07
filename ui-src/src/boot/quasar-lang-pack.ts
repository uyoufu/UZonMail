import { boot } from 'quasar/wrappers'
import { Lang, QuasarLanguage } from 'quasar'
import { useSessionStorage } from '@vueuse/core'

// relative path to your node_modules/quasar/..
// change to YOUR path
const langList = import.meta.glob('/node_modules/quasar/lang/*.js')
// or just a select few (example below with only DE and FR):
// import.meta.glob('../../node_modules/quasar/lang/(de|fr).js')

export default boot(async () => {
  const locale = useSessionStorage('locale', 'zh-CN').value
  try {
    const lang = await langList[`/node_modules/quasar/lang/${locale}.js`]()
    Lang.set((lang as Record<string, QuasarLanguage>).default)

  } catch (err) {
    console.error(err)
    // Requested Quasar Language Pack does not exist,
    // let's not break the app, so catching error
  }
})
