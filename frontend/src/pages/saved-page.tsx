import { useEffect } from 'react'

import { ArticleLayoutToggle } from '@/components/article-layout-toggle'
import { ArticleList } from '@/components/article-list'
import { ArticleListSkeleton } from '@/components/article-list-skeleton'
import { InfiniteScroll } from '@/components/infinite-scroll'
import { PageHeader } from '@/components/page-header'
import { Button } from '@/components/ui/button'
import { useSavedArticles } from '@/hooks/use-saved-articles'

export function SavedPage() {
  const {
    articles,
    isLoading,
    hasMore,
    error,
    loadMore,
    retry,
  } = useSavedArticles()

  useEffect(() => {
    document.title = 'Mentett hírek | hírfolyam'
  }, [])

  return (
    <>
      <PageHeader
        title="Mentett hírek"
        lastUpdatedAt={null}
        action={<ArticleLayoutToggle />}
      />

      {isLoading && articles.length === 0 && <ArticleListSkeleton />}

      <ArticleList articles={articles} label="Mentett hírek" isLoading={isLoading} />

      {!isLoading && !error && !hasMore && articles.length === 0 && (
        <p role="status" className="py-16 text-center text-neutral-400">
          Még nincs mentett híred.
        </p>
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
          loadingLabel="További mentett hírek betöltése…"
        />
      )}
    </>
  )
}
