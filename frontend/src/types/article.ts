export interface Category {
  slug: string
  name: string
}

export interface Article {
  id: number
  title: string
  lead: string | null
  source: {
    key: string
    name: string
    iconUrl: string | null
  }
  category: Category | null
  author: string | null
  imageUrl: string | null
  publishedAt: string
  url: string
}

export interface ArticlesResponse {
  items: Article[]
  page: number
  pageSize: number
  totalItems: number
  totalPages: number
}
