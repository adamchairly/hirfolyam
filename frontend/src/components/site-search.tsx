import { Search, X } from 'lucide-react'
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
        'flex h-10 max-w-full items-center gap-2 overflow-clip rounded-md bg-black px-[3px] transition-[width] duration-200 motion-reduce:transition-none sm:max-w-none',
        expanded
          ? 'w-full sm:w-60 md:w-56'
          : 'w-24 sm:w-26',
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
          'inline-flex h-8 shrink-0 items-center gap-2 rounded-sm bg-transparent px-2 text-xs font-semibold text-neutral-300 transition-colors hover:text-neutral-50 focus-visible:ring-2 focus-visible:ring-neutral-400 focus-visible:ring-inset focus-visible:outline-none sm:text-sm',
          expanded ? 'w-8' : 'w-full',
        )}
        onClick={() => {
          if (expanded) {
            closeSearch(true)
          } else {
            onExpandedChange(true)
          }
        }}
      >
        <Search className="size-4 shrink-0" />

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
              <X className="size-4" />
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
