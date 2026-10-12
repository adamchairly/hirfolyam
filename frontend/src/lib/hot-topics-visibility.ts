import { createStoredValue } from '@/lib/stored-value'

// The summary is shown until the reader hides it.
export const hotTopicsVisibility = createStoredValue(
  'hirfolyam.showHotTopics',
  true,
  (stored) => stored !== 'false',
)
