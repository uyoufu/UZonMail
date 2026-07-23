import path from 'path'
import fs from 'fs'
import { fileURLToPath } from 'url'

const __dirname = path.dirname(fileURLToPath(import.meta.url))

function extractKeys(filePath) {
  const text = fs.readFileSync(filePath, 'utf8')
  const lines = text.split(/\r?\n/)
  const stack = []
  const keys = []
  for (let i = 0; i < lines.length; i++) {
    const line = lines[i].trim()
    if (!line || line.startsWith('//') || line.startsWith('/*')) continue
    const openMatch = line.match(/^([A-Za-z0-9_\-@:.]+)\s*:\s*\{\s*(?:\/\/.*)?$/)
    if (openMatch) {
      stack.push(openMatch[1])
      continue
    }
    const kvMatch = line.match(
      /^([A-Za-z0-9_\-@:.]+)\s*:\s*(?:`([^`]*)`|'([^']*)'|"([^"]*)"|\[|\{|null|undefined|[0-9])/
    )
    if (kvMatch) {
      const key = kvMatch[1]
      const full = stack.concat([key]).join('.')
      keys.push(full)
    }
    // remove quoted substrings to avoid counting braces inside strings
    const safeLine = line.replace(/(['"`])(?:(?!\\1).)*?\1/g, '')
    if (safeLine.includes('}')) {
      const count = (safeLine.match(/}/g) || []).length
      for (let c = 0; c < count; c++) {
        if (stack.length > 0) stack.pop()
      }
    }
  }
  return keys
}

const en = extractKeys(path.resolve(__dirname, '../src/i18n/locales/en-US.ts'))
const zh = extractKeys(path.resolve(__dirname, '../src/i18n/locales/zh-CN.ts'))

const enSet = new Set(en)
const missing = zh.filter((k) => !enSet.has(k))
console.log('Total zh keys:', zh.length)
console.log('Total en keys:', en.length)
console.log('Missing in en (count):', missing.length)
missing.forEach((k) => console.log(k))

// debug: print en keys for outboxManager prefix
console.log('\nSample en keys starting with outboxManager:')
en.filter((s) => s.indexOf('outboxManager.') === 0)
  .slice(0, 50)
  .forEach((s) => console.log(s))
console.log('\nCheck specific key presence:')
;['emailGroup.deleteGroupAndInboxesConfirm', 'outboxManager.col_order', 'sendingTask.bccRecipients'].forEach((k) => {
  console.log(k, en.includes(k))
})
fs.writeFileSync(path.resolve(__dirname, '/tmp/missing_i18n_keys.txt'), missing.join('\n'))
console.log('Wrote /tmp/missing_i18n_keys.txt')
