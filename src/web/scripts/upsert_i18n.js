import fs from 'fs'
import path from 'path'
import { fileURLToPath, pathToFileURL } from 'url'

const __dirname = path.dirname(fileURLToPath(import.meta.url))
const DEFAULT_LOCALES_DIR = path.resolve(__dirname, '../src/i18n/locales')
const SUPPORTED_LOCALES = ['zh-CN', 'en-US']

/**
 * Parses CLI arguments passed as `--name value` pairs, repeated `--pair key zh en` groups or repeated `--delete key`.
 * @param {string[]} argv Command line arguments after the script name.
 * @returns {Record<string, string | Array<{ key: string, translations: Record<string, string> }>>} Parsed argument map.
 */
export function parseCliArgs(argv) {
  const parsedArgs = {}

  for (let index = 0; index < argv.length; index += 1) {
    const currentArg = argv[index]
    if (!currentArg.startsWith('--')) {
      throw new Error(
        `Unexpected argument "${currentArg}". Use --key, --zh-CN and --en-US, --pair key zh en, or --delete key.`
      )
    }

    const argName = currentArg.slice(2)
    if (argName === 'pair') {
      const pairValues = argv.slice(index + 1, index + 4)
      if (pairValues.length < 3 || pairValues.some((pairValue) => pairValue.startsWith('--'))) {
        throw new Error('Missing value for --pair. Expected --pair key zh-CN en-US.')
      }

      if (!parsedArgs.pairs) {
        parsedArgs.pairs = []
      }

      parsedArgs.pairs.push({
        key: pairValues[0],
        translations: {
          'zh-CN': pairValues[1],
          'en-US': pairValues[2]
        }
      })
      index += 3
      continue
    }

    if (argName === 'delete') {
      const deleteKey = argv[index + 1]
      if (deleteKey === undefined || deleteKey.startsWith('--')) {
        throw new Error('Missing value for --delete. Expected --delete key.')
      }

      if (!parsedArgs.deleteKeys) {
        parsedArgs.deleteKeys = []
      }

      parsedArgs.deleteKeys.push(deleteKey)
      index += 1
      continue
    }

    const argValue = argv[index + 1]
    if (argValue === undefined || argValue.startsWith('--')) {
      throw new Error(`Missing value for --${argName}.`)
    }

    parsedArgs[argName] = argValue
    index += 1
  }

  return parsedArgs
}

/**
 * Reads a locale file exported as `export default { ... }`.
 * @param {string} filePath Locale file path.
 * @returns {Record<string, unknown>} Parsed locale object.
 */
export function readLocaleFile(filePath) {
  const sourceText = fs.readFileSync(filePath, 'utf8')
  const objectText = sourceText.replace(/^\s*export\s+default\s+/, '').trim()

  if (!objectText.startsWith('{') || !objectText.endsWith('}')) {
    throw new Error(`Locale file must use "export default { ... }": ${filePath}`)
  }

  return Function(`"use strict"; return (${objectText});`)()
}

/**
 * Writes a locale object as a sorted `export default` TypeScript module.
 * @param {string} filePath Locale file path.
 * @param {Record<string, unknown>} localeData Locale translations.
 * @returns {void}
 */
export function writeLocaleFile(filePath, localeData) {
  fs.writeFileSync(
    filePath,
    `export default ${formatLocaleValue(sortLocaleObject(localeData), 0)}\n`,
    'utf8'
  )
}

/**
 * Adds or updates a translation value at the given dot-path key.
 * @param {Record<string, unknown>} localeData Locale translations.
 * @param {string} dottedKey Translation key such as `loginPage.title`.
 * @param {string} translation Translation value.
 * @returns {void}
 */
export function upsertLocaleValue(localeData, dottedKey, translation) {
  const keyParts = dottedKey.split('.').filter(Boolean)
  if (keyParts.length === 0) {
    throw new Error('Translation key cannot be empty.')
  }

  let currentObject = localeData
  for (const keyPart of keyParts.slice(0, -1)) {
    const currentValue = currentObject[keyPart]
    if (currentValue === undefined) {
      currentObject[keyPart] = {}
    } else if (!isPlainObject(currentValue)) {
      throw new Error(
        `Cannot create nested key "${dottedKey}" because "${keyPart}" is already a string value.`
      )
    }

    currentObject = currentObject[keyPart]
  }

  currentObject[keyParts[keyParts.length - 1]] = translation
}

