export type ArticleLayout = 'list' | 'grid'

const storageKey = 'hirfolyam.articleLayout'
const listeners = new Set<() => void>()

function readArticleLayout(): ArticleLayout {
  try {
    return window.localStorage.getItem(storageKey) === 'grid' ? 'grid' : 'list'
  } catch {
    return 'list'
  }
}

let layout = readArticleLayout()

window.addEventListener('storage', syncArticleLayout)

export function getArticleLayout() {
  return layout
}

function notifyListeners() {
  for (const listener of listeners) {
    listener()
  }
}

function syncArticleLayout(event: StorageEvent) {
  if (event.key !== storageKey && event.key !== null) {
    return
  }

  layout = readArticleLayout()
  notifyListeners()
}

export function subscribeToArticleLayout(listener: () => void) {
  listeners.add(listener)

  return () => {
    listeners.delete(listener)
  }
}

export function updateArticleLayout(value: ArticleLayout) {
  layout = value

  try {
    window.localStorage.setItem(storageKey, value)
  } catch {
    // Keep the layout in memory if storage is not available.
  }

  notifyListeners()
}
