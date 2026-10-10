export type WeatherIconKind = 'sun' | 'moon' | 'partlyCloudy' | 'cloud' | 'fog' | 'rain' | 'snow' | 'storm' | 'unknown'

interface WeatherCondition {
  icon: WeatherIconKind
  label: string
}

export function getWeatherCondition(code: number, isDay: boolean): WeatherCondition {
  switch (code) {
    case 0:
    case 1:
      return {
        icon: isDay ? 'sun' : 'moon',
        label: isDay ? 'Derült, napos idő' : 'Derült éjszaka',
      }
    case 2:
      return {
        icon: 'partlyCloudy',
        label: 'Részben felhős',
      }
    case 3:
      return {
        icon: 'cloud',
        label: 'Borult',
      }
    case 45:
    case 48:
      return {
        icon: 'fog',
        label: 'Ködös',
      }
    case 51:
    case 53:
    case 55:
      return {
        icon: 'rain',
        label: 'Szitálás',
      }
    case 56:
    case 57:
    case 66:
    case 67:
      return {
        icon: 'rain',
        label: 'Ónos csapadék',
      }
    case 61:
    case 63:
    case 65:
    case 80:
    case 81:
    case 82:
      return {
        icon: 'rain',
        label: 'Esős',
      }
    case 71:
    case 73:
    case 75:
    case 77:
    case 85:
    case 86:
      return {
        icon: 'snow',
        label: 'Havazás',
      }
    case 95:
    case 96:
    case 97:
    case 99:
      return {
        icon: 'storm',
        label: 'Zivatar',
      }
    default:
      return {
        icon: 'unknown',
        label: 'Ismeretlen időjárási állapot',
      }
  }
}

