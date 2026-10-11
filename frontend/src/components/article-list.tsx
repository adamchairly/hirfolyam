import { ArticleItem } from '@/components/article-item'
import { useArticleLayout } from '@/hooks/use-article-layout'
import { useCurrentTime } from '@/hooks/use-current-time'
import type { ArticleLayout } from '@/lib/article-layout'
import type { Article } from '@/types/article'

interface ArticleListProps {
  articles: Article[]
  label: string
  isLoading: boolean
  search?: string
  // Overrides the layout chosen by the reader.
  layout?: ArticleLayout
  showLeads?: boolean
}

export function ArticleList({ articles, label, isLoading, search, layout: fixedLayout, showLeads = true }: ArticleListProps) {
  const { layout: chosenLayout } = useArticleLayout()
  const layout = fixedLayout ?? chosenLayout
  const now = useCurrentTime()

  return (
    <ul
      aria-label={label}
      aria-busy={isLoading}
      className={layout === 'grid'
        ? 'grid grid-cols-2 gap-x-4 gap-y-8 py-8 sm:gap-x-6 sm:gap-y-10 lg:grid-cols-3'
        : undefined}
    >
      {articles.map((article) => (
        <li key={article.id}>
          <ArticleItem article={article} now={now} search={search} layout={layout} showLead={showLeads} />
        </li>
      ))}
    </ul>
  )
}
