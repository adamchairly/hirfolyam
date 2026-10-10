import type { Category } from '@/types/article'

// Use the slugs from the backend category catalog.
export const categories: Category[] = [
  {
    slug: 'politics',
    name: 'Politika',
  },
  {
    slug: 'economy',
    name: 'Gazdaság',
  },
  {
    slug: 'world',
    name: 'Külföld',
  },
  {
    slug: 'domestic',
    name: 'Belföld',
  },
  {
    slug: 'culture',
    name: 'Kultúra',
  },
  {
    slug: 'science-tech',
    name: 'Tudomány',
  },
  {
    slug: 'sport',
    name: 'Sport',
  },
  {
    slug: 'tabloid',
    name: 'Bulvár',
  },
  {
    slug: 'lifestyle',
    name: 'Életmód',
  },
]
