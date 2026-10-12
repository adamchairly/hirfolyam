import { useStoredValue } from '@/hooks/use-stored-value'
import { articleLayout } from '@/lib/article-layout'

export function useArticleLayout() {
  const layout = useStoredValue(articleLayout)

  return {
    layout,
    updateLayout: articleLayout.set,
  }
}
