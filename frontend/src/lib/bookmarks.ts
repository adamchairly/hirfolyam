const storageKey = 'hirfolyam.bookmarks'
const listeners = new Set<() => void>()

// The most recently saved article comes first.
function readBookmarks(): number[] {
  try {
    const stored: unknown = JSON.parse(window.localStorage.getItem(storageKey) ?? '[]')

    if (!Array.isArray(stored)) {
      return []
    }

    return [...new Set(stored.filter((id): id is number => Number.isSafeInteger(id) && id > 0))]
  } catch {
    return []
  }
}

let bookmarks = readBookmarks()

window.addEventListener('storage', syncBookmarks)

export function getBookmarks() {
  return bookmarks
}

function notifyListeners() {
  for (const listener of listeners) {
    listener()
  }
}

function syncBookmarks(event: StorageEvent) {
  if (event.key !== storageKey && event.key !== null) {
    return
  }

  bookmarks = readBookmarks()
  notifyListeners()
}

export function subscribeToBookmarks(listener: () => void) {
  listeners.add(listener)

  return () => {
    listeners.delete(listener)
  }
}

export function toggleBookmark(id: number) {
  bookmarks = bookmarks.includes(id)
    ? bookmarks.filter((bookmark) => bookmark !== id)
    : [id, ...bookmarks]

  try {
    window.localStorage.setItem(storageKey, JSON.stringify(bookmarks))
  } catch {
    // Keep the bookmarks in memory if storage is not available.
  }

  notifyListeners()
}
