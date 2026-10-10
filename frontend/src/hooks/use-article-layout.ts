import { useSyncExternalStore } from 'react'

import {
  getArticleLayout,
  subscribeToArticleLayout,
  updateArticleLayout,
} from '@/lib/article-layout'

export function useArticleLayout() {
  const layout = useSyncExternalStore(subscribeToArticleLayout, getArticleLayout)

  return {
    layout,
    updateLayout: updateArticleLayout,
  }
}
