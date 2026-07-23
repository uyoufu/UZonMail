import { defineBoot } from '#q-app/wrappers'
import type { QuasarLanguage } from 'quasar';
import { Lang } from 'quasar'
import { useSessionStorage } from '@vueuse/core'

// relative path to your node_modules/quasar/..
// change to YOUR path
const langList = import.meta.glob('/node_modules/quasar/lang/*.js')
// or just a select few (example below with only DE and FR):
// import.meta.glob('../../node_modules/quasar/lang/(de|fr).js')

export default defineBoot(async () => {
  const locale = useSessionStorage('locale', 'zh-CN').value
  try {
    const quasarLocale = langList[`/node_modules/quasar/lang/${locale}.js`]
    if (!quasarLocale) {
      throw new Error(`Quasar Language Pack ${locale} does not exist`)
    }
    const lang = await quasarLocale() as { default: QuasarLanguage }
    Lang.set(lang.default)

  } catch (err) {
    console.error(err)
    // Requested Quasar Language Pack does not exist,
    // let's not break the app, so catching error
  }
})
