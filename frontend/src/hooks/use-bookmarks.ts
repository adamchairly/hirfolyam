import { useSyncExternalStore } from 'react'

import {
  getBookmarks,
  subscribeToBookmarks,
  toggleBookmark,
} from '@/lib/bookmarks'

export function useBookmarks() {
  const bookmarks = useSyncExternalStore(subscribeToBookmarks, getBookmarks)

  return {
    bookmarks,
    toggleBookmark,
  }
}
