import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'

import { ScrollToTop } from '@/components/scroll-to-top'
import { SiteHeader } from '@/components/site-header'
import { useSourceSelection } from '@/hooks/use-source-selection'
import { CategoryPage } from '@/pages/category-page'
import { FreshPage } from '@/pages/fresh-page'
import { HotTopicPage } from '@/pages/hot-topic-page'
import { SavedPage } from '@/pages/saved-page'
import { SearchPage } from '@/pages/search-page'
import { SourcesPage } from '@/pages/sources-page'

export default function App() {
  const { lastUpdatedAt } = useSourceSelection()

  return (
    <BrowserRouter>
      <div className="min-h-svh bg-neutral-800 font-sans text-neutral-50">
        <SiteHeader />

        <main className="mx-auto max-w-5xl px-4 pb-12 sm:px-6">
          <Routes>
            <Route path="/" element={<Navigate to="/fresh" replace />} />
            <Route path="/fresh" element={<FreshPage lastUpdatedAt={lastUpdatedAt} />} />
            <Route path="/sources" element={<SourcesPage />} />
            <Route path="/saved" element={<SavedPage />} />
            <Route path="/soruces" element={<Navigate to="/sources" replace />} />
            <Route path="/search" element={<SearchPage />} />
            <Route path="/topics/:id" element={<HotTopicPage />} />
            <Route path="/:slug" element={<CategoryPage lastUpdatedAt={lastUpdatedAt} />} />
            <Route path="*" element={<CategoryPage lastUpdatedAt={lastUpdatedAt} />} />
          </Routes>
        </main>

        <ScrollToTop />
      </div>
    </BrowserRouter>
  )
}
