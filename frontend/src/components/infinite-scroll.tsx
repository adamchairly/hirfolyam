import { useEffect, useRef } from 'react'

interface InfiniteScrollProps {
  hasMore: boolean
  isLoading: boolean
  onLoadMore: () => void
  loadingLabel?: string
  // How far below the screen the next page starts loading.
  rootMargin?: string
}

// Loads the next page when the reader scrolls near the end of a list.
export function InfiniteScroll({
  hasMore,
  isLoading,
  onLoadMore,
  loadingLabel = 'Betöltés…',
  rootMargin = '0px 0px 800px 0px',
}: InfiniteScrollProps) {
  const sentinelRef = useRef<HTMLDivElement>(null)
  const onLoadMoreRef = useRef(onLoadMore)

  useEffect(() => {
    onLoadMoreRef.current = onLoadMore
  })

  useEffect(() => {
    const sentinel = sentinelRef.current

    if (!sentinel || !hasMore || isLoading) {
      return
    }

    // A new observer reports the current position at once, so a short page keeps loading until it fills the screen.
    const observer = new IntersectionObserver((entries) => {
      if (entries.some((entry) => entry.isIntersecting)) {
        onLoadMoreRef.current()
      }
    }, { rootMargin })

    observer.observe(sentinel)

    return () => {
      observer.disconnect()
    }
  }, [hasMore, isLoading, rootMargin])

  if (!hasMore) {
    return null
  }

  return (
    <div ref={sentinelRef} className="flex min-h-24 items-center justify-center py-8">
      {isLoading && (
        <p role="status" className="flex items-center gap-2 text-sm text-neutral-400">
          <svg
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth={2}
            strokeLinecap="round"
            aria-hidden="true"
            className="size-4 animate-spin"
          >
            <path d="M12 3a9 9 0 1 0 9 9" />
          </svg>
          {loadingLabel}
        </p>
      )}
    </div>
  )
}
