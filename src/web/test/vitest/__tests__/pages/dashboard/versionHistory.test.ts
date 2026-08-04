import { describe, expect, it } from 'vitest'
import { getVersionHistoryUrl, parseVersionHistoryDocument } from 'src/pages/dashboard/versionHistory'

describe('version history', () => {
  it('uses the localized official history page', () => {
    expect(getVersionHistoryUrl('zh-CN')).toBe('https://uzonmail.uzoncloud.com/downloads')
    expect(getVersionHistoryUrl('en-US')).toBe('https://uzonmail.uzoncloud.com/en/downloads')
  })

  it('extracts release dates, text content, and safe download links without retaining remote HTML', () => {
    const document = new DOMParser().parseFromString(`
      <h2>1.2.0</h2>
      <blockquote><p>2026-08-04</p></blockquote>
      <h3>New features</h3>
      <ol><li>First feature</li><li>Second <strong>feature</strong></li></ol>
      <h3>Fixes</h3>
      <p>Fixed a critical issue.</p>
      <h3>Downloads</h3>
      <p><a href="/files/uzonmail-desktop.zip">uzonmail-desktop.zip</a><br>
      <a href="https://hub.docker.com/r/gmxgalens/uzon-mail/tags">docker</a>
      <a href="javascript:alert('unsafe')">unsafe.zip</a></p>
      <h2>1.1.0</h2>
      <p>Initial release.</p>
    `, 'text/html')

    expect(parseVersionHistoryDocument(document, 'https://uzonmail.uzoncloud.com/en/downloads')).toEqual([
      {
        version: '1.2.0',
        publishedAt: '2026-08-04',
        sections: [
          {
            title: 'New features',
            entries: [
              { fragments: [{ type: 'text', text: 'First feature' }] },
              { fragments: [{ type: 'text', text: 'Second feature' }] }
            ]
          },
          { title: 'Fixes', entries: [{ fragments: [{ type: 'text', text: 'Fixed a critical issue.' }] }] },
          {
            title: 'Downloads',
            entries: [
              {
                fragments: [
                  {
                    type: 'link',
                    text: 'uzonmail-desktop.zip',
                    url: 'https://uzonmail.uzoncloud.com/files/uzonmail-desktop.zip',
                    fileName: 'uzonmail-desktop.zip'
                  },
                  { type: 'link', text: 'docker', url: 'https://hub.docker.com/r/gmxgalens/uzon-mail/tags' },
                  { type: 'text', text: 'unsafe.zip' }
                ]
              }
            ]
          }
        ]
      },
      {
        version: '1.1.0',
        sections: [{ entries: [{ fragments: [{ type: 'text', text: 'Initial release.' }] }] }]
      }
    ])
  })

  it('skips non-version headings and returns no releases for an empty document', () => {
    const document = new DOMParser().parseFromString('<h2>Release notes</h2>', 'text/html')

    expect(parseVersionHistoryDocument(document)).toEqual([])
  })

  it('keeps direct links when their URL has an invalid percent-encoded path', () => {
    const document = new DOMParser().parseFromString(`
      <h2>1.2.0</h2>
      <h3>Downloads</h3>
      <p><a href="https://cdn.uzonmail.example/%E0%A4%A.zip">uzonmail-desktop.zip</a></p>
    `, 'text/html')

    expect(parseVersionHistoryDocument(document)).toEqual([
      {
        version: '1.2.0',
        sections: [
          {
            title: 'Downloads',
            entries: [
              {
                fragments: [
                  {
                    type: 'link',
                    text: 'uzonmail-desktop.zip',
                    url: 'https://cdn.uzonmail.example/%E0%A4%A.zip',
                    fileName: 'uzonmail-desktop.zip'
                  }
                ]
              }
            ]
          }
        ]
      }
    ])
  })
})
