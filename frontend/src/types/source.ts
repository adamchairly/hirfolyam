export interface Source {
  key: string
  name: string
  url: string
  iconUrl: string | null
  // The slug of the category that fits the source best.
  category: string
  articleCount: number
}

export interface SourcesResponse {
  items: Source[]
  lastUpdatedAt: string | null
}
