export type CoverType = 'Yacht' | 'PassengerShip' | 'ContainerShip' | 'BulkCarrier' | 'Tanker';
export type ClaimType = 'Collision' | 'Grounding' | 'BadWeather' | 'Fire';
export interface Claim { id: string; displayId: number; coverId: string; created: string; name: string; type: ClaimType; damageCost: number; }
export interface Cover { id: string; displayId: number; startDate: string; endDate: string; type: CoverType; premium: number; }
export interface ClaimInput { coverId: string; created: string; name: string; type: ClaimType; damageCost: number; }
export interface CoverInput { type: CoverType; startDate: string; endDate: string; }
export interface ProblemDetails { title?: string; detail?: string; errors?: Record<string, string[] | string>; Errors?: Record<string, string[] | string>; }
