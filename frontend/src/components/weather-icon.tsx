import {
  Cloud,
  CloudFog,
  CloudLightning,
  CloudMoon,
  CloudRain,
  CloudSnow,
  CloudSun,
  Moon,
  Sun,
  Thermometer,
  type LucideIcon,
} from 'lucide-react'

import { cn } from '@/lib/utils'
import type { WeatherIconKind } from '@/lib/weather-condition'

const icons: Record<Exclude<WeatherIconKind, 'partlyCloudy'>, LucideIcon> = {
  sun: Sun,
  moon: Moon,
  cloud: Cloud,
  fog: CloudFog,
  rain: CloudRain,
  snow: CloudSnow,
  storm: CloudLightning,
  unknown: Thermometer,
}

interface WeatherIconProps {
  kind: WeatherIconKind
  isDay?: boolean
  className?: string
}

export function WeatherIcon({ kind, isDay = true, className }: WeatherIconProps) {
  const Icon = kind === 'partlyCloudy'
    ? (isDay ? CloudSun : CloudMoon)
    : icons[kind]

  return <Icon strokeWidth={1.5} className={cn('size-6 shrink-0 text-neutral-300', className)} />
}
