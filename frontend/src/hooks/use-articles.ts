import { useEffect, useState } from 'react'

import { useSourceSelection } from '@/hooks/use-source-selection'
import { getArticles } from '@/lib/api'
import type { Article } from '@/types/article'

export function useArticles(category: string | undefined, search?: string) {
  const {
    sourceFilter,
    isLoading: sourcesLoading,
    error: sourcesError,
    retry: retrySources,
  } = useSourceSelection()
  const filterKey = JSON.stringify([
    category,
    search,
    sourceFilter,
  ])
  const [previousFilterKey, setPreviousFilterKey] = useState(filterKey)
  const [articles, setArticles] = useState<Article[]>([])
  const [page, setPage] = useState(1)
  const [attempt, setAttempt] = useState(0)
  const [isLoading, setIsLoading] = useState(true)
  const [hasMore, setHasMore] = useState(false)
  const [totalItems, setTotalItems] = useState(0)
  const [error, setError] = useState<string | null>(null)

  if (previousFilterKey !== filterKey) {
    setPreviousFilterKey(filterKey)
    setArticles([])
    setPage(1)
    setIsLoading(true)
    setHasMore(false)
    setTotalItems(0)
    setError(null)
  }

  useEffect(() => {
    if (sourceFilter === null) {
      return
    }

    const controller = new AbortController()
    const requestedSources = sourceFilter

    async function loadArticles() {
      setIsLoading(true)
      setError(null)

      try {
        const response = await getArticles(
          category,
          page,
          controller.signal,
          requestedSources,
          search,
        )

        if (controller.signal.aborted) {
          return
        }

        setArticles((previous) => {
          if (page === 1) {
            return response.items
          }

          const existingIds = new Set(previous.map((article) => article.id))
          const newArticles = response.items.filter((article) => !existingIds.has(article.id))

          return [...previous, ...newArticles]
        })

        setHasMore(response.page < response.totalPages)
        setTotalItems(response.totalItems)
      } catch {
        if (!controller.signal.aborted) {
          setError('Nem sikerült betölteni a cikkeket.')
        }
      } finally {
        if (!controller.signal.aborted) {
          setIsLoading(false)
        }
      }
    }

    void loadArticles()

    return () => {
      controller.abort()
    }
  }, [category, search, sourceFilter, page, attempt])

  function loadMore() {
    if (isLoading || error || sourcesLoading || sourcesError || !hasMore) {
      return
    }

    setIsLoading(true)
    setPage((previous) => previous + 1)
  }

  function retry() {
    if (sourcesError) {
      retrySources()
      return
    }

    setIsLoading(true)
    setError(null)
    setAttempt((previous) => previous + 1)
  }

  return {
    articles,
    isLoading: sourcesLoading || (sourceFilter !== null && isLoading),
    hasEnabledSources: sourceFilter !== '',
    hasMore,
    totalItems,
    error: sourcesError ?? error,
    loadMore,
    retry,
  }
}
