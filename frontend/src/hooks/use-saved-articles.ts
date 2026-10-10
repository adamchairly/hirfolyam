import { useEffect, useState } from 'react'

import { useBookmarks } from '@/hooks/use-bookmarks'
import { getArticlesByIds } from '@/lib/api'
import type { Article } from '@/types/article'

const pageSize = 20

export function useSavedArticles() {
  const { bookmarks } = useBookmarks()
  // Page through the bookmarks saved when the page opened, so removing one keeps the pages stable.
  const [savedIds] = useState(bookmarks)
  const [articles, setArticles] = useState<Article[]>([])
  const [page, setPage] = useState(1)
  const [attempt, setAttempt] = useState(0)
  const [isLoading, setIsLoading] = useState(savedIds.length > 0)
  const [error, setError] = useState<string | null>(null)
  const hasMore = page * pageSize < savedIds.length

  useEffect(() => {
    const ids = savedIds.slice((page - 1) * pageSize, page * pageSize)

    if (ids.length === 0) {
      return
    }

    const controller = new AbortController()

    async function loadArticles() {
      setIsLoading(true)
      setError(null)

      try {
        const response = await getArticlesByIds(ids, controller.signal)

        if (controller.signal.aborted) {
          return
        }

        const positions = new Map(ids.map((id, index) => [id, index]))
        const items = response.items.toSorted((first, second) =>
          (positions.get(first.id) ?? 0) - (positions.get(second.id) ?? 0))

        setArticles((previous) => {
          if (page === 1) {
            return items
          }

          const existingIds = new Set(previous.map((article) => article.id))

          return [...previous, ...items.filter((article) => !existingIds.has(article.id))]
        })
      } catch {
        if (!controller.signal.aborted) {
          setError('Nem sikerült betölteni a mentett híreket.')
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
  }, [savedIds, page, attempt])

  function loadMore() {
    if (isLoading || error || !hasMore) {
      return
    }

    setIsLoading(true)
    setPage((previous) => previous + 1)
  }

  function retry() {
    setIsLoading(true)
    setError(null)
    setAttempt((previous) => previous + 1)
  }

  return {
    articles: articles.filter((article) => bookmarks.includes(article.id)),
    isLoading,
    hasMore,
    error,
    loadMore,
    retry,
  }
}
