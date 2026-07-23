import { httpClient } from './base/httpClient'

export function getSponsorPageContent () {
  return httpClient.get<string>('/sponsor/content')
}
