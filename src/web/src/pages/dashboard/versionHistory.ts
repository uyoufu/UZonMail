const chineseVersionHistoryUrl = 'https://uzonmail.uzoncloud.com/downloads'
const englishVersionHistoryUrl = 'https://uzonmail.uzoncloud.com/en/downloads'
const releaseHeadingTagName = 'H2'
const contentHeadingTagNames = ['H3', 'H4'] as const
const listTagNames = ['OL', 'UL'] as const
const textContentTagNames = ['P', 'PRE', 'CODE'] as const
const linkUrlProtocol = {
  Http: 'http:',
  Https: 'https:'
} as const
const fragmentType = {
  Text: 'text',
  Link: 'link'
} as const
const lineBreakTagName = 'BR'
const anchorTagName = 'A'
const fragmentLinkPrefix = '#'
const fileNamePattern = /\.[a-z0-9]{1,16}$/i

export const VersionHistoryFragmentType = fragmentType
export type VersionHistoryFragmentType = (typeof VersionHistoryFragmentType)[keyof typeof VersionHistoryFragmentType]

export interface IVersionHistoryEntryFragment {
  type: VersionHistoryFragmentType
  text: string
  url?: string
  fileName?: string
}

export interface IVersionHistoryEntry {
  fragments: IVersionHistoryEntryFragment[]
}

export interface IVersionHistorySection {
  title?: string
  entries: IVersionHistoryEntry[]
}

export interface IVersionHistoryRelease {
  version: string
  publishedAt?: string
  sections: IVersionHistorySection[]
}

/** 根据当前界面语言获取官方版本历史页地址。 */
export function getVersionHistoryUrl(locale: string): string {
  return locale.toLowerCase().startsWith('en') ? englishVersionHistoryUrl : chineseVersionHistoryUrl
}

/** 请求并解析官方版本历史页。 */
export async function getVersionHistory(locale: string): Promise<IVersionHistoryRelease[]> {
  const versionHistoryUrl = getVersionHistoryUrl(locale)
  const response = await fetch(versionHistoryUrl)
  if (!response.ok) {
    throw new Error(`版本历史请求失败：${response.status}`)
  }

  const html = await response.text()
  return parseVersionHistoryDocument(new DOMParser().parseFromString(html, 'text/html'), versionHistoryUrl)
}

/** 将官方版本历史页的发布记录转换为安全的结构化展示模型。 */
export function parseVersionHistoryDocument(
  document: Document,
  sourceUrl: string = chineseVersionHistoryUrl
): IVersionHistoryRelease[] {
  const releases: IVersionHistoryRelease[] = []
  const releaseHeadings = Array.from(document.querySelectorAll(releaseHeadingTagName))

  for (const releaseHeading of releaseHeadings) {
    const version = getElementText(releaseHeading)
    if (!isVersionTitle(version)) continue

    const release: IVersionHistoryRelease = {
      version,
      sections: []
    }
    let activeSection: IVersionHistorySection | undefined
    let contentElement = releaseHeading.nextElementSibling
    while (contentElement && contentElement.tagName !== releaseHeadingTagName) {
      const tagName = contentElement.tagName
      if (contentHeadingTagNames.includes(tagName as (typeof contentHeadingTagNames)[number])) {
        activeSection = {
          title: getElementText(contentElement),
          entries: []
        }
        release.sections.push(activeSection)
      } else if (tagName === 'BLOCKQUOTE') {
        const content = getElementText(contentElement)
        if (!release.publishedAt && isPublicationDate(content)) {
          release.publishedAt = content
        } else {
          activeSection = appendEntries(release.sections, activeSection, [createTextEntry(content)])
        }
      } else {
        const entries = getContentEntries(contentElement, sourceUrl)
        activeSection = appendEntries(release.sections, activeSection, entries)
      }
      contentElement = contentElement.nextElementSibling
    }
    releases.push(release)
  }

  return releases
}

function appendEntries(
  sections: IVersionHistorySection[],
  activeSection: IVersionHistorySection | undefined,
  entries: IVersionHistoryEntry[]
): IVersionHistorySection | undefined {
  if (entries.length === 0) return activeSection

  const targetSection = activeSection ?? { entries: [] }
  if (!activeSection) sections.push(targetSection)
  targetSection.entries.push(...entries)
  return targetSection
}

