import type { Article } from '@/types/article'

export interface HotTopicArticle {
  id: number
  title: string
  url: string
  publishedAt: string
  source: Article['source']
}

export interface HotTopic {
  id: number
  title: string
  summary: string
  articles: HotTopicArticle[]
}

// The newest hourly topics, or the newest summary of a day.
export type HotTopicPeriod = 'hour' | 'today' | 'yesterday'

export interface HotTopicsResponse {
  items: HotTopic[]
  generatedAt: string | null
}

// One topic with its articles in the usual list shape.
export interface HotTopicDetail {
  id: number
  title: string
  summary: string
  generatedAt: string
  articles: Article[]
}
