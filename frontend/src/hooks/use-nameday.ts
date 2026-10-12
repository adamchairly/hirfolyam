import { useEffect, useState } from 'react'

import { getNameday } from '@/lib/nameday'

export function useNameday(date: string) {
  const [nameday, setNameday] = useState<string | null>(null)

  useEffect(() => {
    let disposed = false

    getNameday(date)
      .then((names) => {
        if (!disposed) {
          setNameday(names)
        }
      })
      .catch(() => {
        if (!disposed) {
          setNameday(null)
        }
      })

    return () => {
      disposed = true
    }
  }, [date])

  return nameday
}
