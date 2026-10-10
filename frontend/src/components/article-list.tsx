import { ArticleItem } from '@/components/article-item'
import { useArticleLayout } from '@/hooks/use-article-layout'
import { useCurrentTime } from '@/hooks/use-current-time'
import type { Article } from '@/types/article'

interface ArticleListProps {
  articles: Article[]
  label: string
  isLoading: boolean
  search?: string
}

export function ArticleList({ articles, label, isLoading, search }: ArticleListProps) {
  const { layout } = useArticleLayout()
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
          <ArticleItem article={article} now={now} search={search} layout={layout} />
        </li>
      ))}
    </ul>
  )
}
