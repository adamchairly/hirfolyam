import { useEffect } from 'react'
import { useParams } from 'react-router-dom'

import { ArticleList } from '@/components/article-list'
import { ArticleListSkeleton } from '@/components/article-list-skeleton'
import { PageHeader } from '@/components/page-header'
import { Button } from '@/components/ui/button'
import { useHotTopic } from '@/hooks/use-hot-topic'
import { useSourceSelection } from '@/hooks/use-source-selection'

const notFoundTitle = 'Ez az összefoglaló nem található.'

export function HotTopicPage() {
  const { id } = useParams()

  if (!id || !/^\d+$/.test(id)) {
    return <PageHeader title={notFoundTitle} lastUpdatedAt={null} />
  }

  return <HotTopicArticles key={id} id={Number(id)} />
}

function HotTopicArticles({ id }: { id: number }) {
  const { topic, isLoading, isNotFound, error, retry } = useHotTopic(id)
  const { disabledSourceKeys } = useSourceSelection()
  // Like the summary block, the list leaves out the disabled sources.
  const articles = (topic?.articles ?? []).filter((article) => !disabledSourceKeys.includes(article.source.key))

  useEffect(() => {
    if (topic) {
      document.title = `${topic.title} | hírfolyam`
    }
  }, [topic])

  if (isNotFound) {
    return <PageHeader title={notFoundTitle} lastUpdatedAt={null} />
  }

  return (
    <>
      <PageHeader title={topic?.title ?? 'Összefoglaló'} lastUpdatedAt={null} />

      {isLoading && !topic && <ArticleListSkeleton layout="list" />}

      {topic && (
        <ArticleList
          articles={articles}
          label={`${topic.title} – cikkek`}
          isLoading={isLoading}
          layout="list"
          showLeads={false}
        />
      )}

      {topic && articles.length === 0 && (
        <p role="status" className="py-16 text-center text-neutral-400">
          Az összefoglaló cikkei kikapcsolt forrásokból származnak.
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
    </>
  )
}