/**
 * Deletes a translation value at the given dot-path key and removes empty parent objects.
 * @param {Record<string, unknown>} localeData Locale translations.
 * @param {string} dottedKey Translation key such as `loginPage.title`.
 * @returns {boolean} Whether a locale value was deleted.
 */
export function deleteLocaleValue(localeData, dottedKey) {
  const keyParts = getTranslationKeyParts(dottedKey)
  const parentEntries = []
  let currentObject = localeData

  for (const keyPart of keyParts.slice(0, -1)) {
    const currentValue = currentObject[keyPart]
    if (!isPlainObject(currentValue)) {
      return false
    }

    parentEntries.push({ object: currentObject, key: keyPart })
    currentObject = currentValue
  }

  const leafKey = keyParts[keyParts.length - 1]
  if (!Object.prototype.hasOwnProperty.call(currentObject, leafKey)) {
    return false
  }

  delete currentObject[leafKey]

  for (let index = parentEntries.length - 1; index >= 0; index -= 1) {
    const { object, key } = parentEntries[index]
    const childValue = object[key]
    if (!isPlainObject(childValue) || Object.keys(childValue).length > 0) {
      break
    }

    delete object[key]
  }

  return true
}

/**
 * Upserts one key across all supported locale files.
 * @param {{ key: string, translations: Record<string, string>, localesDir?: string }} options Upsert options.
 * @returns {string[]} Locale files that were written.
 */
export function upsertI18nTranslations({ key, translations, localesDir = DEFAULT_LOCALES_DIR }) {
  return upsertI18nTranslationEntries({
    entries: [{ key, translations }],
    localesDir
  })
}

/**
 * Upserts multiple keys across all supported locale files.
 * @param {{ entries: Array<{ key: string, translations: Record<string, string> }>, localesDir?: string }} options Batch upsert options.
 * @returns {string[]} Locale files that were written.
 */
export function upsertI18nTranslationEntries({ entries, localesDir = DEFAULT_LOCALES_DIR }) {
  validateUpsertEntries(entries)

  const pendingLocaleUpdates = SUPPORTED_LOCALES.map((locale) => {
    const localeFilePath = path.resolve(localesDir, `${locale}.ts`)
    const localeData = readLocaleFile(localeFilePath)
    entries.forEach((entry) => {
      upsertLocaleValue(localeData, entry.key, entry.translations[locale])
    })
    return { localeFilePath, localeData }
  })

  return pendingLocaleUpdates.map(({ localeFilePath, localeData }) => {
    writeLocaleFile(localeFilePath, localeData)
    return localeFilePath
  })
}

/**
 * Deletes multiple keys across all supported locale files.
 * @param {{ keys: string[], localesDir?: string }} options Delete options.
 * @returns {string[]} Locale files that were written.
 */
export function deleteI18nTranslationKeys({ keys, localesDir = DEFAULT_LOCALES_DIR }) {
  validateDeleteKeys(keys)

  const pendingLocaleUpdates = SUPPORTED_LOCALES.map((locale) => {
    const localeFilePath = path.resolve(localesDir, `${locale}.ts`)
    const localeData = readLocaleFile(localeFilePath)
    let hasChanges = false
    keys.forEach((key) => {
      hasChanges = deleteLocaleValue(localeData, key) || hasChanges
    })
    return { localeFilePath, localeData, hasChanges }
  })

  return pendingLocaleUpdates
    .filter(({ hasChanges }) => hasChanges)
    .map(({ localeFilePath, localeData }) => {
      writeLocaleFile(localeFilePath, localeData)
      return localeFilePath
    })
}

/**
 * Validates batch upsert input.
 * @param {Array<{ key: string, translations: Record<string, string> }> | undefined} entries Translation entries.
 * @returns {void}
 */
