const storageKey = 'hirfolyam.showHotTopics'
const listeners = new Set<() => void>()

// The summary is shown until the reader hides it.
function readHotTopicsVisibility(): boolean {
  try {
    return window.localStorage.getItem(storageKey) !== 'false'
  } catch {
    return true
  }
}

let isVisible = readHotTopicsVisibility()

window.addEventListener('storage', syncHotTopicsVisibility)

export function getHotTopicsVisibility() {
  return isVisible
}

function notifyListeners() {
  for (const listener of listeners) {
    listener()
  }
}

function syncHotTopicsVisibility(event: StorageEvent) {
  if (event.key !== storageKey && event.key !== null) {
    return
  }

  isVisible = readHotTopicsVisibility()
  notifyListeners()
}

export function subscribeToHotTopicsVisibility(listener: () => void) {
  listeners.add(listener)

  return () => {
    listeners.delete(listener)
  }
}

export function updateHotTopicsVisibility(value: boolean) {
  isVisible = value

  try {
    window.localStorage.setItem(storageKey, String(value))
  } catch {
    // Keep the choice in memory if storage is not available.
  }

  notifyListeners()
}
