import { useEffect, useState } from 'react'

export function useDebouncedValue<T>(value: T, delay: number, initialValue = value): T {
  const [debouncedValue, setDebouncedValue] = useState(initialValue)

  useEffect(() => {
    const timeout = window.setTimeout(() => {
      setDebouncedValue(value)
    }, delay)

    return () => {
      window.clearTimeout(timeout)
    }
  }, [value, delay])

  return debouncedValue
}
