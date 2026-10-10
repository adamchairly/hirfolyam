import { useCallback, useEffect, useRef } from 'react'
import { useLocation, useNavigate, useSearchParams, type Location } from 'react-router-dom'

import { Button } from '@/components/ui/button'
import { maximumSearchLength } from '@/lib/search'
import { cn } from '@/lib/utils'

interface SiteSearchProps {
  expanded: boolean
  onExpandedChange: (expanded: boolean) => void
}

interface SearchLocationState {
  searchOrigin?: Location
}

export function SiteSearch({ expanded, onExpandedChange }: SiteSearchProps) {
  const location = useLocation()
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const formRef = useRef<HTMLFormElement>(null)
  const inputRef = useRef<HTMLInputElement>(null)
  const toggleRef = useRef<HTMLButtonElement>(null)
  const isSearchPage = location.pathname === '/search'
  const search = isSearchPage ? searchParams.get('q') ?? '' : ''
  const searchOrigin = (location.state as SearchLocationState | null)?.searchOrigin
  const searchOriginRef = useRef(searchOrigin)

  const closeSearch = useCallback((restoreFocus = false) => {
    if (restoreFocus) {
      toggleRef.current?.focus({ preventScroll: true })
    }

    onExpandedChange(false)

    if (isSearchPage && !search.trim()) {
      const returnLocation = searchOrigin ?? searchOriginRef.current

      void navigate(returnLocation ?? '/', {
        replace: true,
        state: returnLocation?.state,
      })
    }
  }, [isSearchPage, navigate, onExpandedChange, search, searchOrigin])

  useEffect(() => {
    if (searchOrigin) {
      searchOriginRef.current = searchOrigin
    }
  }, [searchOrigin])

  useEffect(() => {
    if (expanded) {
      inputRef.current?.focus({ preventScroll: true })
    }
  }, [expanded])

  useEffect(() => {
    if (!expanded) {
      return
    }

    function handleOutsidePointer(event: PointerEvent) {
      if (event.target instanceof Node && !formRef.current?.contains(event.target)) {
        closeSearch()
      }
    }

    document.addEventListener('pointerdown', handleOutsidePointer)

    return () => {
      document.removeEventListener('pointerdown', handleOutsidePointer)
    }
  }, [closeSearch, expanded])

  function updateSearch(value: string) {
    const params = new URLSearchParams()
    const origin = isSearchPage ? searchOrigin ?? searchOriginRef.current : location

    if (value) {
      params.set('q', value)
    }

    void navigate(
      {
        pathname: '/search',
        search: params.toString(),
      },
      {
        replace: isSearchPage,
        state: {
          searchOrigin: origin,
        },
      },
    )
  }

  function clearSearch() {
    updateSearch('')
    inputRef.current?.focus({ preventScroll: true })
  }

  return (
    <form
      ref={formRef}
      role="search"
      className={cn(
        'flex h-10 max-w-full items-center gap-2 overflow-clip rounded-md border border-neutral-600 bg-neutral-900 px-[3px] transition-[width] duration-200 focus-within:border-neutral-300 motion-reduce:transition-none sm:max-w-none',
        expanded
          ? 'w-full sm:w-60 md:w-56'
          : 'w-28',
      )}
      onSubmit={(event) => {
        event.preventDefault()

        if (search.trim()) {
          updateSearch(search)
        }
      }}
      onKeyDown={(event) => {
        if (event.key === 'Escape') {
          event.preventDefault()
          closeSearch(true)
        }
      }}
      onBlur={(event) => {
        if (expanded && !event.currentTarget.contains(event.relatedTarget)) {
          closeSearch()
        }
      }}
    >
      <button
        ref={toggleRef}
        type="button"
        aria-label={expanded ? 'Kereső bezárása' : 'Keresés'}
        aria-expanded={expanded}
        title={expanded ? 'Kereső bezárása' : 'Keresés'}
        className={cn(
          'inline-flex h-8 shrink-0 items-center justify-center gap-2 rounded-sm bg-transparent text-xs font-semibold text-neutral-300 transition-colors hover:text-neutral-50 focus-visible:ring-2 focus-visible:ring-neutral-400 focus-visible:ring-inset focus-visible:outline-none sm:text-sm',
          expanded ? 'w-8' : 'w-full px-2',
        )}
        onClick={() => {
          if (expanded) {
            closeSearch(true)
          } else {
            onExpandedChange(true)
          }
        }}
      >
        <svg
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          strokeWidth={2}
          strokeLinecap="round"
          aria-hidden="true"
          className="size-4"
        >
          <circle cx="10.5" cy="10.5" r="6.5" />
          <path d="m16 16 4 4" />
        </svg>

        {!expanded && <span>Keresés</span>}
      </button>

      {expanded && (
        <>
          <label htmlFor="article-search" className="sr-only">
            Keresés a cikkek címében és leírásában
          </label>

          <input
            ref={inputRef}
            id="article-search"
            type="search"
            name="q"
            value={search}
            maxLength={maximumSearchLength}
            autoComplete="off"
            aria-describedby="search-help"
            aria-invalid={search.length > maximumSearchLength}
            placeholder="Keresés…"
            className="h-full min-w-0 flex-1 bg-transparent pr-1 text-base text-neutral-50 outline-none placeholder:text-neutral-400 [&::-webkit-search-cancel-button]:appearance-none"
            onChange={(event) => {
              updateSearch(event.target.value)
            }}
          />

          {search && (
            <Button
              type="button"
              variant="ghost"
              aria-label="Keresés törlése"
              size="icon"
              title="Keresés törlése"
              className="size-7 shrink-0"
              onClick={clearSearch}
            >
              <svg
                viewBox="0 0 24 24"
                fill="none"
                stroke="currentColor"
                strokeWidth={2}
                strokeLinecap="round"
                aria-hidden="true"
                className="size-4"
              >
                <path d="m6 6 12 12M6 18 18 6" />
              </svg>
            </Button>
          )}
        </>
      )}

      <p id="search-help" className="sr-only">
        Minden kategóriában keresünk. A találatok gépelés után automatikusan megjelennek.
      </p>
    </form>
  )
}
