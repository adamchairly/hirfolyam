import { useStoredValue } from '@/hooks/use-stored-value'
import { bookmarks as storedBookmarks, toggleBookmark } from '@/lib/bookmarks'

export function useBookmarks() {
  const bookmarks = useStoredValue(storedBookmarks)

  return {
    bookmarks,
    toggleBookmark,
  }
}
