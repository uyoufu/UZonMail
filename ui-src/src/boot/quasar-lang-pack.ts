import { boot } from 'quasar/wrappers'
import { Lang } from 'quasar'

// relative path to your node_modules/quasar/..
// change to YOUR path
const langList = import.meta.glob('../../node_modules/quasar/lang/*.js')
// or just a select few (example below with only DE and FR):
// import.meta.glob('../../node_modules/quasar/lang/(de|fr).js')

export default boot(async () => {
  const langIso = 'zh-CN' // ... some logic to determine it (use Cookies Plugin?)

  try {
    const lang = await langList[`../../node_modules/quasar/lang/${langIso}.js`]()
    Lang.set(lang.default)

  } catch (err) {
    console.error(err)
    // Requested Quasar Language Pack does not exist,
    // let's not break the app, so catching error
  }
})
