import type { Article } from '@/types/article'

export interface HotTopicArticle {
  id: number
  title: string
  url: string
  publishedAt: string
  source: Article['source']
}

export interface HotTopic {
  title: string
  summary: string
  articles: HotTopicArticle[]
}

export interface HotTopicsResponse {
  items: HotTopic[]
  generatedAt: string | null
}
