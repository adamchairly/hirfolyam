import { ArticleList } from '@/components/article-list'
import { ArticleListSkeleton } from '@/components/article-list-skeleton'
import { DisabledSourcesMessage } from '@/components/disabled-sources-message'
import { InfiniteScroll } from '@/components/infinite-scroll'
import { Button } from '@/components/ui/button'
import { useArticles } from '@/hooks/use-articles'

interface ArticleFeedProps {
  category?: string
  label: string
  emptyMessage: string
}

export function ArticleFeed({ category, label, emptyMessage }: ArticleFeedProps) {
  const {
    articles,
    isLoading,
    hasMore,
    hasEnabledSources,
    error,
    loadMore,
    retry,
  } = useArticles(category)

  return (
    <div>
      {isLoading && articles.length === 0 && <ArticleListSkeleton />}

      <ArticleList articles={articles} label={label} isLoading={isLoading} />

      {!isLoading && !error && articles.length === 0 && (
        hasEnabledSources ? (
          <p role="status" className="py-16 text-center text-neutral-400">
            {emptyMessage}
          </p>
        ) : (
          <DisabledSourcesMessage />
        )
      )}

      {error && (
        <div className="space-y-4 py-12 text-center">
          <p role="alert" className="text-neutral-300">
            {error}
          </p>
          <Button variant="outline" onClick={retry}>
            Újrapróbálom
          </Button>
        </div>
      )}

      {articles.length > 0 && !error && (
        <InfiniteScroll
          hasMore={hasMore}
          isLoading={isLoading}
          onLoadMore={loadMore}
          loadingLabel="További cikkek betöltése…"
        />
      )}
    </div>
  )
}
