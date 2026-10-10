export interface Source {
  key: string
  name: string
  url: string
  iconUrl: string | null
  articleCount: number
}

export interface SourcesResponse {
  items: Source[]
  lastUpdatedAt: string | null
}
