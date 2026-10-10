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
      <svg
        xmlns="http://www.w3.org/2000/svg"
        viewBox="0 0 24 24"
        fill="none"
        stroke="currentColor"
        strokeWidth={2}
        strokeLinecap="round"
        strokeLinejoin="round"
        aria-hidden="true"
        className="size-5"
      >
        <path d="m5 12 7-7 7 7M12 5v14" />
      </svg>
    </Button>
  )
}
