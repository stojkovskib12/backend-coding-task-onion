import type { Claim, ClaimInput, Cover, CoverInput, CoverType, ProblemDetails } from './contracts';

export class ApiError extends Error {
  readonly status: number;
  readonly details?: ProblemDetails;
  constructor(message: string, status: number, details?: ProblemDetails) { super(message); this.status = status; this.details = details; this.name = 'ApiError'; }
}

export class ClaimsApi {
  private readonly baseUrl: string;
  constructor(baseUrl: string) { this.baseUrl = baseUrl; }

  private async request<T>(path: string, init?: RequestInit): Promise<T> {
    let response: Response;
    try { response = await fetch(`${this.baseUrl.replace(/\/$/, '')}${path}`, { ...init, headers: { ...(init?.body ? { 'Content-Type': 'application/json' } : {}), ...init?.headers } }); }
    catch { throw new ApiError('Could not reach the API. Check that the backend is running and the URL is correct.', 0); }
    if (!response.ok) {
      const details = await response.json().catch(() => undefined) as ProblemDetails | undefined;
      const validationErrors = details?.errors ?? details?.Errors ?? {};
      const messages = Object.entries(validationErrors).flatMap(([field, value]) =>
        (Array.isArray(value) ? value : [value]).map(message => `${field}: ${message}`),
      );
      throw new ApiError(messages.join(' ') || details?.detail || details?.title || `Request failed (${response.status}).`, response.status, details);
    }
    if (response.status === 204) return undefined as T;
    return response.json() as Promise<T>;
  }

  listClaims = () => this.request<Claim[]>('/Claims');
  getClaim = (displayId: number) => this.request<Claim>(`/Claims/${displayId}`);
  createClaim = (input: ClaimInput) => this.request<Claim>('/Claims', { method: 'POST', body: JSON.stringify(input) });
  deleteClaim = (displayId: number) => this.request<void>(`/Claims/${displayId}`, { method: 'DELETE' });
  listCovers = () => this.request<Cover[]>('/Covers');
  getCover = (displayId: number) => this.request<Cover>(`/Covers/${displayId}`);
  createCover = (input: CoverInput) => this.request<Cover>('/Covers', { method: 'POST', body: JSON.stringify(input) });
  deleteCover = (displayId: number) => this.request<void>(`/Covers/${displayId}`, { method: 'DELETE' });
  computePremium = (startDate: string, endDate: string, type: CoverType) => this.request<number>(`/Covers/compute?${new URLSearchParams({ startDate, endDate, coverType: type })}`, { method: 'POST' });
}