function getContentEntries(element: Element, sourceUrl: string): IVersionHistoryEntry[] {
  const tagName = element.tagName
  if (listTagNames.includes(tagName as (typeof listTagNames)[number])) {
    return Array.from(element.children)
      .filter((child) => child.tagName === 'LI')
      .map((child) => createVersionHistoryEntry(child, sourceUrl))
      .filter((entry) => entry.fragments.length > 0)
  }
  if (textContentTagNames.includes(tagName as (typeof textContentTagNames)[number])) {
    const entry = createVersionHistoryEntry(element, sourceUrl)
    return entry.fragments.length > 0 ? [entry] : []
  }
  return []
}

function createTextEntry(text: string): IVersionHistoryEntry {
  return {
    fragments: text ? [{ type: VersionHistoryFragmentType.Text, text }] : []
  }
}

function createVersionHistoryEntry(element: Element, sourceUrl: string): IVersionHistoryEntry {
  const fragments: IVersionHistoryEntryFragment[] = []
  appendElementFragments(element, sourceUrl, fragments)
  return { fragments }
}

function appendElementFragments(element: Element, sourceUrl: string, fragments: IVersionHistoryEntryFragment[]): void {
  for (const childNode of element.childNodes) {
    if (childNode.nodeType === Node.TEXT_NODE) {
      appendTextFragment(fragments, childNode.textContent ?? '')
      continue
    }
    if (childNode.nodeType !== Node.ELEMENT_NODE) continue

    const childElement = childNode as Element
    if (childElement.tagName === anchorTagName) {
      appendLinkFragment(childElement, sourceUrl, fragments)
      continue
    }
    if (childElement.tagName === lineBreakTagName) {
      appendTextFragment(fragments, ' ')
      continue
    }

    appendElementFragments(childElement, sourceUrl, fragments)
  }
}

function appendTextFragment(fragments: IVersionHistoryEntryFragment[], value: string): void {
  const normalizedText = value.replace(/\s+/g, ' ')
  if (!normalizedText.trim()) return

  const previousFragment = fragments[fragments.length - 1]
  if (previousFragment?.type === VersionHistoryFragmentType.Text) {
    previousFragment.text += normalizedText
    return
  }

  fragments.push({ type: VersionHistoryFragmentType.Text, text: normalizedText })
}

function appendLinkFragment(element: Element, sourceUrl: string, fragments: IVersionHistoryEntryFragment[]): void {
  const linkText = getElementText(element)
  const url = getSafeLinkUrl(element.getAttribute('href'), sourceUrl)
  if (!url) {
    appendTextFragment(fragments, linkText)
    return
  }

  const fileName = getDownloadFileName(element, url, linkText)
  fragments.push({
    type: VersionHistoryFragmentType.Link,
    text: linkText || fileName || url,
    url,
    fileName
  })
}

function getSafeLinkUrl(href: string | null, sourceUrl: string): string | undefined {
  if (!href || href.startsWith(fragmentLinkPrefix)) return undefined

  try {
    const parsedUrl = new URL(href, sourceUrl)
    if (parsedUrl.protocol !== linkUrlProtocol.Http && parsedUrl.protocol !== linkUrlProtocol.Https) return undefined
    return parsedUrl.href
  } catch {
    return undefined
  }
}

function getDownloadFileName(element: Element, url: string, linkText: string): string | undefined {
  const declaredFileName = element.getAttribute('download')?.trim()
  if (declaredFileName) return declaredFileName

  const urlFileName = getFileNameFromUrl(url)
  if (fileNamePattern.test(linkText)) return linkText
  if (urlFileName && fileNamePattern.test(urlFileName)) return urlFileName
  if (element.hasAttribute('download')) return linkText || urlFileName || 'download'
  return undefined
}

function getFileNameFromUrl(url: string): string | undefined {
  const fileName = new URL(url).pathname.split('/').pop()
  if (!fileName) return undefined

  try {
    return decodeURIComponent(fileName)
  } catch {
    return fileName
  }
}

function getElementText(element: Element): string {
  return (element.textContent ?? '').replace(/\s+/g, ' ').trim()
}

function isVersionTitle(value: string): boolean {
  return /^v?\d+(?:\.\d+)+$/i.test(value)
}

function isPublicationDate(value: string): boolean {
  return /\d{4}[-/.年]/.test(value)
}