export function validateUpsertEntries(entries) {
  if (!Array.isArray(entries) || entries.length === 0) {
    throw new Error('Missing required --key or --pair value.')
  }

  entries.forEach((entry) => {
    validateUpsertInput(entry.key, entry.translations)
  })
}

/**
 * Validates CLI or programmatic upsert input.
 * @param {string | undefined} key Translation key.
 * @param {Record<string, string>} translations Locale translations.
 * @returns {void}
 */
export function validateUpsertInput(key, translations) {
  validateTranslationKey(key)

  for (const locale of SUPPORTED_LOCALES) {
    if (translations[locale] === undefined) {
      throw new Error(`Missing required --${locale} translation.`)
    }
  }
}

/**
 * Validates batch delete input.
 * @param {string[] | undefined} keys Translation keys to delete.
 * @returns {void}
 */
export function validateDeleteKeys(keys) {
  if (!Array.isArray(keys) || keys.length === 0) {
    throw new Error('Missing required --delete value.')
  }

  keys.forEach((key) => validateTranslationKey(key))
}

/**
 * Validates a translation key and returns its dot-path parts.
 * @param {string | undefined} key Translation key.
 * @returns {string[]} Dot-path key parts.
 */
function getTranslationKeyParts(key) {
  validateTranslationKey(key)
  return key.split('.').filter(Boolean)
}

/**
 * Validates a translation key.
 * @param {string | undefined} key Translation key.
 * @returns {void}
 */
function validateTranslationKey(key) {
  if (!key || !key.trim()) {
    throw new Error('Translation key cannot be empty.')
  }

  if (key.split('.').filter(Boolean).length === 0) {
    throw new Error('Translation key cannot be empty.')
  }
}

/**
 * Recursively sorts locale object keys in alphabetical order.
 * @param {Record<string, unknown>} localeData Locale translations.
 * @returns {Record<string, unknown>} Sorted locale object.
 */
export function sortLocaleObject(localeData) {
  return Object.keys(localeData)
    .sort((leftKey, rightKey) => leftKey.localeCompare(rightKey))
    .reduce((sortedLocaleData, key) => {
      const value = localeData[key]
      sortedLocaleData[key] = isPlainObject(value) ? sortLocaleObject(value) : value
      return sortedLocaleData
    }, {})
}

/**
 * Formats a locale value using the repository's TypeScript object style.
 * @param {unknown} value Locale value.
 * @param {number} indentLevel Current indentation level.
 * @returns {string} Formatted source fragment.
 */
export function formatLocaleValue(value, indentLevel) {
  if (typeof value === 'string') {
    return formatStringLiteral(value)
  }

  if (!isPlainObject(value)) {
    throw new Error('Locale values must be strings or nested objects.')
  }

  const keys = Object.keys(value)
  if (keys.length === 0) {
    return '{}'
  }

  const currentIndent = '  '.repeat(indentLevel)
  const childIndent = '  '.repeat(indentLevel + 1)
  const lines = keys.map(
    (key) =>
      `${childIndent}${formatObjectKey(key)}: ${formatLocaleValue(value[key], indentLevel + 1)}`
  )

  return `{\n${lines.join(',\n')}\n${currentIndent}}`
}

/**
 * Formats a JavaScript object key.
 * @param {string} key Object key.
 * @returns {string} Formatted key.
 */
export function formatObjectKey(key) {
  if (/^[A-Za-z_$][A-Za-z0-9_$]*$/.test(key)) {
    return key
  }

  return formatStringLiteral(key)
}

/**
 * Formats a JavaScript string literal with single quotes.
 * @param {string} value String value.
 * @returns {string} Formatted string literal.
 */
export function formatStringLiteral(value) {
  return `'${value
    .replace(/\\/g, '\\\\')
    .replace(/'/g, "\\'")
    .replace(/\r/g, '\\r')
    .replace(/\n/g, '\\n')}'`
}

/**
 * Checks whether a value is a plain object.
 * @param {unknown} value Value to check.
 * @returns {value is Record<string, unknown>} Whether the value is a plain object.
 */
