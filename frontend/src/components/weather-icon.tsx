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

export function WeatherIcon({ kind, isDay = true }: { kind: WeatherIconKind, isDay?: boolean }) {
  const Icon = kind === 'partlyCloudy'
    ? (isDay ? CloudSun : CloudMoon)
    : icons[kind]

  return <Icon strokeWidth={1.5} className="size-6 shrink-0 text-neutral-300" />
}
