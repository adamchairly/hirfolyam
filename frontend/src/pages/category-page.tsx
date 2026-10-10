import { useEffect } from 'react'
import { useParams } from 'react-router-dom'

import { ArticleFeed } from '@/components/article-feed'
import { ArticleLayoutToggle } from '@/components/article-layout-toggle'
import { PageHeader } from '@/components/page-header'
import { categories } from '@/lib/categories'

interface CategoryPageProps {
  lastUpdatedAt: string | null
}

export function CategoryPage({ lastUpdatedAt }: CategoryPageProps) {
  const { slug } = useParams()
  const category = categories.find((item) => item.slug === slug)

  useEffect(() => {
    if (category) {
      document.title = `${category.name} | hírfolyam`
    }
  }, [category])

  if (!category) {
    return (
      <PageHeader title="Ez a kategória nem található." lastUpdatedAt={lastUpdatedAt} />
    )
  }

  return (
    <>
      <PageHeader
        title={category.name}
        lastUpdatedAt={lastUpdatedAt}
        action={<ArticleLayoutToggle />}
      />

      <ArticleFeed
        key={category.slug}
        category={category.slug}
        label={`${category.name} cikkei`}
        emptyMessage="Ebben a kategóriában még nincs cikk."
      />
    </>
  )
}
