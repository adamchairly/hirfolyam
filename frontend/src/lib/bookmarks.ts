import { createStoredValue } from '@/lib/stored-value'

// The most recently saved article comes first.
export const bookmarks = createStoredValue<number[]>(
  'hirfolyam.bookmarks',
  [],
  (stored) => {
    const ids: unknown = JSON.parse(stored)

    if (!Array.isArray(ids)) {
      return []
    }

    return [...new Set(ids.filter((id): id is number => Number.isSafeInteger(id) && id > 0))]
  },
  JSON.stringify,
)

export function toggleBookmark(id: number) {
  const current = bookmarks.get()

  bookmarks.set(current.includes(id)
    ? current.filter((bookmark) => bookmark !== id)
    : [id, ...current])
}
