import axios from 'axios'

import type { ArticlesResponse } from '@/types/article'
import type { HotTopicDetail, HotTopicPeriod, HotTopicsResponse } from '@/types/hot-topic'
import type { SourcesResponse } from '@/types/source'
import type { VersionResponse } from '@/types/version'

const api = axios.create({
  baseURL: '/api',
  timeout: 15_000,
})

export async function getArticles(
  category: string | undefined,
  page: number,
  signal: AbortSignal,
  sourceFilter: string,
  excludedCategories: string[],
  search?: string,
): Promise<ArticlesResponse> {
  const response = await api.get<ArticlesResponse>('/articles', {
    params: {
      category,
      search,
      page,
      pageSize: 20,
      sources: sourceFilter.split(','),
      excludedCategories,
    },
    paramsSerializer: {
      indexes: null,
    },
    signal,
  })

  return response.data
}

// Saved articles ignore the source selection.
export async function getArticlesByIds(ids: number[], signal: AbortSignal): Promise<ArticlesResponse> {
  const response = await api.get<ArticlesResponse>('/articles', {
    params: {
      ids,
      pageSize: ids.length,
    },
    paramsSerializer: {
      indexes: null,
    },
    signal,
  })

  return response.data
}

export async function getHotTopics(period: HotTopicPeriod, signal: AbortSignal): Promise<HotTopicsResponse> {
  const response = await api.get<HotTopicsResponse>('/hot-topics', {
    params: {
      period,
    },
    signal,
  })

  return response.data
}

export async function getHotTopic(id: number, signal: AbortSignal): Promise<HotTopicDetail> {
  const response = await api.get<HotTopicDetail>(`/hot-topics/${id}`, {
    signal,
  })

  return response.data
}

export async function getSources(signal: AbortSignal): Promise<SourcesResponse> {
  const response = await api.get<SourcesResponse>('/sources', {
    signal,
  })

  return response.data
}

export async function getVersion(signal: AbortSignal): Promise<VersionResponse> {
  const response = await api.get<VersionResponse>('/version', {
    signal,
  })

  return response.data
}
