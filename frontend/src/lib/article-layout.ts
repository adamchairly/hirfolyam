import { createStoredValue } from '@/lib/stored-value'

export type ArticleLayout = 'list' | 'grid'

export const articleLayout = createStoredValue<ArticleLayout>(
  'hirfolyam.articleLayout',
  'list',
  (stored) => stored === 'grid' ? 'grid' : 'list',
)
