import { useEffect } from 'react'

import { ArticleFeed } from '@/components/article-feed'
import { ArticleLayoutToggle } from '@/components/article-layout-toggle'
import { HotTopics } from '@/components/hot-topics'
import { HotTopicsToggle } from '@/components/hot-topics-toggle'
import { PageHeader } from '@/components/page-header'

interface FreshPageProps {
  lastUpdatedAt: string | null
}

export function FreshPage({ lastUpdatedAt }: FreshPageProps) {
  useEffect(() => {
    document.title = 'Friss | hírfolyam'
  }, [])

  return (
    <>
      <PageHeader
        title="Friss"
        lastUpdatedAt={lastUpdatedAt}
        action={(
          <div className="flex items-center gap-2">
            <HotTopicsToggle />
            <ArticleLayoutToggle />
          </div>
        )}
      />

      <HotTopics />

      <ArticleFeed label="Friss cikkek" emptyMessage="Még nincs cikk." />
    </>
  )
}
