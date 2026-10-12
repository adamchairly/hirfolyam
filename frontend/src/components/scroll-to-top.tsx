import { ArrowUp } from 'lucide-react'
import { useEffect, useState } from 'react'

import { Button } from '@/components/ui/button'

const scrollThreshold = 400

export function ScrollToTop() {
  const [isVisible, setIsVisible] = useState(false)

  useEffect(() => {
    function updateVisibility() {
      setIsVisible(window.scrollY > scrollThreshold)
    }

    updateVisibility()

    window.addEventListener('scroll', updateVisibility, { passive: true })

    return () => {
      window.removeEventListener('scroll', updateVisibility)
    }
  }, [])

  function scrollToTop() {
    const prefersReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches

    window.scrollTo({
      top: 0,
      behavior: prefersReducedMotion ? 'instant' : 'smooth',
    })
  }

  if (!isVisible) {
    return null
  }

  return (
    <Button
      type="button"
      size="icon"
      aria-label="Vissza a lap tetejére"
      title="Vissza a lap tetejére"
      className="fixed right-4 bottom-4 z-40 size-12 rounded-full shadow-lg motion-reduce:transition-none sm:right-6 sm:bottom-6"
      onClick={scrollToTop}
    >
      <ArrowUp className="size-5" />
    </Button>
  )
}