function isPlainObject(value) {
  return value !== null && typeof value === 'object' && !Array.isArray(value)
}

/**
 * Runs the CLI entrypoint.
 * @param {string[]} argv Command line arguments after the script name.
 * @returns {void}
 */
export function runCli(argv) {
  const args = parseCliArgs(argv)
  const deleteKeys = getCliDeleteKeys(args)
  if (deleteKeys.length > 0) {
    const writtenFiles = deleteI18nTranslationKeys({
      keys: deleteKeys,
      localesDir: args.localesDir
    })

    console.log(
      `Deleted ${deleteKeys.length} translation key(s). Updated ${writtenFiles.length} locale file(s).`
    )
    writtenFiles.forEach((filePath) => console.log(filePath))
    return
  }

  const entries = getCliTranslationEntries(args)
  const writtenFiles = upsertI18nTranslationEntries({ entries, localesDir: args.localesDir })

  console.log(
    `Updated ${writtenFiles.length} locale files for ${entries.length} translation key(s).`
  )
  writtenFiles.forEach((filePath) => console.log(filePath))
}

/**
 * Builds translation entries from parsed CLI arguments.
 * @param {Record<string, string | Array<{ key: string, translations: Record<string, string> }>>} args Parsed CLI arguments.
 * @returns {Array<{ key: string, translations: Record<string, string> }>} Translation entries.
 */
export function getCliTranslationEntries(args) {
  const pairEntries = args.pairs
  const hasPairEntries = Array.isArray(pairEntries) && pairEntries.length > 0
  const hasSingleEntryArgs =
    args.key !== undefined || args['zh-CN'] !== undefined || args['en-US'] !== undefined
  const hasDeleteArgs = hasCliDeleteKeys(args)

  if (hasDeleteArgs && (hasPairEntries || hasSingleEntryArgs)) {
    throw new Error('Do not mix --delete with --pair, --key, --zh-CN or --en-US.')
  }

  if (hasPairEntries && hasSingleEntryArgs) {
    throw new Error('Do not mix --pair with --key, --zh-CN or --en-US.')
  }

  if (hasPairEntries) {
    return pairEntries
  }

  if (!hasSingleEntryArgs) {
    return []
  }

  return [
    {
      key: args.key,
      translations: {
        'zh-CN': args['zh-CN'],
        'en-US': args['en-US']
      }
    }
  ]
}

/**
 * Builds delete keys from parsed CLI arguments.
 * @param {Record<string, string | string[] | Array<{ key: string, translations: Record<string, string> }>>} args Parsed CLI arguments.
 * @returns {string[]} Translation keys to delete.
 */
export function getCliDeleteKeys(args) {
  const deleteKeys = args.deleteKeys
  if (!Array.isArray(deleteKeys) || deleteKeys.length === 0) {
    return []
  }

  const hasUpsertArgs =
    args.key !== undefined ||
    args['zh-CN'] !== undefined ||
    args['en-US'] !== undefined ||
    hasCliPairEntries(args)
  if (hasUpsertArgs) {
    throw new Error('Do not mix --delete with --pair, --key, --zh-CN or --en-US.')
  }

  return deleteKeys
}

/**
 * Checks whether parsed CLI arguments contain pair entries.
 * @param {Record<string, unknown>} args Parsed CLI arguments.
 * @returns {boolean} Whether pair entries exist.
 */
function hasCliPairEntries(args) {
  return Array.isArray(args.pairs) && args.pairs.length > 0
}

/**
 * Checks whether parsed CLI arguments contain delete keys.
 * @param {Record<string, unknown>} args Parsed CLI arguments.
 * @returns {boolean} Whether delete keys exist.
 */
function hasCliDeleteKeys(args) {
  return Array.isArray(args.deleteKeys) && args.deleteKeys.length > 0
}

const isCliEntrypoint = process.argv[1] && import.meta.url === pathToFileURL(process.argv[1]).href

if (isCliEntrypoint) {
  try {
    runCli(process.argv.slice(2))
  } catch (error) {
    console.error(error instanceof Error ? error.message : error)
    process.exitCode = 1
  }
}

